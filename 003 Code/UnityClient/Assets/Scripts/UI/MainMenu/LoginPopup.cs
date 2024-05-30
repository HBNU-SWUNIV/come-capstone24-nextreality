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
	public class LoginPopup : MainMenuPopup
	{
		HttpRequests httpRequests;
        MessageSetter messageSetter;
        UserManager userManager;

		public Button closeBtn;

		public TMP_InputField idField;
		public TMP_InputField pwField;
		public TMP_Text msgTxt;

		public TMP_Text nickname;

		public void Init()
		{

		}

		public void Open()
		{
			this.gameObject.SetActive(true);
			msgTxt.gameObject.SetActive(false);
			httpRequests = Utilities.HttpUtil;
            messageSetter = Utilities.MessageUtil;
            userManager = Managers.User;	
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

			StartCoroutine(httpRequests.RequestPost
				(httpRequests.GetServerUrl
				(HttpRequests.ServerEndpoints.Login), userDataJson, (callback) =>
			{
				Debug.Log("RequestPost Callback : " + callback);

				if (callback == null)
				{
                    messageSetter.SetText(msgTxt, "Something Wrong. Try Again", Color.red);
                    userManager.ResetUser();
					mainMenu.ButtonChange(userManager.IsLogin);
				}
				else
				{

					try
					{
                        ResponseData responseData = JsonUtility.FromJson<ResponseData>(callback);
                        if (callback != null)
                        {
                            if (responseData.CheckResult())
                            {
                                //Debug.Log(responseData.message);
                                LoginResponseData loginResponseData = JsonUtility.FromJson<LoginResponseData>(callback);

                                nickname.SetText(loginResponseData.message.nickname);
                                msgTxt.gameObject.SetActive(false);

                                userManager.SetUser(loginResponseData.message.user_id, loginResponseData.message.nickname, loginResponseData.message.email);
								mainMenu.ButtonChange(userManager.IsLogin);
								this.Close();
                            }
                            else
                            {
                                messageSetter.SetText(msgTxt, responseData.message, Color.red);
                                userManager.ResetUser();
								mainMenu.ButtonChange(userManager.IsLogin);
							}
                        }
                    }
					catch(System.Exception e)
					{
						Debug.Log("LoginPopup.RequestPost Error : " + e.ToString());
                        messageSetter.SetText(msgTxt, "Something Wrong. Try Again", Color.red);
                        userManager.ResetUser();
						mainMenu.ButtonChange(userManager.IsLogin);
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

