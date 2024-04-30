using NextReality.Asset.UI;
using NextReality.Utility.UI;
using UnityEngine;

namespace NextReality.Asset
{
	public class AssetSelectorWindow : MonoBehaviour
	{
		[SerializeField] private AssetCategoryDropBoxContainer _dropBoxContainer;
		public AssetCategoryDropBoxContainer DropBoxContainer { get { return _dropBoxContainer; } }
		[SerializeField] private AssetThumbnailScrollView _assetThumbnailScrollView;
		public AssetThumbnailScrollView AssetThumbnailScrollView { get { return _assetThumbnailScrollView; } }

		[SerializeField] private DrawerLayout drawerLayout;

		// Start is called before the first frame update
		void Start()
		{
			_dropBoxContainer.AddLayoutHandler(drawerLayout.AlignLayout);
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void RequireCategoryList(string jsonText)
		{
			RawAssetCategoriesResult raw = JsonUtility.FromJson<RawAssetCategoriesResult>(jsonText);

			AssetCategory[] categories = new AssetCategory[raw.result.Length];

			int index = 0;
			foreach (RawAssetCategory lt in raw.result)
			{
				Debug.Log(lt.GetCategoryNameTree(0));
				categories[index] = new AssetCategory(lt);
				index++;
			}

			DropBoxContainer.RequireChildCategory(0, categories);
		}
	}
}

