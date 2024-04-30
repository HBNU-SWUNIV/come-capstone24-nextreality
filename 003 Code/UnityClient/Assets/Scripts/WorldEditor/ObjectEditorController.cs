using NextReality.Asset.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace NextReality.Asset
{
	public class ObjectEditorController : MonoBehaviour
	{

		[SerializeField] private Canvas editorCanvas;
		private RectTransform editorCanvasRect;

		[SerializeField] private ObjectTransformer transformer;

		public readonly float positionUnit = 0;
		public readonly float rotationRatio = 0;
		public readonly float scaleUnit = 0;

		private Vector3? inputPosition;
		private GameObject firstSelectObject;
		private bool isFirstSelected = false;
		public LayerMask objectLayer;

		protected IDeformableObject selectedObject = null;
		protected bool isSelectedObjectLocal = false;
		private float inputTime = 0;

		private static ObjectEditorController instance;

		private void Awake()
		{
			if (instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}

			instance = this;
		}

		public static ObjectEditorController Instance
		{
			get { return instance; }
		}
		// Start is called before the first frame update
		void Start()
		{
			transformer.InitTransformer(this);
		}

		// Update is called once per frame
		void Update()
		{
		}

		private IEnumerator SelectCustomObject()
		{
			while (EditorCanvas)
			{
				InputSelectObject();
				//DrawBox();
				yield return null;
			}
		}

		private bool IsValidInput
		{
			get
			{
				if (Input.mousePresent)
				{
					return Input.GetMouseButton(0);
				}
				else
				{
					return Input.touchCount == 1;
				}
			}
		}

		private void InputSelectObject()
		{
			if (!IsValidInput)
			{
				if (inputPosition != null && inputTime < 0.1f)
				{
					if (firstSelectObject != null)
					{
						OnPointerClick(
							inputPosition.Value,
							obj =>
							{
								if (obj == firstSelectObject)
								{
									if (obj.TryGetComponent<IDeformableObjectCollider>(out IDeformableObjectCollider objectCollider))
									{
										SelectObjectEvent(objectCollider.TargetObject);
									}
								}
							},
							() =>
							{
								if (selectedObject != null && !isSelectedObjectLocal)
								{
									EndTransformObject();
								}
							}
						);
						inputPosition = null;
					}
					else
					{
						if (selectedObject != null && !isSelectedObjectLocal)
						{
							EndTransformObject();
						}
					}
				}
				isFirstSelected = false;
				firstSelectObject = null;
			}
			else
			{
				if (!isFirstSelected)
				{
					if (!Managers.Input.IsPointerOverUIObjectAll())
					{
						inputPosition = Input.mousePosition;
						OnPointerClick(
							inputPosition.Value,
							obj => { firstSelectObject = obj; },
							() => { firstSelectObject = null; }
						);
					}
					inputTime = 0;
					isFirstSelected = true;
				}
				else
				{
					inputTime += Time.deltaTime;
				}
			}
		}

		private void OnPointerClick(Vector3 mousePosition, Action<GameObject> success, Action fail)
		{
			Managers.Camera.Raycast(
				new Vector2(inputPosition.Value.x, inputPosition.Value.y),
				100,
				(isHitted, ray) =>
				{
					if (isHitted && ray.collider?.CompareTag("Object") == true)
					{
						success(ray.collider.gameObject);
					}
					else
					{
						fail();
					}
				},
				objectLayer
			);
		}

		protected void SelectObjectEvent(IDeformableObject assetObject)
		{
			this.TransformObject(assetObject);
		}

		void RemoveCustomObjectEvnet(IDeformableObject customObject)
		{

		}

		protected void TransformObject(IDeformableObject assetObject)
		{
			if (this.transformer.TargetCustomObject != assetObject && assetObject.IsEnabled)
			{
				this.transformer.TransformObject(assetObject);
			}
		}

		public void EndTransformObject()
		{
			this.transformer.EndTransformObject();

		}

		public Canvas EditorCanvas { get { return editorCanvas; } }
		public RectTransform EditorCanvasRect { get { if (editorCanvasRect == null) editorCanvasRect = editorCanvas.GetComponent<RectTransform>(); return editorCanvasRect; } }

	}

}
