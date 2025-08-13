using Game.Utilities;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
namespace Game.Gameplay
{
	public class GameCanvas : GameBehaviour
	{
		[SerializeField, ObjectReference("UpperEdge"),]
		Image upperEdge;
		[SerializeField, ObjectReference("LowerEdge"),]
		Image lowerEdge;
		readonly DampSmoothing upperEdgePositionSmoothing;
		readonly DampSmoothing lowerEdgePositionSmoothing;
		GameCanvas()
		{
			upperEdgePositionSmoothing = new(0,
				v =>
				{
					Assert.IsNotNull(upperEdge, "Upper edge image is not assigned.");
					upperEdge.rectTransform.anchoredPosition = new(0, v.Remapped(0, 1, upperEdge.rectTransform.rect.height, 0));
				});
			lowerEdgePositionSmoothing = new(0,
				v =>
				{
					Assert.IsNotNull(lowerEdge, "Lower edge image is not assigned.");
					lowerEdge.rectTransform.anchoredPosition = new(0, v.Remapped(0, 1, -lowerEdge.rectTransform.rect.height, 0));
				});
		}
		public void Filmic(bool enable)
		{
			if (enable)
			{
				upperEdgePositionSmoothing.Set(1, 0.5f);
				lowerEdgePositionSmoothing.Set(1, 0.5f);
			}
			else
			{
				upperEdgePositionSmoothing.Set(0, 0.5f);
				lowerEdgePositionSmoothing.Set(0, 0.5f);
			}
		}
	}
}
