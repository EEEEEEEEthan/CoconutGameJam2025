using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game.Utilities
{
	public interface IPointerEventListener
	{
		public bool Down { get; }
		public event Action OnPointerDown;
		public event Action OnPointerUp;
		public event Action OnPointerEnter;
		public event Action OnPointerExit;
		public event Action OnPointerClick;
	}
	sealed class PointerEventListener : MonoBehaviour,
		IPointerEventListener,
		IPointerDownHandler,
		IPointerUpHandler,
		IPointerEnterHandler,
		IPointerExitHandler,
		IPointerClickHandler
	{
		public bool Down { get; private set; }
		public event Action OnPointerDown;
		public event Action OnPointerUp;
		public event Action OnPointerEnter;
		public event Action OnPointerExit;
		public event Action OnPointerClick;
		void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => OnPointerClick?.TryInvoke();
		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			Down = true;
			OnPointerDown?.TryInvoke();
		}
		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			Down = false;
			OnPointerUp?.TryInvoke();
		}
		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => OnPointerEnter?.TryInvoke();
		void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => OnPointerExit?.TryInvoke();
	}
	public static partial class Extensions
	{
		public static IPointerEventListener GetPointerListener(this RectTransform @this) => @this.gameObject.GetOrAddComponent<PointerEventListener>();
		public static IPointerEventListener GetPointerListener(this Graphic @this) => @this.gameObject.GetOrAddComponent<PointerEventListener>();
		public static IPointerEventListener GetPointerListener(this Selectable @this) => @this.gameObject.GetOrAddComponent<PointerEventListener>();
	}
}
