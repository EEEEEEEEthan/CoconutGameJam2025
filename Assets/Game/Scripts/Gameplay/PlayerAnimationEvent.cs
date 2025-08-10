using UnityEngine;

namespace Game.Gameplay
{
	public class PlayerAnimationEvent : GameBehaviour
	{
		[SerializeField]
		private float defaultShakeDuration = 0.2f;
		
		public void AnimEvt_EnabledHandIK() => GameRoot.Player.HandIkInput.SetWeight(1, 0.2f);
		public void AnimEvt_DisabledHandIK() => GameRoot.Player.HandIkInput.SetWeight(0, 0.2f);
		public void AnimEvt_SpecialAnimEnd() => GameRoot.Player.SetSpecialAnimEnd();
		public void AnimEvt_SyncHandIK() => GameRoot.Player.HandIkInput.SyncAnimationToIK();
		public void AnimEvt_ResetIK() => GameRoot.Player.HandIkInput.ResetMotion();
		public void AnimEvt_ShakeCamera() => GameRoot.CameraController.Shake(defaultShakeDuration);
	}
}
