using Game.Gameplay.WaterGame;
using UnityEngine;
using UnityEngine.Rendering;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour
	{
		[SerializeField] Player player;
		[SerializeField] WaterGameJudge waterGame;
		[SerializeField] Volume volume;
		bool volumeProfileCopied;
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
		public Player Player => player ??= GetComponentInChildren<Player>();
		public WaterGameJudge WaterGame => waterGame;
	}
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
