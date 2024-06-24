using NextReality.Data.Schema;
using System.Collections.Generic;

namespace NextReality.Data
{
	[System.Serializable]
	public class MapData
	{
		public int map_id = 0;
		public string user_id;
		public string mapName = "";
		public string mapCTime;
		public int version = 0;
		public MapSize mapSize = new MapSize();
		public List<string> Tags = new List<string>();
		public int chunkSize;
		public int chunkNum;
		public int objCount;
	}

	[System.Serializable]
	public class MapListData // use in select map popup
	{
        public string mapName;
        public int map_id;
    }

	[System.Serializable]
	public class MapSize
	{
		public float horizontal;
		public float vertical;
		public float height;
	}


	[System.Serializable]
	public class MapObjectData
	{
		public int map_id;
		public int version;
		public int chunkNum;
		public List<ObjectData> objList = new List<ObjectData>();
	}

	[System.Serializable]
	public class ObjectData
	{
		public int obj_id;
		public string ast_id;
		public ObjTransform transform = new ObjTransform();
		public string type;
		public bool isRigidbody = false;
		public bool isMeshCollider = false;
	}

	[System.Serializable]
	public class ObjTransform
	{
		public Position position = new Position();
		public Rotation rotation = new Rotation();
		public Scale scale = new Scale();
	}

	[System.Serializable]
	public class Position : ProtoVector
	{
		public float x;
		public float y;
		public float z;

		float ProtoVector.x { get => x; set { x = value; } }
		float ProtoVector.y { get => y; set { y = value; } }
		float ProtoVector.z { get => z; set { z = value; } }
	}

	[System.Serializable]
	public class Rotation : ProtoVector
	{
		public float x;
		public float y;
		public float z;

		float ProtoVector.x { get => x; set { x = value; } }
		float ProtoVector.y { get => y; set { y = value; } }
		float ProtoVector.z { get => z; set { z = value; } }
	}

	[System.Serializable]
	public class Scale : ProtoVector
	{
		public float x;
		public float y;
		public float z;

		float ProtoVector.x { get => x; set { x = value; } }
		float ProtoVector.y { get => y; set { y = value; } }
		float ProtoVector.z { get => z; set { z = value; } }
	}
}