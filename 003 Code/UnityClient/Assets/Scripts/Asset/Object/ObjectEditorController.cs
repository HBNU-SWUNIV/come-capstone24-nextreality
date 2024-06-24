using NextReality.Asset.UI;
using NextReality.Data.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
		private float inputTime = 0;

		private Coroutine inputSelectCoroutine;

		// key: object_id, value: userId
		private Dictionary<string, string> assetOwnerMap = new Dictionary<string, string>();

		public WireFrame wireFramePrefab;

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

			Managers.Camera.AddCursorEventListener((cursorLock) =>
			{
				if (cursorLock)
				{
					if (inputSelectCoroutine != null)
					{
						StopCoroutine(inputSelectCoroutine);
						inputSelectCoroutine = null;
					}
				}
				else
				{
					if (inputSelectCoroutine == null)
						inputSelectCoroutine = StartCoroutine(SelectCustomObject());
				}
			});

			Managers.Client.AddJoinLocalPlayerEvent((player, userId) =>
			{
				editorCanvas.renderMode = RenderMode.ScreenSpaceCamera;
				editorCanvas.worldCamera = Managers.Camera.mainGameCamera.uiCam;
			});
		}


		private IEnumerator SelectCustomObject()
		{
			while (true)
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
			if (Input.GetMouseButtonDown(0))
			{
				if (!Managers.Input.IsPointerOverUIObjectAll())
				{
					OnPointerClick(
					Input.mousePosition,
					obj =>
					{
						//if (selectedObject != null)
						//{
						//    SendDeselect();
						//}

						if (obj.TryGetComponent<IDeformableObjectCollider>(out IDeformableObjectCollider objectCollider))
						{
							//Debug.Log(objectCollider.TargetObject.gameObject.name);
							if (objectCollider != null)
							{
								if (objectCollider.TargetObject != null)
								{
									SendSelect(objectCollider.TargetObject.gameObject);
								}

							} // ���� Ȯ��

							//SelectObjectEvent(objectCollider.TargetObject);
						}
					},
					() =>
					{
						if (selectedObject != null)
						{
							SendDeselect();
							//EndTransformObject();
						}
					}
				);
				}
			}
			else
			{

			}
		}

		private void OnPointerClick(Vector3 mousePosition, Action<GameObject> success, Action fail)
		{
			Managers.Camera.Raycast(
				new Vector2(mousePosition.x, mousePosition.y),
				100,
				(isHitted, ray) =>
				{
					if (isHitted && ray.collider) //&& ray.collider.gameObject.layer == (int)Layers.EditableObject)
					{
						Debug.Log("Raycast Hitted");
						//Debug.Log(ray.collider.gameObject.ToString());
						success(ray.collider.gameObject);
					}
					else
					{
						fail();
					}
				},
				objectLayer
			);
			//Debug.DrawLine(new Vector2(mousePosition.x, mousePosition.y), Camera.main.ScreenPointToRay(mousePosition).direction, Color.red, 1);
		}

		public void SelectObjectEvent(IDeformableObject assetObject)
		{
			selectedObject = assetObject;
			this.TransformObject(assetObject);
		}

		public void SelectObjectEvent(string objectId)
		{

			selectedObject = Managers.Gltf.GetAssetObjectById(objectId);
			if (selectedObject != null)
			{
				this.TransformObject(selectedObject);
			}
		}

		public void RemoveCustomObjectEvnet(IDeformableObject customObject)
		{
			if (customObject == null) return;

			AssetDeleteSchema schema = new AssetDeleteSchema();

			schema.obj_id = customObject.gameObject.name;

			Managers.Network.SendMessage(schema.StringifyData());
			EndTransformObject();
		}

		public void RemoveCustomObjectByObjID(string obj_id)
		{
			UnSetAssetOwner(obj_id);

			AssetObject customObject = Managers.Gltf.GetAssetObjectById(obj_id);
			if (customObject == null) return;

			Managers.Gltf.RemoveAssetObjectById(obj_id);

			Destroy(customObject.gameObject);

			EndTransformObject();
		}

		protected void TransformObject(IDeformableObject assetObject)
		{
			if (this.transformer.TargetCustomObject != assetObject && assetObject.IsEnabled)
			{
				// Debug.Log($"[ObjectEditorController] Select:    {assetObject.gameObject.name}");
				this.transformer.TransformObject(assetObject);
			}
		}

		public void EndTransformObject()
		{
			selectedObject = null;
			this.transformer.EndTransformObject();
		}

		public Canvas EditorCanvas { get { return editorCanvas; } }
		public RectTransform EditorCanvasRect { get { if (editorCanvasRect == null) editorCanvasRect = editorCanvas.GetComponent<RectTransform>(); return editorCanvasRect; } }

		public void SendSelect(GameObject obj)
		{
			Managers.Network.SendMessage(selectMessage(obj));
		}

		public string selectMessage(GameObject obj)
		{
			AssetSelectSchema schema = new AssetSelectSchema();
			schema.objectId = obj.name;
			string message = schema.StringifyData();

			Debug.Log("[ObjectEditorController] Send:    " + message);

			return message;
		}

		public void SendDeselect()
		{
			Managers.Network.SendMessage(deselectMessage);
		}

		public string deselectMessage
		{
			get
			{
				AssetDeselectSchema schema = new AssetDeselectSchema();
				schema.objectId = selectedObject.gameObject.name;
				string message = schema.StringifyData();

				Debug.Log("[ObjectEditorController] Send:    " + message);

				return message;
			}
		}

		public void SetAssetOwner(string obj_id, string userId)
		{
			try
			{
				List<string> removeObjectId = new List<string>();
				foreach (var item in assetOwnerMap)
				{
					if (item.Value == userId)
						if (obj_id == item.Key) continue;
					removeObjectId.Add(item.Key);
				};

				foreach (var item in removeObjectId)
				{
					assetOwnerMap.Remove(item);
				}

				var assetObject = Managers.Gltf.GetAssetObjectById(obj_id);
				if (assetObject != null)
				{
					if (assetOwnerMap.ContainsValue(obj_id))
					{
						assetOwnerMap[obj_id] = userId;
					} else
					{
						assetOwnerMap.Add(obj_id, userId);
					}
				}

			}
			catch
			{

			}

			SetAssetOwnerBoundingBox();


		}

		public void UnSetAssetOwner(string obj_id)
		{
			assetOwnerMap.Remove(obj_id);
			SetAssetOwnerBoundingBox();
		}

		public void SetAssetOwnerBoundingBox()
		{
			foreach (var item in Managers.Gltf.objInstances)
			{
				item.Value?.WireFrame?.SetActive(false);

			}

			foreach (var item in assetOwnerMap)
			{
				var assetObject = Managers.Gltf.GetAssetObjectById(item.Key);
				if (assetObject != null)
				{
					assetObject.WireFrame.SetActive(true);
					assetObject.WireFrame.SetText(item.Value);
				}
			}
			Debug.LogFormat("[ObjectEditor] ownerMap: {0}", string.Join(",", assetOwnerMap.ToArray().Select((item => item.Key + ":" + item.Value))));
		}

	}

}
