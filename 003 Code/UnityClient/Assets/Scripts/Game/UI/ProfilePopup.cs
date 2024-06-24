using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace NextReality.Game.UI
{
	public class ProfilePopup : MainMenuPopup
	{
		UserManager userManager;

		public Button closeBtn;

		public TMP_Text mainProfileTxt;
		public Button mainLoginBtn;

		public TMP_Text userIdTxt;
		public TMP_Text userNicknameTxt;
		public TMP_Text userEmailTxt;

		public void Open()
		{
			this.gameObject.SetActive(true);
			userManager = Managers.User;

			userIdTxt.SetText(userManager.Id);
			userNicknameTxt.SetText(userManager.Nickname);
			userEmailTxt.SetText(userManager.Email);
		}

		public void Close()
		{
			this.gameObject.SetActive(false);
		}

		public void OnClickLogout()
		{
			userManager.ResetUser();
			mainMenu.ButtonChange(userManager.IsLogin);
			mainLoginBtn.gameObject.SetActive(true);
			mainProfileTxt.SetText("Guest");
			this.Close();
		}
	}

}
