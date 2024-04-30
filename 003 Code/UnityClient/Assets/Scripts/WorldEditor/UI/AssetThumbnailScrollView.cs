using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		// Start is called before the first frame update
		void Start()
		{
			SetItem(GetTestAssetItem(10)); // 임시
		}


		AssetItem[] GetTestAssetItem(int length)
		{
			AssetItem[] result = new AssetItem[length];
			for (int i = 0; i < length; i++)
			{
				AssetItem asset = new AssetItem();

				asset.id = i + 1;
				asset.name = "오브젝트 " + (i + 1);
				asset.thumbnail = Resources.Load("TestNumber/number-" + (i + 1)) as Texture2D;

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
				RegisterAssetThumbnail(assetItem);
			}
		}

		public void RegisterAssetThumbnail(AssetItem assetItem)
		{
			AssetThumbnailElement element = (assetThumbnailPool.Count > 0) ? assetThumbnailPool.Dequeue() : Instantiate(assetThumbnailElementPrefab, contentContainer).InitScrollView(this);

			activeAssetThumbnails.Add(element);
			element.SetAsset(assetItem);
			element.SetActive(true);
		}

	}
}

