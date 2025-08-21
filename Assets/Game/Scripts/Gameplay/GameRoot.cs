using System.Collections.Generic;
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
		bool volumeProfileCopied;
		public UICamera UiCamera => uiCamera;
		public IReadOnlyList<Collider> GroundColliders => groundColliders;
		public Sunlight Sunlight => sun;
		public GameCanvas GameCanvas => gameCanvas;
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
