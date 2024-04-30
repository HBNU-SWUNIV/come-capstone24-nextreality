using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NextReality.Networking.Response;
using NextReality.Networking.Request;
using NextReality.Data;

namespace NextReality.Game.UI
{
	public class LoginPopup : MonoBehaviour
	{

		HttpRequests httpRequests;

		UserManager userManager;
		MessageSetter messageSetter;

		public Button mainLoginBtn;
		public Button closeBtn;

		public TMP_InputField idField;
		public TMP_InputField pwField;
		public TMP_Text msgTxt;

		public TMP_Text nickname;

		private string url = "http://192.168.50.140:8000/login";

		public void Init()
		{

		}

		public void Open()
		{
			this.gameObject.SetActive(true);
			httpRequests = new HttpRequests();
			userManager = UserManager.Instance;
			messageSetter = new MessageSetter();
		}

		public void Close()
		{
			this.gameObject.SetActive(false);
			pwField.text = string.Empty; // 로그인 팝업 닫으면 비밀번호 지워지게
		}

		public void OnClickLogin()
		{

			msgTxt.gameObject.SetActive(false);

            UserLoginData userData = new UserLoginData(idField.text, pwField.text);

			string userDataJson = JsonUtility.ToJson(userData);


			Debug.Log("ID : " + idField.text + " | PW : " + pwField.text);
			Debug.Log("String Json User Data : " + userDataJson);

			StartCoroutine(httpRequests.RequestPost(url, userDataJson, (callback) =>
			{
				Debug.Log("RequestPost Callback : " + callback);

				if (callback == null)
				{
					messageSetter.SetText(msgTxt, "Something Wrong. Try Again", Color.red);
                    userManager.ResetUser();
                }
				else
				{

					try
					{
                        ResponseData responseData = JsonUtility.FromJson<ResponseData>(callback);
                        if (responseData != null)
                        {
                            Debug.Log("response message : " + responseData.message);
                            if (responseData.code.Equals("1"))
                            {
								LoginResponseData loginResponseData = JsonUtility.FromJson<LoginResponseData>(callback);
                                nickname.SetText(loginResponseData.message.nickname);
                                msgTxt.gameObject.SetActive(false);
								
                                userManager.SetUser(loginResponseData.message.user_id, loginResponseData.message.nickname, loginResponseData.message.email);
                                mainLoginBtn.gameObject.SetActive(false);
                                this.Close();
                            }
                            else if (responseData.code.Equals("0"))
                            {

                                messageSetter.SetText(msgTxt, responseData.message, Color.red);
                                userManager.ResetUser();
                            }
                        }
                    }
					catch(System.Exception e)
					{
                        messageSetter.SetText(msgTxt, "Something Wrong. Try Again", Color.red);
                        userManager.ResetUser();
                    }
                }
			}));


			Debug.Log("Login Clicked");
		}

		public void OnClickSignUp()
		{
			Debug.Log("Sign Up Clicked");
		}

		public void OnClickForgot()
		{
			Debug.Log("Forgot Password? Clicked");
		}

	}

}

