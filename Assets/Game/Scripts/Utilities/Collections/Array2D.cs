using UnityEngine;
namespace Game.Utilities.Collections
{
	public interface IReadOnlyArray2D<T>
	{
		public RectIndexMapper IndexMapper { get; }
		T this[int index] { get; }
		T this[int x, int y] { get; }
		T this[Vector2Int position] { get; }
		bool TryGetValue(int index, out T value);
		bool TryGetValue(int x, int y, out T value);
		bool TryGetValue(Vector2Int position, out T value);
	}
}
