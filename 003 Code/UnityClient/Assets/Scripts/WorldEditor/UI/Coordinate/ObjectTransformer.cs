using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Asset.UI
{
	public class ObjectTransformer : MonoBehaviour
	{
		IDeformableObject targetCustomObject;
		[SerializeField] ObjectTransformCursor cursor;

		protected float posCorrectedUnit = 0;
		protected float rotCorrectedUnit = 0;
		protected float scaleCorrectedUnit = 0;

		RectTransform rectTransform;
		ObjectEditorController objectEditorController;
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			SyncTransformerTransform();
		}


		public void InitTransformer(ObjectEditorController _objectEditorController)
		{
			objectEditorController = _objectEditorController;
			if (objectEditorController.positionUnit > 0) this.posCorrectedUnit = 1 / objectEditorController.positionUnit;
			if (objectEditorController.rotationRatio > 0) this.rotCorrectedUnit = 360 / objectEditorController.rotationRatio;
			if (objectEditorController.scaleUnit > 0) this.scaleCorrectedUnit = 1 / objectEditorController.scaleUnit;
		}

		public IDeformableObject CustomObjectInterface
		{
			get { return targetCustomObject; }
		}

		public void EndTransformObject()
		{
			targetCustomObject = null;
			gameObject.SetActive(false);
		}
		public void DeleteCustomObject()
		{
			if (IsOnActive)
			{
				targetCustomObject.SetDisable();

				//FixeMe: 삭제 동기화
			}
		}

		public void AddCustomObjectPosition(Vector3 addVector)
		{
			Vector3 curPosition = targetCustomObject.LocalPosition + addVector;
			targetCustomObject.LocalPosition = CheckCustomObjectPostion(curPosition);
			//targetCustomObject.CalibratePositionByUnit(posCorrectedUnit);
		}

		public Vector3 CheckCustomObjectPostion(Vector3 position)
		{
			Vector3 curPosition = position;

			if (curPosition.x > CurMaxPosition.x) curPosition.x = CurMaxPosition.x;
			else if (curPosition.x < CurMinPosition.x) curPosition.x = CurMinPosition.x;

			if (curPosition.y > CurMaxPosition.y) curPosition.y = CurMaxPosition.y;
			else if (curPosition.y < CurMinPosition.y) curPosition.y = CurMinPosition.y;

			if (curPosition.z > CurMaxPosition.z) curPosition.z = CurMaxPosition.z;
			else if (curPosition.z < CurMinPosition.z) curPosition.z = CurMinPosition.z;

			return curPosition;
		}

		public void AddCustomObjectScale(Vector3 addVector)
		{
			Vector3 curScale = targetCustomObject.LocalScale + addVector;
			targetCustomObject.LocalScale = CheckCustomObjectScale(curScale);
			//targetCustomObject.CalibrateScaleByUnit(scaleCorrectedUnit);
		}

		public Vector3 CheckCustomObjectScale(Vector3 scale)
		{
			Vector3 curScale = scale;

			if (curScale.x > CurMaxScale.x) curScale.x = CurMaxScale.x;
			else if (curScale.x < CurMinScale.x) curScale.x = CurMinScale.x;

			if (curScale.y > CurMaxScale.y) curScale.y = CurMaxScale.y;
			else if (curScale.y < CurMinScale.y) curScale.y = CurMinScale.y;

			if (curScale.z > CurMaxScale.z) curScale.z = CurMaxScale.z;
			else if (curScale.z < CurMinScale.z) curScale.z = CurMinScale.z;

			return curScale;
		}
		public void SyncTransformerTransform()
		{
			if (targetCustomObject != null)
			{
				Vector2? showVector = Managers.Camera.GetUIPositionByWorldObject(this.targetCustomObject.Position, Managers.ObjectEditor.EditorCanvasRect);
				if (showVector != null)
				{
					this.RectTransform.anchoredPosition = showVector.Value;
				}
			}
		}

		public void TransformObject(IDeformableObject targetObject)
		{
			if (targetObject != null)
			{
				this.targetCustomObject = targetObject;
				this.gameObject.SetActive(true);
				this.InitInterface();
				this.SyncTransformerTransform();
				//if (!this.sendCoroutine) this.sendCoroutine = this.StartCoroutine(this.SendCustomObjectState());
			}
		}

		protected void InitInterface()
		{
			if (this.targetCustomObject != null)
			{
				this.cursor.InitCoordinatesOrReset(this);
				SetDirectionMode();
			}
		}

		protected void SetDirectionMode(ObjectTransformCursor.DirectionMode directionMode = ObjectTransformCursor.DirectionMode.None)
		{
			if (directionMode == ObjectTransformCursor.DirectionMode.None) CurDirectionMode = directionMode;
			//this.transformButton.gameObject.SetActive(false);
			//this.rotationButton.gameObject.SetActive(false);
			//this.scaleButton.gameObject.SetActive(false);
			//this.colorButton.gameObject.SetActive(false);
			//this.rotationResetButton.gameObject.SetActive(false);
			switch (CurDirectionMode)
			{
				case ObjectTransformCursor.DirectionMode.Position:
					//this.transformButton.gameObject.SetActive(true);
					break;
				case ObjectTransformCursor.DirectionMode.Rotation:
					//this.rotationButton.gameObject.SetActive(true);
					//this.rotationResetButton.gameObject.SetActive(true);
					break;
				case ObjectTransformCursor.DirectionMode.Scale:
					//this.scaleButton.gameObject.SetActive(true);
					break;
				case ObjectTransformCursor.DirectionMode.None:
					break;
					//this.transformButton.gameObject.SetActive(true);
					//this.rotationButton.gameObject.SetActive(true);
					//this.scaleButton.gameObject.SetActive(true);
			}

			this.cursor.UpdateCoordinatesByDirectionMode();
		}

		public bool IsOnActive { get { return this.gameObject.activeInHierarchy && this.targetCustomObject != null; } }
		public RectTransform RectTransform 
		{
			get 
			{
				if(rectTransform == null)
				{
					rectTransform = GetComponent<RectTransform>();
				}

				return rectTransform;
			} 
		}

		public IDeformableObject TargetCustomObject { get { return targetCustomObject; } }

		public float PosCorrectedUnit { get { return posCorrectedUnit; } }
		public float RotCorrectedUnit { get { return rotCorrectedUnit; } }
		public float ScaleCorrectedUnit { get { return scaleCorrectedUnit; } }

		//FiexMe
		public Vector3 CurMinPosition { get; }
		public Vector3 CurMaxPosition { get; }

		public Vector3 CurMinScale{ get; }
		public Vector3 CurMaxScale { get; }

		public ObjectTransformCursor.DirectionMode CurDirectionMode
		{
			get { return cursor.GetCurDirectionMode(); }
			set { cursor.SetCurDirectionMode(value); }
		}

	}

}

