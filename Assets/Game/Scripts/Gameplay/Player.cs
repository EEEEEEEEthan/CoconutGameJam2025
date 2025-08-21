using System;
using System.Collections;
using Game.FingerRigging;
using Game.ResourceManagement;
using Game.Utilities;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	[Serializable]
	public struct InputBlock
	{
		public static readonly InputBlock all = new()
		{
			leftForward = true,
			leftUp = true,
			leftBackward = true,
			rightForward = true,
			rightUp = true,
			rightBackward = true,
			greetings = true,
			surprise = true,
			shy = true,
			angry = true,
		};
		public static InputBlock operator |(InputBlock a, InputBlock b) =>
			new()
			{
				leftForward = a.leftForward || b.leftForward,
				leftUp = a.leftUp || b.leftUp,
				leftBackward = a.leftBackward || b.leftBackward,
				rightForward = a.rightForward || b.rightForward,
				rightUp = a.rightUp || b.rightUp,
				rightBackward = a.rightBackward || b.rightBackward,
				greetings = a.greetings || b.greetings,
				surprise = a.surprise || b.surprise,
				shy = a.shy || b.shy,
				angry = a.angry || b.angry,
			};
		public bool leftForward;
		public bool leftUp;
		public bool leftBackward;
		public bool rightForward;
		public bool rightUp;
		public bool rightBackward;
		public bool greetings;
		public bool surprise;
		public bool shy;
		public bool angry;
	}
	public class Player : GameBehaviour
	{
		public static class AnimatorHashes
		{
			public static readonly int walkLeft = Animator.StringToHash("WalkLeft");
			public static readonly int walkRight = Animator.StringToHash("WalkRight");
			public static readonly int standLeft = Animator.StringToHash("StandLeft");
			public static readonly int standRight = Animator.StringToHash("StandRight");
			public static readonly int hi = Animator.StringToHash(nameof(EmotionCode.Hi));
			public static readonly int surprise = Animator.StringToHash(nameof(EmotionCode.Surprise));
			public static readonly int shy = Animator.StringToHash(nameof(EmotionCode.Shy));
			public static readonly int angry = Animator.StringToHash(nameof(EmotionCode.Angry));
		}
		public enum EmotionCode
		{
			Hi,
			Surprise,
			Shy,
			Angry,
		}
		[SerializeField] LayerMask airWallLayerMask = (int)LayerMaskCode.AirWall;
		[SerializeField] float raycastDistance = 1.0f;
		[SerializeField] InputBlock inputBlock;
		[SerializeField] InputBlock locked;
		[SerializeField, ObjectReference,] HandIKInput handIKInput;
		[SerializeField, ObjectReference("HandWithIK"),]
		Animator animator;
		[SerializeField, ObjectReference(nameof(CameraTarget)),]
		Transform cameraTarget;
		[SerializeField, ObjectReference("PlayerPosition"),]
		Collider playerPositionTrigger;
		[SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
		Coroutine _colorCoroutine; // 当前颜色渐变协程
		int? _cachedColorPropertyId; // 缓存找到的颜色属性
		Coroutine _dissolveCoroutine; // 当前溶解渐变协程
		int? _cachedDissolvePropertyId; // 缓存溶解属性 ID
		bool isInSpecialAnim;
		public Collider PlayerPositionTrigger => playerPositionTrigger;
		public Transform CameraTarget => cameraTarget;
		public HandIKInput HandIkInput => handIKInput;
		public InputBlock Block => inputBlock | Locked;
		public InputBlock InputBlock
		{
			get => inputBlock;
			set => inputBlock = value;
		}
		public InputBlock Locked
		{
			get => locked;
			set => locked = value;
		}
		void Awake()
		{
			skinnedMeshRenderer.sharedMaterial = new(skinnedMeshRenderer.sharedMaterial);
		}

		#region Smooth Material Color API
		/// <summary>
		/// 平滑设置角色材质颜色。默认尝试 _BaseColor (URP/HDRP) -> _Color (标准/旧材质)。
		/// </summary>
		/// <param name="target">目标颜色</param>
		/// <param name="duration">渐变时长(秒)。<=0 将立即设置。</param>
		/// <param name="onComplete">完成回调</param>
		/// <param name="propertyName">指定颜色属性(可选)。为空则自动匹配。</param>
		public void SmoothSetMaterialColor(Color target, float duration, Action onComplete = null, string propertyName = null)
		{
			if (skinnedMeshRenderer == null) return;
			Material mat = skinnedMeshRenderer.sharedMaterial;
			if (mat == null) return;

			// 确定颜色属性 ID
			int colorPropId = ResolveColorProperty(mat, propertyName);
			if (colorPropId < 0) return; // 找不到属性

			if (duration <= 0f)
			{
				mat.SetColor(colorPropId, target);
				onComplete?.Invoke();
				return;
			}

			if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
			_colorCoroutine = StartCoroutine(SmoothSetMaterialColorRoutine(mat, colorPropId, target, duration, onComplete));
		}

		int ResolveColorProperty(Material mat, string explicitName)
		{
			if (explicitName != null)
				return mat.HasProperty(explicitName) ? Shader.PropertyToID(explicitName) : -1;

			if (_cachedColorPropertyId.HasValue)
				return _cachedColorPropertyId.Value;

			string[] candidates = { "_BaseColor", "_Color", "_Tint", "_MainColor" };
			foreach (var c in candidates)
			{
				if (mat.HasProperty(c))
				{
					_cachedColorPropertyId = Shader.PropertyToID(c);
					return _cachedColorPropertyId.Value;
				}
			}
			return -1;
		}

		IEnumerator SmoothSetMaterialColorRoutine(Material mat, int colorPropId, Color target, float duration, Action onComplete)
		{
			Color start = mat.GetColor(colorPropId);
			float t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime / duration;
				mat.SetColor(colorPropId, Color.LerpUnclamped(start, target, Mathf.Clamp01(t)));
				yield return null;
			}
			mat.SetColor(colorPropId, target);
			onComplete?.Invoke();
			_colorCoroutine = null;
		}
		#endregion

		#region Smooth Dissolve API
		/// <summary>
		/// 平滑设置材质溶解（默认属性名 _Dissolve）。若材质无该属性则忽略。
		/// </summary>
		/// <param name="target">目标溶解值（一般 0-1 之间）</param>
		/// <param name="duration">时长(秒)，<=0 立即设置</param>
		/// <param name="onComplete">完成回调</param>
		/// <param name="propertyName">自定义属性名（可选，默认 _Dissolve）</param>
		public void SmoothSetDissolve(float target, float duration, Action onComplete = null, string propertyName = "_Dissolve")
		{
			if (skinnedMeshRenderer == null) return;
			var mat = skinnedMeshRenderer.sharedMaterial;
			if (mat == null) return;
			int propId = ResolveDissolveProperty(mat, propertyName);
			if (propId < 0) return; // 无属性
			target = Mathf.Clamp01(target);
			if (duration <= 0f)
			{
				mat.SetFloat(propId, target);
				onComplete?.Invoke();
				return;
			}
			if (_dissolveCoroutine != null) StopCoroutine(_dissolveCoroutine);
			_dissolveCoroutine = StartCoroutine(SmoothSetDissolveRoutine(mat, propId, target, duration, onComplete));
		}

		int ResolveDissolveProperty(Material mat, string explicitName)
		{
			if (!string.IsNullOrEmpty(explicitName))
				return mat.HasProperty(explicitName) ? Shader.PropertyToID(explicitName) : -1;
			if (_cachedDissolvePropertyId.HasValue) return _cachedDissolvePropertyId.Value;
			string[] candidates = { "_Dissolve", "_DissolveAmount", "_Fade" };
			foreach (var c in candidates)
			{
				if (mat.HasProperty(c))
				{
					_cachedDissolvePropertyId = Shader.PropertyToID(c);
					return _cachedDissolvePropertyId.Value;
				}
			}
			return -1;
		}

		IEnumerator SmoothSetDissolveRoutine(Material mat, int propId, float target, float duration, Action onComplete)
		{
			float start = mat.GetFloat(propId);
			float t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime / duration;
				mat.SetFloat(propId, Mathf.Lerp(start, target, Mathf.Clamp01(t)));
				yield return null;
			}
			mat.SetFloat(propId, target);
			onComplete?.Invoke();
			_dissolveCoroutine = null;
		}
		#endregion
		public EmotionCode CurrentEmotion { get; private set; }
		public event Action<EmotionCode> OnEmotionTriggered;
		void Update()
		{
			if (isInSpecialAnim) return;
			handIKInput.transform.position = handIKInput.transform.position.WithZ(0);

			// 检测左右空气墙
			var hasLeftWall = Physics.Raycast(transform.position, Vector3.left, raycastDistance, airWallLayerMask);
			var hasRightWall = Physics.Raycast(transform.position, Vector3.right, raycastDistance, airWallLayerMask);
			if (Input.GetKey(KeyCode.Q) && !Block.leftForward && !hasRightWall)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
				animator.SetBool(AnimatorHashes.walkLeft, true);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			else if (Input.GetKey(KeyCode.A) && !Block.leftUp)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, true);
			}
			else if (Input.GetKey(KeyCode.Z) && !Block.leftBackward && !hasLeftWall)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftBackward;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			else
			{
				handIKInput.LeftLeg = LegPoseCode.Idle;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			if (Input.GetKey(KeyCode.W) && !Block.rightForward && !hasRightWall)
			{
				handIKInput.RightLeg = LegPoseCode.LiftForward;
				animator.SetBool(AnimatorHashes.walkRight, true);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			else if (Input.GetKey(KeyCode.S) && !Block.rightUp)
			{
				handIKInput.RightLeg = LegPoseCode.LiftUp;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, true);
			}
			else if (Input.GetKey(KeyCode.X) && !Block.rightBackward && !hasLeftWall)
			{
				handIKInput.RightLeg = LegPoseCode.LiftBackward;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			else
			{
				handIKInput.RightLeg = LegPoseCode.Idle;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			/*
			if (Input.GetKeyDown(KeyCode.Space)) handIKInput.Crunch(true);
			if (Input.GetKeyUp(KeyCode.Space))
			{
				handIKInput.Crunch(false);
				handIKInput.Jump(1, () => Debug.Log("Landed!"));
			}
			*/
			if (isInSpecialAnim) return;
			if (Input.GetKeyDown(KeyCode.Alpha1) && !Block.greetings)
			{
				animator.SetTrigger(AnimatorHashes.hi);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Hi);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && !Block.surprise)
			{
				animator.SetTrigger(AnimatorHashes.surprise);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Surprise);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) && !Block.shy)
			{
				animator.SetTrigger(AnimatorHashes.shy);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Shy);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4) && !Block.angry)
			{
				animator.SetTrigger(AnimatorHashes.angry);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Angry);
				isInSpecialAnim = true;
			}
		}
		void OnDrawGizmos() => Gizmos.DrawRay(transform.position, Vector3.right * 100);
		public void Unlock(KeyCode key, bool withAnimation)
		{
			var locked = Locked;
			switch (key)
			{
				case KeyCode.A:
					locked.leftUp = false;
					break;
				case KeyCode.S:
					locked.rightUp = false;
					break;
				case KeyCode.Q:
					locked.leftForward = false;
					break;
				case KeyCode.W:
					locked.rightForward = false;
					break;
				case KeyCode.Z:
					locked.leftBackward = false;
					break;
				case KeyCode.X:
					locked.rightBackward = false;
					break;
				case KeyCode.Alpha1:
					locked.greetings = false;
					break;
				case KeyCode.Alpha2:
					locked.surprise = false;
					break;
				case KeyCode.Alpha3:
					locked.shy = false;
					break;
				case KeyCode.Alpha4:
					locked.angry = false;
					break;
			}
			Locked = locked;
			if(withAnimation)
				GameRoot.UiCamera.Hints[key].Show();
		}
		public void SetSpecialAnimEnd() => isInSpecialAnim = false;
	}
}
