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

		bool cursorLock = true;

		GameObject parentCharacter;
		CharacterMove characterMove;

		private void Awake()
		{
			// �θ� ��ü (ĳ����) ��������
			parentCharacter = this.transform.parent.gameObject;
			// ĳ������ sensitivity�� �״�� ������
			characterMove = parentCharacter.GetComponent<CharacterMove>();
		}

		void Start()
		{
			CursorOnOff(true);
		}

		void Update()
		{
			if (cursorLock)
			{
				CameraRotationY();
				CameraRotationX();
			}

			if (Input.GetKeyDown(KeyCode.Tab))
			{
				CursorOnOff(!cursorLock);
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

		void CursorOnOff(bool _cursorLock)
		{
			// cursor on -> off
			if (_cursorLock)
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			// cursor off -> on
			else
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			cursorLock = _cursorLock;
		}
	}

}

