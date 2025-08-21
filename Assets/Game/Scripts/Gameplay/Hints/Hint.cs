using System.Collections;
using Game.Gameplay.WaterGame;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.Hints
{
	public class Hint : GameBehaviour
	{
		[SerializeField] Transform hook;
		[SerializeField] Rigidbody stuff;
		[SerializeField] float velocityMultiplier = 1f;
		[SerializeField] float accelerationMultiplier = 1f;
		[SerializeField] AnimationCurve showCurve;
		// 颜色曲线: 0 => 全白, 1 => 全红 (可在Inspector里设成闪烁)
		[SerializeField] AnimationCurve colorCurve;
		[SerializeField] SpriteRenderer sprite;
		// 备份原始材质, 避免直接修改共享材质
		Material originalSharedMaterial;
		bool materialCloned;
		KeyCode key;
		VelocityCalculator cameraControllerVelocityCalculator;
		bool visible;
		Coroutine colorBlinkCoroutine;
		void Awake()
		{
			originalSharedMaterial = sprite != null ? sprite.sharedMaterial : null;
			hook.transform.localPosition = new(0, 5, 0);
		}
		/*
		void Update()
		{
			cameraControllerVelocityCalculator = GameRoot.CameraController.VelocityCalculator;
			var force = cameraControllerVelocityCalculator.Velocity * velocityMultiplier;
			force += cameraControllerVelocityCalculator.Acceleration * accelerationMultiplier;
			if (force.x != 0 || force.y != 0 || force.z != 0)
				if (force.x != float.NaN && force.y != float.NaN && force.z != float.NaN)
					if (!float.IsInfinity(force.x) && !float.IsInfinity(force.y) && !float.IsInfinity(force.z))
						stuff.AddForce(force);
		}
		*/
		public void Initialize(KeyCode key) => this.key = key;
		public void Show()
		{
			if (visible) return;
			visible = true;
			StartCoroutine(show());
			// 开始颜色闪烁 3 秒
			if (colorBlinkCoroutine != null) StopCoroutine(colorBlinkCoroutine);
			colorBlinkCoroutine = StartCoroutine(BlinkColorForSeconds(3f));
			IEnumerator show()
			{
				var startTime = Time.time;
				var endTime = Time.time + 0.5f;
				while (Time.time < endTime)
				{
					var t = (Time.time - startTime) / (endTime - startTime);
					var y = showCurve.Evaluate(t);
					y = y.Remapped(0, 1, 5, 0);
					hook.transform.localPosition = new(0, y, 0);
					yield return null;
				}
				hook.transform.localPosition = new(0, 0, 0);
			}
		}
		public void Hide()
		{
			// 停止闪烁并复原颜色
			if (colorBlinkCoroutine != null)
			{
				StopCoroutine(colorBlinkCoroutine);
				colorBlinkCoroutine = null;
			}
			SetMaterialColor(Color.white);
			if (isActiveAndEnabled) StartCoroutine(hide());
			IEnumerator hide()
			{
				var velocity = Vector3.zero;
				while (true)
				{
					hook.transform.localPosition = Vector3.SmoothDamp(hook.transform.localPosition, new(0, 10, 0), ref velocity, 1f);
					if (hook.localPosition.y > 5) break;
					yield return null;
				}
				gameObject.SetActive(false);
			}
		}
		IEnumerator BlinkColorForSeconds(float seconds)
		{
			var start = Time.time;
			var end = start + seconds;
			EnsureMaterialInstance();
			while (Time.time < end)
			{
				var tNorm = (Time.time - start) / seconds; // 0..1
				var curveValue = colorCurve != null ? colorCurve.Evaluate(tNorm) : 0f; // 0=>白,1=>红
				SetMaterialColor(Color.Lerp(Color.white, Color.red, Mathf.Clamp01(curveValue)));
				yield return null;
			}
			SetMaterialColor(Color.white); // 结束后恢复
			colorBlinkCoroutine = null;
		}
		void EnsureMaterialInstance()
		{
			if (materialCloned || sprite == null) return;
			// 克隆 sharedMaterial 避免影响别的对象
			sprite.sharedMaterial = new(sprite.sharedMaterial);
			materialCloned = true;
		}
		void SetMaterialColor(Color c)
		{
			if (sprite == null) return;
			EnsureMaterialInstance();
			// 修改实例材质的颜色 (如果 shader 使用 _Color)
			if (sprite.sharedMaterial.HasProperty("_Color")) sprite.sharedMaterial.color = c;
		}
	}
}
