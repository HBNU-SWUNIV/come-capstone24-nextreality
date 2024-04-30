using NextReality.Asset;
using System;
using UnityEngine;

namespace NextReality.Data
{
    public class AssetCreateSchema : Schema
    {
        public ObjectData objData;


        public AssetCreateSchema(string message)
        {
            ParsingData(message);
        }

        public override void ParsingData(string message)
        {
            string[] datas = message.Split(";");

            SetData(datas[0], message, new DateTime(), new DateTime());

            objData = new ObjectData();
            objData.ast_id = Convert.ToInt32(datas[2]);
            objData.obj_id = Convert.ToInt32(datas[3]);

            string[] pos = datas[4].Split("/");
            objData.transform.position = new Position { x = int.Parse(pos[0]), y = int.Parse(pos[1]), z = int.Parse(pos[2]) };
            string[] rot = datas[5].Split("/");
            objData.transform.rotation = new Rotation { x = int.Parse(rot[0]), y = int.Parse(rot[1]), z = int.Parse(rot[2]) };
            string[] scl = datas[6].Split("/");
            objData.transform.scale = new Scale { x = int.Parse(scl[0]), y = int.Parse(scl[1]), z = int.Parse(scl[2]) };

            objData.type = "Object";
            objData.isMeshCollider = datas[7] == "true";
            objData.isRigidbody = datas[8] == "true";
        }

    }
}