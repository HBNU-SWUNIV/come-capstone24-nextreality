using GLTFast;
using NextReality.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace NextReality.Game
{
	public class ModelPreviewer : MonoBehaviour
	{

		GameObject targetObject;

		public Camera previewCamera;
		public Transform previewCameraArm;
		public Transform previewContainer;

		GltfImport gltfImport;

		string curHash;

		Bounds targetBounds;

		public float viewPaddingRatio = 1.2f;

		float curZoom = 1.0f;

		public float zoomConstraint = 1.1f;

		public CheckMeshStandard checkMeshStandard;

		public float CameraDistance
		{
			get
			{
				return (targetBounds.extents.magnitude * viewPaddingRatio) / (float)Mathf.Tan(previewCamera.fieldOfView/2 * Mathf.Deg2Rad);
			}
		}

		public float ObjectRadius
		{
			get
			{
				return targetBounds.extents.magnitude;
			}
		}

		public bool IsReadyTarget
		{
			get
			{
				return targetObject != null;
			}
		}

		// Start is called before the first frame update
		void Start()
		{
			ClearPreview();
		}

		static string ComputeSHA256(byte[] bytes)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				// 주어진 문자열의 해시를 계산합니다.
				byte[] hashValue = sha256.ComputeHash(bytes);

				// 바이트 배열을 문자열 형식으로 변환
				return BitConverter.ToString(hashValue).Replace("-", "");
			}
		}

		public void InstantiatePreviewObject(byte[] bytes)
		{
			AsyncInstantiateObject(bytes);
		}

		async void AsyncInstantiateObject(byte[] bytes)
		{
			string tempHash = ComputeSHA256(bytes);
			gltfImport = new GltfImport();
			curHash = tempHash;
			ClearPreview();
			bool isLoad = await gltfImport.LoadGltfBinary(bytes);
			if (!isLoad || curHash != tempHash) return;
			
			bool isGenerated = await gltfImport.InstantiateMainSceneAsync(previewContainer); // 에셋 적용
			if (!isGenerated || curHash != tempHash) return;
			previewContainer.GetChild(0).position = Vector3.zero;
            foreach (var item in previewContainer.GetComponentsInChildren<Transform>())
            {
				item.gameObject.layer = 7;
			}
			Renderer[] renderers = previewContainer.GetComponentsInChildren<Renderer>();
			if(renderers.Length > 0)
			{
				targetBounds = renderers[0].bounds;
				for (int i = 1; i< renderers.Length; i++)
				{
					targetBounds.Encapsulate(renderers[i].bounds);
					renderers[i].gameObject.layer = (int)Layers.PreviewObject;
				}
			}


			targetObject = previewContainer.GetChild(0)?.gameObject ?? null;

			checkMeshStandard.SetTargetObject(targetObject);

			InitCamera();
			SettingCamera(0, 0);

			Debug.LogFormat("[ModelPreiviewer] {0}: {1}", targetObject.name, ObjectRadius);
		}

		void SettingCamera(float x, float y)
		{

			if (x < 180f)
			{
				x = Mathf.Clamp(x, -1f, 89f);
			}
			else
			{
				x = Mathf.Clamp(x, 275f, 364f);
			}

			previewCameraArm.transform.eulerAngles = new Vector3(x, y, 0);
			previewCameraArm.rotation = Quaternion.Euler(x, y, 0);

		}

		public void RotateCamera(float mvX, float mvY)
		{
			Vector3 camAngle = previewCameraArm.localRotation.eulerAngles;
			SettingCamera(camAngle.x+mvY, camAngle.y + mvX);
		}

		public void ZoomCamera(float zoomDelta)
		{
			curZoom += zoomDelta;
			previewCamera.transform.localPosition = new Vector3(0, 0, -CameraDistance* (Mathf.Pow(zoomConstraint,curZoom) - zoomConstraint + 1));
		}

		void InitCamera()
		{
			previewCameraArm.transform.localPosition = targetBounds.center;
			previewCamera.transform.localPosition = new Vector3(0, 0, -CameraDistance);
			curZoom = 1.0f;
			previewCamera.gameObject.SetActive(true);
		}

		private void OnDrawGizmos()
		{
			if (targetBounds == null) return;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(targetBounds.center, ObjectRadius);
		}

		public void ClearPreview()
		{
			if(targetObject != null)
			{
				targetObject = null;
				checkMeshStandard.SetTargetObject(null);
				foreach (Transform obj in previewContainer)
				{
					Destroy(obj.gameObject);
				}
			}

			if(previewCamera.targetTexture != null)
			{
				previewCamera.targetTexture.Release();
			}
			previewCamera.gameObject.SetActive(false);

			targetBounds = new Bounds(previewContainer.position, Vector3.one);

			curZoom = 1.0f;
		}

		public bool IsAppropriateModel
		{
			get
			{
				return checkMeshStandard.IsContained;
			}
		}

	}

}
