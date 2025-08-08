using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class HandPositionUpdater : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] Vector3 offset;
		public float YOffset
		{
			get => offset.y;
			set => offset.y = value;
		}
		void LateUpdate() => hand.HandRoot.position = (hand.Left.Target.position + hand.Right.Target.position) * 0.5f + offset;
	}
}
