using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Data;

namespace NextReality.Data
{
    public class PlayerLeaveSchema : Schema
    {
        public string leavePlayerId;

        public PlayerLeaveSchema(string message)
        {
            ParsingData(message);
        }

        public override void ParsingData(string message)
        {
            Debug.Log("PlayerLeaveSchema : " + message);
            string[] datas = message.Split(";");

            leavePlayerId = datas[0];
        }

    }
}
