using UnityEngine;
using UnityEngine.Rendering;
namespace Game.Gameplay
{
	public class WaterSpace : MonoBehaviour
	{
		const float Sqrt3Over2 = 0.8660254037844386f; // Mathf.Sqrt(3f)/2f 的常量缓存
		[SerializeField] Mesh mesh;
		[SerializeField, Min(1),] int width = 100; // 列数（每行顶点数为 width + 1）
		[SerializeField, Min(1),] int height = 100; // 行数（顶点行数为 height + 1）
		[SerializeField, Min(0.0001f),] float triangleSize = 0.1f; // 等边三角形边长（世界单位）
		[SerializeField] bool centerPivot = true; // 是否将网格居中于原点
		void Awake() =>
			// 生成由正三角形组成的平面网格
			// 前后是平直的边（z 最小/最大两行在同一直线）
			// 左右是锯齿边（由于行间 x 偏移导致）
			// 这个平面是光滑平面（多个三角形共用顶点）
			GenerateMesh();
		void OnValidate()
		{
			// 在编辑器中参数变更时实时更新
			if (!isActiveAndEnabled) return;
			if (Application.isPlaying) return;
			GenerateMesh();
		}
		[ContextMenu(nameof(GenerateMesh))]
		void GenerateMesh()
		{
			var vertexColumns = width + 1;
			var vertexRows = height + 1;
			var vertexCount = vertexColumns * vertexRows;
			var triangleCount = width * height * 2;

			// 准备 Mesh 容器
			if (mesh == null)
				mesh = new()
					{ name = "WaterSpace", };
			else
				mesh.Clear();

			// 尽量避免 16-bit 索引溢出
			mesh.indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
			var vertices = new Vector3[vertexCount];
			var uvs = new Vector2[vertexCount];
			var normals = new Vector3[vertexCount];
			var triangles = new int[triangleCount * 3];
			var dx = triangleSize; // 同一行相邻顶点的 x 距离
			var dz = triangleSize * Sqrt3Over2; // 相邻行的 z 距离，保证等边

			// 先生成顶点（带交替 0.5*dx 的行偏移），并记录 AABB 用于 UV/居中
			float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
			float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
			for (var r = 0; r < vertexRows; r++)
			{
				var rowOffsetX = (r & 1) == 1 ? 0.5f * dx : 0f; // 交替行偏移，形成左右锯齿边
				var z = r * dz;
				for (var c = 0; c < vertexColumns; c++)
				{
					var vi = r * vertexColumns + c;
					var x = c * dx + rowOffsetX;
					vertices[vi] = new(x, 0f, z);

					// 更新 AABB
					if (x < minX) minX = x;
					if (x > maxX) maxX = x;
					if (z < minZ) minZ = z;
					if (z > maxZ) maxZ = z;
				}
			}

			// UV 基于未居中前的 AABB 归一化到 [0,1]
			var invSizeX = 1f / Mathf.Max(1e-6f, maxX - minX);
			var invSizeZ = 1f / Mathf.Max(1e-6f, maxZ - minZ);
			for (var r = 0; r < vertexRows; r++)
			{
				for (var c = 0; c < vertexColumns; c++)
				{
					var vi = r * vertexColumns + c;
					var p = vertices[vi];
					uvs[vi] = new((p.x - minX) * invSizeX, (p.z - minZ) * invSizeZ);
					normals[vi] = Vector3.up;
				}
			}

			// 可选：将网格居中在原点
			if (centerPivot)
			{
				var center = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
				for (var i = 0; i < vertexCount; i++) vertices[i] -= center;
			}

			// 生成三角形：按行连接，交替行使用不同拓扑，保证等边形状与一致绕序
			var ti = 0; // triangles 写入索引
			for (var r = 0; r < height; r++)
			{
				var evenRow = (r & 1) == 0;
				for (var c = 0; c < width; c++)
				{
					var topLeft = r * vertexColumns + c;
					var topRight = topLeft + 1;
					var bottomLeft = topLeft + vertexColumns;
					var bottomRight = bottomLeft + 1;
					if (evenRow)
					{
						// 三角 1: TL, BL, TR ； 三角 2: TR, BL, BR （俯视为顺时针，法线朝上）
						triangles[ti++] = topLeft;
						triangles[ti++] = bottomLeft;
						triangles[ti++] = topRight;
						triangles[ti++] = topRight;
						triangles[ti++] = bottomLeft;
						triangles[ti++] = bottomRight;
					}
					else
					{
						// 交替行翻转连接方式以保持等边
						triangles[ti++] = topLeft;
						triangles[ti++] = bottomLeft;
						triangles[ti++] = bottomRight;
						triangles[ti++] = topLeft;
						triangles[ti++] = bottomRight;
						triangles[ti++] = topRight;
					}
				}
			}

			// 提交到 Mesh（使用属性，兼容更广泛的 Unity 版本）
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.normals = normals;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();

			// 绑定到 MeshFilter
			var meshFilter = GetComponent<MeshFilter>();
			if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
		}
	}
}
