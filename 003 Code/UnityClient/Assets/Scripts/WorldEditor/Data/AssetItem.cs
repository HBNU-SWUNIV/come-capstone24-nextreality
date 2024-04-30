using System;
using UnityEngine;

namespace NextReality.Asset
{
	[Serializable]
	public class AssetItem
	{
		public int id;
		public string name;
		public int categoryId;
		public Texture2D thumbnail;
		public int fileId;
		public DateTime UploadDate;
		public int DownloadCount;
		public int price;
		public bool isDisable;
	}
}


