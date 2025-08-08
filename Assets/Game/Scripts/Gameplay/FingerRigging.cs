using UnityEditor;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class FingerRigging : MonoBehaviour
	{
#if UNITY_EDITOR
		[CustomEditor(typeof(FingerRigging))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				{
					var property = serializedObject.FindProperty(nameof(targetDistance));
					var value = EditorGUILayout.Slider("Target Distance", property.floatValue, 0, ((FingerRigging)target).MaxLength);
					if (value != property.floatValue)
					{
						property.floatValue = value;
						serializedObject.ApplyModifiedProperties();
						serializedObject.Update();
					}
				}
				if (GUILayout.Button("Update Finger Rigging"))
				{
					var target = (FingerRigging)this.target;
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
		[SerializeField, HideInInspector,] float targetDistance;
		[SerializeField, HideInInspector,] float metacarpophalangeal2ProximalInterphalangeal;
		[SerializeField, HideInInspector,] float proximalInterphalangeal2DistalInterphalangeal;
		[SerializeField, HideInInspector,] float distalInterphalangeal2Tip;
		public float MaxLength => metacarpophalangeal2ProximalInterphalangeal + proximalInterphalangeal2DistalInterphalangeal + distalInterphalangeal2Tip;
		void Update() { }
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
				var preferredPosition = ray.GetPoint(targetDistance);
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(preferredPosition, 0.001f);
				Gizmos.DrawLine(a.position, preferredPosition);
			}
#if UNITY_EDITOR
			if (a && b) Handles.Label((a.position + b.position) * 0.5f, metacarpophalangeal2ProximalInterphalangeal.ToString("F2"));
			if (b && c) Handles.Label((b.position + c.position) * 0.5f, proximalInterphalangeal2DistalInterphalangeal.ToString("F3"));
			if (c && d) Handles.Label((c.position + d.position) * 0.5f, distalInterphalangeal2Tip.ToString("F3"));
#endif
		}
	}
}
