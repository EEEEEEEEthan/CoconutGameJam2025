using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Transform Find(this Transform @this, string path, bool includeInactive)
		{
			var pathParts = path.Split('/');
			var current = @this;
			foreach (var part in pathParts)
			{
				if (part.IsNullOrEmpty()) continue;
				var count = current.childCount;
				for (var i = 0; i < count; ++i)
				{
					var child = current.GetChild(i);
					if (!includeInactive && !child.gameObject.activeInHierarchy) continue;
					if (child.name == part)
					{
						current = child;
						goto EARLY_BREAK;
					}
				}
				return null;
			EARLY_BREAK: ;
			}
			return current;
		}
		public static void DestroyAllChildren(this Transform @this, bool immediate)
		{
			if (immediate)
				for (var i = @this.childCount - 1; i >= 0; --i)
				{
					var child = @this.GetChild(i);
					if (child) child.gameObject.DestroyImmediate();
				}
			else
				for (var i = @this.childCount - 1; i >= 0; --i)
				{
					var child = @this.GetChild(i);
					if (child) child.gameObject.Destroy();
				}
		}
	}
}
