using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Asset.UI
{
	public class AssetCategoryDropBoxContainer : MonoBehaviour
	{
		[SerializeField] AssetCategoryDropBox assetDropBoxPrefab;
		[SerializeField] AssetCategoryDropBox[] assetDropBoxes = new AssetCategoryDropBox[0];

		[SerializeField] private RectTransform _container;

		AssetCategory[] assetCategories;

		AssetCategory selectedCategory;

		Del_VoidHandler HandleLayout;

		int curDepth = 0;

		Coroutine layoutCoroutine;

		public void SetCategory(AssetCategory[] _assetCategories)
		{
			assetCategories = _assetCategories;
		}

		public void RequireChildCategory(int depth, AssetCategory[] assetCategories )
		{

			int activeDepth = depth;
			if (assetCategories != null && assetCategories.Length > 0)
			{
				// Depth 크기만큼 DropBox 생성
				if (assetDropBoxes.Length <= depth)
				{
					AssetCategoryDropBox[] newArray = new AssetCategoryDropBox[depth + 1];
					Array.Copy(assetDropBoxes, newArray, assetDropBoxes.Length);
					for (int i = assetDropBoxes.Length; i < depth + 1; i++)
					{
						newArray[i] = Instantiate(assetDropBoxPrefab, _container).InitCategoryContainer(this);
					}
					assetDropBoxes = newArray;
				}
				assetDropBoxes[depth].SetCategoryArray(assetCategories, depth);
			} else
			{
				activeDepth -= 1;
			}

			for (int i = 0; i < assetDropBoxes.Length; i++)
			{
				if (i <= activeDepth)
				{
					assetDropBoxes[i].SetActive(true);
				}
				else
				{
					assetDropBoxes[i].SetActive(false);
				}
			}
			
			if (layoutCoroutine != null) StopCoroutine(layoutCoroutine);
			layoutCoroutine = StartCoroutine(ForceAlignLayout());
		}

		IEnumerator ForceAlignLayout()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
			yield return null;
			HandleLayout();
		}

		public void AddLayoutHandler(Del_VoidHandler handle)
		{
			HandleLayout += handle;
		}

		public void ClearChildCategory(int curDepth)
		{
			int nextDepth = curDepth + 1;
			for (int i = 0; i < assetDropBoxes.Length; i++)
			{
				if (i < nextDepth)
				{
					assetDropBoxes[i].SetActive(true);
				}
				else
				{
					assetDropBoxes[i].SetActive(false);
				}
			}
		}

	}
}

