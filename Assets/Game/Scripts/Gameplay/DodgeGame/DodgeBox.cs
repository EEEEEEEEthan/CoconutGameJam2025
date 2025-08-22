using System.Collections;
using TMPro;
using UnityEngine;
namespace Game.Gameplay.DodgeGame
{
	public class DodgeBox : GameBehaviour
	{
		const float dissolveDuration = 0.5f; // 动画时长 0.5 秒
		const string dissolveProperty = "_Dissolve"; // 溶解属性名
		public static System.Action<DodgeBox> OnBoxHitPlayer;
		public static System.Action<DodgeBox> OnBoxDodged;
		static readonly int ColorPropId = Shader.PropertyToID("_BaseColor"); // HDRP / URP Lit 常用
		static readonly int LegacyColorPropId = Shader.PropertyToID("_Color"); // 备用
		// 仅愤怒 / 怒气 / 咬牙 / 不爽表情（半角 ASCII，无旋转侧脸）。
		static readonly string[] Emoticons =
		{
			">_<",
			">_<!",
			">_<!!",
			">_<!!!",
			">_<#",
			">_<##",
			">_<###",
			">_<!!!!",
			">0<",
			">0<!",
			">0<!!",
			">0<!!!",
			">0<#",
			">0<##",
			">0<!!!!",
			">.<",
			">.<!!",
			">.<#",
			">o<",
			">o<!!",
			">o<#",
			">O<",
			">O<!!",
			">O<#",
			">^<",
			">^<!!",
			"<_<",
			"<_<!!",
			"<__<",
			"<__<!!",
			">__>",
			">__>!!",
			">_>",
			">_>!",
			"-_-",
			"-_-!",
			"-_-!!",
			"-_-#",
			"-_-##",
			"=_=",
			"=_=#",
			"=_=!!",
			"=__=",
			"=__=!!",
			"-__-",
			"-__-#",
			"-.-",
			"-.-#",
			"o_o#",
			"O_O#",
			"O_O!!",
			"O_O!!!",
			"x_x#",
			"x_x!!",
			"X_X#",
			">_<%",
			">_<%%",
			">0<%",
			">0<%%",
			"<_<%",
			"<_<##",
			">_>#",
			">_>##",
			">^<##",
			">.<##",
			"GRR",
			"GRR!",
			"GRRR",
			"GRRR!",
			"RRR",
		};
		[Header("飞行配置")] public float speed = 10f;
		public Vector3 targetPosition;
		[Header("碰撞检测")] public LayerMask playerLayer;
		[Header("出现动画")] [SerializeField] float spawnScaleDuration = 0.5f; // 出生缩放时长
		[SerializeField] float spawnScaleStartFactor = 0.2f; // 出生初始缩放比例（相对初始尺寸）
		[SerializeField] AnimationCurve spawnScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 缩放插值曲线（0-1 输入 -> 0-1 输出）
		[SerializeField] TMP_Text text;
		// 子节点抖动配置（需求：第一个子节点快速抖动，使用 PerlinNoise 并 Remap 0..1 -> -range..range）
		[SerializeField] bool enableFirstChildShake = true; // 是否启用
		[SerializeField] float firstChildShakeRange = 0.05f; // 抖动幅度（世界单位 / 本地坐标单位）
		[SerializeField] float firstChildShakeSpeed = 20f; // 噪声采样速度（越大越快）
		[SerializeField] Vector3 firstChildShakeAxes = new(1f, 1f, 0f); // 哪些轴启用抖动（1 启用 0 关闭）
		bool hasPassedTarget;
		bool isDissolving;
		Vector3 flyDirection;
		Renderer boxRenderer;
		Rigidbody boxRigidbody;
		MeshRenderer meshRenderer;
		Material runtimeMaterial;
		Vector3 initialScale;
		Transform firstChild; // 缓存第一个子节点
		Vector3 firstChildInitialLocalPos; // 初始本地位置
		float firstChildShakeTime; // 累积时间（实例独立）
		Vector3 firstChildShakeSeed; // 噪声随机种子偏移（实例独立）
		Coroutine spawnScaleCoroutine;
		bool isSpawning;
		void Awake()
		{
			meshRenderer = GetComponentInChildren<MeshRenderer>();
			// 缓存第一个子节点（如果存在）
			if (transform.childCount > 0)
			{
				firstChild = transform.GetChild(0);
				firstChildInitialLocalPos = firstChild.localPosition;
			}
			// 为该实例生成随机种子，避免所有 Box 同步抖动
			firstChildShakeSeed = new(Random.value * 10f, Random.value * 10f, Random.value * 10f);
			if (meshRenderer != null)
			{
				// 生成运行时独立材质实例，避免修改共享资源
				runtimeMaterial = new(meshRenderer.sharedMaterial);
				meshRenderer.material = runtimeMaterial;
				// 初始化溶解为 0
				if (runtimeMaterial.HasProperty(dissolveProperty)) runtimeMaterial.SetFloat(dissolveProperty, 0f);
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
			UpdateFirstChildShake();
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
		public void Initialize(Vector3 startPos, float flySpeed)
		{
			transform.position = startPos;
			// 改为朝向玩家当前位置（向上偏移 0.1f），不再使用传入的随机目标点。
			// 若未能找到玩家，则回退到传入的 targetPos（保持兼容）。
			// Unity 2023+: 使用 FindFirstObjectByType；若不可用可回退到 (已弃用) 的旧 API（编译期符号区分）。
			var player = GameRoot.Player;
			targetPosition = player.transform.position + Vector3.up * 0.05f;
			speed = flySpeed;
			hasPassedTarget = false;
			isDissolving = false;
			isSpawning = true;
			if (spawnScaleCoroutine != null) StopCoroutine(spawnScaleCoroutine);
			if (runtimeMaterial != null && runtimeMaterial.HasProperty(dissolveProperty)) runtimeMaterial.SetFloat(dissolveProperty, 0f);
			// 设置出生初始缩放并播放动画
			transform.localScale = initialScale * spawnScaleStartFactor;
			spawnScaleCoroutine = StartCoroutine(SpawnScaleRoutine());
			if (text != null) text.text = GetRandomEmoticon();
		}
		/// <summary>
		///     开始溶解动画（0.5s 内：缩放到 1.1，_Dissolve 从 0 -> 1）颜色不在此处强制修改
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
		///     外部设置颜色（命中/躲避等不同语义）
		/// </summary>
		public void SetColor(Color color)
		{
			if (runtimeMaterial == null) return;
			if (runtimeMaterial.HasProperty(ColorPropId)) runtimeMaterial.SetColor(ColorPropId, color);
			if (runtimeMaterial.HasProperty(LegacyColorPropId)) runtimeMaterial.SetColor(LegacyColorPropId, color);
		}
		string GetRandomEmoticon()
		{
			if (Emoticons == null || Emoticons.Length == 0) return ":)";
			var idx = Random.Range(0, Emoticons.Length);
			return Emoticons[idx];
		}

		// 使用 PerlinNoise 对第一个子节点做局部抖动
		void UpdateFirstChildShake()
		{
			if (!enableFirstChildShake || firstChild == null) return;
			if (firstChildShakeRange <= 0f) return;
			firstChildShakeTime += Time.deltaTime * firstChildShakeSpeed; // 每个实例独立推进
			var t = firstChildShakeTime;
			// Perlin 返回 0..1, Remap 到 -range..range
			float Sample(float ox, float oy) => (Mathf.PerlinNoise(ox, oy) * 2f - 1f) * firstChildShakeRange; // 0..1 -> -1..1 -> * range
			// 使用不同偏移使 XYZ 不完全相关
			var dx = firstChildShakeAxes.x != 0f ? Sample(firstChildShakeSeed.x + t, firstChildShakeSeed.x) : 0f;
			var dy = firstChildShakeAxes.y != 0f ? Sample(firstChildShakeSeed.y, firstChildShakeSeed.y + t) : 0f;
			var dz = firstChildShakeAxes.z != 0f ? Sample(firstChildShakeSeed.z + t, firstChildShakeSeed.z + t * 0.37f) : 0f;
			firstChild.localPosition = firstChildInitialLocalPos + new Vector3(dx, dy, dz);
		}
		IEnumerator DissolveRoutine()
		{
			var t = 0f;
			var startScale = transform.localScale;
			var targetScale = initialScale * 1.3f;
			var hasDissolveProp = runtimeMaterial != null && runtimeMaterial.HasProperty(dissolveProperty);
			while (t < dissolveDuration)
			{
				t += Time.deltaTime;
				var normalized = Mathf.Clamp01(t / dissolveDuration);
				// 缩放
				transform.localScale = Vector3.Lerp(startScale, targetScale, normalized);
				// 溶解
				if (hasDissolveProp) runtimeMaterial.SetFloat(dissolveProperty, normalized);
				yield return null;
			}
			Destroy(gameObject);
		}
		IEnumerator SpawnScaleRoutine()
		{
			var t = 0f;
			var startScale = initialScale * spawnScaleStartFactor;
			var targetScale = initialScale;
			while (t < spawnScaleDuration)
			{
				// 若在生成阶段被触发溶解，则退出
				if (isDissolving) yield break;
				t += Time.deltaTime;
				var linear = Mathf.Clamp01(t / spawnScaleDuration);
				var curveValue = spawnScaleCurve != null ? Mathf.Clamp01(spawnScaleCurve.Evaluate(linear)) : linear;
				transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);
				yield return null;
			}
			transform.localScale = targetScale;
			isSpawning = false;
		}
	}
}
