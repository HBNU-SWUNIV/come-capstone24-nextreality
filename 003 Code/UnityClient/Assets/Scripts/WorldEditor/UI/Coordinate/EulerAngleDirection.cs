using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.InputSystem.PlayerInput;

namespace NextReality.Asset.UI
{
	public class EulerAngleDirection : ObjectDirectionBase
	{
		[SerializeField] private CoordinateDirectionElement[] quadrantX;

		[SerializeField] private CoordinateDirectionElement[] quadrantY;

		[SerializeField] private CoordinateDirectionElement[] quadrantZ;

		protected override void RenderDirection()
		{
			Transform cameraTransform = CanvasCamera?.transform;
			if (!cameraTransform || TargetObject != null) return;
			Vector3 dirVector = cameraTransform.position - TargetObject.transform.position;

			Vector3 curEulerAngles = quadrantX[0].transform.localEulerAngles;
			if (dirVector.z > 0)
			{
				if (curEulerAngles.y != 180) foreach (var quadrant in quadrantZ)
					{
						quadrant.transform.localEulerAngles = new Vector3(0, 180, 0);
					}

			}
			else
			{
				if (curEulerAngles.y != 0) foreach (var quadrant in quadrantZ)
					{
						quadrant.transform.localEulerAngles = new Vector3(0, 0, 0);
					}

			}
			if (dirVector.y > 0)
			{
				if (curEulerAngles.x != 90) foreach (var quadrant in quadrantY)
					{
						quadrant.transform.localEulerAngles = new Vector3(90, 0, 0);
					}


			}
			else
			{
				if (curEulerAngles.x != -90) foreach (var quadrant in quadrantY)
					{
						quadrant.transform.localEulerAngles = new Vector3(-90, 0, 0);
					}

			}
			if (dirVector.x > 0)
			{
				if (curEulerAngles.y != -90) foreach (var quadrant in quadrantX)
					{
						quadrant.transform.localEulerAngles = new Vector3(0, -90, 0);
					}

			}
			else
			{
				if (curEulerAngles.y != 90) foreach (var quadrant in quadrantX)
					{
						quadrant.transform.localEulerAngles = new Vector3(0, 90, 0);
					}

			}
			/*1*/
			if (dirVector.x > 0 && dirVector.y > 0 && dirVector.z > 0) SortQuadrant(1, 3, 1);
			/*2*/
			else if (dirVector.x <= 0 && dirVector.y > 0 && dirVector.z > 0) SortQuadrant(0, 2, 0);
			/*3*/
			else if (dirVector.x > 0 && dirVector.y <= 0 && dirVector.z > 0) SortQuadrant(2, 0, 2);
			/*4*/
			else if (dirVector.x <= 0 && dirVector.y <= 0 && dirVector.z > 0) SortQuadrant(3, 1, 3);
			/*5*/
			else if (dirVector.x > 0 && dirVector.y > 0 && dirVector.z <= 0) SortQuadrant(0, 0, 0);
			/*6*/
			else if (dirVector.x <= 0 && dirVector.y > 0 && dirVector.z <= 0) SortQuadrant(1, 1, 1);
			/*7*/
			else if (dirVector.x > 0 && dirVector.y <= 0 && dirVector.z <= 0) SortQuadrant(3, 3, 3);
			/*8*/
			else if (dirVector.x <= 0 && dirVector.y <= 0 && dirVector.z <= 0) SortQuadrant(2, 2, 2);
		}

		private void SortQuadrant(int xIndex, int yIndex, int zIndex)
		{
			quadrantX[xIndex].transform.SetAsLastSibling();
			quadrantY[yIndex].transform.SetAsLastSibling();
			quadrantZ[zIndex].transform.SetAsLastSibling();

			quadrantX[(xIndex + 2) % 4].transform.SetAsFirstSibling();
			quadrantY[(yIndex + 2) % 4].transform.SetAsFirstSibling();
			quadrantZ[(zIndex + 2) % 4].transform.SetAsFirstSibling();
		}
		public override void InitEvent(ObjectTransformAction[] eventXYZ)
		{
			foreach (var quadrant in quadrantX) { quadrant.SetTranslucent(); }
			foreach (var quadrant in quadrantY) { quadrant.SetTranslucent(); }
			foreach (var quadrant in quadrantZ) { quadrant.SetTranslucent(); }
			SetEventByDirection(quadrantX, eventXYZ[0]);
			SetEventByDirection(quadrantY, eventXYZ[1]);
			SetEventByDirection(quadrantZ, eventXYZ[2]);
		}

		private void SetEventByDirection(CoordinateDirectionElement[] directions, ObjectTransformAction actionEvent)
		{

			foreach (var direction in directions)
			{

				for (int i = 0; i < 3; i++)
				{
					direction.pointerDownAction = (data, direction) =>
					{
						foreach (var dir in directions)
						{
							dir.SetTranslucent(false);
						}
					};
					direction.beginDragAction = (data, direction) =>
					{
						foreach (var dir in directions)
						{
							dir.SetTranslucent(false);
						}
						this.Drag(data, actionEvent);
					};
					direction.dragAction = (data, direction) =>
					{
						foreach (var dir in directions)
						{
							dir.SetTranslucent(false);
						}
						this.Drag(data, actionEvent);
					};
					direction.endDragAction = (data, direction) =>
					{
						foreach (var dir in directions)
						{
							dir.SetTranslucent(true);
						}
						this.Drag(data, actionEvent, false);
					};
					direction.pointerUpAction = (data, direction) =>
					{
						foreach (var dir in directions)
						{
							dir.SetTranslucent(true);
						}
					};

				}
			}
		}
	}

}
