using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public delegate void ObjectTransformAction(IDeformableObject obj, PointerEventData eventData, bool isPointerIn);
	public abstract class ObjectDirectionBase : MonoBehaviour
	{
		protected ObjectTransformCursor cursor;
		public readonly bool isConstraint = false;
		// Start is called before the first frame update
		void Update()
		{
			LimitRotation();
			RenderDirection();
		}

		protected void LimitRotation()
		{
			if (!this.isConstraint || TargetObject == null) return;
			this.transform.rotation = TargetObject.Rotation;
		}

		protected abstract void RenderDirection();

		public void SetCoordinatesObject(ObjectTransformCursor _cursor)
		{
			cursor = _cursor;
		}

		public abstract void InitEvent(
			ObjectTransformAction[] eventXYZ
		);

		protected void Drag(PointerEventData data, ObjectTransformAction actionEvent, bool isPointerIn = true)
		{
			if (TargetObject != null)
			{

				actionEvent(TargetObject, data, isPointerIn);
			}
		}

		public void SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}
		public bool IsActive
		{
			get { return gameObject.activeInHierarchy; }
		}
		protected IDeformableObject TargetObject
		{
			get { return cursor.GetTargetObject(); }
		}
		protected Camera CanvasCamera
		{
			get { return this.cursor.TargetCamera; }

		}
	}


}

