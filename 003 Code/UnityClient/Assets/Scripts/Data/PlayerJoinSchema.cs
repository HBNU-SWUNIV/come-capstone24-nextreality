using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NextReality.Data
{
    public class PlayerJoinSchema : Schema
    {
        public string joinPlayerId;
        public string joinPlayerNickname;

        public PlayerJoinSchema(string message)
        {
            ParsingData(message);
        }

        public override void ParsingData(string message)
        {
            Debug.Log("PlayerJoinSchema : " + message);
            string[] datas = message.Split(";");

            SetData(datas[0], message, new DateTime(), new DateTime());

            joinPlayerId = datas[0];
            joinPlayerNickname = datas[1];
        }
    }
}

