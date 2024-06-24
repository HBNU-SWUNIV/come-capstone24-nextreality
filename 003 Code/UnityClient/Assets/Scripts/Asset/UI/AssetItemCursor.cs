using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using NextReality.Data;

namespace NextReality.Asset.UI
{
	public class AssetItemCursor : MonoBehaviour
	{
		[SerializeField] RawImage thumbnail;
		[SerializeField] Canvas targetCanvas;

		AssetItem assetItem;

		private RectTransform _rectTransform;
		public RectTransform rectTransform
		{
			get {
				if(_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
 				return _rectTransform; 
			}
		}

		public AssetItem AssetItem {  get { return assetItem; } }
		// Start is called before the first frame update
		void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
		}

		public bool isActive { get { return gameObject.activeSelf; } }
		public void SetActive(bool active) { gameObject.SetActive(active); }

		public void SetAsset(AssetItem item)
		{
			assetItem = item;
			thumbnail.texture = item.thumbnail2D;
		}

		public void SetCusorPosition(Vector3 position)
		{
			rectTransform.anchoredPosition = position / targetCanvas.scaleFactor;
		}

		public AssetItem PopAssetItem()
		{
			var result = assetItem;
			assetItem = null;
			return result;
		}
	}
}

