using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NextReality.Data.Schema
{
	public interface ProtoVector
	{
		float x { get; set; }
		float y { get; set; }
		float z { get; set; }
	}

	public class ProtocolConverter
	{
		public List<string> stream;

		public int curIndex = 0;
		public int maxIndex = 0;

		bool isEncoder;

		public static string commandSeparator = "$";

		public ProtocolConverter(string message = null)
		{
			if(message == null)
			{
				stream = new List<string>();
				isEncoder = true;
			} else
			{
				stream = message.Split(";").ToList<string>();
				maxIndex = stream.Count - 1;

				isEncoder = false;
			}

			curIndex = 0;

		}

		// 프로토콜의 int 타입을 넘길 때
		public ProtocolConverter CastInt32(ref int data)
		{

			if(isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			} else
			{
				if (curIndex > maxIndex) return this;
				int.TryParse(stream[curIndex], out data);
				curIndex++;
				return this;
			}

		}

		// 프로토콜의 float 타입을 넘길 때
		public ProtocolConverter CastFloat32(ref float data)
		{
			if (isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				float.TryParse(stream[curIndex], out data);
				curIndex++;
				return this;
			}

		}

		// 프로토콜의 double 타입을 넘길 때
		public ProtocolConverter CastDouble64(ref double data)
		{
			if (isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				double.TryParse(stream[curIndex], out data);
				curIndex++;
				return this;
			}

		}

		// 프로토콜의 string 타입을 넘길 때
		public ProtocolConverter CastString(ref string data)
		{
			if (isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				data = stream[curIndex];
				curIndex++;
				return this;
			}
		}

		// 프로토콜의 bool 타입을 넘길 때
		public ProtocolConverter CastBool(ref bool data)
		{
			if (isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				bool.TryParse(stream[curIndex], out data);
				curIndex++;
				return this;
			}
		}

		// 프로토콜의 ProtoVector 타입을 넘길 때 (인스턴스의 프로퍼티의 접근할 때 사용)
		public ProtocolConverter CastVector<T>(T data) where T : ProtoVector
		{
			if (isEncoder)
			{
				string[] xyz =
				{
					data.x.ToString(),
					data.y.ToString(),
					data.z.ToString()
				};
				stream.Add(string.Join("/", xyz));
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				string[] temp = stream[curIndex].Split("/");
				data.x = float.Parse(temp[0]);
				data.y = float.Parse(temp[1]);
				data.z = float.Parse(temp[2]);
				curIndex++;
				return this;
			}
		}

		// 프로토콜의 ProtoVector 타입을 넘길 때
		public ProtocolConverter CastVector<T>(ref T data) where T : ProtoVector
		{
			return CastVector(data);
		}

		// 프로토콜의 벡터 타입을 넘길 때
		public ProtocolConverter CastVector(ref Vector3 data)
		{
			if (isEncoder)
			{
				string[] xyz =
				{
					data.x.ToString(),
					data.y.ToString(),
					data.z.ToString()
				};
				stream.Add(string.Join("/", xyz));
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				string[] temp = stream[curIndex].Split("/");
				float.TryParse(temp[0], out data.x);
				float.TryParse(temp[1], out data.y);
				float.TryParse(temp[2], out data.z);
				curIndex++;
				return this;
			}
		}

		// 프로토콜의 dateTime 타입의 레코드를 넘길 때
		public ProtocolConverter CastDateTime(ref DateTime data)
		{
			if (isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				DateTime.TryParse(stream[curIndex], out data);
				curIndex++;
				return this;
			}
		}
		public ProtocolConverter CastTicks(ref DateTime data)
		{
			if (isEncoder)
			{
				stream.Add(data.Ticks.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				long tick;
				long.TryParse(stream[curIndex], out tick);
				data = new DateTime(tick);
				curIndex++;
				return this;
			}
		}
		public ProtocolConverter CastMiliSeconds(ref DateTime data)
		{
			if (isEncoder)
			{
				var date = new DateTimeOffset(data);

				stream.Add(date.ToUnixTimeMilliseconds().ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(stream[curIndex]));
				// data = dateTimeOffset.UtcDateTime.AddHours(9);
                data = dateTimeOffset.UtcDateTime;
                curIndex++;
				return this;
			}
		}

		// 프로토콜의 그냥 레코드를 넘길 때
		public ProtocolConverter CastEmpty() {
			if (isEncoder)
			{
				stream.Add("");
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				curIndex++;
				return this;
			}
		}

		public ProtocolConverter Cast<T>(ref T data)
		{
			if (isEncoder)
			{
				stream.Add(data.ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				var tmp = (T)Convert.ChangeType(stream[curIndex], typeof(T));
				data = tmp;
				curIndex++;
				return this;
			}
		}

		// 클래스 인스턴스의 속성의 값을 연결할 때, data = 클래스 인스턴스, propetyname = nameof(클래스.프로퍼티명)
		public ProtocolConverter CastAnyFromProperty(object data, string propertyName)
		{
			var prop = data.GetType().GetField(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			if (isEncoder)
			{
				stream.Add(prop.GetValue(data).ToString());
				return this;
			}
			else
			{
				if (curIndex > maxIndex) return this;
				var tmp = Convert.ChangeType(stream[curIndex], prop.FieldType);

				prop.SetValue(data, tmp);
				curIndex++;
				return this;
			}
		}

		public string ToStream()
		{
			return string.Join(";", stream);
		}

		public bool GetEncoder()
		{
			return isEncoder;
		}
	}

}
