using Game.ResourceManagement;
using UnityEngine;
namespace Game.FingerRigging
{
	static class Extensions
	{
		public static Vector3? GetTerrainHit(this Vector3 position, float up = 0)
		{
			if (Physics.Raycast(position + Vector3.up * up, Vector3.down, out var hit, float.MaxValue, (int)LayerMaskCode.Stand)) return hit.point;
			return null;
		}
	}
}
