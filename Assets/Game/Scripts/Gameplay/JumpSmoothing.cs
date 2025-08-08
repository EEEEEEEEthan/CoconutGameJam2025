using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	public class JumpSmoothing : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] float acceleration = 10f; // Acceleration when jumping
		[SerializeField, HideInInspector,] float velocity;
		[SerializeField, HideInInspector,] float y;
		[SerializeField, HideInInspector,] bool jumping;
		[SerializeField, HideInInspector,] bool crunching;
		[SerializeField, HideInInspector,] Vector3 lastRaycastHitPoint;
		[SerializeField, HideInInspector,] float smoothVelocity;
		public bool Jumping => jumping;
		void Update()
		{
			if (hand.RaycastSource.CenterHitPoint.HasValue) lastRaycastHitPoint = hand.RaycastSource.CenterHitPoint.Value;
			if (jumping)
			{
				y += velocity * Time.deltaTime;
				var multiplier = velocity < 0 ? 0.1f : 1;
				velocity -= acceleration * Time.deltaTime * multiplier;
				if (velocity < 0 && y < lastRaycastHitPoint.y) jumping = false;
			}
			else
			{
				if (crunching)
					y = -0.01f;
				else
					y = 0;
			}
			hand.HandPositionUpdater.YOffset = Mathf.SmoothDamp(hand.HandPositionUpdater.YOffset, y, ref smoothVelocity, 0.01f);
		}
		public void Jump(float speed)
		{
			if (jumping)
			{
				Debug.LogError("Already jumping, cannot jump again!");
				return;
			}
			jumping = true;
			crunching = false;
			velocity = speed;
		}
		public void Crunch()
		{
			if (jumping)
			{
				Debug.LogError("Cannot crunch while jumping!");
				return;
			}
			crunching = true;
		}
		public void Stand()
		{
			if (jumping)
			{
				Debug.LogError("Cannot stand while jumping!");
				return;
			}
			crunching = false;
		}
	}
}
