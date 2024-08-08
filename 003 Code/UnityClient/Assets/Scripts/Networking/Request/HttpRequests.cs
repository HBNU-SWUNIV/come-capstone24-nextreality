using NextReality.Asset;
using NextReality.Networking.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Networking;


namespace NextReality.Networking.Request
{
    public class HttpRequests : MonoBehaviour 
    {
        // http endpoint codes
        public enum ServerEndpoints
        {
            Login = 10,
            AssetUpload = 21,
            AssetSearch = 22,
            AssetInfo = 23,
            AssetDownload = 24,
            AssetDownPart = 25,
            MapUpload = 31,
            MapList = 32,
            MapCreate = 33,
            MapDownload = 34
        }

        private static HttpRequests instance = null;

        public string serverUrl;// = "http://172.25.17.134";
        public string assetServerUrl;// = "http://172.25.17.134:8080"
        public string loginServerUrl;// = "http://172.25.17.134:8000";
        public string mapServerUrl;// = "http://172.25.17.134:8070";

        public static HttpRequests Instance
        {
            get
            {
                if (instance == null)
                    return null;
                return instance;
            }
        }

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

        private void Start()
        {
            serverUrl = "http://" + Managers.Conf.GetConfigData("serverIP");
            assetServerUrl = serverUrl + ":" + Managers.Conf.GetConfigData("assetServerPort");
            loginServerUrl = serverUrl + ":" + Managers.Conf.GetConfigData("loginServerPort");
            mapServerUrl = serverUrl + ":" + Managers.Conf.GetConfigData("mapServerPort");
            Debug.Log("���� URL : " + serverUrl);
            Debug.Log("���¼��� URL : " + assetServerUrl);
            Debug.Log("�α��μ��� URL : " +  loginServerUrl);
            Debug.Log("�ʼ��� URL : " + mapServerUrl);
        }

        public string GetServerUrl(ServerEndpoints endpointCode)
        {
            switch (endpointCode)
            {
                case ServerEndpoints.Login:
                    return loginServerUrl + "/login";
                case ServerEndpoints.AssetUpload:
                    return assetServerUrl + "/asset_upload";
                case ServerEndpoints.AssetSearch:
                    return assetServerUrl + "/asset_search";
                case ServerEndpoints.AssetInfo:
                    return assetServerUrl + "/asset_info";
                case ServerEndpoints.AssetDownload:
                    return assetServerUrl + "/asset_down";
                case ServerEndpoints.AssetDownPart:
                    return assetServerUrl + "/asset_down_part";
                case ServerEndpoints.MapUpload:
                case ServerEndpoints.MapDownload:
                    return mapServerUrl + "/map_data";
                case ServerEndpoints.MapList:
                    return mapServerUrl + "/map_list";
                case ServerEndpoints.MapCreate:
                    return mapServerUrl + "/create_map";
            }
            return null;
        }


        // now we can use eazy!
        // ex : 
        // StartCoroutine(httpRequests.RequestPost(httpRequests.GetServerUrl(ServerEndpoints.Login), userDataJson, (callback) =>

        public bool CheckResult(string body)
        {
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(body);
            try
            {
                if (responseData != null)
                {
                    if (responseData.code.Equals("1"))
                        return true;

                }

            }
            catch (Exception ex)
            {
                Debug.Log("HttpRequests.CheckResult Error : " + ex);
                return false;
            }
            return false;
        }

        public IEnumerator RequestPost(string url, string json, Action<string> callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, json, "application/json"))
            {
                // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

                // request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                string result;

                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    result = "{\"code\" : \"0\", " +
                            "\"message\" : \"" + request.error + "\"}";
                }
                else
                {
                    result = request.downloadHandler.text;
                }
                // Debug.Log("Request Text : " + result);
                callback(result);
            }
        }

        public IEnumerator RequestGet(string url, Action<string> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            // response�� �� ������ �� �ѱ�
            string result;

            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                result = "{\"code\" : \"0\", " +
                        "\"message\" : \"" + request.error + "\"}";
            }
            else
            {
                result = request.downloadHandler.text;
            }
            // Debug.Log("Request Text : " + result);
            callback(result);
        }

        public IEnumerator RequestGet(string url, Dictionary<string, string> queryPair, Action<string> callback)
        {
            string urlWithQuery = url;

            if(queryPair != null && queryPair.Count > 0)
            {
                urlWithQuery += "?" + string.Join("&", queryPair.ToArray().Select(pair => pair.Key + "=" + pair.Value));
			}
            UnityWebRequest request = UnityWebRequest.Get(urlWithQuery);

            string result;

            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                result = "{\"code\" : \"0\", " +
                        "\"message\" : \"" + request.error + "\"}";
            }
            else
            {
                result = request.downloadHandler.text;
            }
            // Debug.Log("Request Text : " + result +"\n" + urlWithQuery);
            callback(result);
        }
    }
}

