using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace NextReality.Asset.UI
{
	public class CoordinateDirection : ObjectDirectionBase
	{

		[SerializeField] private CoordinateDirectionElement[] dirElementXYZ = new CoordinateDirectionElement[3];
		private Transform[] sortDirectionArray = new Transform[3];


		public override void InitEvent(ObjectTransformAction[] eventXYZ)
		{
			for (int i = 0; i < 3; i++)
			{
				dirElementXYZ[i].SetInitEvent(this);
				dirElementXYZ[i].SetTranslucent();

			}
			for (int i = 0; i < 3; i++)
			{
				dirElementXYZ[i].pointerDownAction = (data, direction) =>
				{
					direction.SetTranslucent(false);
				};
				dirElementXYZ[i].beginDragAction = (data, direction) =>
				{
					direction.SetTranslucent(false);
					this.Drag(data, eventXYZ[i]);
				};
				dirElementXYZ[i].dragAction = (data, direction) =>
				{
					direction.SetTranslucent(false);
					this.Drag(data, eventXYZ[i]);
				};
				dirElementXYZ[i].endDragAction = (data, direction) =>
				{
					direction.SetTranslucent(true);
					this.Drag(data, eventXYZ[i], false);
				};
				dirElementXYZ[i].pointerUpAction = (data, direction) =>
				{
					direction.SetTranslucent(true);
				};

			}
		}

		protected override void RenderDirection()
		{
			Transform cameraTransform = CanvasCamera?.transform;
			if (!cameraTransform || TargetObject != null) return;
			for (int i = 0; i < 3; i++) { sortDirectionArray[i] = dirElementXYZ[i].transform; }

			Array.Sort(sortDirectionArray, (rectA, rectB) =>
			{
				float distanceToCamera1 = Vector3.SqrMagnitude(cameraTransform.position - (TargetObject.transform.position + GetDirVector(rectA)));
				float distanceToCamera2 = Vector3.SqrMagnitude(cameraTransform.position - (TargetObject.transform.position + GetDirVector(rectB)));

				// 내림차순으로 정렬하려면 아래의 줄을 바꿉니다.
				return distanceToCamera2 > distanceToCamera1 ? 1 : -1;
			});

			foreach (var rect in sortDirectionArray)
			{
				rect.parent.SetAsLastSibling();
			}

			foreach (var direction in dirElementXYZ)
			{
				direction.transform.rotation = cameraTransform.rotation;
				direction.transform.localRotation = Quaternion.Euler(
					new Vector3(0, direction.transform.localEulerAngles.y, 0)
				);
			}
		}

		private Vector3 GetDirVector(Transform targetDir)
		{

			if (targetDir == dirElementXYZ[0].transform) return transform.right;
			if (isConstraint)
			{
				if (targetDir == dirElementXYZ[0].transform) return transform.right;
				else if (targetDir == dirElementXYZ[1].transform) return transform.up;
				else if (targetDir == dirElementXYZ[2].transform) return transform.forward;
			}
			else
			{
				if (targetDir == dirElementXYZ[0].transform) return Vector3.right;
				else if (targetDir == dirElementXYZ[1].transform) return Vector3.up;
				else if (targetDir == dirElementXYZ[2].transform) return Vector3.forward;
			}

			return Vector3.zero;
		}

	}

}
