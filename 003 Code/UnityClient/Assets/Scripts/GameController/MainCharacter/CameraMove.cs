using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
	public class CameraMove : MonoBehaviour
	{
		public float sensitivity = 200f; // 마우스 민감도
		float rotationX;
		float rotationY;

		bool cursorLock = true;

		GameObject parentCharacter;
		CharacterMove characterMove;

		private void Awake()
		{
			// 부모 객체 (캐릭터) 가져오기
			parentCharacter = this.transform.parent.gameObject;
			// 캐릭터의 sensitivity를 그대로 가져옴
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

			// 카메라를 위, 아래로 회전시킬 때 제한을 설정
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

