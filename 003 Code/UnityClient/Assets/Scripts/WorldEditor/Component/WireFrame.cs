using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace NextReality.Asset
{
	public class WireFrame : MonoBehaviour
	{
		GameObject targetObject;

		public Transform boundingBox;
		public TMP_Text contentText;

		private void Awake()
		{
			boundingBox.GetComponent<MeshRenderer>().material.SetFloat("_WireThickness", 2.5f);
		}

		public WireFrame SetTargetObject(GameObject targetObject)
		{
			this.targetObject = targetObject;
			Vector3 pos = targetObject.transform.position;
			Vector3 ang = targetObject.transform.eulerAngles;
			Vector3 scl = targetObject.transform.localScale;

			Collider[] colliders = targetObject.GetComponentsInChildren<Collider>();

			if (colliders.Length > 0)
			{
				targetObject.transform.eulerAngles = Vector3.zero;
				targetObject.transform.localScale = Vector3.one;

				Bounds bodyBounds = colliders[0].bounds;

				for (int i = 1; i < colliders.Length; i++)
				{
					bodyBounds.Encapsulate(colliders[i].bounds);

				}

				this.transform.SetParent(targetObject.transform);
				transform.localScale = Vector3.one;
				transform.localPosition = Vector3.zero;
				transform.localEulerAngles = Vector3.zero;

				boundingBox.localPosition = targetObject.transform.InverseTransformPoint(bodyBounds.center);
				boundingBox.localEulerAngles = Vector3.zero;
				//boundingBox.localScale = targetObject.transform.InverseTransformVector(bodyBounds.size);
				//boundingBox.localScale = targetObject.transform.InverseTransformDirection(bodyBounds.size);
				boundingBox.localScale = bodyBounds.size;

				contentText.transform.localPosition = boundingBox.localPosition;

				targetObject.transform.eulerAngles = ang;
				targetObject.transform.localScale = scl;

			}

			return this;

		}

		public void SetText(string text)
		{
			contentText.text = text.Trim();
		}

		void LateUpdate()
		{
			if (Managers.Camera.mainGameCamera)
			{
				contentText.transform.eulerAngles = Managers.Camera.mainGameCamera.transform.eulerAngles;
			}
		}

		public void SetActive(bool active)
		{
			gameObject.SetActive(active);
		}
	}

}
