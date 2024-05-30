using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using NextReality.Game;
using NextReality.Networking.Request;
using NextReality.Game.UI;
using NextReality.Networking.Response;
using System;
using System.Collections;
using TMPro;
using static Reporter;

namespace NextReality.Asset.UI
{
	public class AssetThumbnailScrollView : MonoBehaviour
	{
		[SerializeField] private AssetThumbnailElement assetThumbnailElementPrefab;
		[SerializeField] private RectTransform contentContainer;

		public AssetItem[] assetItems;

		private Queue<AssetThumbnailElement> assetThumbnailPool = new Queue<AssetThumbnailElement>();
		private List<AssetThumbnailElement> activeAssetThumbnails = new List<AssetThumbnailElement>();

		protected RectTransform _rectTransform;
		public RectTransform rectTransform { get { return _rectTransform; } }

		[SerializeField] private ScrollRect scrollRect;
		public ScrollRect ScrollRect { get { return scrollRect; } }

		public float thumbnailLongPressDuration = 1f;

		[SerializeField] private GameObject assetLoadingPage;

		[SerializeField] private TMP_Text assetHelpText;


		HttpRequests httpRequests;
		MessageSetter messageSetter;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		// Start is called before the first frame update
		void Start()
		{
			httpRequests = Utilities.HttpUtil;
			messageSetter = Utilities.MessageUtil;
			GetAssetIdList();
			// SetItem(GetTestAssetItem(20)); // 임시
		}

		public void GetAssetIdList(int categoryid = -1, string assetName = "")
		{
			//Debug.Log("Load Asset List Start");
			SetAssetListIsLoad(false);

			Dictionary<string, string> searchQuery = new Dictionary<string, string>();
			if(categoryid >=0 )
			{
				searchQuery.Add("categoryid", categoryid.ToString());
			}

			assetName = assetName.Trim();
			if(assetName.Length > 0)
			{
				searchQuery.Add("name",assetName);
			}

			StartCoroutine(httpRequests.RequestGet
				(httpRequests.GetServerUrl((HttpRequests.ServerEndpoints.AssetSearch)), searchQuery, (callback) =>
				{
					// Debug.Log("RequestGet Callback : " + callback);

					if (callback == null)
					{
						SetAssetListIsLoad(false, "오류가 발생하였습니다.");
					}
					else
					{
						try
						{
							ResponseData responseData = JsonUtility.FromJson<ResponseData>(callback);
							if (callback!= null)
							{
								if (responseData.CheckResult())
								{
									AssetQueryResponseData assetListResponse = JsonUtility.FromJson<AssetQueryResponseData>(callback);

									// http post result list
									AssetQuery[] assetQueryList = assetListResponse.data;

									// result of this function
									AssetItem[] result = new AssetItem[assetQueryList.Length];

									string log = "";

									if (result.Length == 0) log = "검색된 에셋이 없습니다.";

									for (int i = 0; i < result.Length; i++)
									{
										AssetItem asset = new AssetItem();

										asset.id = assetQueryList[i].id;
										asset.name = assetQueryList[i].name;
										result[i] = asset;
									}
									SetItem(result);
									SetAssetListIsLoad(true, log);
								}
							} else
							{
								SetAssetListIsLoad(false, "오류가 발생하였습니다.");
							}
						}
						catch(Exception e)
						{
							Debug.Log("Asset List Load Error : " + e.ToString());
							SetAssetListIsLoad(false, "Asset List Load Error");
						}
					}
				}));
		}

		AssetItem[] GetTestAssetItem(int length)
		{
			AssetItem[] result = new AssetItem[length];
			for (int i = 0; i < length; i++)
			{
				AssetItem asset = new AssetItem();

				asset.id = "663dd3624fe104b4551ca873";
				asset.name = "오브젝트 " + (i + 1);
				asset.thumbnail2D = Resources.Load("TestNumber/number-" + (i + 1)) as Texture2D;

				result[i] = asset;
			}

			return result;
		}

		public void SetItem(AssetItem[] _assetItems)
		{
			assetItems = _assetItems;
			activeAssetThumbnails.ForEach
				(assetItem =>
				{
					assetThumbnailPool.Enqueue(assetItem);
					assetItem.SetActive(false);
				});
			activeAssetThumbnails.Clear();

			foreach (var assetItem in assetItems)
			{

				GetOneThumbnail(RegisterAssetThumbnail(assetItem));
			}
		}

		// public void GetThumbnail() { }

		public AssetThumbnailElement RegisterAssetThumbnail(AssetItem assetItem)
		{
			AssetThumbnailElement element = (assetThumbnailPool.Count > 0) ? assetThumbnailPool.Dequeue() : Instantiate(assetThumbnailElementPrefab, contentContainer).InitScrollView(this);

			activeAssetThumbnails.Add(element);
			element.SetAsset(assetItem);
			element.SetActive(true);

			return element;
		}

		private void SetAssetListIsLoad(bool isLoaded, string text = "")
		{
			if(text.Length > 0)
			{
				assetLoadingPage.SetActive(false);
				scrollRect.verticalScrollbar.gameObject.SetActive(false);
				scrollRect.viewport.gameObject.SetActive(false);
				if(Utilities.MessageUtil)
				{
					Utilities.MessageUtil.SetText(assetHelpText, text, isLoaded ? Color.black : Color.red);
				} else
				{
					assetHelpText.text = text;
				}
				assetHelpText.gameObject.SetActive(true);
			} else
			{
				assetLoadingPage.SetActive(!isLoaded);
				scrollRect.verticalScrollbar.gameObject.SetActive(isLoaded);
				scrollRect.viewport.gameObject.SetActive(isLoaded);
				assetHelpText.gameObject.SetActive(false);
			}

		}

		IEnumerator StartAssetThumbanilDown()
		{
			while(gameObject.activeInHierarchy)
			{
                foreach (var item in activeAssetThumbnails)
                {
					GetOneThumbnail(item);

				}
				yield return null;
			}
		}

		public Texture2D GetOneThumbnail(AssetThumbnailElement element)
		{
			Texture2D result = new Texture2D(0, 0);

            Dictionary<string, string> assetIdQuery = new Dictionary<string, string>
            {
                { "id", element.AssetItem.id }
            };

            //Debug.Log("Load Asset List Start");
            StartCoroutine(httpRequests.RequestGet
                (httpRequests.assetServerUrl+"/asset_thumbnail", assetIdQuery, (callback) =>
                {
                    //Debug.Log("RequestGet Callback : " + callback);

                    if (callback == null)
                    {

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
                                    AssetPreviewResponseData assetThumbnailResponse = JsonUtility.FromJson<AssetPreviewResponseData>(callback);

                                    // http post result list
                                    AssetImage[] assetThumbnails = assetThumbnailResponse.data;

									foreach(AssetImage assetThumbnail in assetThumbnails)
									{
										byte[] thumbnailBytes = Convert.FromBase64String(assetThumbnail.thumbnail);
										result.LoadImage(thumbnailBytes);

										element.AssetItem.thumbnail = assetThumbnail.thumbnail;
										element.AssetItem.thumbnail2D = result;
										element.SetAsset(element.AssetItem);
									}




								}
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Asset List Load Error : " + e.ToString());
                        }
                    }
                }));
            return result;
		}

	}
}

