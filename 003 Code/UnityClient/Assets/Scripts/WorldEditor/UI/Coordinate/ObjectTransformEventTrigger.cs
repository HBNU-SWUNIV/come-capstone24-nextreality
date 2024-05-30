using NextReality.Asset.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public class ObjectTransformEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public CoordinateDirectionElement directionElement;

		public void OnBeginDrag(PointerEventData eventData)
		{
			directionElement.beginDragAction?.Invoke(eventData, directionElement);
		}

		public void OnDrag(PointerEventData eventData)
		{
			directionElement.dragAction?.Invoke(eventData, directionElement);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			directionElement.endDragAction?.Invoke(eventData, directionElement);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			directionElement.pointerDownAction?.Invoke(eventData, directionElement);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			directionElement.pointerUpAction?.Invoke(eventData, directionElement);
		}
	}

}
