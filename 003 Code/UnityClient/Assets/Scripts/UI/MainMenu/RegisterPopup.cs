using NextReality.Networking.Request;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace NextReality.Game.UI
{
	public class UserInfo
	{
		public string id;
		public string email;
		public string pw;
	}

	public class RegisterPopup : MainMenuPopup
	{
		HttpRequests httpRequests;

		public Button closeBtn;
		public TMP_InputField idField;
		public TMP_InputField emailField;
		public TMP_InputField pwField;
		public TMP_Text msgTxt;

		private string id;
		private string email;
		private string pw;

		private string url = "http://localhost:4000";
		private string duplicateIDUrl = "/users/duplicate?type=loginId&content=";

		public void Awake()
		{

		}

		public void Start()
		{

		}

		public void Update()
		{

		}


		public void Open()
		{
			this.gameObject.SetActive(true);
		}

		public void Close()
		{
			this.gameObject.SetActive(false);
		}

		public void OnClickRegister()
		{
			msgTxt.gameObject.SetActive(false);

			id = idField.text;
			email = emailField.text;
			pw = pwField.text;

			Debug.Log("Register Clicked");
			Debug.Log("ID : " + id + " Email : " + email + " PW : " + pw);


			// ID 중복 검사를 위한 코루틴 생성 및 실행
			StartCoroutine(httpRequests.RequestGet(url + duplicateIDUrl + id, (callback) =>
			{
				Debug.Log("RequestGet Callback : " + callback);
				if (callback == "true")
				{
					msgTxt.gameObject.SetActive(true);
					msgTxt.color = Color.green;
					msgTxt.text = "This ID is UNIQUE! GOOD!";
				}
				else
				{
					msgTxt.gameObject.SetActive(true);
					msgTxt.color = Color.red;
					msgTxt.text = "Register Failed. Check your ID";
				}
			}));
		}
	}

}
