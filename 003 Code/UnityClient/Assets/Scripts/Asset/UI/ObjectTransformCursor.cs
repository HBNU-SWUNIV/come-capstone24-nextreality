using NextReality.Data.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NextReality.Asset.UI
{

	public class ObjectTransformCursor : MonoBehaviour
	{
		protected enum DirectionType { None, X, Y, Z }
		public enum DirectionMode { Position, Rotation, Scale, None }

		[SerializeField] Canvas coordinatesCanvas;
		[SerializeField] protected CoordinateDirection posDirection;
		[SerializeField] protected EulerAngleDirection rotDirection;
		[SerializeField] protected CoordinateDirection scaleDirection;

		[SerializeField] protected float viewScaleRate;

		protected DirectionType curPosDirection = DirectionType.None;

		private Plane? posPlane = null;
		private Vector3? firstHitPosPoint = null;
		private Vector3? firstObjPosPoint = null;

		protected DirectionType curRotDirection = DirectionType.None;
		private Plane? rotPlane = null;

		private DirectionType curScaleDirection = DirectionType.None;
		private Vector3? firstHitScalePoint = null;
		private Vector3? firstObjScalePoint = null;
		private Plane? scalePlane = null;

		protected DirectionMode curDirectionMode = DirectionMode.Position;

		protected ObjectTransformer targetTransformer;


		// Start is called before the first frame update
		void Start()
		{
			coordinatesCanvas.worldCamera = Managers.Camera.UICamera;

			this.posDirection.InitEvent(
				new ObjectTransformAction[3]{
					(targetItem, pointerData, isPointerIn) =>
					{
						this.PosDirectionEvent(DirectionType.X, Vector3.up, Vector3.forward, targetItem, pointerData.position, isPointerIn);
					},
					(targetItem, pointerData, isPointerIn) =>
					{
						this.PosDirectionEvent(DirectionType.Y, Vector3.forward, Vector3.right, targetItem, pointerData.position, isPointerIn);
					},
					(targetItem, pointerData, isPointerIn) =>
					{
						this.PosDirectionEvent(DirectionType.Z, Vector3.right, Vector3.up, targetItem, pointerData.position, isPointerIn);
					}
				}
			);
			this.rotDirection.InitEvent(
				 new ObjectTransformAction[3] {
				(targetItem, pointerData, isPointerIn) =>
				{
					this.RotDirectionEvent(DirectionType.X, targetItem, pointerData, isPointerIn);
				},
				(targetItem, pointerData, isPointerIn) =>
				{
					this.RotDirectionEvent(DirectionType.Y, targetItem, pointerData, isPointerIn);
				},
				(targetItem, pointerData, isPointerIn) =>
				{
					this.RotDirectionEvent(DirectionType.Z, targetItem, pointerData, isPointerIn);
				}
				 }
			);
			this.scaleDirection.InitEvent(
				 new ObjectTransformAction[3] {
				(targetItem, pointerData, isPointerIn) =>
				{
					this.ScaleDirectionEvent(DirectionType.X, Vector3.up, Vector3.forward, targetItem, pointerData.position, isPointerIn);
				},
				(targetItem, pointerData, isPointerIn) =>
				{
					this.ScaleDirectionEvent(DirectionType.Y, Vector3.forward, Vector3.right, targetItem, pointerData.position, isPointerIn);
				},
				(targetItem, pointerData, isPointerIn) =>
				{
					this.ScaleDirectionEvent(DirectionType.Z, Vector3.right, Vector3.up, targetItem, pointerData.position, isPointerIn);
				}
				 }
			);

			this.posDirection.SetCoordinatesObject(this);
			this.rotDirection.SetCoordinatesObject(this);
			this.scaleDirection.SetCoordinatesObject(this);
		}


		Vector3 tempPosition;
		Vector3 tempRotation;
		Vector3 tempScale;

		// Update is called once per frame
		void Update()
		{
			UpdateLocation();

			if (!targetTransformer.CustomObjectInterface.Position.Equals(tempPosition) || !targetTransformer.CustomObjectInterface.EulerAngles.Equals(tempRotation) || !targetTransformer.CustomObjectInterface.LocalScale.Equals(tempScale))
			{
                AssetMoveSchema schema = new AssetMoveSchema();
                schema.objectId = targetTransformer.CustomObjectInterface.gameObject.name;
                schema.position = targetTransformer.CustomObjectInterface.Position;
                schema.rotation = targetTransformer.CustomObjectInterface.EulerAngles;
                schema.scale = targetTransformer.CustomObjectInterface.LocalScale;
                Managers.Network.SendMessage(schema.StringifyData());
            } 
            tempPosition = targetTransformer.CustomObjectInterface.Position;
            tempRotation = targetTransformer.CustomObjectInterface.EulerAngles;
            tempScale = targetTransformer.CustomObjectInterface.LocalScale;


        }

		private void PosDirectionEvent(DirectionType targetDirection, Vector3 upVector1, Vector3 upVector2, IDeformableObject targetItem, Vector2 mousePosition, bool isPointerIn)
		{
			if (this.curPosDirection != targetDirection || posPlane == null)
			{
				this.curPosDirection = targetDirection;
				this.posPlane = new Plane(FindClosestFacingVector(
					TargetCamera.transform.forward,
					upVector1,
					upVector2
				), targetItem.Position);
			}
			Plane plane = posPlane.Value;

			Vector3? hitPoint = Managers.Camera.GetPlaneHitPoint(plane, mousePosition);
			if (hitPoint != null)
			{
				Vector3 point = hitPoint.Value;
				Vector3 targetPoint = targetItem.Position;
				switch (targetDirection)
				{
					case DirectionType.X:
						targetPoint.x = point.x;
						break;
					case DirectionType.Y:
						targetPoint.y = point.y;
						break;
					case DirectionType.Z:
						targetPoint.z = point.z;
						break;
				}

				if (firstObjPosPoint == null) firstObjPosPoint = targetItem.LocalPosition;

				targetItem.Position = targetPoint;

				if (firstHitPosPoint == null) firstHitPosPoint = targetItem.LocalPosition;
				targetItem.LocalPosition = firstObjPosPoint + (targetItem.LocalPosition - firstHitPosPoint) ?? targetItem.LocalPosition;
				targetItem.LocalPosition = this.targetTransformer.CheckCustomObjectPostion(targetItem.LocalPosition);

				//targetItem.CalibratePositionByUnit(this.targetTransformer.PosCorrectedUnit);
			}

			if (!isPointerIn)
			{
				this.curPosDirection = DirectionType.None;
				this.firstHitPosPoint = null;
				this.firstObjPosPoint = null;
			}
		}

		Vector3 FindClosestFacingVector(Vector3 directionVector, Vector3 vectorA, Vector3 vectorB)
		{

			// 벡터 A와 벡터 B를 내적하여 두 벡터가 얼마나 직교하는지 확인합니다.
			float dotProductA = Vector3.Dot(directionVector, vectorA);
			float dotProductB = Vector3.Dot(directionVector, vectorB);

			if (Math.Abs(dotProductA) > Math.Abs(dotProductB))
			{
				return vectorA;
			}

			else
			{
				return vectorB;
			}
		}

		private void RotDirectionEvent(DirectionType targetDirection, IDeformableObject targetItem, PointerEventData pointerData, bool isPointerIn)
		{
			if (curRotDirection != targetDirection || rotPlane.Equals(default(Plane)))
			{
				curRotDirection = targetDirection;
				rotPlane = new Plane(GetUpVector(targetDirection), targetItem.Position);
			}

			Plane plane = rotPlane.Value;

			Vector3? curHit = Managers.Camera.GetPlaneHitPoint(plane, pointerData.position);
			Vector3? prevHit = Managers.Camera.GetPlaneHitPoint(plane, pointerData.position - pointerData.delta);

			if (curHit.HasValue && prevHit.HasValue)
			{
				Quaternion addRotation = Quaternion.FromToRotation(prevHit.Value - targetItem.Position, curHit.Value - targetItem.Position);
				targetItem.Rotation = addRotation * targetItem.Rotation;

				//Debug.Log($"[ObjectTransformCursor] RotDirectionEvent {prevHit.Value}:{prevHit.Value}:{addRotation}");
			}

			//targetItem.CalibrateEulerAnglesByUnit(targetTransformer.RotCorrectedUnit);

			if (!isPointerIn)
				curRotDirection = DirectionType.None;
		}

		private void ScaleDirectionEvent(DirectionType targetDirection, Vector3 upVector1, Vector3 upVector2, IDeformableObject targetItem, Vector2 mousePosition, bool isPointerIn)
		{
			if (curScaleDirection != targetDirection || scalePlane == null)
			{
				curScaleDirection = targetDirection;
				scalePlane = new Plane(targetItem.TransformDirection(FindClosestFacingVector(TargetCamera.transform.forward, upVector1, upVector2)), targetItem.Position);
			}

			Plane plane = scalePlane.Value;
			Vector3? targetPoint = Managers.Camera.GetPlaneHitPoint(plane, mousePosition);

			if (targetPoint.HasValue)
			{
				Vector3 scaleDirection = Vector3.Scale(GetUpVector(targetDirection), targetItem.InverseTransformDirection(targetPoint.Value - targetItem.Position));
				Vector3 curScale = targetItem.LocalScale;
				switch (targetDirection)
				{
					case DirectionType.X:
						curScale.x = scaleDirection.x;
						break;
					case DirectionType.Y:
						curScale.y = scaleDirection.y;
						break;
					case DirectionType.Z:
						curScale.z = scaleDirection.z;
						break;
				}

				if (!firstObjScalePoint.HasValue)
					firstObjScalePoint = targetItem.LocalScale;

				targetItem.LocalScale = curScale;

				if (!firstHitScalePoint.HasValue)
					firstHitScalePoint = targetItem.LocalScale;

				targetItem.LocalScale += firstObjScalePoint.Value - firstHitScalePoint.Value;
				targetItem.LocalScale = targetTransformer.CheckCustomObjectScale(targetItem.LocalScale);
				//targetItem.CalibrateScaleByUnit(targetTransformer.ScaleCorrectedUnit);
			}

			if (!isPointerIn)
			{
				curScaleDirection = DirectionType.None;
				firstHitScalePoint = null;
				firstObjScalePoint = null;
			}
		}


		public IDeformableObject GetTargetObject()
		{
			return targetTransformer.CustomObjectInterface;
		}

		protected Vector3 GetUpVector(DirectionType directionType)
		{
			switch (directionType)
			{
				case DirectionType.X: return Vector3.right;
				case DirectionType.Y: return Vector3.up;
				case DirectionType.Z: return Vector3.forward;
			}
			return Vector3.zero;
		}

		public void UpdateCoordinatesByDirectionMode()
		{
			this.posDirection.SetActive(false);
			this.rotDirection.SetActive(false);
			this.scaleDirection.SetActive(false);
			switch (curDirectionMode)
			{
				case DirectionMode.Position:
					posDirection.SetActive(true);
					break;
				case DirectionMode.Rotation:
					rotDirection.SetActive(true);
					break;
				case DirectionMode.Scale:
					scaleDirection.SetActive(true);
					break;
			}
		}

		public void InitCoordinatesOrReset(ObjectTransformer targetTransformer = null)
		{
			this.targetTransformer = targetTransformer;

			this.gameObject.SetActive(targetTransformer != null);
		}

		public DirectionMode GetCurDirectionMode()
		{
			return curDirectionMode;
		}
		public void SetCurDirectionMode(DirectionMode directionMode)
		{
			curDirectionMode = directionMode;
		}

		public void UpdateLocation()
		{
			transform.position = targetTransformer.CustomObjectInterface.Position;
			float ratio = viewScaleRate * Vector3.Distance(TargetCamera.transform.position, transform.position);
			transform.localScale = new Vector3(ratio, ratio, ratio);
		}

		public void SetCanvasCamera(Camera camera)
		{
			coordinatesCanvas.worldCamera = camera;
		}

		public Camera TargetCamera
		{
			get
			{
				return coordinatesCanvas.worldCamera;
			}
		}

	}

}
