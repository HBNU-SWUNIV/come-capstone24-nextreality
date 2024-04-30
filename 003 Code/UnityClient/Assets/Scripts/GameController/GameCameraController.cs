using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

namespace NextReality.Game
{
	public class GameCameraController : MonoBehaviour
	{
		Camera mainCamera;

		private static GameCameraController instance = null;

		public LayerMask detectLayer;

		void Awake()
		{
			if (instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}

			instance = this;
		}

		public static GameCameraController Instance
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
		// Start is called before the first frame update
		void Start()
		{
		}
		public void Raycast(Vector2 pointerPosition, float maxDistance, Action<bool, RaycastHit>  action, LayerMask layerMask)
		{
			if (!mainCamera) return;

			RaycastHit RefRayCastHit;
			bool isHitted = Physics.Raycast(
				mainCamera.ScreenPointToRay(new Vector3(pointerPosition.x, pointerPosition.y, 0)),
				out RefRayCastHit,
				maxDistance,
				layerMask.value
			);
			action(isHitted, RefRayCastHit);
		}

		public Vector2? GetUIPositionByWorldObject(Vector3 worldPosition, RectTransform canvasRect)
		{
			return GetUIPositionByWorldObject(worldPosition, canvasRect, mainCamera);
		}

		public Vector2? GetUIPositionByWorldObject(Vector3 worldPosition, RectTransform canvasRect, Camera camera)
		{
			if (!camera) return null;
			Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);

			if (viewportPosition.z < 0) return null;

			Vector2 worldObject_ScreenPosition = new Vector2(
			((viewportPosition.x * canvasRect.rect.width) - (canvasRect.rect.width * 0.5f)),
			((viewportPosition.y * canvasRect.rect.height) - (canvasRect.rect.height * 0.5f)));

			//now you can set the position of the ui element
			return worldObject_ScreenPosition;
		}

		public Vector3? GetPlaneHitPoint(Plane objectPlane, Vector2 mousePosition)
		{
			float distance;
			var point = this.GetPlaneHitPointWithHeight(objectPlane, mousePosition, out distance);
			return point;
		}

		public Vector3? GetPlaneHitPointWithHeight(Plane objectPlane, Vector2 mousePosition, out float hitDistance)
		{
			if (!this.mainCamera)
			{
				hitDistance = 0f;
				return null;
			}
			Ray targetRay = mainCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));

			// 오브젝트 표면과 교차하는 위치를 얻기 위해 Raycast를 사용합니다.

			if (objectPlane.Raycast(targetRay, out hitDistance))
			{
				var hitPoint = targetRay.GetPoint(hitDistance);
				return hitPoint;
			}
			return null;
		}
	}

}
