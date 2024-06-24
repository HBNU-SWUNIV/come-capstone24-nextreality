using NextReality;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game
{
	public class GameSettingController : MonoBehaviour
	{

		public GameSettingButton settingButtonPrefab;

		public Canvas canvas;

		public RectTransform container;

		public Animator drawerAnimator;

		public Button drawerButton;

		public GameSettingButton[] gameSettingButtons;

		bool isDrawerOpen = false;

		Coroutine saveButtonCoroutine = null;
		// Start is called before the first frame update
		void Start()
		{
			Managers.Client.AddJoinLocalPlayerEvent((player, userId) =>
			{
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = Managers.Camera.mainGameCamera.uiCam;
			});

			drawerButton.onClick.AddListener(() =>
			{
				SetDrawerPanel(!isDrawerOpen);
			});

			TestSetButtonAddListener();
		}

		void SetDrawerPanel(bool isOn)
		{
			isDrawerOpen = isOn;

			drawerAnimator.SetBool("open", isOn);
		}

		void TestSetButtonAddListener()
		{
			if (gameSettingButtons.Length > 1)
			{
				gameSettingButtons[0].AddListener(() =>
				{
					Managers.Client.ExitGame();
				});

				gameSettingButtons[1].AddListener(() =>
				{
					
					StartCoroutine(Managers.Map.MapSave());
					if (saveButtonCoroutine != null) StopCoroutine(saveButtonCoroutine);
					saveButtonCoroutine = StartCoroutine(StartWaitSaveTime(gameSettingButtons[1]));
				});

				gameSettingButtons[2].AddListener(() =>
				{
					Managers.Map.SendMapInit();
                    if (saveButtonCoroutine != null) StopCoroutine(saveButtonCoroutine);
                    saveButtonCoroutine = StartCoroutine(StartWaitSaveTime(gameSettingButtons[2]));
                });
			}
		}

		IEnumerator StartWaitSaveTime(GameSettingButton button)
		{
			float time = 0;
			float maxTime = 5f;
			button.button.interactable = false;
			while (gameObject.activeInHierarchy)
			{
				time += Time.deltaTime;
				if (time > maxTime) break;
				button.buttonGraphic.fillAmount = time / maxTime;
				yield return null;
			}
			button.buttonGraphic.fillAmount = 1;
			button.button.interactable = true;

			saveButtonCoroutine = null;
		}


	}

}
