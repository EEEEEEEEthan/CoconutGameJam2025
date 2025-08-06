// 自定义的红色阴影Shader - Unlit版本
// 这个shader不受光照影响，但会根据阴影给物体添加红色效果
Shader "Custom/RedShadowShader"
{
    // Properties是shader的可调节参数，会在Material Inspector中显示
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}        // 基础贴图
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)   // 基础颜色，默认白色
        _Gradient ("Gradient", 2D) = "white" {}           // Toon渐变纹理，u=0面光，u=1背光
        _ShadowColor ("Shadow Color", Color) = (1, 0, 0, 1) // 阴影区域的颜色，默认红色
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.5 // 阴影效果强度，0-1范围
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.9 // 阴影阈值，控制阴影边缘锐度
        _ShadowBlurRadius ("Shadow Blur Radius", Range(0, 0.01)) = 0.003 // 阴影模糊半径
        _ShadowSampleCount ("Shadow Sample Count", Range(1, 32)) = 5 // 阴影采样次数（奇数）
    }

    // SubShader包含实际的渲染代码
    SubShader
    {
        // Tags告诉Unity如何渲染这个shader
        Tags 
        {
            "RenderType" = "Opaque"           // 不透明物体
            "RenderPipeline" = "UniversalPipeline" // 使用URP渲染管线
            "Queue" = "Geometry"             // 在几何体队列中渲染（最常用的队列）
        }

        // 主要的渲染Pass，负责绘制物体的颜色
        Pass
        {
            Name "UniversalForward"  // Pass名称
            Tags { "LightMode" = "UniversalForward" } // 告诉URP这是前向渲染Pass

            HLSLPROGRAM
            // 指定顶点着色器和片元着色器的入口函数
            #pragma vertex vert     // 顶点着色器函数名
            #pragma fragment frag   // 片元着色器函数名

            // URP的多重编译关键词 - 让shader支持不同的渲染特性
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS          // 主光源阴影支持
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE  // 级联阴影贴图支持
            #pragma multi_compile _ _SHADOWS_SOFT                // 软阴影支持
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS // 额外光源支持
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS           // 额外光源阴影

            // 包含URP的核心函数库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"     // 核心函数
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" // 光照函数
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"  // 阴影函数

            // 顶点着色器的输入数据结构
            struct Attributes
            {
                float4 positionOS : POSITION;  // 物体空间的顶点位置
                float3 normalOS : NORMAL;      // 物体空间的法线
                float2 uv : TEXCOORD0;         // 纹理坐标
            };

            // 从顶点着色器传递到片元着色器的数据结构
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // 裁剪空间位置（最终屏幕位置）
                float2 uv : TEXCOORD0;           // 纹理坐标
                float3 positionWS : TEXCOORD1;   // 世界空间位置（用于计算阴影）
                float3 normalWS : TEXCOORD2;     // 世界空间法线
            };

            // 声明纹理和采样器
            TEXTURE2D(_BaseMap);        // 声明基础纹理
            SAMPLER(sampler_BaseMap);   // 基础纹理采样器
            TEXTURE2D(_Gradient);       // 声明Toon渐变纹理
            SAMPLER(sampler_Gradient);  // 渐变纹理采样器

            // 常量缓冲区，包含Material的属性
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;      // 纹理的缩放和偏移 (Scale, Transform)
                float4 _Gradient_ST;     // 渐变纹理的缩放和偏移
                half4 _BaseColor;        // 基础颜色
                half4 _ShadowColor;      // 阴影颜色
                half _ShadowIntensity;   // 阴影强度
                half _ShadowThreshold;   // 阴影阈值
                half _ShadowBlurRadius;  // 阴影模糊半径
                half _ShadowSampleCount; // 阴影采样次数
            CBUFFER_END

            // 计算模糊阴影的函数 - 通过多次采样获得平均值
            half CalculateBlurredShadow(float3 worldPos)
            {
                half shadowSum = 0;
                int sampleCount = (int)_ShadowSampleCount;
                
                // 确保采样数是奇数，这样可以包含中心点
                sampleCount = (sampleCount % 2 == 0) ? sampleCount + 1 : sampleCount;
                
                // 计算采样网格的半径
                int halfSamples = sampleCount / 2;
                
                // 在XZ平面上进行网格采样（阴影通常在水平面上投射）
                for(int x = -halfSamples; x <= halfSamples; x++)
                {
                    for(int z = -halfSamples; z <= halfSamples; z++)
                    {
                        // 计算采样偏移位置
                        float3 samplePos = worldPos + float3(
                            x * _ShadowBlurRadius, 
                            0, 
                            z * _ShadowBlurRadius
                        );
                        
                        // 获取该位置的阴影坐标
                        float4 shadowCoord = TransformWorldToShadowCoord(samplePos);
                        
                        // 采样阴影并累加
                        Light sampleLight = GetMainLight(shadowCoord);
                        shadowSum += sampleLight.shadowAttenuation;
                    }
                }
                
                // 返回平均值
                return shadowSum / (sampleCount * sampleCount);
            }

            // 顶点着色器函数
            // 每个顶点都会执行一次，负责将顶点从物体空间变换到屏幕空间
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;  // 初始化输出结构体
                
                // 获取顶点在不同空间中的位置
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;  // 裁剪空间位置（用于GPU光栅化）
                output.positionWS = vertexInput.positionWS;  // 世界空间位置（用于阴影计算）
                
                // 应用纹理的缩放和偏移变换
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                // 将法线从物体空间转换到世界空间
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                return output;
            }

            // 片元着色器函数
            // 每个像素都会执行一次，负责计算该像素的最终颜色
            half4 frag(Varyings input) : SV_Target
            {
                // 1. 采样基础纹理并与基础颜色相乘
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                // 2. 获取主光源信息（不包含阴影，用于光照计算）
                Light mainLight = GetMainLight();
                
                // 3. 计算Toon渐变采样坐标
                float3 lightDir = normalize(mainLight.direction);
                float3 normalWS = normalize(input.normalWS);
                
                // 计算法线和光线的dot product，范围[-1, 1]
                float NdotL = dot(normalWS, lightDir);
                // 将范围转换为[0, 1]，0=背光，1=面光
                float u = 0.5 - NdotL * 0.5;
                
                // 采样Toon渐变纹理，使用u作为x坐标，y坐标固定为0.5
                half4 gradientColor = SAMPLE_TEXTURE2D(_Gradient, sampler_Gradient, float2(u, 0.5));
                
                // 基础颜色与渐变相乘
                baseColor.rgb *= gradientColor.rgb;
                
                // 4. 计算模糊阴影衰减值
                // 使用多次采样获得平滑的阴影效果
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    half shadowAttenuation = CalculateBlurredShadow(input.positionWS);
                #else
                    half shadowAttenuation = 1.0; // 没有阴影时完全亮
                #endif
                
                // 应用阈值处理
                shadowAttenuation = step(_ShadowThreshold, shadowAttenuation);
                
                // 5. 以处理后的基础颜色作为起始颜色
                half4 color = baseColor;
                
                // 6. 应用红色阴影效果
                // shadowFactor: 0=无阴影区域，1=完全阴影区域
                half shadowFactor = 1.0 - shadowAttenuation;
                // 在阴影区域混合红色，非阴影区域保持原色
                half3 shadowTint = lerp(half3(1, 1, 1), _ShadowColor.rgb, shadowFactor * _ShadowIntensity);
                color.rgb *= shadowTint;
                
                return color;
            }
            ENDHLSL
        }

        // 阴影投射Pass - 让这个物体能够投射阴影到其他物体上
        // 这个Pass不绘制颜色，只写入深度信息到阴影贴图
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" } // 告诉URP这是阴影投射Pass

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // 深度only Pass - 只写入深度缓冲区，用于深度预处理
        // 在某些渲染管线优化中使用，可以提前剔除被遮挡的像素
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" } // 告诉URP这是深度only Pass

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}