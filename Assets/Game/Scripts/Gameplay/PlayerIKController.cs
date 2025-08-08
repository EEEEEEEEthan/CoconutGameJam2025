using System;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public class PlayerIKController : GameBehaviour
	{
		[SerializeField] Transform leftFoot;
		[SerializeField] Transform rightFoot;
		[SerializeField] Transform playerRoot;
		[SerializeField] float legLength;
		void Update()
		{
			var mid = (leftFoot.position + rightFoot.position) * 0.5f;
			var distance = Vector3.Distance(leftFoot.position, rightFoot.position);
			var halfDistance = distance * 0.5f;
			var height = Mathf.Sqrt(Mathf.Max(legLength * legLength - halfDistance * halfDistance, 0));
			playerRoot.position = mid.WithY(Mathf.Min(leftFoot.position.y, rightFoot.position.y) + (height - legLength));
		}
		void OnDrawGizmos()
		{
			Gizmos.DrawSphere(playerRoot.position, 0.01f);
			var distance = Vector3.Distance(leftFoot.position, rightFoot.position);
			var halfDistance = distance * 0.5f;
			var height = Mathf.Sqrt(Mathf.Max(legLength * legLength - halfDistance * halfDistance, 0));
			Gizmos.DrawRay(leftFoot.position, Vector3.up * legLength);
		}
	}
}
