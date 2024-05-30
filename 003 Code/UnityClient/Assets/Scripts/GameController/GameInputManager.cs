using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NextReality.Game
{
	public class GameInputManager : MonoBehaviour
	{
		[SerializeField] private float touchZoomSpeed = 0.005f;
		[SerializeField] private float mouseZoomSpeed = 0.2f;

		public static GameInputManager instance;

		void Awake()
		{
			if (instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}

			instance = this;
		}

		public static GameInputManager Instance
		{
			get
			{
				if (null == instance)
				{
					return null;
				}
				return instance;
			}
		}

		public bool IsPointerOverUIObjectByIndex(int index)
		{
			if (Input.touchCount <= index) return false;

			Vector2 position = Input.GetTouch(index).position;
			return IsPointerOverUIObjectByVector(position);
		}

		public bool IsPointerOverUIObjectByTouch(Touch touch)
		{
			Vector2 position = touch.position;
			return IsPointerOverUIObjectByVector(position);
		}

		private bool IsPointerOverUIObjectByVector(Vector2 position)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			eventData.position = position;

			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, raycastResults);

			return raycastResults.Count > 0;
		}

		public bool IsPointerOverUIObjectAll(int mouseIndex = 0)
		{
			foreach (Touch touch in Input.touches)
			{
				if (IsPointerOverUIObjectByTouch(touch)) return true;
			}

			if (Input.mousePresent && Input.GetMouseButton(mouseIndex))
			{
				return IsPointerOverUIObjectByVector(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			}

			return false;
		}

		public bool isBeganTouch
		{
			get
			{
				if (!Input.GetMouseButton(0) || Input.touchCount != 1)
				{
					return false;
				}

				if (Input.mousePresent)
				{
					return Input.GetMouseButtonDown(0);
				}
				else
				{
					return Array.FindIndex(Input.touches, touch => touch.phase != TouchPhase.Began) < 0;
				}
			}
		}

		public void CallPointerEventData(System.Action<PointerEventData> action)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			action(eventData);
		}

		public float GetZoomDelta(bool isReverseTouch = false)
		{
			float deltaZoom = 0;

			if (Input.touchSupported)
			{
				if (Input.touchCount == 2)
				{
					Touch tZero = Input.GetTouch(0);
					Touch tOne = Input.GetTouch(1);
					Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
					Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

					float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
					float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

					deltaZoom = (oldTouchDistance - currentTouchDistance) * touchZoomSpeed;
					if (isReverseTouch) deltaZoom *= -1;
				}
			}
			else
			{
				deltaZoom = Input.GetAxis("Mouse ScrollWheel") * mouseZoomSpeed;
			}

			return deltaZoom;
		}
	}
}
