using NextReality.Asset.UI;
using UnityEngine;

namespace NextReality.Asset
{
	public class WorldEditorController : MonoBehaviour
	{

		//s: 임시
		public TextAsset textAsset;

		//e: 임시

		[SerializeField] private AssetSelectorWindow _assetSelectorWindow;
		public AssetSelectorWindow AssetSelectorWindow
		{
			get { return _assetSelectorWindow; }
		}

		[SerializeField] private AssetQuickSlotLayout _assetQuickSlotLayout;
		public AssetQuickSlotLayout AssetQuickSlotLayout
		{
			get { return _assetQuickSlotLayout; }
		}

		[SerializeField] AssetItemCursor assetItemCursor;
		public AssetItemCursor AssetItemCursor { get { return assetItemCursor; } }

		private static WorldEditorController instance = null;

		void Awake()
		{
			if (null == instance)
			{
				instance = this;

				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}
		}

		public static WorldEditorController Instance
		{
			get
			{
				if (null == instance)
				{
					return null;
				}
				return instance;
			}
		}
		// Start is called before the first frame update
		void Start()
		{
			assetItemCursor.SetActive(false);
			_assetSelectorWindow.RequireCategoryList(textAsset.text);
		}

		public void DragAssetItemCursor(AssetItem assetItem, Vector3 position)
		{
			if (assetItem != assetItemCursor.AssetItem || !assetItemCursor.isActive)
			{
				assetItemCursor.SetAsset(assetItem);
				assetItemCursor.SetActive(true);
			}

			assetItemCursor.SetCusorPosition(position);
		}
		public void EndDragAssetItemCursor()
		{
			assetItemCursor.SetActive(false);
		}

		public AssetItem SelectedAsset
		{
			get
			{
				return _assetQuickSlotLayout.SelectedAsset;
			}
		}

	}
}

