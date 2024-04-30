using NextReality.Data;
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
    }

    [System.Serializable]

    public class LoginResponseData : ResponseData
    {
        public new UserData message;
    }
}

