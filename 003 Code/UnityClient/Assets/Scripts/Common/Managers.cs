using NextReality.Asset;
using NextReality.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextReality
{
	public static class Managers
	{
		public static GameCameraController Camera { get { return GameCameraController.Instance; } }
		public static ObjectEditorController ObjectEditor { get { return ObjectEditorController.Instance; } }
		public static GameInputManager Input { get { return GameInputManager.Instance; } }
		public static UserManager User { get { return UserManager.Instance; }}

		public static ClientManager Client { get { return ClientManager.Instance; }}
	}
}

