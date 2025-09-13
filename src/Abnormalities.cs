using System;
using System.Collections.Generic;
using LobotomyCorpSaveManager.Exceptions;

namespace LobotomyCorpSaveManager.Abnormalities
{
	class Abnormality
	{
		static public List<int> All = new List<int> {
			100000,
			100001,
			100002,
			100003,
			100004,
			100005,
			100006,
			100007,
			100008,
			100009,
			100010,
			100011,
			100012,
			100013,
			100014,
			100015,
			100016,
			100017,
			100018,
			100019,
			100020,
			100021,
			100022,
			100023,
			100024,
			100025,
			100026,
			100027,
			100028,
			100029,
			100030,
			100031,
			100032,
			100033,
			100034,
			100035,
			100036,
			100037,
			100038,
			100039,
			100040,
			100041,
			100042,
			100043,
			100044,
			100045,
			100046,
			100047,
			100048,
			100049,
			100050,
			100051,
			100052,
			100053,
			100054,
			100055,
			100056,
			100057,
			100058,
			100059,
			100060,
			100061,
			100062,
			100063,
			100064,
			100065,
			100101,
			100102,
			100103,
			100104,
			100105,
			100106,
			200001,
			200002,
			200003,
			200004,
			200005,
			200006,
			200007,
			200009,
			200010,
			200013,
			200015,
			200016,
			300001,
			300002,
			300003,
			300004,
			300005,
			300006,
			300007,
			300101,
			300102,
			300103,
			300104,
			300105,
			300106,
			300107,
			300108,
			300109,
			300110,			
		};

		static public List<int> Tools = new List<int> {
			300001,
			300002,
			300003,
			300004,
			300005,
			300006,
			300007,
			300101,
			300102,
			300103,
			300104,
			300105,
			300106,
			300107,
			300108,
			300109,
			300110
		};

		static public List<int> Channeled = new List<int> {
			300003,
			300004,
			300006,
			300101,
			300105,
		};

		static public List<int> Equippable = new List<int> {
			300001,
			300002,
			300106,
			300107,
			300109,
		};

		static public List<int> Single = new List<int> {
			300005,
			300007,
			300102,
			300103,
			300104,
			300108,
			300110,
		};

		private readonly int id;

		public bool IsTool
		{
			get
			{
				return Abnormality.Tools.Contains(this.id);
			}
		}

		public string Type  // Sin? Maybe.
		{
			get
			{
				if (!this.IsTool)
				{
					return "normal";
				}
				if (Abnormality.Channeled.Contains(this.id))
				{
					return "tool_channeled";
				}
				if (Abnormality.Equippable.Contains(this.id))
				{
					return "tool_equippable";
				}
				if (Abnormality.Single.Contains(this.id))
				{
					return "tool_single";
				}
				throw new UnreachableCodeException();
			}
		}

		public Abnormality(int id)
		{
			this.id = id;
		}
	}
}