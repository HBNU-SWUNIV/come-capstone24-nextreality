using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public class CoordinateDirectionElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public CoordinateDirection coordinateDirection;

		public Action<PointerEventData, CoordinateDirectionElement> pointerDownAction;
		public Action<PointerEventData, CoordinateDirectionElement> pointerUpAction;
		public Action<PointerEventData, CoordinateDirectionElement> dragAction;
		public Action<PointerEventData, CoordinateDirectionElement> beginDragAction;
		public Action<PointerEventData, CoordinateDirectionElement> endDragAction;

		[SerializeField] protected Graphic targetGraphic;

		public void SetInitEvent(CoordinateDirection _coordinateDirection)
		{
			coordinateDirection = _coordinateDirection;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			beginDragAction?.Invoke(eventData, this);
		}

		public void OnDrag(PointerEventData eventData)
		{
			pointerDownAction?.Invoke(eventData, this);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			endDragAction?.Invoke(eventData, this);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			pointerDownAction?.Invoke(eventData, this);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerUpAction?.Invoke(eventData, this);
		}

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void SetTranslucent(bool _isTranslucent = true)
		{
			Color color = targetGraphic.color;
			color.a = _isTranslucent ? 0.6f : 1f;
			targetGraphic.color = color;
		}
	}

}
