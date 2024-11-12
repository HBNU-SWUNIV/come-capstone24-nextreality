using NextReality.Asset;
using NextReality.Data;
using NextReality.Data.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NextReality.Game
{
	public enum MoveState
	{
		Idle = 0,
		Move = 1,
		Jump = 2
	}

	[RequireComponent(typeof(CharacterController))]
	public class GameAvatar : MonoBehaviour
	{

		protected MoveState moveState = MoveState.Idle;

		protected CharacterController characterController;
		protected Rigidbody rigidBody;

		public Transform head;
		public Transform body;
		public Transform headSocket;

		protected Camera gameCamera;

		public Animator animator;

		public NickNameText nicknameText;

		protected Transform headIK;
		protected Transform headIKSocket;

		protected bool isJumped = false;

		protected Vector3 targetMovePosition;

		float yVelocity = 0;

		public UserInfo userInfo;

		protected bool isFirst = false;

		public DateTime lastMoveTime = DateTime.UtcNow;

		private float lastCallTime = 0;

		private bool isMoved = false;
		private Vector3 prevPosition;

		bool jumpTrigger = false;

		private string playerId;

		private void Awake()
		{
			if(characterController == null) characterController = GetComponent<CharacterController>();
			if(animator == null) animator = GetComponent<Animator>();
		}

		private void Start()
		{
			targetMovePosition = transform.position;
			if (IsLocal) StartCoroutine(ReplaceSpawnPoint());
		}

		public void Init(string playerId)
		{
			this.playerId = playerId;

			characterController = GetComponent<CharacterController>();
			animator = GetComponent<Animator>();
			if (headIK == null)
			{
				headIKSocket = new GameObject("headIKSocket").transform;
				headIKSocket.SetParent(headSocket);
				headIKSocket.localPosition = new Vector3(0, GameCharacterManager.Instance.cameraYOffset, 0);
				headIKSocket.localEulerAngles = Vector3.zero;

				headIK = new GameObject("headIK").transform;
				headIK.SetParent(headIKSocket);
				headIK.localRotation = Quaternion.identity;
				headIK.localPosition = new Vector3(0, 0, 2);
			}
		}

		// Start is called before the first frame update
		void Update()
		{
			CharacterMove();
			CheckIsMoved();
		}

		float jumpTime = 0;
		bool isIdle = true;
		float prevSpeed = 0;

		protected void CharacterMove()
		{
			float delta = intervalCallDeltaTime;
			Vector3 moveDirection = targetMovePosition - transform.position;

			if (isJumped) { 
				SetMoveState(MoveState.Jump);
				isIdle = false;
			}
			else
			{
				if(moveDirection.magnitude>(isJumped? 10:5))
				{
					Teleport(targetMovePosition, GetSchemaRotation());
				}
				if(moveDirection.magnitude < Managers.Chacacter.stopInterval)
				{
					Stop();
					if(isIdle) SetMoveState(MoveState.Idle);
					isIdle = true;
				} else
				{
					float tempSpeed = moveDirection.magnitude * delta / Time.deltaTime;
					SetRunState((prevSpeed+tempSpeed)*0.5f);
					SetMoveState(MoveState.Move);
					prevSpeed = tempSpeed;
					isIdle = false;
				}
			}

			moveDirection.y = yVelocity;
			moveDirection = new Vector3(moveDirection.x * delta, moveDirection.y * Time.deltaTime, moveDirection.z * delta);
			characterController.Move(moveDirection);
			//Debug.Log("[CharacterMove]" + this.gameObject.name + ": " + moveDirection + "\t" + Math.Round(delta, 7) + "/" + Math.Round(moveDirection.magnitude*delta, 7));

			if (characterController.isGrounded) // 캐릭터가 땅에 붙어있을 때
			{
				if (!isJumped) yVelocity = 0;
				isJumped = false;
				jumpTime = 0;
			}
			else
			{
				jumpTime += Time.deltaTime;
				yVelocity += (GameCharacterManager.Instance.gravity * Time.deltaTime);


				if(jumpTrigger)
				{
					PlayerJumpSchema schema = new PlayerJumpSchema(transform.position,GetSchemaRotation());
					Managers.Network.SendMessage(schema.StringifyData());
				}
			}

			jumpTrigger = false;

            //Debug.LogFormat("[CharacterMove : isGround: {0}, moveDirection: {1}, isJumped: {2}, velocityY: {3}", characterController.isGrounded, moveDirection, isJumped, yVelocity);
        }

		private float moveTime = 0;
		protected void CheckIsMoved()
		{
			if (transform.position.Equals(prevPosition)) isMoved = false;
			else isMoved = true;
			prevPosition = transform.position;

			if(isMoved)
			{
				moveTime += Time.deltaTime;
			} else
			{
				moveTime = 0;
			}

		}

		protected void SetAvatarPart()
		{
			if(headIKSocket != null) headIKSocket.localPosition = new Vector3(0, GameCharacterManager.Instance.cameraYOffset, 0);
		}

		protected void SetMoveState(MoveState state)
		{
			if (moveState != state)
			{
				moveState = state;
				animator.SetInteger("MoveState", ((int)state));
			}
		}

		protected void SetRunState(float runSpeed)
		{
			float ratio = (runSpeed - Managers.Chacacter.moveSpeed)/(Managers.Chacacter.runSpeed - Managers.Chacacter.moveSpeed);
			if(ratio>1f) ratio = 1f;
			else if(ratio<0f) ratio = 0f;
			animator.SetFloat("MoveSpeed", ratio);
		}

		public void Move(Vector3 targetMove)
		{
			targetMovePosition = targetMove;
			lastCallTime = Time.time;
			isFirst = true;
		}

		public void MoveConsistently(Vector3 moveDirection)
		{
			Move(transform.position + moveDirection);
		}

		public void Stop()
		{
			targetMovePosition = transform.position;
		}

		public void Jump(float jumpSpeed) {
			yVelocity = jumpSpeed;
			isJumped = true;
			if(IsLocal) jumpTrigger = true;


			
        }
		public void Teleport(Vector3 position, Vector3 eulerAngles)
		{
			Move(position);	
			transform.position = position;
			SetRoation(eulerAngles);
		}

		protected float intervalCallDeltaTime
		{
			get
			{
				if (lastCallTime == 0) lastCallTime = Time.time;
				float delta = Time.time - lastCallTime;
				return delta>Managers.Client.sendIdleDelta? Managers.Client.sendIdleDelta : delta;
			}
		}

		public CharacterController AvatarController
		{
			get
			{
				return characterController;
			}
		}

		public bool IsJumped
		{
			get
			{
				return isJumped;
			}
		}

		public void SetRoation(Vector3 eulerAngles)
		{
			CharacterRotationX(eulerAngles.y);
			CameraRotationY(eulerAngles.x);
		}

		public void CharacterRotationX(float rotationX)
		{
			this.transform.eulerAngles = new Vector3(0, rotationX, 0);
		}

		public void CameraRotationY(float rotationY)
		{
			rotationY = rotationY > 89f ? 89f : rotationY;
			rotationY = rotationY < -89f ? -89f : rotationY;

			headSocket.localEulerAngles = new Vector3(-rotationY, 0, 0);
		}

		public float GetRotationY()
		{
			return this.transform.eulerAngles.y;
		}

		public float GetRotationX()
		{
			float x = headSocket.localEulerAngles.x;
			if (x > 270f) x = 360 - x;
			else x *= -1;
			return x;
		}

		public Vector3 GetSchemaRotation()
		{
			return new Vector3(GetRotationX(), GetRotationY(), 0);
		}

		void OnAnimatorIK(int layerIndex)
		{
			animator.SetLookAtWeight(1.0f);
			animator.SetLookAtPosition(headIK.position);
		}

		public void SetUserInfo(UserInfo user)
		{
			userInfo = user;
			gameObject.name = user.userId;
			nicknameText.SetNickName(userInfo.nickname);
		}

		public float GetMoveTime()
		{
			return moveTime;
		}

		public bool IsMoved
		{
			get
			{
				return isMoved;
			}
		}

		public string UserId
		{
			get
			{
				return userInfo.userId;
			}
		}

		public string NickName
		{
			get
			{
				return userInfo.nickname;
			}
		}

		public bool IsLocal
		{
			get
			{
				return userInfo.isLocal;
			}
		}

		public Camera GameCamera
		{
			get
			{
				return gameCamera;
			}
		}

		public bool IsFirst
		{
			get
			{
				return isFirst;
			}
		}

		public IEnumerator ReplaceSpawnPoint()
		{
			while (Application.isPlaying)
			{
				if (!Managers.Gltf.CheckPosition(this.transform.position))
				{
					//while (this.transform.position.y < -20)
					//{
					//                   // Debug.Log("CheckTelpo");
					//                   this.transform.Translate(new Vector3(0, 5, 0));
					//               }
					//this.gameObject.SetActive(false);
					//               this.transform.Translate(new Vector3(0, 5, 0));
					//               this.gameObject.SetActive(true);

					Teleport(Vector3.zero, Vector3.zero);
                }
                yield return new WaitForSeconds(1f);
			}
		}
    }

}
