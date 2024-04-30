using System;
using System.Collections;
using UnityEngine;

namespace NextReality.Data
{
	public class MapChangeSchema : Schema
	{
		public int map_id;


		public MapChangeSchema(string message)
		{
			ParsingData(message);
		}

		public override void ParsingData(string message)
		{
			string[] datas = message.Split(";");

			SetData(datas[0], message, new DateTime(), new DateTime());

			map_id = Convert.ToInt32(datas[2]);
		}
	}
}