using UnityEngine;

namespace NextReality.Asset
{
	public interface IDeformableObject
	{
		IDeformableObjectCollider[] Colliders { get; }
		Transform transform { get; }
		GameObject gameObject { get; }

		Vector3 Position { get; set; }
		Quaternion Rotation { get; set; }
		Vector3 EulerAngles { get; set; }
		Vector3 LossyScale { get; }
		Vector3 LocalPosition { get; set; }
		Quaternion LocalRotation { get; set; }
		Vector3 LocalEulerAngles { get; set; }
		Vector3 LocalScale { get; set; }

		//Vector3 ObjPosition { get; }
		//Quaternion ObjRotation { get; }
		//Vector3 ObjEulerAngles { get; }
		//Vector3 ObjLossyScale { get; }
		//Vector3 ObjLocalPosition { get; }
		//Quaternion ObjLocalRotation { get; }
		//Vector3 ObjLocalEulerAngles { get; }
		//Vector3 ObjLocalScale { get; }

		Vector3 Up { get; }
		Vector3 Forward { get; }
		Vector3 Right { get; }

		bool IsEnabled { get;}
		void SetDisable();

		void Rotate(Vector3 value);
		Vector3 TransformDirection(Vector3 dir);
		Vector3 TransformPoint(Vector3 point);
		Vector3 InverseTransformDirection(Vector3 dir);
		Vector3 InverseTransformPoint(Vector3 point);
		void CalibratePositionByUnit(float unit);
		void CalibrateEulerAnglesByUnit(float unit);
		void CalibrateScaleByUnit(float unit);
	}

}
