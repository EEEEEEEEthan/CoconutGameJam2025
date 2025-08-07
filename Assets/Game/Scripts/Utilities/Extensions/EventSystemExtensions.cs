using Game.Utilities.Pools;
using UnityEngine.EventSystems;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		/// <summary>
		///     Get the first raycast result from the EventSystem.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="eventData"></param>
		/// <param name="raycastResult"></param>
		/// <returns></returns>
		public static bool TryRaycast(this EventSystem @this, PointerEventData eventData, out RaycastResult raycastResult)
		{
			using (ListPoolThreaded<RaycastResult>.Rent(out var results))
			{
				@this.RaycastAll(eventData, results);
				if (results.Count > 0)
				{
					raycastResult = results[0];
					return true;
				}
				raycastResult = default;
				return false;
			}
		}
	}
}
