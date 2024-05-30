using UnityEngine;
using UnityEngine.SceneManagement;
using NextReality;
using UnityEngine.UI;
using NextReality.Networking.Response;
using TMPro;

namespace NextReality.Game.UI
{
    public class MainMenu : MonoBehaviour
    {
        public LoginPopup loginPopup;
        public ProfilePopup profilePopup;
        public UploadPopup uploadPopup;
        public MapPopup mapPopup;

        public Button startBtn;
        public Button uploadBtn;
        public Button loginBtn;
        public TMP_Text nickname;

        UserManager userManager;

        void Awake()
        {
            loginPopup.SetMainMenu(this);
            profilePopup.SetMainMenu(this);
            uploadPopup.SetMainMenu(this);
            mapPopup.SetMainMenu(this);

        }

        private void Start()
        {
            userManager = Managers.User;
            if (userManager.IsLogin)
            {
                ButtonChange(userManager.IsLogin);
                nickname.SetText(userManager.Nickname);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ButtonChange(bool isLogin)
        {
            startBtn.gameObject.SetActive(isLogin);
            uploadBtn.gameObject.SetActive(isLogin);
            loginBtn.gameObject.SetActive(!isLogin);
        }

        public void OnClickLogin()
        {
            this.loginPopup.closeBtn.onClick.AddListener(() =>
            {
                this.loginPopup.Close();
            });
            this.loginPopup.Open();
        }

        public void OnClickUpload()
        {
            this.uploadPopup.closeBtn.onClick.AddListener(() =>
            {
                this.uploadPopup.Close();
            });
            this.uploadPopup.Open();
        }


        public void OnClickStart()
        {
            // SceneManager.LoadScene("WorldGenerator_Scene"); // Scene Change

            this.mapPopup.closeBtn.onClick.AddListener(() =>
            {
                this.mapPopup.Close();
            });
            this.mapPopup.Open();


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
        Application.Quit();
#endif
        }

        public void OnClickProfile()
        {
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
