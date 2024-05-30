using NextReality.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NextReality.Utility
{
	public enum SceneName
	{
		MainMenu = 0,
		Game = 1,
		LoadingScene = 2
	}
	public class SceneChangeManager : MonoBehaviour
	{


		public struct TypeObjectPair {
			public Type type; 
			public object target; 

			public TypeObjectPair(Type t, object o)
			{
				type =t; target = o;
			}
		}


		// ΩÃ±€≈Ê
		private static SceneChangeManager instance = null;

		private Dictionary<string, TypeObjectPair> sceneParameterMap = new Dictionary<string, TypeObjectPair>();

		private void Awake()
		{
			if (null == instance)
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}
		}

		public static SceneChangeManager Instance
		{
			get
			{
				if (null == instance)
				{
					return null;
				}
				return instance;
			}
		}

		public void TransformScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}

		public void TransformScene(SceneName sceneName)
		{
			TransformScene(GetSceneName(sceneName));
		}

		public void LoadScene(string sceneName, LoadSceneMode mode, bool isAsync = false)
		{
			if(isAsync) SceneManager.LoadSceneAsync(sceneName, mode);
			else SceneManager.LoadScene(sceneName, mode);
		}
		public void LoadScene(SceneName sceneName, LoadSceneMode mode, bool isAsync = false)
		{
			LoadScene(GetSceneName(sceneName), mode, isAsync);
		}

		public void UnLoadScene(string sceneName)
		{
			SceneManager.UnloadSceneAsync(sceneName);
		}
		public void UnLoadScene(SceneName sceneName)
		{
			UnLoadScene(GetSceneName(sceneName));
		}

		public void LoadGame()
		{
			TransformScene(SceneName.Game);
			LoadScene(SceneName.LoadingScene, LoadSceneMode.Additive);
		}

		public void LoadMainMenu()
		{
			TransformScene(SceneName.MainMenu);
		}

		public void AddSceneParameter<T>(string name, T obj)
		{
			var value = new TypeObjectPair(typeof(T), obj);
			if (sceneParameterMap.ContainsKey(name))
			{
				sceneParameterMap[name] = value;
			} else {
				sceneParameterMap.Add(name, value);
			}
				
		}

		public bool GetSceneParameter<T> (string name, out T data)
		{
			if(sceneParameterMap.TryGetValue(name, out var obj))
			{
				data = (T)sceneParameterMap[name].target;
				return true;
			} else
			{
				data = default(T);
				return false;
			}
		}

		public string GetSceneName(SceneName sceneName)
		{
			switch(sceneName)
			{
				case SceneName.MainMenu: return "FirstScene";
				case SceneName.Game: return "InGameMap";
				case SceneName.LoadingScene: return "LoadingScene";
				default: return "";
			}
		}

	}

}
