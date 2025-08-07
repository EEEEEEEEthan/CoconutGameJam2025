using Game.Utilities.Pools;
using UnityEngine;
using UnityEngine.UI;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static void ForceRebuildLayoutImmediate(this RectTransform @this)
		{
			if (!@this) return;
			if (!@this.gameObject.activeInHierarchy)
			{
				Debug.LogError($"{nameof(ForceRebuildLayoutImmediate)} Error: {@this} is not active in hierarchy");
				return;
			}
			using (ListPoolThreaded<RectTransform>.Rent(out var list))
			{
				list.Add(@this);
				for (var i = 0; i < list.Count; i++)
				{
					var current = list[i];
					var childCount = current.transform.childCount;
					for (var j = 0; j < childCount; j++)
					{
						var child = current.transform.GetChild(j);
						if (child.gameObject.activeSelf && child is RectTransform rect) list.Add(rect);
					}
				}
				for (var i = list.Count; i-- > 0;)
				{
					var transform = list[i].transform as RectTransform;
					if (transform) LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
				}
			}
		}
		public static void FillParent(this RectTransform @this, RectOffset paddings = null)
		{
			var parent = @this.parent as RectTransform;
			if (!parent) return;
			@this.anchorMin = new(0, 0);
			@this.anchorMax = new(1, 1);
			if (paddings is null)
			{
				@this.offsetMin = Vector2.zero;
				@this.offsetMax = Vector2.zero;
			}
			else
			{
				@this.offsetMin = new(paddings.left, paddings.bottom);
				@this.offsetMax = new(-paddings.right, -paddings.top);
			}
		}
		public static void FillParent(this RectTransform @this, int left, int right, int top, int bottom)
		{
			var parent = @this.parent as RectTransform;
			if (!parent) return;
			@this.anchorMin = new(0, 0);
			@this.anchorMax = new(1, 1);
			@this.offsetMin = new(left, bottom);
			@this.offsetMax = new(-right, -top);
		}
		public static void FillParent(this RectTransform @this, (int left, int right, int top, int bottom) paddings) =>
			@this.FillParent(paddings.left, paddings.right, paddings.top, paddings.bottom);
		public static void SetAnchor(this RectTransform @this, Vector2 anchor)
		{
			@this.anchorMin = anchor;
			@this.anchorMax = anchor;
		}
		public static void SetAnchors(this RectTransform @this, Vector2 anchorMin, Vector2 anchorMax)
		{
			@this.anchorMin = anchorMin;
			@this.anchorMax = anchorMax;
		}
		public static void SetColorRecursive(this RectTransform @this, Color color)
		{
			if (!@this) return;
			using (@this.GetComponentsInChildren<Graphic>(out var graphics, includeInactive: true))
			{
				foreach (var graphic in graphics) graphic.color = color;
			}
		}
		public static void CrossFadeColorRecursive(this RectTransform @this, Color color, float duration, bool ignoreTimeScale = false, bool useAlpha = true)
		{
			if (!@this) return;
			using (@this.GetComponentsInChildren<Graphic>(out var graphics, includeInactive: true))
			{
				foreach (var graphic in graphics) graphic.CrossFadeColor(color, duration, ignoreTimeScale, useAlpha);
			}
		}
		public static void CrossFadeAlphaRecursive(this RectTransform @this, float alpha, float duration, bool ignoreTimeScale = false)
		{
			if (!@this) return;
			using (@this.GetComponentsInChildren<Graphic>(out var graphics, includeInactive: true))
			{
				foreach (var graphic in graphics) graphic.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
			}
		}
	}
}
