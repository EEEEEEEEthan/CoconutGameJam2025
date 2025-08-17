namespace Game.Gameplay
{
	public class PlayerAnimationEvent : GameBehaviour
	{
		Player player;
		Player Player => player ??= GetComponentInParent<Player>();
		public void AnimEvt_EnabledHandIK() => Player.HandIkInput.SetWeight(1, 0.2f);
		public void AnimEvt_DisabledHandIK() => Player.HandIkInput.SetWeight(0, 0.2f);
		public void AnimEvt_SpecialAnimEnd() => Player.SetSpecialAnimEnd();
		public void AnimEvt_SyncHandIK() => Player.HandIkInput.SyncAnimationToIK();
		public void AnimEvt_ResetIK() => Player.HandIkInput.ResetMotion();
		public void AnimEvt_ShakeCamera() => GameRoot.CameraController.Shake(0.2f);
	}
}
