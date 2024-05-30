using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NextReality.Asset.UI
{
	public class AssetCategoryDropBox : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown dropDown;
		int depth;
		AssetCategory[] assetCategories;
		AssetCategoryDropBoxContainer container;

		int _curIndex;
		public int CurIndex
		{
			get { return _curIndex; }
			set { _curIndex = value; dropDown.value = value + 1; }
		}

		public AssetCategory CurAssetCategory
		{
			get
			{
				return CurIndex <0 ? null : assetCategories[CurIndex];
			}
		}

		// Start is called before the first frame update
		void Start()
		{
			dropDown.onValueChanged.AddListener(delegate
			{
				SettingCategoryDropBox();
			});
		}

		public AssetCategoryDropBox InitCategoryContainer(AssetCategoryDropBoxContainer _container)
		{
			container = _container;

			return this;
		}

		public void SetCategoryArray(AssetCategory[] _assetCategories, int _depth, int _categiryIndex = -1)
		{
			if (_assetCategories.Length == 0) return;

			if(_assetCategories != assetCategories)
			{
				assetCategories = _assetCategories;
				List<string> options = new List<string>();
				options.Add("ÀüÃ¼");
				for (int i = 0; i < assetCategories.Length; i++)
				{
					options.Add(assetCategories[i].categoryName);
				}
				dropDown.ClearOptions();
				dropDown.AddOptions(options);
			}

			depth = _depth;
			if(_curIndex!=_categiryIndex) CurIndex = _categiryIndex;
		}

		void SettingCategoryDropBox()
		{
			_curIndex = dropDown.value - 1;
			int targetIndex = (CurIndex == -1) ? depth : depth + 1;
			var categories = (CurIndex == -1) ? assetCategories : assetCategories[CurIndex].child;
			container.RequireChildCategory(targetIndex, categories);
		}

		public void SetActive(bool isActive)
		{
			gameObject.SetActive(isActive);
		}
	}
}

