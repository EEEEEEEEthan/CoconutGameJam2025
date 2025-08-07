Shader "Custom/ToonShader"
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
        _DiagonalDensity ("Diagonal Line Density", Range(1, 100)) = 20 // 斜线密度，控制斜线间距
        _DiagonalWidth ("Diagonal Line Width", Range(0.1, 5)) = 1 // 斜线宽度
        
        [Header(Outline Settings)]
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01 // 描边宽度
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // 描边颜色，默认黑色
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

        // 描边Pass - 通过法线外扩绘制描边效果
        // 这个Pass会先渲染，绘制放大的模型作为描边
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            // 渲染状态设置
            Cull Front      // 剔除前面（只渲染背面）
            ZTest LEqual    // 深度测试
            ZWrite On       // 写入深度
            
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // 描边用的材质属性声明
            CBUFFER_START(UnityPerMaterial)
                half _OutlineWidth;      // 描边宽度
                half4 _OutlineColor;     // 描边颜色
            CBUFFER_END
            
            // 描边用的顶点输入结构
            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            // 描边用的顶点输出结构
            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;
            };
            
            // 描边顶点着色器
            OutlineVaryings OutlineVert(OutlineAttributes input)
            {
                OutlineVaryings output;
                
                // 将法线转换到世界空间并归一化
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                normalWS = normalize(normalWS);
                
                // 将顶点位置转换到世界空间
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                // 沿法线方向外扩顶点位置
                positionWS += normalWS * _OutlineWidth;
                
                // 转换到裁剪空间
                output.positionCS = TransformWorldToHClip(positionWS);
                
                return output;
            }
            
            // 描边片元着色器 - 只需要输出描边颜色
            half4 OutlineFrag(OutlineVaryings input) : SV_Target
            {
                return _OutlineColor;
            }
            
            ENDHLSL
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
                float4 screenPos : TEXCOORD3;    // 屏幕空间坐标（用于斜线图案）
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
                half _DiagonalDensity;   // 斜线密度
                half _DiagonalWidth;     // 斜线宽度
                half _OutlineWidth;      // 描边宽度
                half4 _OutlineColor;     // 描边颜色
            CBUFFER_END

            // 构建与法线垂直的坐标系的辅助函数
            void BuildTangentSpace(float3 normal, out float3 tangent, out float3 bitangent)
            {
                // 使用世界空间上方向作为参考向量
                float3 up = float3(0, 1, 0);
                
                // 如果法线接近上方向，使用前方向作为参考
                if (abs(dot(normal, up)) > 0.9)
                {
                    up = float3(0, 0, 1);
                }
                
                // 计算切向量（与法线垂直）
                tangent = normalize(cross(up, normal));
                
                // 计算副切向量（与法线和切向量都垂直）
                bitangent = normalize(cross(normal, tangent));
            }

            // 计算模糊阴影的函数 - 通过多次采样获得平均值
            half CalculateBlurredShadow(float3 worldPos, float3 normal)
            {
                half shadowSum = 0;
                int sampleCount = (int)_ShadowSampleCount;
                
                // 确保采样数是奇数，这样可以包含中心点
                sampleCount = (sampleCount % 2 == 0) ? sampleCount + 1 : sampleCount;
                
                // 计算采样网格的半径
                int halfSamples = sampleCount / 2;
                
                // 构建与法线垂直的坐标系
                float3 tangent, bitangent;
                BuildTangentSpace(normal, tangent, bitangent);
                
                // 在与法线垂直的平面上进行网格采样
                for(int x = -halfSamples; x <= halfSamples; x++)
                {
                    for(int z = -halfSamples; z <= halfSamples; z++)
                    {
                        // 计算采样偏移位置（在切空间平面上）
                        float3 samplePos = worldPos + 
                            tangent * (x * _ShadowBlurRadius) + 
                            bitangent * (z * _ShadowBlurRadius);
                        
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

            // 生成屏幕空间斜线图案的函数
            // screenPos: 屏幕空间坐标（像素坐标）
            // 返回值: 1.0表示斜线区域，0.0表示空白区域
            half GenerateDiagonalPattern(float2 screenPos)
            {
                // 计算左上到右下的斜线
                // 使用 (x + y) 来生成对角线模式
                float diagonal = screenPos.x + screenPos.y;
                
                // 根据密度调整斜线间距
                diagonal *= _DiagonalDensity * 0.01; // 0.01是缩放因子，调整合适的密度范围
                
                // 使用fmod生成周期性图案
                float pattern = fmod(diagonal, _DiagonalWidth + 1.0);
                
                // 当pattern小于_DiagonalWidth时，绘制斜线
                return step(pattern, _DiagonalWidth);
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
                
                // 计算屏幕空间坐标（用于斜线图案）
                output.screenPos = ComputeScreenPos(output.positionCS);
                
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
                    half shadowAttenuation = CalculateBlurredShadow(input.positionWS, normalWS);
                #else
                    half shadowAttenuation = 1.0; // 没有阴影时完全亮
                #endif
                
                // 应用阈值处理
                shadowAttenuation = max(step(shadowAttenuation, _ShadowThreshold), step(_ShadowThreshold, u));
                
                // 5. 以处理后的基础颜色作为起始颜色
                half4 color = baseColor;
                
                // 6. 应用斜线阴影效果
                // shadowFactor: 0=无阴影区域，1=完全阴影区域
                half shadowFactor = shadowAttenuation;
                
                // 只在阴影区域应用斜线图案
                if(shadowFactor > 0.01) // 避免在非阴影区域计算
                {
                    // 计算屏幕像素坐标
                    float2 screenPixelPos = input.screenPos.xy / input.screenPos.w * _ScreenParams.xy;
                    
                    // 生成斜线图案
                    half diagonalMask = GenerateDiagonalPattern(screenPixelPos);
                    
                    // 在斜线区域应用阴影颜色，其他区域保持原色
                    // diagonalMask: 1.0=斜线区域，0.0=空白区域
                    half3 shadowTint = lerp(half3(1, 1, 1), _ShadowColor.rgb, diagonalMask * shadowFactor * _ShadowIntensity);
                    color.rgb *= shadowTint;
                }
                
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
