using GLTFast;
using NextReality.Asset;
using NextReality.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

namespace NextReality.Networking.Response
{
    [System.Serializable]
    public class ResponseData
    {
        public string code; // ���Ŀ� int�� �ٲٴ� ���� ���ƺ���
        public string message;

        public bool CheckResult()
        {
            try
            {
                if (this != null)
                {
                    if (code != null && code.Equals("1"))
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
    }

    [System.Serializable]

    public class LoginResponseData : ResponseData
    {
        public new UserData message;
    }

    [System.Serializable]

    public class MapListResponseData : ResponseData
    {
        public new MapListData[] message;
    }

    /*
    [System.Serializable]
    
    public class AssetFullResponseData : ResponseData
    {
        public new Asset
    }
    */

  
    [System.Serializable]
    // ���� ��ο� â���� �˻��� ����� �޾ƿ� �� ���
    public class AssetQueryResponseData : ResponseData
    {
        public AssetQuery[] data;
        // AssetQuery : id, name
    }

    [System.Serializable]
    public class AssetPreviewResponseData : ResponseData
    {
        public AssetImage[] data;
        // AssetImage : id, image(string�̶� �ٲ����)
    }

    [System.Serializable]
    public class AssetID
    {
        public string id;
        
    }

    [System.Serializable]
    public class AssetQuery : AssetID
    {
        public string name;
    }

    [System.Serializable]
    public class AssetImage : AssetID
    {
        public string thumbnail;
    }
    
    [System.Serializable]
    public class MapResponseData : ResponseData
    {
        public new MapData message;
    }

    [System.Serializable]
    public class ObjResponseData : ResponseData
    {
        public new MapObjectData message;
    }

    [System.Serializable]
    public class  AstResponseData : ResponseData
    {
        public List<AssetDownload> data;
    }

    [System.Serializable]
    public class CreatorListResponseData : ResponseData
    {
        public new MapCreatorInfo message;
	}
}

