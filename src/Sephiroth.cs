using System;
using System.Collections.Generic;

namespace LobotomyCorpSaveManager.Sephiroth
{
	public class Sephirah
	{
		private enum SephirahType
		{
			Malkuth,
			Yesod,
			Hod,
			Netzach,
			Tiphereth,
			Gebura,
			Chesed,
			Daat,
			Binah,
			Hokma,
			Kether,
		}

		private readonly SephirahType type;

		private Sephirah(SephirahType type)
		{
			this.type = type;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Sephirah))
			{
				return false;
			}
			var s = obj as Sephirah;
			return s.type == this.type;
		}

		public override int GetHashCode()
		{
			return this.type.GetHashCode();
		}

		public override string ToString()
		{
			return ToStringMap[this];
		}

		public string ToLowerString()
		{
			return this.ToString().ToLowerInvariant();
		}

		public static Sephirah GetSephirahByGameIndex(string indexStr)  // I hate PM.
		{
			return GetSephirahByGameIndex(int.Parse(indexStr));
		}

		public static Sephirah GetSephirahByGameIndex(int index)
		{
			return GameIndexSephirahMap[index - 1];
		}

		public static Sephirah GetSephirahByGameString(string s)
		{
			return GameStringSephirahMap[s];
		}

		public static readonly Sephirah Malkuth = new Sephirah(SephirahType.Malkuth);
		public static readonly Sephirah Yesod = new Sephirah(SephirahType.Yesod);
		public static readonly Sephirah Hod = new Sephirah(SephirahType.Hod);
		public static readonly Sephirah Netzach = new Sephirah(SephirahType.Netzach);
		public static readonly Sephirah Tiphereth = new Sephirah(SephirahType.Tiphereth);
		public static readonly Sephirah Gebura = new Sephirah(SephirahType.Gebura);
		public static readonly Sephirah Chesed = new Sephirah(SephirahType.Chesed);
		public static readonly Sephirah Daat = new Sephirah(SephirahType.Daat);
		public static readonly Sephirah Binah = new Sephirah(SephirahType.Binah);
		public static readonly Sephirah Hokma = new Sephirah(SephirahType.Hokma);
		public static readonly Sephirah Kether = new Sephirah(SephirahType.Kether);
		private static readonly List<Sephirah> GameIndexSephirahMap = new List<Sephirah>()
		{
			Sephirah.Malkuth,
			Sephirah.Netzach,
			Sephirah.Hod,
			Sephirah.Yesod,
			Sephirah.Tiphereth,
			Sephirah.Tiphereth,
			Sephirah.Gebura,
			Sephirah.Chesed,
			Sephirah.Binah,
			Sephirah.Hokma,
			Sephirah.Kether,
		};
		private static readonly Dictionary<string, Sephirah> GameStringSephirahMap = new Dictionary<string, Sephirah>()
		{
			{ "Malkut", Sephirah.Malkuth },
			{ "Yesod", Sephirah.Yesod },
			{ "Hod", Sephirah.Hod },
			{ "Netzach", Sephirah.Netzach },
			{ "Tiphereth1", Sephirah.Tiphereth },
			{ "Tiphereth2", Sephirah.Tiphereth },
			{ "Geburah", Sephirah.Gebura },
			{ "Chesed", Sephirah.Chesed },
			{ "Daat", Sephirah.Daat },
			{ "Binah", Sephirah.Binah },
			{ "Chokhmah", Sephirah.Hokma },
			{ "Kether", Sephirah.Kether },
		};
		private static readonly Dictionary<Sephirah, string> ToStringMap = new Dictionary<Sephirah, string>()
		{
			{ Sephirah.Malkuth, "Malkuth"},
			{ Sephirah.Yesod, "Yesod"},
			{ Sephirah.Hod, "Hod"},
			{ Sephirah.Netzach, "Netzach"},
			{ Sephirah.Tiphereth, "Tiphereth"},
			{ Sephirah.Gebura, "Gebura"},
			{ Sephirah.Chesed, "Chesed"},
			{ Sephirah.Daat, "Daat"},
			{ Sephirah.Binah, "Binah"},
			{ Sephirah.Hokma, "Hokma"},
			{ Sephirah.Kether, "Kether"},
		};
		public static readonly List<Sephirah> All = new List<Sephirah>()
		{
			Sephirah.Malkuth,
			Sephirah.Yesod,
			Sephirah.Hod,
			Sephirah.Netzach,
			Sephirah.Tiphereth,
			Sephirah.Gebura,
			Sephirah.Chesed,
			Sephirah.Daat,
			Sephirah.Binah,
			Sephirah.Hokma,
			Sephirah.Kether,
		};
		public static readonly List<Sephirah> AllWithoutDaat = new List<Sephirah>()
		{
			Sephirah.Malkuth,
			Sephirah.Yesod,
			Sephirah.Hod,
			Sephirah.Netzach,
			Sephirah.Tiphereth,
			Sephirah.Gebura,
			Sephirah.Chesed,
			Sephirah.Binah,
			Sephirah.Hokma,
			Sephirah.Kether,
		};
	}
}
