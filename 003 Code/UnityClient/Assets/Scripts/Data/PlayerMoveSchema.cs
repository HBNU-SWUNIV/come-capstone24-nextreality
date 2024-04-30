using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Asset;
using System;

namespace NextReality.Data
{
    public class PlayerMoveSchema : Schema
    {
        public string movePlayerId;
        public Vector3 movePosition;
        public Vector3 moveRotation;


        public PlayerMoveSchema(string message)
        {
            ParsingData(message);
        }

        public override void ParsingData(string message)
        {
            string[] datas = message.Split(";");

            SetData(datas[0], message, new DateTime(), new DateTime());

            movePlayerId = datas[0];

            string[] pos = datas[1].Split("/");
            movePosition = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            string[] rot = datas[2].Split("/");
            moveRotation = new Vector3(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]));

        }
    }
}

