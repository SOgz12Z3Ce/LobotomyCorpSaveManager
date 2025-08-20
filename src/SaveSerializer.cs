using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LobotomyCorpSaveManager.Exceptions;
using LobotomyCorpSaveManager.Sephiroth;

namespace LobotomyCorpSaveManager.SaveSerializer
{
	abstract class SaveSerializerBase
	{
		private readonly string datFileName;
		private readonly string jsonFileName;

		protected SaveSerializerBase(string datFileName, string jsonFileName)
		{
			this.datFileName = datFileName;
			this.jsonFileName = jsonFileName;
		}

		// TODO: add func SerializeToDat

		private void SerializeToJson(JObject data, string path)
		{
			File.WriteAllText(path, data.ToString());
		}

		private void SerializeToJson(JObject data)
		{
			string currentDir = Directory.GetCurrentDirectory();
			string path = Path.Combine(currentDir, jsonFileName);
			SerializeToJson(data, path);
		}

		public JObject Deserialize(string path)
		{
			string fileName = Path.GetFileName(path);
			if (fileName == this.datFileName)
			{
				return this.Reorganize(this.DeserializeDat(path));
			}
			if (fileName == this.jsonFileName)
			{
				return this.DeserializeJson(path);
			}
			throw new BadFileException(string.Format("Expected \"{0}\" or \"{1}\", got \"{2}\".", this.datFileName, this.jsonFileName, fileName));
		}
		
		protected abstract JObject Reorganize(JObject save);

		private JObject DeserializeJson(string path)
		{
			FileStream stream = File.OpenRead(path);
			string json = File.ReadAllText(path);
			stream.Close();
			return JObject.Parse(json);
		}

		private JObject DeserializeDat(string path)
		{
			FileStream stream = File.OpenRead(path);
			var data = new BinaryFormatter().Deserialize(stream);
			stream.Close();
			return JObject.FromObject(data);
		}
	}

	class SettingsSaveSerializer : SaveSerializerBase
	{
		static private Dictionary<string, string> languageMap = new Dictionary<string, string>  // convert to BCP 47
		{
			{ "en",    "en" },
			{ "kr",    "ko" },
			{ "cn",    "zh-Hans" },
			{ "cn_tr", "zh-Hant" },
			{ "jp",    "ja" },
			{ "ru",    "ru" },
			{ "vn",    "vi" },  // Who is Misui? Check OptionUI.credit[4] and call OptionUI.OnSetLanguage("vn")!
			{ "bg",    "bg" },
			{ "es",    "es-419" },
			{ "fr",    "fr" },
			{ "pt_br", "pt-BR" },
			{ "pt_pt", "pt-PT" },
		};

		public SettingsSaveSerializer() : base("Lobotomy170808state.dat", "settings.json")
		{
		}

		protected override JObject Reorganize(JObject save)
		{
			var ret = new JObject();

			ret["masterVolume"] = save["masterVolume"];
			ret["bgmVolume"] = save["bgmVolume"];
			ret["enableTooltip"] = save["tooltip"];
			ret["enableDlcAbnormalities"] = save["dlcCreatureOn"];
			ret["logIndex"] = save["logIndex"];
			ret["language"] = languageMap[save.Value<string>("language")];

			return ret;
		}
	}

	class EtcSaveSerializer : SaveSerializerBase
	{
		public EtcSaveSerializer() : base("etc170808.dat", "etc.json")
		{
		}

		protected override JObject Reorganize(JObject save)
		{
			var ret = new JObject();

			ret["isCoreSuppressionTutorialPlayed"] = save["sefirabossTutorialPlayed"];
			ret["isCoreSuppressionTutorialPlayed"] = save["sefirabossTutorialPlayed"];
			ret["isKetherCoreSuppression1Completed"] = save["e0"];
			ret["isKetherCoreSuppression2Completed"] = save["e1"];
			ret["isKetherCoreSuppression3Completed"] = save["e2"];
			ret["isKetherCoreSuppression4Completed"] = save["e3"];
			ret["isKetherCoreSuppression5Completed"] = save["e4"];
			ret["extractedAbnormalitiesIdQueue"] = save["waitingCreature"];

			return ret;
		}
	}

	class MasterSaveSerializer : SaveSerializerBase
	{
		public MasterSaveSerializer() : base("saveData170808.dat", "master.json")
		{
		}

		private class DayRetBuilder
		{
			private JObject ret;
			private JObject save;

			public DayRetBuilder(JObject save)
			{
				var sephiroth = new JObject();
				foreach (Sephirah s in Sephirah.All)
				{
					sephiroth[s.ToLowerString()] = new JObject();
				}
				this.ret = new JObject();
				this.ret["sephiroth"] = sephiroth;
				this.save = save;
			}

			public JObject build()
			{
				return ret;
			}

