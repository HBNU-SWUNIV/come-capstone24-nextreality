using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextReality.Asset;
using System;

namespace NextReality.Data.Schema
{
    public class PlayerJumpSchema : ZSchema
    {
        public string jumpPlayerId
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
                return "PlayerJump";

            }
        }

        public Vector3 jumpPosition;
        public Vector3 jumpRotation;

        public PlayerJumpSchema() : base() { }

        public PlayerJumpSchema(string message) : base(message)
        {

        }

        public PlayerJumpSchema(Vector3 jumpPosition, Vector3 jumpRotation) : this()
        {
            this.jumpPosition = jumpPosition;
            this.jumpRotation = jumpRotation;
        }

        protected override ProtocolConverter GetProtocolStreamByIndividual(ProtocolConverter prev)
        {
            //if (!prev.GetEncoder())
            //{
            //    Debug.Log("PlayerMoveSchema : " + message);
            //}

            return prev.CastVector(ref jumpPosition)
                .CastVector(ref jumpRotation);
        }

    }
}

