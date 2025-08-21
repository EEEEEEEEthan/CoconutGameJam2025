using System.Collections;
using TMPro;
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
		[Header("出现动画")] [SerializeField] float spawnScaleDuration = 0.5f; // 出生缩放时长
		[SerializeField] float spawnScaleStartFactor = 0.2f; // 出生初始缩放比例（相对初始尺寸）
		[SerializeField] AnimationCurve spawnScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 缩放插值曲线（0-1 输入 -> 0-1 输出）
		[SerializeField] TMP_Text text;
		static readonly int ColorPropId = Shader.PropertyToID("_BaseColor"); // HDRP / URP Lit 常用
		static readonly int LegacyColorPropId = Shader.PropertyToID("_Color"); // 备用
		Coroutine spawnScaleCoroutine;
		bool isSpawning;
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
				// 击中玩家：通知事件；颜色改变放到外部（GameManager）
				OnBoxHitPlayer?.Invoke(this);
				StartDissolve();
			}
		}
		public void Initialize(Vector3 startPos, Vector3 targetPos, float flySpeed)
		{
			transform.position = startPos;
			targetPosition = targetPos;
			speed = flySpeed;
			hasPassedTarget = false;
			isDissolving = false;
			isSpawning = true;
			if (spawnScaleCoroutine != null) StopCoroutine(spawnScaleCoroutine);
			if (runtimeMaterial != null && runtimeMaterial.HasProperty(dissolveProperty))
				runtimeMaterial.SetFloat(dissolveProperty, 0f);
			// 设置出生初始缩放并播放动画
			transform.localScale = initialScale * spawnScaleStartFactor;
			spawnScaleCoroutine = StartCoroutine(SpawnScaleRoutine());
		}

		/// <summary>
		/// 开始溶解动画（0.5s 内：缩放到 1.1，_Dissolve 从 0 -> 1）颜色不在此处强制修改
		/// </summary>
		public void StartDissolve()
		{
			if (isDissolving) return;
			isDissolving = true;
			// 如果仍在出生动画，停止它
			if (isSpawning && spawnScaleCoroutine != null)
			{
				StopCoroutine(spawnScaleCoroutine);
				isSpawning = false;
			}
			// 停止物理运动 & 禁用碰撞防止多次触发
			if (boxRigidbody != null)
			{
				boxRigidbody.velocity = Vector3.zero;
				boxRigidbody.isKinematic = true;
			}
			var colliders = GetComponentsInChildren<Collider>();
			foreach (var c in colliders) c.enabled = false;
			StartCoroutine(DissolveRoutine());
		}

		/// <summary>
		/// 外部设置颜色（命中/躲避等不同语义）
		/// </summary>
		public void SetColor(Color color)
		{
			if (runtimeMaterial == null) return;
			if (runtimeMaterial.HasProperty(ColorPropId)) runtimeMaterial.SetColor(ColorPropId, color);
			if (runtimeMaterial.HasProperty(LegacyColorPropId)) runtimeMaterial.SetColor(LegacyColorPropId, color);
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

		IEnumerator SpawnScaleRoutine()
		{
			float t = 0f;
			Vector3 startScale = initialScale * spawnScaleStartFactor;
			Vector3 targetScale = initialScale;
			while (t < spawnScaleDuration)
			{
				// 若在生成阶段被触发溶解，则退出
				if (isDissolving) yield break;
				t += Time.deltaTime;
				float linear = Mathf.Clamp01(t / spawnScaleDuration);
				float curveValue = spawnScaleCurve != null ? Mathf.Clamp01(spawnScaleCurve.Evaluate(linear)) : linear;
				transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);
				yield return null;
			}
			transform.localScale = targetScale;
			isSpawning = false;
		}
	}
}