			/*
				Ignored field(s):
				- "saveInnerVer": Always "ver1".
				- "saveState": Always "manage". See `LobotomyCorp save.md`.
				- "playerData": Not necessary (substituted with "day").
			*/
			public DayRetBuilder AddBasicInfo()
			{
				this.ret["day"] = this.save["day"];
				this.ret["lobPoints"] = this.save["money"]["money"];

				return this;
			}

			/*
				Ignored field(s):
				- "activated": Not necessary (Inferable, from "openLevel").
			*/
			public DayRetBuilder AddExpansionLevel()
			{
				foreach (KeyValuePair<string, JToken> kvp in this.save["sefiras"] as JObject)
				{
					var sephirah = Sephirah.GetSephirahByGameString(kvp.Key);
					this.ret["sephiroth"][sephirah.ToLowerString()]["expansionLevel"] = kvp.Value["openLevel"];
				}

				return this;
			}

			/*
				Ignored field(s):
				- "nextInstId": Not necessary (Inferable, from "creatureList").
			*/
			public DayRetBuilder AddAbnormalities()
			{
				foreach (Sephirah s in Sephirah.AllWithoutDaat)
				{
					this.ret["sephiroth"][s.ToLowerString()]["abnormalities"] = new JObject();
				}
				var Abnormalities = new Dictionary<Sephirah, Dictionary<string, object>[]>()
				{
					{ Sephirah.Malkuth, new Dictionary<string, object>[4] },
					{ Sephirah.Yesod, new Dictionary<string, object>[4] },
					{ Sephirah.Hod, new Dictionary<string, object>[4] },
					{ Sephirah.Netzach, new Dictionary<string, object>[4] },
					{ Sephirah.Tiphereth, new Dictionary<string, object>[8] },
					{ Sephirah.Gebura, new Dictionary<string, object>[4] },
					{ Sephirah.Chesed, new Dictionary<string, object>[4] },
					{ Sephirah.Binah, new Dictionary<string, object>[4] },
					{ Sephirah.Hokma, new Dictionary<string, object>[4] },
					{ Sephirah.Kether, new Dictionary<string, object>[8] },
				};

				foreach (JToken abnormalitySave in this.save["creatures"]["creatureList"] as JArray)
				{
					var sephirah = Sephirah.GetSephirahByGameIndex(abnormalitySave.Value<string>("sefiraNum"));
					int index = Sephirah.GetContainmentUnitIndexByGameEntryNodeId(abnormalitySave.Value<string>("entryNodeId"));
					Abnormalities[sephirah][index] = new Dictionary<string, object>();
					Abnormalities[sephirah][index]["id"] = abnormalitySave["metadataId"];
					Abnormalities[sephirah][index]["index"] = abnormalitySave["instanceId"];
				}
				foreach (Sephirah s in Sephirah.AllWithoutDaat)
				{
					foreach (Dictionary<string, object> abnormality in Abnormalities[s])
					{
						if (abnormality == null) break;
						var abnormalitiesList = this.ret["sephiroth"][s.ToLowerString()]["abnormalities"] as JArray;
						abnormalitiesList.Add(abnormality);
					}
				}

				return this;
			}

			public DayRetBuilder AddAgents()
			{
				foreach (Sephirah s in Sephirah.AllWithoutDaat)
				{
					this.ret["sephiroth"][s.ToLowerString()]["agents"] = new JObject();
				}

				foreach (JToken agentSave in this.save["agent"]["agentList"] as JArray)
				{
					var sephirah = Sephirah.GetSephirahByGameIndex(agentSave.Value<string>("sefira"));
					var sephirahAgents = this.ret["sephiroth"][sephirah.ToLowerString()]["agents"] as JArray;

					var agentRet = new JObject();
					agentRet["name"] = agentSave["name"];
					agentRet["workFail"] = agentSave["history"]["workFail"];
					//TODO: add rest data
					sephirahAgents.Add(agentRet);
				}

				return this;
			}
		}

		protected override JObject Reorganize(JObject save)
		{
			var ret = new JObject();

			// Basic data
			ret["playtime"] = save["playTime"];
			int currentDay = save.Value<int>("lastDay");
			int memoryRepositoryDay = save.Value<int>("checkPointDay");
			/*
				Ignored field(s):
				- "saveVer": Always "ver1".
			*/

			// Days data
			ret["days"] = new JObject();
			ret["days"]["current"] = this.GetDayRet(save["dayList"][currentDay] as JObject);
			ret["days"]["memoryRepository"] = this.GetDayRet(save["dayList"][memoryRepositoryDay] as JObject);

			return ret;
		}

		private JObject GetDayRet(JObject save)
		{
			return new DayRetBuilder(save).AddBasicInfo()
			                              .AddExpansionLevel()
			                              .AddAbnormalities()
			                              .AddAgents()
			                              .build();
		}
	}
}
