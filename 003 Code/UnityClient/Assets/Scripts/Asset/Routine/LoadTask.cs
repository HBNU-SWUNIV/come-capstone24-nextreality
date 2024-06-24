using GLTFast;
using NextReality.Data;

namespace NextReality.Asset.Routine
{
	// Routine에게 부여되는 Task의 정보가 담겨 있는 클래스
	public class LoadTask
	{
		public string userId;
		public string astId;
		public AssetObject gltfObj;
		public ObjectData objData;

		public bool isDownSuccess = false;
		public bool isLoadSuccess = false;
		public bool isWearSuccess = false;
		public bool isAllSuccess = false;

		public bool isFailOrStop = false;
		public bool isExist = false;

		public GltfImport gltfImport;


		public LoadTask(string user_id, string ast_id, ObjectData objInfoData)
		{
			userId = user_id;
			astId = ast_id;
			objData = objInfoData;

			TaskInit();
		}

		public void TaskInit()
		{
			isDownSuccess = false;
			isLoadSuccess = false;
			isWearSuccess = false;
			isAllSuccess = false;
			isFailOrStop = false;
			isExist = false;

			gltfImport = new GltfImport();
		}
	}
}