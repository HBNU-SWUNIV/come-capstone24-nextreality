using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game
{
	public class CharacterMove : MonoBehaviour
	{
		public CharacterController characterController;
		public float moveSpeed = 10f; // 기본 이동속도
		public float jumpSpeed = 10f; // 점프 속도
		public float runSpeed = 2f; // 달리기 이동속도 배수
		public float gravity = -20f; // 중력


		float yVelocity = 0;

		void Start()
		{
		}
		void Update()
		{
			CharacterMoving();
		}


		void CharacterMoving()
		{
			float input_h = Input.GetAxis("Horizontal"); // A, D키 입력
			float input_v = Input.GetAxis("Vertical"); // W, S키 입력

			Vector3 moveDirection = new Vector3(input_h, 0, input_v);
			moveDirection = transform.TransformDirection(moveDirection);

			moveDirection *= moveSpeed;
			if (Input.GetKey(KeyCode.LeftShift)) // 좌 쉬프트 = 달리기
			{
				moveDirection *= runSpeed;
			}

			if (characterController.isGrounded) // 캐릭터가 땅에 붙어있을 때
			{
				yVelocity = 0;
				if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바 누르면 점프
				{
					yVelocity = jumpSpeed;
				}

			}

			yVelocity += (gravity * Time.deltaTime);

			moveDirection.y = yVelocity;

			characterController.Move(moveDirection * Time.deltaTime);
		}

		public void CharacterRotationX(float rotationX)
		{
			this.transform.eulerAngles = new Vector3(0, rotationX, 0);
		}

	}


}
