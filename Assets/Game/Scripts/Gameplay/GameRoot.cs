using System.Collections.Generic;
using System.Collections;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using SketchPostProcess;
using UnityEngine;
using UnityEngine.Rendering;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour
	{
		[SerializeField] Player player;
		[SerializeField] Volume volume;
		[SerializeField] CameraController cameraController;
		[SerializeField, ObjectReference(nameof(GameCanvas)),]
		GameCanvas gameCanvas;
		[SerializeField, ObjectReference(nameof(Sunlight)),]
		Sunlight sun;
		[SerializeField] Collider[] groundColliders;
		[SerializeField] UICamera uiCamera;
		[SerializeField] AudioSource bgm;
		Coroutine bgmFadeCoroutine;
		float bgmDefaultVolume = 1f;
		bool volumeProfileCopied;
		public UICamera UiCamera => uiCamera;
		public IReadOnlyList<Collider> GroundColliders => groundColliders;
		public Sunlight Sunlight => sun;
		public GameCanvas GameCanvas => gameCanvas;
		void Awake()
		{
			if (bgm != null) bgmDefaultVolume = bgm.volume;
		}
		public VolumeProfile VolumeProfile
		{
			get
			{
				if (!volumeProfileCopied)
				{
					volume.sharedProfile = volume.profile;
					volumeProfileCopied = true;
				}
				return volume.sharedProfile;
			}
		}
		public CameraController CameraController => cameraController;
		public Player Player => player;
		public void FadeOutBGM(float duration) => FadeBGMTo(0f, duration);
		void FadeBGMTo(float targetVolume, float duration)
		{
			if (bgm == null) return;
			if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
			bgmFadeCoroutine = StartCoroutine(FadeBGMToCoroutine(targetVolume, duration));
		}
		IEnumerator FadeBGMToCoroutine(float target, float duration)
		{
			if (bgm == null) yield break;
			var start = bgm.volume;
			if (Mathf.Approximately(start, target))
			{
				bgmFadeCoroutine = null;
				yield break;
			}
			if (duration <= 0f)
			{
				bgm.volume = target;
				bgmFadeCoroutine = null;
				yield break;
			}
			var elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / duration);
				bgm.volume = Mathf.Lerp(start, target, t);
				yield return null;
			}
			bgm.volume = target;
			bgmFadeCoroutine = null;
		}
		public void CancelSketch()
		{
			var volume = VolumeProfile.TryGet(typeof(SketchVolume), out VolumeComponent component) ? (SketchVolume)component : null;
			var smooth = new DampSmoothing(1, v => volume.Weigth = v);
			smooth.Set(0, 1f);
		}
	}
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
