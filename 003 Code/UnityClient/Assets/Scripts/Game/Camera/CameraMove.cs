using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
	public class CameraMove : MonoBehaviour
	{
		public float sensitivity = 200f; // ���콺 �ΰ���
		float rotationX;
		float rotationY;

		GameObject parentCharacter;
		CharacterMove characterMove;

		private void Awake()
		{
			// �θ� ��ü (ĳ����) ��������
			parentCharacter = this.transform.parent.gameObject;
			// ĳ������ sensitivity�� �״�� ������
			characterMove = parentCharacter.GetComponent<CharacterMove>();
		}

		void Update()
		{
			if (Managers.Camera.CursorLock)
			{
				CameraRotationY();
				CameraRotationX();
			}
		}

		void CameraRotationY()
		{

			float mouseY = Input.GetAxis("Mouse Y");

			rotationY += mouseY * sensitivity * Time.deltaTime;

			Vector3 parentAngle = parentCharacter.transform.eulerAngles;

			// ī�޶� ��, �Ʒ��� ȸ����ų �� ������ ����
			rotationY = rotationY > 89f ? 89f : rotationY;
			rotationY = rotationY < -89f ? -89f : rotationY;

			transform.eulerAngles = new Vector3(-rotationY, parentAngle.y, 0);
		}

		void CameraRotationX()
		{
			float mouseX = Input.GetAxis("Mouse X");

			rotationX += mouseX * sensitivity * Time.deltaTime;

			characterMove.CharacterRotationX(rotationX);
		}

	}

}

