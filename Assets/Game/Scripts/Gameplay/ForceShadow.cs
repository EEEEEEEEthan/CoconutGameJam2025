using UnityEngine;
namespace Game.Gameplay
{
	public class ForceShadow : MonoBehaviour
	{
		void Awake()
		{
			foreach (var renderer in GetComponentsInChildren<SpriteRenderer>()) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		}
	}
}
