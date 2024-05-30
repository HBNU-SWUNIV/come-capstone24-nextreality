using NextReality.Asset.UI;
using NextReality.Game;
using NextReality.Game.UI;
using NextReality.Utility.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Asset
{
	public class AssetSelectorWindow : MonoBehaviour
	{
		[SerializeField] private AssetCategoryDropBoxContainer _dropBoxContainer;

		[SerializeField] private Button assetSearchButton;
		[SerializeField] private TMP_InputField assetSearchinputField;

		public AssetCategoryDropBoxContainer DropBoxContainer { get { return _dropBoxContainer; } }
		[SerializeField] private AssetThumbnailScrollView _assetThumbnailScrollView;
		public AssetThumbnailScrollView AssetThumbnailScrollView { get { return _assetThumbnailScrollView; } }

		[SerializeField] private SidePopUpUI sidePopUpUI;
		[SerializeField] private Button popUpButton;

		// Start is called before the first frame update
		void Start()
		{
			Managers.Camera.AddCursorEventListener((cursorLock) => PopUpDrawer(!cursorLock));
			popUpButton.onClick.AddListener(()=> PopUpDrawer(!sidePopUpUI.IsPopUp));

			assetSearchButton.onClick.AddListener(() =>
			{
				_assetThumbnailScrollView.GetAssetIdList(DropBoxContainer.SelectedCategory?.id ?? -1, assetSearchinputField.text);
			});

			assetSearchinputField.onSubmit.AddListener((search)=>
			{
				_assetThumbnailScrollView.GetAssetIdList(DropBoxContainer.SelectedCategory?.id ?? -1, search);
			});
		}

		public void PopUpDrawer(bool isPopUp)
		{
			sidePopUpUI.PopUp(isPopUp);
		}

	}
}

