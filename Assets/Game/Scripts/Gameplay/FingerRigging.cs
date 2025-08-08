using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class FingerRigging : MonoBehaviour
	{
#if UNITY_EDITOR
		[UnityEditor.CustomEditor(typeof(FingerRigging))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				var target = (FingerRigging)this.target;
				UnityEditor.EditorGUILayout.LabelField("distance",
					target
						.GetDistance(
							target.proximalInterphalangealJoint.localEulerAngles.x,
							target.distalInterphalangealJoint.localEulerAngles.x)
						.ToString("F3"));
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
		[SerializeField] Transform metacarpophalangealJoint;
		[SerializeField] Transform proximalInterphalangealJoint;
		[SerializeField] Transform distalInterphalangealJoint;
		[SerializeField] Transform tip;
		[SerializeField, Range(0, 1),] float progress;
		[SerializeField, HideInInspector,] float metacarpophalangeal2ProximalInterphalangeal;
		[SerializeField, HideInInspector,] float proximalInterphalangeal2DistalInterphalangeal;
		[SerializeField, HideInInspector,] float distalInterphalangeal2Tip;
		public float MaxLength => metacarpophalangeal2ProximalInterphalangeal + proximalInterphalangeal2DistalInterphalangeal + distalInterphalangeal2Tip;
		void Update()
		{
			var distance = progress * MaxLength;
			GetAngles(distance, out var proximalInterphalangealDegrees, out var distalInterphalangealDegrees);
			if (metacarpophalangealJoint)
			{
				proximalInterphalangealJoint.localEulerAngles = new(-proximalInterphalangealDegrees, 0, 0);
				distalInterphalangealJoint.localEulerAngles = new(-distalInterphalangealDegrees, 0, 0);
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
				Vector3.Distance(a.position, d.position);
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
		float GetDistance(float proximalInterphalangealDegrees, float distalInterphalangealDegrees)
		{
			var v0 = new Vector2(metacarpophalangeal2ProximalInterphalangeal, 0);
			var v1 = new Vector2(proximalInterphalangeal2DistalInterphalangeal, 0).RotateClockwise(proximalInterphalangealDegrees);
			var v2 = new Vector2(distalInterphalangeal2Tip, 0).RotateClockwise(proximalInterphalangealDegrees + distalInterphalangealDegrees);
			var p1 = v0;
			var p2 = p1 + v1;
			var p3 = p2 + v2;
			if (p3.y > 0) return -p3.magnitude;
			return p3.magnitude;
		}
		void GetAngles(float preferredDistance, out float proximalInterphalangealDegrees, out float distalInterphalangealDegrees)
		{
			// 初始化角度
			proximalInterphalangealDegrees = 0f;
			distalInterphalangealDegrees = 0f;
			
			// 如果目标距离大于等于最大长度，不需要弯曲
			if (preferredDistance >= MaxLength)
			{
				return;
			}
			
			// 如果目标距离为负数或接近0，完全弯曲
			if (preferredDistance <= 0.001f)
			{
				proximalInterphalangealDegrees = 90f;
				distalInterphalangealDegrees = 90f;
				return;
			}
			
			// 使用数值方法求解最佳角度
			float bestProximal = 0f;
			float bestDistal = 0f;
			float bestError = float.MaxValue;
			
			// 粗略搜索
			for (int i = 0; i <= 18; i++) // 0-90度，每5度一个步长
			{
				float proximal = i * 5f;
				for (int j = 0; j <= 18; j++)
				{
					float distal = j * 5f;
					float currentDistance = GetDistance(proximal, distal);
					float error = Mathf.Abs(currentDistance - preferredDistance);
					
					if (error < bestError)
					{
						bestError = error;
						bestProximal = proximal;
						bestDistal = distal;
					}
				}
			}
			
			// 精细搜索 - 在最佳点周围进行更精确的搜索
			float searchRange = 5f;
			float step = 0.5f;
			
			for (float proximal = Mathf.Max(0, bestProximal - searchRange); 
			     proximal <= Mathf.Min(90, bestProximal + searchRange); 
			     proximal += step)
			{
				for (float distal = Mathf.Max(0, bestDistal - searchRange); 
				     distal <= Mathf.Min(90, bestDistal + searchRange); 
				     distal += step)
				{
					float currentDistance = GetDistance(proximal, distal);
					float error = Mathf.Abs(currentDistance - preferredDistance);
					
					if (error < bestError)
					{
						bestError = error;
						bestProximal = proximal;
						bestDistal = distal;
					}
				}
			}
			
			proximalInterphalangealDegrees = bestProximal;
			distalInterphalangealDegrees = bestDistal;
		}
	}
}
