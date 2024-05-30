using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Asset;
using System;

namespace NextReality.Data.Schema
{
    public class PlayerMoveSchema : ZSchema
    {
        public string movePlayerId
        {
            get
            {
                return userId;
            }
        }

        public override string SchemaType
        {
            get
            {
                return "PlayerMove";

            }
        }

        public Vector3 movePosition;
        public Vector3 moveRotation;

        public PlayerMoveSchema() : base() { }

        public PlayerMoveSchema(string message) : base(message)
        {

        }

        public PlayerMoveSchema(Vector3 movePosition, Vector3 moveRotation) : this()
        {
            this.movePosition = movePosition;
            this.moveRotation = moveRotation;
        }

        protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
        {
            //if (!prev.GetEncoder())
            //{
            //    Debug.Log("PlayerMoveSchema : " + message);
            //}

            return prev.CastVector(ref movePosition)
                .CastVector(ref moveRotation);
        }

    }
}

