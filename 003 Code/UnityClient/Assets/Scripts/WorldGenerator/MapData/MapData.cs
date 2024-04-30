using System.Collections.Generic;

namespace NextReality.Asset
{
	[System.Serializable]
	public class MapData
	{
		public int map_id;
		public string mapName;
		public float mapCTime;
		public MapSize mapSize = new MapSize();
		public List<string> Tags = new List<string>();
		public int objCount;
	}

	[System.Serializable]
	public class MapSize
	{
		public int horizontal;
		public int vertical;
		public int height;
	}


	[System.Serializable]
	public class MapObjectData
	{
		public int map_id;
		public List<ObjectData> objList = new List<ObjectData>();
	}

	[System.Serializable]
	public class ObjectData
	{
		public int obj_id;
		public int ast_id;
		public ObjTransform transform = new ObjTransform();
		public string type;
		public bool isRigidbody = false;
		public bool isMeshCollider = false;
	}

	[System.Serializable]
	public class ObjTransform
	{
		public Position position;
		public Rotation rotation;
		public Scale scale;
	}

	[System.Serializable]
	public class Position
	{
		public float x;
		public float y;
		public float z;
	}

	[System.Serializable]
	public class Rotation
	{
		public float x;
		public float y;
		public float z;
	}

	[System.Serializable]
	public class Scale
	{
		public float x;
		public float y;
		public float z;
	}
}