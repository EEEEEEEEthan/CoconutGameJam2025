using Game.Utilities;
using UnityEngine;
namespace Game.FingerRigging
{
	[ExecuteAlways]
	class FingerMuscle : MonoBehaviour
	{
#if UNITY_EDITOR
		[UnityEditor.CustomEditor(typeof(FingerMuscle))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				var target = (FingerMuscle)this.target;
				UnityEditor.EditorGUILayout.LabelField("true progress",
					target
						.GetProgress(target.proximalInterphalangealJoint.localEulerAngles.x, target.distalInterphalangealJoint.localEulerAngles.x)
						.ToString("F3"));
				UnityEditor.EditorGUILayout.LabelField("root angle", target.RootAngle.ToString("F3"));
				if (GUILayout.Button("Record Positions"))
				{
					{
						var property = serializedObject.FindProperty(nameof(metacarpophalangeal2ProximalInterphalangeal));
						property.floatValue = Vector3.Distance(
							target.metacarpophalangealJoint.position,
							target.proximalInterphalangealJoint.position);
					}
					{
						var property = serializedObject.FindProperty(nameof(proximalInterphalangeal2DistalInterphalangeal));
						property.floatValue = Vector3.Distance(
							target.proximalInterphalangealJoint.position,
							target.distalInterphalangealJoint.position);
					}
					{
						var property = serializedObject.FindProperty(nameof(distalInterphalangeal2Tip));
						property.floatValue = Vector3.Distance(
							target.distalInterphalangealJoint.position,
							target.tip.position);
					}
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
				}
			}
		}
#endif
		[SerializeField] Finger finger;
		[SerializeField] Transform metacarpophalangealJoint;
		[SerializeField] Transform proximalInterphalangealJoint;
		[SerializeField] Transform distalInterphalangealJoint;
		[SerializeField] Transform tip;
		[SerializeField, Range(0, 1),] float progress;
		[SerializeField, HideInInspector,] float metacarpophalangeal2ProximalInterphalangeal;
		[SerializeField, HideInInspector,] float proximalInterphalangeal2DistalInterphalangeal;
		[SerializeField, HideInInspector,] float distalInterphalangeal2Tip;
		[SerializeField] Transform handRoot;
		Transform[] transforms;
		internal float MaxLength => metacarpophalangeal2ProximalInterphalangeal + proximalInterphalangeal2DistalInterphalangeal + distalInterphalangeal2Tip;
		internal Vector3 Direction => tip.position - metacarpophalangealJoint.position;
		internal float RootAngle => Vector3.Angle(metacarpophalangealJoint.up, tip.position - metacarpophalangealJoint.position);
		internal float Progress
		{
			get => progress;
			set
			{
				progress = value;
				UpdateAngles();
			}
		}
		void OnDrawGizmos()
		{
			var a = metacarpophalangealJoint;
			var b = proximalInterphalangealJoint;
			var c = distalInterphalangealJoint;
			var d = tip;
			if (a && b) Gizmos.DrawLine(a.position, b.position);
			if (b && c) Gizmos.DrawLine(b.position, c.position);
			if (c && d) Gizmos.DrawLine(c.position, d.position);
			if (a) Gizmos.DrawSphere(a.position, 0.001f);
			if (b) Gizmos.DrawSphere(b.position, 0.001f);
			if (c) Gizmos.DrawSphere(c.position, 0.001f);
			if (d) Gizmos.DrawSphere(d.position, 0.001f);
			if (a && d)
			{
				var ray = new Ray(a.position, d.position - a.position);
				var targetDistance = MaxLength * progress;
				var preferredPosition = ray.GetPoint(targetDistance);
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(preferredPosition, 0.001f);
				Gizmos.DrawLine(a.position, preferredPosition);
			}
#if UNITY_EDITOR
			if (a && b) UnityEditor.Handles.Label((a.position + b.position) * 0.5f, metacarpophalangeal2ProximalInterphalangeal.ToString("F2"));
			if (b && c) UnityEditor.Handles.Label((b.position + c.position) * 0.5f, proximalInterphalangeal2DistalInterphalangeal.ToString("F3"));
			if (c && d) UnityEditor.Handles.Label((c.position + d.position) * 0.5f, distalInterphalangeal2Tip.ToString("F3"));
#endif
		}
		void OnValidate()
		{
			if (Application.isPlaying) return;
			UpdateAngles();
		}
		internal void UpdateDirection() => transform.LookAt(finger.transform.position, handRoot.transform.right);
		void UpdateAngles()
		{
			transforms ??= metacarpophalangealJoint.GetComponentsInChildren<Transform>();
			foreach (var transform in transforms) transform.localEulerAngles = default;
			GetAngles(progress, out var proximalInterphalangealDegrees, out var distalInterphalangealDegrees);
			proximalInterphalangealJoint.localEulerAngles = new(proximalInterphalangealDegrees, 0, 0);
			distalInterphalangealJoint.localEulerAngles = new(distalInterphalangealDegrees, 0, 0);
			var rootAngle = Vector3.Angle(metacarpophalangealJoint.up, tip.position - metacarpophalangealJoint.position);
			metacarpophalangealJoint.localEulerAngles = new(rootAngle, 0, 0);
		}
		float GetProgress(float proximalInterphalangealDegrees, float distalInterphalangealDegrees)
		{
			var v0 = new Vector2(metacarpophalangeal2ProximalInterphalangeal, 0);
			var v1 = new Vector2(proximalInterphalangeal2DistalInterphalangeal, 0).RotateClockwise(-proximalInterphalangealDegrees);
			var v2 = new Vector2(distalInterphalangeal2Tip, 0).RotateClockwise(-proximalInterphalangealDegrees - distalInterphalangealDegrees);
			var p1 = v0;
			var p2 = p1 + v1;
			var p3 = p2 + v2;
			return p3.magnitude / MaxLength;
		}
		void GetAngles(float preferredProgress, out float proximalInterphalangealDegrees, out float distalInterphalangealDegrees)
		{
			const int maxIterations = 256;
			const float tolerance = 0.0001f;
			proximalInterphalangealDegrees = 0;
			distalInterphalangealDegrees = 0;
			var minProximalInterphalangealDegrees = 0f;
			var minDistalInterphalangealDegrees = 0f;
			var maxProximalInterphalangealDegrees = -100f;
			var maxDistalInterphalangealDegrees = -50f;
			if (preferredProgress >= 1)
			{
				proximalInterphalangealDegrees = minProximalInterphalangealDegrees;
				distalInterphalangealDegrees = minDistalInterphalangealDegrees;
				return;
			}
			var minProgress = GetProgress(maxProximalInterphalangealDegrees, maxDistalInterphalangealDegrees);
			if (preferredProgress <= minProgress)
			{
				proximalInterphalangealDegrees = maxProximalInterphalangealDegrees;
				distalInterphalangealDegrees = maxDistalInterphalangealDegrees;
				return;
			}
			for (var i = 0; i < maxIterations; i++)
			{
				proximalInterphalangealDegrees = (maxProximalInterphalangealDegrees + minProximalInterphalangealDegrees) * 0.5f;
				distalInterphalangealDegrees = (maxDistalInterphalangealDegrees + minDistalInterphalangealDegrees) * 0.5f;
				var progress = GetProgress(proximalInterphalangealDegrees, distalInterphalangealDegrees);
				var delta = progress - preferredProgress;
				if (Mathf.Abs(delta) <= tolerance) break;
				if (delta < 0)
				{
					maxProximalInterphalangealDegrees = proximalInterphalangealDegrees;
					maxDistalInterphalangealDegrees = distalInterphalangealDegrees;
				}
				else
				{
					minProximalInterphalangealDegrees = proximalInterphalangealDegrees;
					minDistalInterphalangealDegrees = distalInterphalangealDegrees;
				}
			}
		}
	}
}
