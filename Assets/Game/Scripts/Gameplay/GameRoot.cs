using Game.Gameplay.WaterGame;
using ReferenceHelper;
using UnityEngine;
using UnityEngine.Rendering;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour
	{
		[SerializeField] Player player;
		[SerializeField] WaterGameJudge waterGame;
		[SerializeField] Volume volume;
		[SerializeField] CameraController cameraController;
		[SerializeField, ObjectReference(nameof(GameCanvas)),]
		GameCanvas gameCanvas;
		[SerializeField, ObjectReference(nameof(Sunlight)),]
		Sunlight sun;
		bool volumeProfileCopied;
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
		public WaterGameJudge WaterGame => waterGame;
	}
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
