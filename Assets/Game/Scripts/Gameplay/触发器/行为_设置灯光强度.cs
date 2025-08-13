using UnityEngine;
namespace Game.Gameplay.触发器
{
	public class 行为_设置灯光强度 : GameBehaviour
	{
		[SerializeField] float intensity = 1f;
		[SerializeField] float duration = 0.2f;
		void OnEnable() => GameRoot.Sunlight.SetIntensity(intensity, duration);
		void OnDisable() => GameRoot.Sunlight.ResetIntensity(duration);
	}
}
