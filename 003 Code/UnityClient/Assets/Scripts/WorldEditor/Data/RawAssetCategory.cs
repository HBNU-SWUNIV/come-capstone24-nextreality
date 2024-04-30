using System;
using System.Linq;

namespace NextReality.Asset
{
	[Serializable]
	public class RawAssetCategoriesResult
	{
		public RawAssetCategory[] result;
	}
	[Serializable]
	public class RawAssetCategory
	{
		public int id;
		public string categoryCode;
		public string categoryName;
		public RawAssetCategory[] child;
		public string GetCategoryNameTree(int depth = 0)
		{
			string message = String.Join("", Enumerable.Repeat("--", depth).ToArray()) + categoryName + "\n";
			if (child != null && child.Length > 0)
			{
				foreach (RawAssetCategory cate in child)
				{
					message += cate.GetCategoryNameTree(depth + 1);
				}
			}

			return message;
		}
	}
}
