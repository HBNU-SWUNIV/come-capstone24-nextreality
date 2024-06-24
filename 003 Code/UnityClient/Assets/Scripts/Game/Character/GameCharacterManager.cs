using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
	public class GameCharacterManager : MonoBehaviour
	{
		[Header("Character")]
		public float moveSpeed = 1f; // �⺻ �̵��ӵ�
		public float jumpSpeed = 1f; // ���� �ӵ�
		public float runSpeed = 2f; // �޸��� �̵��ӵ� ���
		public float gravity = -0.2f; // �߷�
		public float stopInterval = 0.03f;

		[Header("Camera")]
		public GameCamera gameCameraPrefab;
		public float sensitivity = 2f; // ���콺 �ΰ���
		public float cameraYOffset = 0.2f; // ī�޶� ���� ��ġ
		public float cameraZOffset = 0.25f; // ī�޶� ���� ��ġ
		float rotationX;
		float rotationY;

		// Start is called before the first frame update
		float yVelocity = 0;

		public GameAvatar localChacter;

		private static GameCharacterManager instance = null;

		void Awake()
		{
			if (instance != null && instance.gameObject != null)
			{
				Destroy(instance.gameObject);
			}

			instance = this;
		}

		public static GameCharacterManager Instance
		{
			get
			{
				if (null == instance)
				{
					return null;
				}
				return instance;
			}
		}

		public void InitLocalPlayer(GameAvatar player)
		{
			localChacter = player;
			localChacter.gameObject.tag = "Player";
			var gameCamera = Instantiate(gameCameraPrefab, localChacter.headSocket);
			gameCamera.transform.localPosition = new Vector3(0, cameraYOffset, cameraZOffset);

			Managers.Camera.mainGameCamera = gameCamera;

			StartCoroutine(StartGameInputKey());
		}

		IEnumerator StartGameInputKey ()
		{
			while (gameObject.activeInHierarchy)
			{
				if (Managers.Camera.CursorLock)
				{
					CameraRotationY();
					CameraRotationX();
				}
				CharacterMoving();

				yield return null;
			}

		}

		void CharacterMoving()
		{
			float input_h = Input.GetAxis("Horizontal"); // A, DŰ �Է�
			float input_v = Input.GetAxis("Vertical"); // W, SŰ �Է�

			Vector3 moveDirection = new Vector3(input_h, 0, input_v).normalized;
			moveDirection = localChacter.transform.TransformDirection(moveDirection);

			float speed = moveSpeed;

			if (Input.GetKey(KeyCode.LeftShift)) // �� ����Ʈ = �޸���
			{
				speed = runSpeed;
			}

			if (!localChacter.IsJumped) // ĳ���Ͱ� ���� �پ����� ��
			{
				if (Input.GetKeyDown(KeyCode.Space)) // �����̽��� ������ ����
				{
					localChacter.Jump(jumpSpeed);
				}

			}

			moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z) * speed;

			localChacter.MoveConsistently(moveDirection);

		}

		void CameraRotationY()
		{
			float mouseY = Input.GetAxis("Mouse Y");
			rotationY += mouseY * sensitivity;

			localChacter.CameraRotationY(rotationY);
		}

		void CameraRotationX()
		{
			float mouseX = Input.GetAxis("Mouse X");
			rotationX += mouseX * sensitivity;

			localChacter.CharacterRotationX(rotationX);
		}

	}
}


