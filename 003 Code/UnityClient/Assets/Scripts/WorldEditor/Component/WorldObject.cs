using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace NextReality.Asset
{
	public class WorldObject : MonoBehaviour, IDeformableObject
{

		protected bool isEnabled = true;
		protected Bounds boyBounds;

		WireFrame wireFrame;

		public Vector3 Position
		{
			get { return this.transform.position; }
			set { this.transform.position = value; }
		}

		public Quaternion Rotation
		{
			get { return this.transform.rotation; }
			set { this.transform.rotation = value; }
		}

		public Vector3 EulerAngles
		{
			get { return this.transform.eulerAngles; }
			set { this.transform.eulerAngles = value; }
		}

		public Vector3 LossyScale
		{
			get { return this.transform.lossyScale; }
		}

		public Vector3 LocalPosition
		{
			get { return this.transform.localPosition; }
			set { this.transform.localPosition = value; }
		}

		public Quaternion LocalRotation
		{
			get { return this.transform.localRotation; }
			set { this.transform.localRotation = value; }
		}

		public Vector3 LocalEulerAngles
		{
			get { return this.transform.localEulerAngles; }
			set { this.transform.localEulerAngles = value; }
		}

		public Vector3 LocalScale
		{
			get { return this.transform.localScale; }
			set { this.transform.localScale = value; }
		}


		public Vector3 Up
		{
			get{ return transform.up; }
		}

		public Vector3 Forward
		{
			get { return transform.forward; }
		}

		public Vector3 Right
		{
			get { return transform.right; }
		}

		public bool IsEnabled
		{
			get { return isEnabled; }
		}

		public void CalibrateEulerAnglesByUnit(float unit)
		{
			throw new System.NotImplementedException();
		}

		public void CalibratePositionByUnit(float unit)
		{
			throw new System.NotImplementedException();
		}

		public void CalibrateScaleByUnit(float unit)
		{
			throw new System.NotImplementedException();
		}

		public Vector3 InverseTransformDirection(Vector3 dir)
		{
			return transform.InverseTransformDirection(dir);
		}

		public Vector3 InverseTransformPoint(Vector3 point)
		{
			return transform.InverseTransformPoint(point);
		}

		public void Rotate(Vector3 value)
		{
			transform.Rotate(value);
		}


		public void SetDisable()
		{
			isEnabled = false;
		}

		public Vector3 TransformDirection(Vector3 dir)
		{
			return transform.TransformDirection(dir);
		}

		public Vector3 TransformPoint(Vector3 point)
		{
			return transform.TransformPoint(point);
		}

		public Bounds BodyBounds
		{
			get
			{
				if(boyBounds.size.magnitude == 0)
				{
					var cols = Colliders;

					if (colliders.Count == 0) return new Bounds();
					Vector3 tempEuler = EulerAngles;
					EulerAngles = Vector3.zero;
					boyBounds = cols[0].TargetCollider.bounds;

					for (int i = 1; i < Colliders.Length; i++)
					{
						boyBounds.Encapsulate(Colliders[i].TargetCollider.bounds);

					}

					EulerAngles = tempEuler;
				}

				return boyBounds;
			}
		}

		public List<IDeformableObjectCollider> colliders =	new List<IDeformableObjectCollider>();

        public IDeformableObjectCollider[] Colliders
		{
			get
			{
                return colliders.ToArray();
            }
		}

		protected virtual void Awake()
		{
			colliders.Clear();
			boyBounds = new Bounds(transform.position, Vector3.zero);
		}

		protected virtual void Start()
		{
			WireFrame?.SetActive(false);
		}

		protected void InitWireFrame()
		{
			if (WireFrame == null)
			{
				wireFrame = Instantiate(Managers.ObjectEditor.wireFramePrefab).SetTargetObject(gameObject);
			}
			wireFrame.SetActive(false);
		}

		public WireFrame WireFrame
		{
			get
			{
				if (wireFrame == null) wireFrame = gameObject.GetComponentInChildren<WireFrame>(true);
				return wireFrame;
			}
		}
	}

}