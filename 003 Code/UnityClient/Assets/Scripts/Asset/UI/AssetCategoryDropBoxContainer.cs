using NextReality.Utility.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NextReality.Utility;

namespace NextReality.Asset.UI
{
	public class AssetCategoryDropBoxContainer : MonoBehaviour
	{
		[SerializeField] AssetCategoryDropBox assetDropBoxPrefab;
		[SerializeField] AssetCategoryDropBox[] assetDropBoxes = new AssetCategoryDropBox[0];

		[SerializeField] private RectTransform _container;
		[SerializeField] private RectTransform refreshContainer;

		[SerializeField] private DrawerLayout drawerLayout;

		AssetCategory[] assetCategories;

		Del_VoidHandler HandleLayout;

		int curDepth = 0;

		Coroutine layoutCoroutine;


		public void Start()
		{
			if(drawerLayout) this.AddLayoutHandler(drawerLayout.AlignLayout);
			if(refreshContainer)
			{
				AddLayoutHandler(() => TryForceRebuildLayoutImmediate(refreshContainer));
			}
		}

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

			curDepth = activeDepth;

			if (layoutCoroutine != null) StopCoroutine(layoutCoroutine);
			if (gameObject.activeInHierarchy)
			{
				layoutCoroutine = StartCoroutine(ForceAlignLayout(_container));
			} else
			{
				layoutCoroutine = null;
			}

		}

		void TryForceRebuildLayoutImmediate(RectTransform refreshTarget)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(refreshTarget);
		}

		IEnumerator ForceAlignLayout(RectTransform refreshTarget)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(refreshTarget);
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

		public void Clear()
		{
			this.RequireChildCategory(0, assetCategories);
		}

		public void RequireCategoryList(string jsonText)
		{
			Debug.Log(jsonText);
			RawAssetCategoriesResult raw = JsonUtility.FromJson<RawAssetCategoriesResult>(jsonText);

			AssetCategory[] categories = new AssetCategory[raw.result.Length];

			int index = 0;
			foreach (RawAssetCategory lt in raw.result)
			{
				// Debug.Log(lt.GetCategoryNameTree(0));
				categories[index] = new AssetCategory(lt);
				index++;
			}

			SetCategory(categories);

			this.RequireChildCategory(0, categories);
		}

		public AssetCategory SelectedCategory
		{
			get
			{
				return curDepth <= 0 ? null : (assetDropBoxes[curDepth].CurAssetCategory ?? assetDropBoxes[curDepth - 1].CurAssetCategory);
			}
		}

	}
}

