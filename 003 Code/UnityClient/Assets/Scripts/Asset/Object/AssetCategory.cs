using System.Linq;

namespace NextReality.Asset
{
	public class AssetCategory : RawAssetCategory
	{
		public new AssetCategory[] child;
		public AssetCategory parentAssetCategory = null;

		AssetCategory(
			int _id,
			string _categoryName,
			string _categoryCode,
			AssetCategory _parentAssetCategory = null
		)
		{
			id = _id;
			categoryName = _categoryName;
			categoryCode = _categoryCode;

			parentAssetCategory = _parentAssetCategory;

			if (parentAssetCategory.child == null || parentAssetCategory.child.Length == 0)
				parentAssetCategory.child = new AssetCategory[0];
			parentAssetCategory.child.Concat(new[] { this });
		}

		public AssetCategory(RawAssetCategory raw, AssetCategory parent = null)
		{
			categoryName = raw.categoryName;
			categoryCode = raw.categoryCode;
			id = raw.id;
			if (parent != null) parentAssetCategory = parent;
			if (raw.child !=null && raw.child.Length > 0)
			{
				child = new AssetCategory[raw.child.Length];
				for (int i = 0; i < child.Length; i++)
				{
					child[i] = new AssetCategory(raw.child[i], this);
				}
			}
		}
	}
}

