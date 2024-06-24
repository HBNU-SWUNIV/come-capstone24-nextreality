using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality.Game
{
	public class GameCharacterManager : MonoBehaviour
	{
		[Header("Character")]
		public float moveSpeed = 1f; // 기본 이동속도
		public float jumpSpeed = 1f; // 점프 속도
		public float runSpeed = 2f; // 달리기 이동속도 배수
		public float gravity = -0.2f; // 중력
		public float stopInterval = 0.03f;

		[Header("Camera")]
		public GameCamera gameCameraPrefab;
		public float sensitivity = 2f; // 마우스 민감도
		public float cameraYOffset = 0.2f; // 카메라 부착 위치
		public float cameraZOffset = 0.25f; // 카메라 부착 위치
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
			float input_h = Input.GetAxis("Horizontal"); // A, D키 입력
			float input_v = Input.GetAxis("Vertical"); // W, S키 입력

			Vector3 moveDirection = new Vector3(input_h, 0, input_v).normalized;
			moveDirection = localChacter.transform.TransformDirection(moveDirection);

			float speed = moveSpeed;

			if (Input.GetKey(KeyCode.LeftShift)) // 좌 쉬프트 = 달리기
			{
				speed = runSpeed;
			}

			if (!localChacter.IsJumped) // 캐릭터가 땅에 붙어있을 때
			{
				if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바 누르면 점프
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


