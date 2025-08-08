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
				if (GUILayout.Button("Update Finger Rigging"))
				{
					var fingerRigging = (FingerRigging)target;
					fingerRigging.RecordDistances();
					UnityEditor.EditorUtility.SetDirty(fingerRigging);
				}
			}
		}
#endif
		[SerializeField] Transform metacarpophalangealJoint;
		[SerializeField] Transform proximalInterphalangealJoint;
		[SerializeField] Transform distalInterphalangealJoint;
		[SerializeField] Transform tip;
		[SerializeField] Transform target;
		[SerializeField, HideInInspector,] float metacarpophalangeal2ProximalInterphalangeal;
		[SerializeField, HideInInspector,] float proximalInterphalangeal2DistalInterphalangeal;
		[SerializeField, HideInInspector,] float distalInterphalangeal2Tip;
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
			if (target)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(target.position, 0.001f);
				if (d) Gizmos.DrawLine(target.position, d.position);
			}
#if UNITY_EDITOR
			if (a && b) UnityEditor.Handles.Label((a.position + b.position) * 0.5f, metacarpophalangeal2ProximalInterphalangeal.ToString("F2"));
			if (b && c) UnityEditor.Handles.Label((b.position + c.position) * 0.5f, proximalInterphalangeal2DistalInterphalangeal.ToString("F3"));
			if (c && d) UnityEditor.Handles.Label((c.position + d.position) * 0.5f, distalInterphalangeal2Tip.ToString("F3"));
#endif
		}
		void RecordDistances()
		{
			metacarpophalangeal2ProximalInterphalangeal = Vector3.Distance(metacarpophalangealJoint.position, proximalInterphalangealJoint.position);
			proximalInterphalangeal2DistalInterphalangeal = Vector3.Distance(proximalInterphalangealJoint.position, distalInterphalangealJoint.position);
			distalInterphalangeal2Tip = Vector3.Distance(distalInterphalangealJoint.position, tip.position);
		}
		void Update()
		{
			
		}
	}
}
