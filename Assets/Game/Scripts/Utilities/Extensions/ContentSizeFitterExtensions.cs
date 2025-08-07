using UnityEngine.UI;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static void SetFit(this ContentSizeFitter fitter, ContentSizeFitter.FitMode horizontal, ContentSizeFitter.FitMode vertical)
		{
			fitter.horizontalFit = horizontal;
			fitter.verticalFit = vertical;
		}
		public static void SetFit(this ContentSizeFitter fitter, ContentSizeFitter.FitMode horizontalAndVertical) =>
			fitter.verticalFit = fitter.horizontalFit = horizontalAndVertical;
	}
}
