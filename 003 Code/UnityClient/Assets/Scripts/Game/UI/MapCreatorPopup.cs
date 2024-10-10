using NextReality.Networking.Request;
using NextReality.Networking.Response;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Game.UI
{
	public class MapCreatorPopup : MonoBehaviour
	{

		[SerializeField] private TMP_InputField mapNamInput;
		[SerializeField] private Button createButton;
		[SerializeField] private Button closeButton;
		[SerializeField] private TMP_Text userNameText;
		[SerializeField] private GameObject popupContiner;
		[SerializeField] private TMP_Text messageText;

		private MapPopup mapPopup;

		HttpRequests httpRequests;

		MessageSetter messageSetter;

		string creatorId = null;
		// Start is called before the first frame update
		void Start()
		{
			closeButton.onClick.AddListener(() =>
			{
				Close();
			});

			createButton.onClick.AddListener(() => { CreateMap(); });

			httpRequests = Utilities.HttpUtil;
			messageText.text = String.Empty;
			messageSetter = Utilities.MessageUtil;
		}

		public void SetMapPopup(MapPopup popup)
		{
			mapPopup = popup;
		}

		private void CreateMap()
		{
			if(creatorId == null)
			{
				messageSetter.SetText(messageText, "You have to login.", Color.red);
				return;
			}

			if (mapNamInput.text.Trim().Length == 0)
			{
				messageSetter.SetText(messageText, "Input world name.", Color.red);
				return;
			}

			string command = string.Format("?creator_id={0}&map_name={1}", creatorId, mapNamInput.text);

			createButton.interactable = false;

			StartCoroutine(httpRequests.RequestGet
				(httpRequests.GetServerUrl
				(HttpRequests.ServerEndpoints.MapCreate) + command, (callback) =>
				{
					createButton.interactable = true;
					// Debug.Log("RequestGet Callback:  " + callback);

					if (callback == null)
					{
						messageSetter.SetText(messageText, "Map Create Failed.", Color.red);
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
									// 성공 시
									ResetInput();
									Close();
									mapPopup?.Open();
								}
								else
								{
									// 실패 시
									messageSetter.SetText(messageText, "Retry", Color.red);
								}
								
							}
						}
						catch (Exception e)
						{
							Debug.Log("MapCreatePopup.RequestGet Error : " + e.ToString());
				
						}
					}

				}));
		}

		public void ResetInput()
		{
			mapNamInput.text = String.Empty;
			creatorId = null;
			messageSetter.ResetText(messageText);
			userNameText.text = String.Empty;
		}

		public void Open()
		{
			SetActivePanel(true);
			creatorId = Managers.User.Id;
			messageSetter.ResetText(messageText);
			userNameText.text = creatorId.ToString();
		}

		public void Close()
		{
			SetActivePanel(false);
		}

		private void SetActivePanel(bool isActive)
		{
			this.popupContiner.SetActive(isActive);
		}
	}
}
