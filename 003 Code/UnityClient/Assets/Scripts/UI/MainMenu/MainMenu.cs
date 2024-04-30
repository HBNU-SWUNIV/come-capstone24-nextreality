using UnityEngine;
using UnityEngine.SceneManagement;

namespace NextReality.Game.UI
{
	public class MainMenu : MonoBehaviour
	{
		public LoginPopup loginPopup;
		public ProfilePopup profilePopup;
		UserManager userManager;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void OnClickLogin()
		{
			this.loginPopup.closeBtn.onClick.AddListener(() =>
			{
				this.loginPopup.Close();
			});
			this.loginPopup.Open();
		}

		public void OnClickStart()
		{
			SceneManager.LoadScene("WorldGenerator_Scene");
			Debug.Log("Start Clicked");
		}

		public void OnClickOption()
		{
			Debug.Log("Option Clicked");
		}

		public void OnClickQuit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false; // ����Ƽ �����Ϳ��� ���� ���� ���� �����ϴ� �Լ�
#else
        Application.Quit(); // ���� ������ ���� �������� �� �����ϴ� �Լ�
#endif
		}

		public void OnClickProfile()
		{
			userManager = UserManager.Instance;

			Debug.Log(userManager.Id);
			if (userManager.Id != null)
			{
				this.profilePopup.closeBtn.onClick.AddListener(() =>
				{
					this.profilePopup.Close();
				});
				this.profilePopup.Open();
			}
		}


	}

}
