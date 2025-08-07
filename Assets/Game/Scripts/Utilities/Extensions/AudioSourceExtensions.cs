using Game.Utilities.Transformers;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static void Reset(this AudioSource @this) => @this.HermiteSetVolume(1, 0, null);
	}
}
