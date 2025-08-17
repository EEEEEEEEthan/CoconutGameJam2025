using System.Collections;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.GuardGame
{
	public class GuardGameController : GameBehaviour
	{
		[SerializeField] Transform guardPosition;
		[SerializeField] Animator guardAnimator;
		void Update() => guardPosition.position = guardAnimator.transform.position.WithY(0);
		public void Attack()
		{
			StopAllCoroutines();
			StartCoroutine(attack());
			IEnumerator attack()
			{
				yield return new WaitForSeconds(0.8f);
				guardAnimator.SetTrigger("Attack");
				yield return new WaitForSeconds(0.4f);
				yield return guardAnimator.transform.WaitJump(guardAnimator.transform.position + guardPosition.forward * 0.05f, 0.05f, 0.2f);
				yield return guardAnimator.transform.WaitJump(guardAnimator.transform.position + guardPosition.forward * 0.05f, 0.05f, 0.2f);
				yield return guardAnimator.transform.WaitJump(guardAnimator.transform.position + guardPosition.forward * 0.05f, 0.05f, 0.2f);
				yield return guardAnimator.transform.WaitJump(guardAnimator.transform.position + guardPosition.forward * 0.05f, 0.05f, 0.2f);
				yield return new WaitForSeconds(0.7f);
				GameRoot.CameraController.Shake(0.2f);
			}
		}
	}
}
