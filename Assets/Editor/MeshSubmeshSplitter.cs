using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshSubmeshSplitter
{
    [MenuItem("Assets/Split Submeshes", false, 30)]
    private static void SplitSubmeshes()
    {
        // 获取选中的对象
        Object[] selectedObjects = Selection.objects;
        
        foreach (Object obj in selectedObjects)
        {
            // 检查是否为Mesh资源
            if (obj is Mesh mesh)
            {
                SplitMeshSubmeshes(mesh);
            }
            else
            {
                // 检查是否为包含Mesh的模型文件
                string assetPath = AssetDatabase.GetAssetPath(obj);
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                
                foreach (Object asset in assets)
                {
                    if (asset is Mesh meshAsset)
                    {
                        SplitMeshSubmeshes(meshAsset);
                    }
                }
            }
        }
        
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Assets/Split Submeshes", true)]
    private static bool ValidateSplitSubmeshes()
    {
        // 验证选中的对象是否包含Mesh
        Object[] selectedObjects = Selection.objects;
        
        foreach (Object obj in selectedObjects)
        {
            if (obj is Mesh)
            {
                return true;
            }
            
            // 检查模型文件是否包含Mesh
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object asset in assets)
                {
                    if (asset is Mesh)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    private static void SplitMeshSubmeshes(Mesh originalMesh)
    {
        if (originalMesh == null || originalMesh.subMeshCount <= 1)
        {
            Debug.LogWarning($"Mesh '{originalMesh.name}' has no submeshes to split.");
            return;
        }
        
        // 获取原始Mesh的路径
        string originalPath = AssetDatabase.GetAssetPath(originalMesh);
        string directory = Path.GetDirectoryName(originalPath);
        string baseName = Path.GetFileNameWithoutExtension(originalPath);
        
        // 如果原始Mesh是模型文件的一部分，使用Mesh的名称
        if (string.IsNullOrEmpty(baseName) || baseName != originalMesh.name)
        {
            baseName = originalMesh.name;
        }
        
        Debug.Log($"Splitting mesh '{originalMesh.name}' with {originalMesh.subMeshCount} submeshes.");
        
        // 为每个submesh创建单独的Mesh
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            Mesh newMesh = CreateSubmesh(originalMesh, i);
            if (newMesh != null)
            {
                // 生成新的文件名
                string newFileName = $"{baseName}_submesh_{i}.asset";
                string newPath = Path.Combine(directory, newFileName).Replace('\\', '/');
                
                // 确保文件名唯一
                newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                
                // 创建并保存新的Mesh资源
                AssetDatabase.CreateAsset(newMesh, newPath);
                Debug.Log($"Created submesh: {newPath}");
            }
        }
    }
    
    private static Mesh CreateSubmesh(Mesh originalMesh, int submeshIndex)
    {
        if (submeshIndex >= originalMesh.subMeshCount)
        {
            return null;
        }
        
        // 获取submesh的三角形索引
        int[] triangles = originalMesh.GetTriangles(submeshIndex);
        if (triangles.Length == 0)
        {
            return null;
        }
        
        // 创建新的Mesh
        Mesh newMesh = new Mesh();
        newMesh.name = $"{originalMesh.name}_submesh_{submeshIndex}";
        
        // 复制顶点数据
        newMesh.vertices = originalMesh.vertices;
        newMesh.normals = originalMesh.normals;
        newMesh.tangents = originalMesh.tangents;
        newMesh.colors = originalMesh.colors;
        newMesh.colors32 = originalMesh.colors32;
        
        // 复制UV数据
        for (int uvChannel = 0; uvChannel < 8; uvChannel++)
        {
            var uvs = originalMesh.uv;
            if (uvs.Length > 0 && uvs[0] != Vector2.zero)
            {
                newMesh.SetUVs(uvChannel, uvs);
            }
        }
        
        // 设置三角形（只有一个submesh）
        newMesh.triangles = triangles;
        
        // 复制其他属性
        newMesh.bounds = originalMesh.bounds;
        
        // 重新计算法线和切线（如果需要）
        if (originalMesh.normals == null || originalMesh.normals.Length == 0)
        {
            newMesh.RecalculateNormals();
        }
        
        if (originalMesh.tangents == null || originalMesh.tangents.Length == 0)
        {
            newMesh.RecalculateTangents();
        }
        
        return newMesh;
    }
}
