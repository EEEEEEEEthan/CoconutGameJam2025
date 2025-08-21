using System.Collections;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.GuardGame
{
	public class GuardGameController : GameBehaviour
	{
		[SerializeField] Transform guardPosition;
		[SerializeField] Animator guardAnimator;
		[SerializeField] Transform playerResetPosition;
		[SerializeField] Transform guardResetPosition;
		[SerializeField] MeshRenderer badgeRenderer;
		[SerializeField] Collider unlockCollider;
		bool attacking;
		bool guarding;
		void Awake() => guarding = true;
		void Update() => guardPosition.position = guardAnimator.transform.position.WithY(0);
		public void Attack()
		{
			if (!guarding) return;
			if (attacking) return;
			attacking = true;
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
				yield return new WaitForSeconds(0.5f);
				attacking = false;
				if (guardAnimator.transform.localPosition.z > 0) yield break;
				guarding = false;
				yield return new WaitForSeconds(1.5f);
				GameRoot.GameCanvas.Filmic(true);
				yield return new WaitForSeconds(0.5f);
				guardAnimator.SetTrigger("Hi");
				yield return new WaitForSeconds(0.8f);
				yield return guardAnimator.transform.WaitJump(guardAnimator.transform.position + guardPosition.right * 0.08f, 0.05f, 0.3f);
				yield return guardAnimator.transform.WaitJump(guardAnimator.transform.position + guardPosition.right * 0.08f, 0.05f, 0.3f);
				yield return new WaitForSeconds(1.7f);
				badgeRenderer.enabled = false;
				unlockCollider.enabled = true;
				GameRoot.GameCanvas.Filmic(false);
				GameRoot.Player.Unlock(KeyCode.Alpha4, true);
			}
		}
		public void Restart()
		{
			attacking = false;
			guarding = true;
			StopAllCoroutines();
			StartCoroutine(restart());
			IEnumerator restart()
			{
				GameRoot.Player.HandIkInput.ResetPosition(playerResetPosition.position);
				yield return new WaitForSeconds(0.5f);
				guardAnimator.transform.position = guardResetPosition.position;
			}
		}
	}
}
