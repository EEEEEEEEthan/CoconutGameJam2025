using UnityEngine;
using UnityEngine.UI;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(LayoutElementMinHeightTransformer))]
	sealed class LayoutElementMinHeightTransformer : SmoothSingleTransformer
	{
		LayoutElement cachedLayoutElement;
		protected override float Value
		{
			get => LayoutElement.minHeight;
			set => LayoutElement.minHeight = value;
		}
		LayoutElement LayoutElement => cachedLayoutElement ? cachedLayoutElement : cachedLayoutElement = GetComponent<LayoutElement>();
	}
	public static partial class Extensions
	{
		public static void SmoothSetLayoutElementMinHeight(this LayoutElement @this, float height, float smoothTime)
		{
			var transformer = @this.gameObject.GetOrAddComponent<LayoutElementMinHeightTransformer>();
			transformer.SetValue(height, smoothTime);
		}
	}
}
