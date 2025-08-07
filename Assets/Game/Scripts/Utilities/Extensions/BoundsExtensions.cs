using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		struct BoundsCornerEnumerator : IEnumerable<Vector3>, IEnumerator<Vector3>
		{
			int step;
			Bounds bounds;
			public Vector3 Current =>
				step switch
				{
					0 => bounds.min,
					1 => new(bounds.min.x, bounds.min.y, bounds.max.z),
					2 => new(bounds.min.x, bounds.max.y, bounds.min.z),
					3 => new(bounds.min.x, bounds.max.y, bounds.max.z),
					4 => new(bounds.max.x, bounds.min.y, bounds.min.z),
					5 => new(bounds.max.x, bounds.min.y, bounds.max.z),
					6 => new(bounds.max.x, bounds.max.y, bounds.min.z),
					7 => bounds.max,
					_ => throw new InvalidOperationException(),
				};
			object IEnumerator.Current => Current;
			public BoundsCornerEnumerator(Bounds bounds)
			{
				this.bounds = bounds;
				step = -1;
			}
			public IEnumerator<Vector3> GetEnumerator() => this;
			public bool MoveNext() => ++step < 8;
			public void Reset() => step = -1;
			public void Dispose() { }
			IEnumerator IEnumerable.GetEnumerator() => this;
		}
		public static IEnumerable<Vector3> Vertices(this Bounds @this) => new BoundsCornerEnumerator(@this);
	}
}
