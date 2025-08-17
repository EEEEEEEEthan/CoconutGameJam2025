using UnityEngine;
using UnityEditor;
using System.IO;

public class NoiseTextureGenerator
{
    [MenuItem("Assets/Create/Noise Texture 2048x2048")]
    public static void CreateNoiseTexture()
    {
        // 创建2048x2048的纹理
        Texture2D noiseTexture = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
        
        // 生成随机噪声数据
        Color[] pixels = new Color[2048 * 2048];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            // 每个通道都是0-255的随机值，转换为0-1范围
            pixels[i] = new Color(
                Random.Range(0f, 1f),  // R
                Random.Range(0f, 1f),  // G
                Random.Range(0f, 1f),  // B
                Random.Range(0f, 1f)   // A
            );
        }
        
        // 应用像素数据
        noiseTexture.SetPixels(pixels);
        noiseTexture.Apply();
        
        // 转换为PNG字节数组
        byte[] pngData = noiseTexture.EncodeToPNG();
        
        // 获取当前选中的文件夹路径，如果没有选中则使用Assets根目录
        string selectedPath = "Assets";
        if (Selection.activeObject != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(assetPath))
            {
                selectedPath = assetPath;
            }
            else if (File.Exists(assetPath))
            {
                selectedPath = Path.GetDirectoryName(assetPath);
            }
        }
        
        // 生成唯一的文件名
        string fileName = "NoiseTexture_2048x2048";
        string filePath = Path.Combine(selectedPath, fileName + ".png");
        int counter = 1;
        
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(selectedPath, fileName + "_" + counter + ".png");
            counter++;
        }
        
        // 保存文件
        File.WriteAllBytes(filePath, pngData);
        
        // 刷新资源数据库
        AssetDatabase.Refresh();
        
        // 选中新创建的纹理
        Object createdAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
        Selection.activeObject = createdAsset;
        EditorGUIUtility.PingObject(createdAsset);
        
        // 清理临时纹理
        Object.DestroyImmediate(noiseTexture);
        
        Debug.Log($"噪声纹理已创建: {filePath}");
    }
}