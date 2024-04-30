using GLTFast;

namespace NextReality.Asset.Routine
{
	// Routine에게 부여되는 Task의 정보가 담겨 있는 클래스
	public class LoadTask
	{
		public string userId;
		public int astId;
		public AssetObject gltfObj;

		public bool isDownSuccess = false;
		public bool isLoadSuccess = false;
		public bool isWearSuccess = false;
		public bool isAllSuccess = false;

		public bool isFailOrStop = false;

		public GltfImport gltfImport;


		public LoadTask(string user_id, int ast_id, AssetObject astObj)
		{
			userId = user_id;
			astId = ast_id;
			gltfObj = astObj;

			TaskInit();
		}

		public void TaskInit()
		{
			isDownSuccess = false;
			isLoadSuccess = false;
			isWearSuccess = false;
			isAllSuccess = false;
			isFailOrStop = false;

			gltfImport = new GltfImport();
		}
	}
}