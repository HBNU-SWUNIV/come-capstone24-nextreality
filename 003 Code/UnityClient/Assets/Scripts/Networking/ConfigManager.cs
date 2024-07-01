using NextReality.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace NextReality.Networking
{
    public class ConfigManager : MonoBehaviour
    {
        private Dictionary<string, string> configs = new Dictionary<string, string>();
        public string configFileName = "config.txt";
        private string configFilePath;

        private static ConfigManager instance = null;

        public static ConfigManager Instance
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

        private void Awake()
        {
            if (null == instance)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            configFilePath = Path.Combine(Application.streamingAssetsPath, configFileName);

            LoadConfig();
        }

        public void LoadConfig()
        {
            if (true||File.Exists(configFilePath))
            {
                string config = File.ReadAllText(configFilePath);

                string[] tuples = config.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                
                foreach (var tuple in tuples)
                {
                    string[] KeyValue = tuple.Replace("\r", "").Split('\t');
                    configs.Add(KeyValue[0], KeyValue[1]);
                }

                //Debug.Log("Config loaded: " + config);
            }
            else
            {
                Debug.LogError("Config file not found at path: " + configFilePath);
            }
        }

        public string GetConfigData(string key)
        {
            //Debug.Log("Get Url: " +  key + configs[key]);
            if (configs.ContainsKey(key))
                return configs[key];
            else return null;
        }
    }
}