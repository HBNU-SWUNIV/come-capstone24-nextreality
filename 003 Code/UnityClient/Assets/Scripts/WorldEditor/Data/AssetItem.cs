using System;
using UnityEngine;

namespace NextReality.Asset
{
    [Serializable]
    public class AssetItem : AssetUpload
    {
        public string id;
        public Texture2D thumbnail2D;
        public byte[] fileByte;

        /*
    {
        get
        {
            if(temp == null)
            {
                temp =
            }
            return temp;
        }
    }

        */
        // Texture2D temp;
        // public byte[] _thumbnail;
        public DateTime UploadDate;
        public int DownloadCount;
    }

    // AssetUplaod�� request�� body�� ������
    // response�� message�δ� ������ ���¿� �ο��� ID�� �����
    [Serializable]
    public class AssetUpload
    {
        public string name;
        public int categoryid;
        public string thumbnail; // byte �迭 -> string
                                 // public byte[] thumbnail;
        public string thumbnailext; // ����� Ȯ���ڸ� (jpg, png)
        public string file; // byte �迭 -> string
                            // public byte[] file;
                            // public string uploaddate; // 2024-05-08

        public int downloadcount;
        public int price;
        public bool isdisable;
    }


    // �ʿ������ �̰� ������ ��
    [Serializable]
    public class AssetDownload
    {
        public string id;
        public string file;
    }
}


