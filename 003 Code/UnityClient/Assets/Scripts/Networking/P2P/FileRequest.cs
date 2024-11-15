using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Networking.P2P
{
	[Serializable]
	public class FileRequest
	{
		public string assetId;
		public int mode;
	}
}