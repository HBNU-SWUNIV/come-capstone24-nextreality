using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using NextReality.Networking.Response;
using NextReality.Networking.Request;
using NextReality.Data;
using NextReality.Asset;
using System;
using NextReality.Utility.UI;
using System.Linq;

namespace NextReality.Game.UI
{
    public class MapPopup : MainMenuPopup
	{
        HttpRequests httpRequests;
        MessageSetter messageSetter;
        UserManager userManager;

        public MapInfo mapInfoPrefab;

        public Button closeBtn;
        public Button prevBtn;
        public Button nextBtn;

        public TMP_InputField searchField;
        public TMP_Text msgTxt;
        public Button startBtn;

        public GameObject touchGuard;

        public List<MapInfo> mapInfos = new List<MapInfo>();

        [SerializeField] private GameObject bottomBar;

        MapListData[] mapList;

        bool isLoading = false;

        int currentPage = 0;

        int selectedIndex = -1; // 선택 된 인덱스
        int selectedMapId = -1; // 선택 된 맵 아이디

        public FlexibleGridLayout mapInfoContainer;
        Vector2 curScreenSize = Vector2.zero;

        Coroutine loadMapInfoCoroutine;

        public TMP_Text pageNumTxt;

        // 추후에 currentPage * 10 = startOffset 하고 거기서부터 10개 받아오면 됨


        // Start is called before the first frame update
        void Start()
        {

			foreach (Transform child in mapInfoContainer.transform)
			{
				Destroy(child.gameObject);
			}
		}

        public void Open()
        {
            this.gameObject.SetActive(true);
			msgTxt.gameObject.SetActive(false);
			ActiveTouchGuard(true);
			httpRequests = Utilities.HttpUtil;
            messageSetter = Utilities.MessageUtil;
            userManager = Managers.User;

			if (loadMapInfoCoroutine != null) StopCoroutine(loadMapInfoCoroutine);
			loadMapInfoCoroutine = StartCoroutine(StartWaitMapInfo());
        }

        public void Close()
        {
            this.gameObject.SetActive(false);

			if (loadMapInfoCoroutine != null) StopCoroutine(loadMapInfoCoroutine);

		}

        public void LodingChange(bool isCompleteLoading)
        {
            isLoading = isCompleteLoading;

            SetPageNavigation(currentPage);
			ActiveTouchGuard(!isLoading);


        }

        IEnumerator StartWaitMapInfo()
        {
            yield return new WaitUntil(() => mapInfoContainer.PreferCount != 0 && mapInfoContainer.PreferCount == mapInfos.Count);
			GetMapList();

            loadMapInfoCoroutine = null;
		}

        private void ActiveTouchGuard(bool isActive)
        {
            touchGuard.SetActive(isActive);
            bottomBar.SetActive(!isActive);


		}

        // 나중에는 시작 오프셋 줘서 개당 10개씩 
        public void GetMapList()
        {
            Debug.Log("Load Map List Start");
            StartCoroutine(httpRequests.RequestGet
                (httpRequests.GetServerUrl
                (HttpRequests.ServerEndpoints.MapList), (callback) =>
            {
                // Debug.Log("RequestGet Callback:  " + callback);

                if (callback == null)
                {
                    messageSetter.SetText(msgTxt, "Load Map List Failed.", Color.red);
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
                                MapListResponseData mapListData = JsonUtility.FromJson<MapListResponseData>(callback);
                                //Debug.Log("MapListData : " + mapListData);

                                mapList = mapListData.message;

                                //foreach(MapListData maplistone in mapList)
                                //{
                                //    Debug.Log("ID : " + maplistone.map_id);
                                //    Debug.Log("Name : " + maplistone.mapName);
                                //}

                                messageSetter.SetText(msgTxt, "Load Map List Complete.", Color.green);
                            }
                            else
                            {
                                messageSetter.SetText(msgTxt, "Load Map List Failed.", Color.red);
                            }
							LodingChange(callback != null);
						}
                    }
                    catch(Exception e)
                    {
                        Debug.Log("MapPopup.RequestGet Error : " + e.ToString());
                        messageSetter.SetText(msgTxt, "Load Map List Failed.", Color.red);
                    }
                }

            }));   
        }
        

        public void OnClickPrev()
        {
            if (currentPage - 1 < 0) return;
            Debug.Log("prevBtn Clicked");
			SetPageNavigation(currentPage-1);
		}

        public void OnClickNext()
        {
			if ((currentPage + 1) * mapInfoContainer.PreferCount >= mapList.Length) return;
			Debug.Log("nextBtn Clicked");
			SetPageNavigation(currentPage + 1);
		}

        public void OnClickStart()
        {
            Debug.Log("Start Clicked. Map Index : " + selectedIndex.ToString());
            Debug.Log("Start Clicked. Map ID : " + selectedMapId.ToString());
        }

        public void ClickMapInfo(int _selectedMapId, int _selectedMapIndex)
        {
            selectedMapId = _selectedMapId;
            selectedIndex = _selectedMapIndex;

            Managers.Scene.LoadGame();
			Managers.Scene.AddSceneParameter("MapId", selectedMapId);
		}

        private void SetPageNavigation(int page)
        {
            if(page * mapInfoContainer.PreferCount >= mapList.Length )
            {
                SetPageNavigation(page - 1);
            } else
            {
				currentPage = page;
				pageNumTxt.text = (currentPage+1).ToString();

				prevBtn.gameObject.SetActive(page != 0);
				nextBtn.gameObject.SetActive((page + 1) * mapInfoContainer.PreferCount < mapList.Length);

				SetMapInfoList(page);
			}
        }

        private void SetMapInfoList(int page)
        {
            int mapInfoIndex = 0;
            int startIndex = page * mapInfoContainer.PreferCount;

			for (int index = startIndex; index < startIndex + mapInfoContainer.PreferCount; ++index)
            {
                if (mapInfoIndex >= mapInfoContainer.PreferCount) break;
				if (index < mapList.Length)
                {
					mapInfos[mapInfoIndex].SetData(mapList[index], index);
				} else
                {
                    mapInfos[mapInfoIndex].ResetData();

				}

				

				mapInfoIndex++;

			}
        }

		// Update is called once per frame
		void Update()
		{
            CheckScreenSize();
		}

        private void CheckScreenSize()
        {
			if (curScreenSize.x != Screen.width ||  curScreenSize.y != Screen.height || mapInfoContainer.PreferCount > mapInfos.Count)
            {
				
				curScreenSize = new Vector2 (Screen.width, Screen.height);

                if(mapInfoContainer.PreferCount > 0 )
                {
                    for(int i = 0;i<mapInfoContainer.PreferCount-mapInfos.Count;i++)
                    {
                        var newMapInfo = Instantiate(mapInfoPrefab, mapInfoContainer.container);

						mapInfos.Add(newMapInfo);
                        newMapInfo.mapPopup = this;


						//Debug.Log("[MapPopup] CheckScreenSize "+ curScreenSize.y);
					}

                    

                }

                if(mapList != null && mapList.Length>0 && mapInfoContainer.PreferCount <= mapInfos.Count) SetPageNavigation(currentPage);

			}
        }
	}
}

