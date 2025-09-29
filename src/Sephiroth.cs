using System;
using System.Collections.Generic;
using LobotomyCorpSaveManager.Exceptions;

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
		
		public static int GetContainmentUnitIndexByGameEntryNodeId(string entryNodeId)
		{
			int ret = -1;
			foreach (KeyValuePair<Sephirah, Dictionary<string, int>> kvp in ContainmentUnitIndexMap)
			{
				if (kvp.Value.TryGetValue(entryNodeId, out ret))
				{
					return ret;
				}
			}
			throw new BadEntryNodeIdException(string.Format("\"{0}\" is not an entry node for containment unit."));
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
		private static readonly Dictionary<Sephirah, Dictionary<string, int>> ContainmentUnitIndexMap = new Dictionary<Sephirah, Dictionary<string, int>>
		{
			{
				Sephirah.Malkuth,
				new Dictionary<string, int>
				{
					{ "left-upper-way5", 0 },
					{ "right-upper-way4", 1 },
					{ "left-down-way2", 2 },
					{ "right-down-way2", 3 },
				}
			},
			{
				Sephirah.Yesod,
				new Dictionary<string, int>
				{
					{ "T-left-way4", 0 },
					{ "T-right-way4", 1 },
					{ "T-leftdown-way3", 2 },
					{ "T-rightdown-way3", 3 },
				}
			},
			{
				Sephirah.Hod,
				new Dictionary<string, int>
				{
					{ "H-upper-way4", 0 },
					{ "H-upper-way2", 1 },
					{ "H-down-way4", 2 },
					{ "H-down-way2", 3 },
				}
			},
			{
				Sephirah.Netzach,
				new Dictionary<string, int>
				{
					{ "N-upper-way2", 0 },
					{ "N-upper-way4", 1 },
					{ "N-down-way2", 2 },
					{ "N-down-way4", 3 },
				}
			},
			{
				Sephirah.Tiphereth,
				new Dictionary<string, int>
				{
					{ "Tiphereth1-left-way2", 0 },
					{ "Tiphereth1-left-way5", 1 },
					{ "Tiphereth1-right-way3", 2 },
					{ "Tiphereth1-right-way6", 3 },
					{ "Tiphereth2-left-way2", 4 },
					{ "Tiphereth2-left-way5", 5 },
					{ "Tiphereth2-right-way3", 6 },
					{ "Tiphereth2-right-way6", 7 },
				}
			},
			{
				Sephirah.Gebura,
				new Dictionary<string, int>
				{
					{ "Geburah-rb-way2", 0 },
					{ "Geburah-rb-way4", 1 },
					{ "Geburah-ru-way2", 2 },
					{ "Geburah-ru-way4", 3 },
				}
			},
			{
				Sephirah.Chesed,
				new Dictionary<string, int>
				{
					{ "Chesed-rb-way4", 0 },
					{ "Chesed-rb-way2", 1 },
					{ "Chesed-ru-way4", 2 },
					{ "Chesed-ru-way2", 3 },
				}
			},
			{
				Sephirah.Binah,
				new Dictionary<string, int>
				{
					{ "binah-way3", 0 },
					{ "binah-way4", 1 },
					{ "binah-way5", 2 },
					{ "binah-way6", 3 },
				}
			},
			{
				Sephirah.Hokma,
				new Dictionary<string, int>
				{
					{ "chokhmah-way6", 0 },
					{ "chokhmah-way5", 1 },
					{ "chokhmah-way4", 2 },
					{ "chokhmah-way3", 3 },
				}
			},
			{
				Sephirah.Kether,
				new Dictionary<string, int>
				{
					{ "kether-leftway2", 0 },
					{ "kether-rightway2", 1 },
					{ "kether-leftway3", 2 },
					{ "kether-rightway3", 3 },
					{ "kether-leftway5", 4 },
					{ "kether-rightway5", 5 },
					{ "kether-leftway6", 6 },
					{ "kether-rightway6", 7 },
				}
			},
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
		public static readonly List<Sephirah> AllWithoutDaatAndKether = new List<Sephirah>()
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
		};
	}
}
