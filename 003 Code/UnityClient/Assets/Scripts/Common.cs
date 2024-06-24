using NextReality.Data;
using NextReality.Game;
using NextReality.Asset;
using NextReality.Game.UI;
using NextReality.Utility;
using NextReality.Networking;
using NextReality.Networking.Request;

namespace NextReality
{
	public static class Managers
	{
		public static GameCameraController Camera { get { return GameCameraController.Instance; } }
		public static ObjectEditorController ObjectEditor { get { return ObjectEditorController.Instance; } }
		public static GameInputManager Input { get { return GameInputManager.Instance; } }
		public static UserManager User { get { return UserManager.Instance; } }

		public static ClientManager Client { get { return ClientManager.Instance; } }

		public static MapDataController Map { get { return MapDataController.Instance; } }

		public static GameCharacterManager Chacacter { get { return GameCharacterManager.Instance; } }
		public static SceneChangeManager Scene { get { return SceneChangeManager.Instance; } }
		public static BroadcastHandler BroadcastHandler { get { return BroadcastHandler.Instance; } }
        public static NetworkManager Network { get { return NetworkManager.Instance; } }
		public static GltfRoutineManager Gltf { get { return GltfRoutineManager.Instance; } }
		public static ConfigManager Conf { get { return ConfigManager.Instance; } }
    }

    public static class Utilities
    {
        public static HttpRequests HttpUtil { get { return HttpRequests.Instance; } }
        // public static UdpRequests UdpUtil { get { return UdpRequests.Instance; } }
        public static MessageSetter MessageUtil { get { return MessageSetter.Instance; } }
    }

    public enum Layers
    {
        Defaut = 0,
        UI = 5,
        EditableObject = 6,
        PreviewObject = 7,
        Transpanercy = 8
    }
}

