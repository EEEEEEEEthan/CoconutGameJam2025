using ReferenceHelper;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
namespace Game.Gameplay
{
	public class AutoDepthOfField : GameBehaviour
	{
		[SerializeField, ObjectReference,] CinemachineFollow follow;
		[SerializeField, MinMaxRangeSlider(0, 1),]
		Vector2 minRange = new(0.1f, 0.5f);
		[SerializeField, MinMaxRangeSlider(1, 20),]
		Vector2 maxRange = new(2, 3);
		DepthOfField depthOfField;
		DepthOfField DepthOfField =>
			depthOfField ??= GameRoot.VolumeProfile.TryGet(typeof(DepthOfField), out VolumeComponent component) ? (DepthOfField)component : null;
		void Update()
		{
			var distance = follow.FollowOffset.magnitude;
			DepthOfField.nearFocusStart.value = distance * minRange.x;
			DepthOfField.nearFocusEnd.value = distance * minRange.y;
			DepthOfField.farFocusStart.value = distance * maxRange.x;
			DepthOfField.farFocusEnd.value = distance * maxRange.y;
		}
	}
}
