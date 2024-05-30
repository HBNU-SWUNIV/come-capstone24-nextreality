using NextReality.Asset.UI;
using NextReality.Game.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public interface IInteractable
	{
		bool interactable { get; set; }
	}

	public class ObjectTransformer : MonoBehaviour
	{
		IDeformableObject targetCustomObject;
		[SerializeField] ObjectTransformCursor cursor;

		protected float posCorrectedUnit = 0;
		protected float rotCorrectedUnit = 0;
		protected float scaleCorrectedUnit = 0;

		RectTransform rectTransform;
		ObjectEditorController objectEditorController;

		[SerializeField] private SelectableColoredButton positionButton;
		[SerializeField] private SelectableColoredButton rotationButton;
		[SerializeField] private SelectableColoredButton scaleButton;
		[SerializeField] private SelectableColoredButton deleteButton;
		[SerializeField] private SelectableColoredButton closeButton;

		private SelectableColoredButton[] transformButtons;

		private List<IInteractable> interactableButtonList = new List<IInteractable>();
		private IInteractable[] interactableButtons = new IInteractable[3];

		private SidePopUpUI sidePopUpUI;

		// Start is called before the first frame update
		void Start()
		{
			sidePopUpUI = GetComponent<SidePopUpUI>();

			positionButton.onClick.AddListener(() => this.SetDirectionMode(ObjectTransformCursor.DirectionMode.Position));
			rotationButton.onClick.AddListener(() => this.SetDirectionMode(ObjectTransformCursor.DirectionMode.Rotation));
			scaleButton.onClick.AddListener(() => this.SetDirectionMode(ObjectTransformCursor.DirectionMode.Scale));
			deleteButton.onClick.AddListener(() => Managers.ObjectEditor.RemoveCustomObjectEvnet(TargetCustomObject));
			closeButton.onClick.AddListener(() => Managers.ObjectEditor.SendDeselect());

			transformButtons = new SelectableColoredButton[3];
			transformButtons[0] = positionButton;
			transformButtons[1] = rotationButton;
			transformButtons[2] = scaleButton;

			interactableButtonList.Add(positionButton);
			interactableButtonList.Add(rotationButton);
			interactableButtonList.Add(scaleButton);
			interactableButtonList.Add(deleteButton);
			interactableButtonList.Add(closeButton);

			SelectTransfromButton(-1);
		}

		//// Update is called once per frame
		//void Update()
		//{
		//	SyncTransformerTransform();
		//}


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
			SelectTransfromButton(-1);
			cursor.InitCoordinatesOrReset();

			sidePopUpUI.PopUp(false);
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
				this.InitInterface();
				//this.SyncTransformerTransform();
				//if (!this.sendCoroutine) this.sendCoroutine = this.StartCoroutine(this.SendCustomObjectState());

				sidePopUpUI.PopUp(true);
			}
		}

		protected void InitInterface()
		{
			if (this.targetCustomObject != null)
			{
				this.cursor.InitCoordinatesOrReset(this);
				SetDirectionMode(CurDirectionMode);
			}
		}

		protected void SetDirectionMode(ObjectTransformCursor.DirectionMode directionMode)
		{
			CurDirectionMode = directionMode;
			
			switch (CurDirectionMode)
			{
				case ObjectTransformCursor.DirectionMode.Position:
					SelectTransfromButton(0);
					break;
				case ObjectTransformCursor.DirectionMode.Rotation:
					SelectTransfromButton(1);
					break;
				case ObjectTransformCursor.DirectionMode.Scale:
					SelectTransfromButton(2);
					break;
				case ObjectTransformCursor.DirectionMode.None:
					SelectTransfromButton(-1);
					break;
			}

			this.cursor.UpdateCoordinatesByDirectionMode();
		}

		public void SelectTransfromButton(int index)
		{
			for(int i = 0; i < transformButtons.Length; i++)
			{
				if (i == index) transformButtons[i].Select(true);
				else transformButtons[i].Select(false);
			}

			interactableButtonList.ForEach(button => { button.interactable = targetCustomObject != null; });
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
		public Vector3 CurMinPosition { get { return GetMinMax(false); } }
		public Vector3 CurMaxPosition { get { return GetMinMax(true); } }
		public Vector3 CurMinScale { get { return new Vector3(0.1f,0.1f,0.1f); } }
		public Vector3 CurMaxScale { get { return new Vector3(2f, 2f, 2f); } }

		public ObjectTransformCursor.DirectionMode CurDirectionMode
		{
			get { return cursor.GetCurDirectionMode(); }
			set { cursor.SetCurDirectionMode(value); }
		}


		public Vector3 GetMinMax(bool command)
		{
			float x = (Managers.Map.mapInfo.mapSize.horizontal) / 2;
			float y = (Managers.Map.mapInfo.mapSize.height);
			float z = (Managers.Map.mapInfo.mapSize.vertical) / 2;

			if (command)
			{
				return new Vector3(x, y, z);
			}
			else
			{
				return - new Vector3(x, 0, z);
			}
		}
	}

}

