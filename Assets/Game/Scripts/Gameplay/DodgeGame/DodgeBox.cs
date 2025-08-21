using System.Collections;
using UnityEngine;
namespace Game.Gameplay.DodgeGame
{
	public class DodgeBox : MonoBehaviour
	{
		public static System.Action<DodgeBox> OnBoxHitPlayer;
		public static System.Action<DodgeBox> OnBoxDodged;
		[Header("飞行配置")] public float speed = 10f;
		public Vector3 targetPosition;
		[Header("碰撞检测")] public LayerMask playerLayer;
		bool hasPassedTarget;
		bool isDissolving;
		Vector3 flyDirection;
		Renderer boxRenderer;
		Rigidbody boxRigidbody;
		MeshRenderer meshRenderer;
		Material runtimeMaterial;
		Vector3 initialScale;
		const float dissolveDuration = 0.5f; // 动画时长 0.5 秒
		const string dissolveProperty = "_Dissolve"; // 溶解属性名
		static readonly int ColorPropId = Shader.PropertyToID("_BaseColor"); // HDRP / URP Lit 常用
		static readonly int LegacyColorPropId = Shader.PropertyToID("_Color"); // 备用
		void Awake()
		{
			meshRenderer = GetComponentInChildren<MeshRenderer>();
			if (meshRenderer != null)
			{
				// 生成运行时独立材质实例，避免修改共享资源
				runtimeMaterial = new Material(meshRenderer.sharedMaterial);
				meshRenderer.material = runtimeMaterial;
				// 初始化溶解为 0
				if (runtimeMaterial.HasProperty(dissolveProperty))
					runtimeMaterial.SetFloat(dissolveProperty, 0f);
			}
			initialScale = transform.localScale;
		}
		void Start()
		{
			flyDirection = (targetPosition - transform.position).normalized;
			if (boxRigidbody != null) boxRigidbody.velocity = flyDirection * speed;
		}
		void Update()
		{
			if (!hasPassedTarget && transform.position.z < 0)
			{
				hasPassedTarget = true;
				OnBoxDodged?.Invoke(this);
			}
			if (boxRigidbody == null) transform.position += flyDirection * speed * Time.deltaTime;
		}
		void OnTriggerEnter(Collider other)
		{
			if (!isDissolving && other.GetComponentInParent<Player>())
			{
				// 击中玩家：立即标记并播放消失动画
				OnBoxHitPlayer?.Invoke(this);
				StartDissolve(hitPlayer: true);
			}
		}
		public void Initialize(Vector3 startPos, Vector3 targetPos, float flySpeed)
		{
			transform.position = startPos;
			targetPosition = targetPos;
			speed = flySpeed;
			hasPassedTarget = false;
			isDissolving = false;
			if (runtimeMaterial != null && runtimeMaterial.HasProperty(dissolveProperty))
				runtimeMaterial.SetFloat(dissolveProperty, 0f);
			transform.localScale = initialScale;
		}

		/// <summary>
		/// 开始溶解动画（0.5s 内：缩放到 1.1，_Dissolve 从 0 -> 1；若击中玩家立即变红）
		/// </summary>
		/// <param name="hitPlayer">是否是击中玩家触发</param>
		public void StartDissolve(bool hitPlayer)
		{
			if (isDissolving) return;
			isDissolving = true;
			// 停止物理运动 & 禁用碰撞防止多次触发
			if (boxRigidbody != null)
			{
				boxRigidbody.velocity = Vector3.zero;
				boxRigidbody.isKinematic = true;
			}
			var colliders = GetComponentsInChildren<Collider>();
			foreach (var c in colliders) c.enabled = false;
			if (hitPlayer && runtimeMaterial != null)
			{
				// 立即变红（尝试常用颜色属性）
				if (runtimeMaterial.HasProperty(ColorPropId)) runtimeMaterial.SetColor(ColorPropId, Color.red);
				if (runtimeMaterial.HasProperty(LegacyColorPropId)) runtimeMaterial.SetColor(LegacyColorPropId, Color.red);
			}
			StartCoroutine(DissolveRoutine());
		}

		IEnumerator DissolveRoutine()
		{
			float t = 0f;
			Vector3 startScale = transform.localScale;
			Vector3 targetScale = initialScale * 1.3f;
			bool hasDissolveProp = runtimeMaterial != null && runtimeMaterial.HasProperty(dissolveProperty);
			while (t < dissolveDuration)
			{
				t += Time.deltaTime;
				float normalized = Mathf.Clamp01(t / dissolveDuration);
				// 缩放
				transform.localScale = Vector3.Lerp(startScale, targetScale, normalized);
				// 溶解
				if (hasDissolveProp)
					runtimeMaterial.SetFloat(dissolveProperty, normalized);
				yield return null;
			}
			Destroy(gameObject);
		}
	}
}
