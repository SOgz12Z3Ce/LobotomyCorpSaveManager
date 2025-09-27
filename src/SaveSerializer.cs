using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LobotomyCorpSaveManager.Exceptions;
using LobotomyCorpSaveManager.Sephiroth;
using LobotomyCorpSaveManager.Abnormalities;

namespace LobotomyCorpSaveManager.SaveSerializer
{
	abstract class SaveSerializerBase
	{
		// static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
		// {
		// 	ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		// };
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

		protected abstract JObject Reorganize(Dictionary<string, object> rawSave);

		private JObject DeserializeJson(string path)
		{
			FileStream stream = File.OpenRead(path);
			string json = File.ReadAllText(path);
			stream.Close();
			return JObject.Parse(json);
		}

		private Dictionary<string, object> DeserializeDat(string path)
		{
			FileStream stream = File.OpenRead(path);
			var data = new BinaryFormatter().Deserialize(stream) as Dictionary<string, object>;
			return data;
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

		protected override JObject Reorganize(Dictionary<string, object> rawSave)
		{
			var ret = new JObject();
			var save = JObject.FromObject(rawSave);

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

		protected override JObject Reorganize(Dictionary<string, object> rawSave)
		{
			var ret = new JObject();
			var save = JObject.FromObject(rawSave);

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
				foreach (Sephirah s in Sephirah.AllWithoutDaat)
				{
					sephiroth[s.ToLowerString()] = new JObject();
				}
				this.ret = new JObject();
				this.ret["sephiroth"] = sephiroth;
				this.save = save;
			}

			public JObject Build()
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
					if (sephirah == Sephirah.Daat) continue;
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
					this.ret["sephiroth"][s.ToLowerString()]["abnormalities"] = new JArray();
				}
				var abnormalities = new Dictionary<Sephirah, Dictionary<string, int>[]>()
				{
					{ Sephirah.Malkuth, new Dictionary<string, int>[4] },
					{ Sephirah.Yesod, new Dictionary<string, int>[4] },
					{ Sephirah.Hod, new Dictionary<string, int>[4] },
					{ Sephirah.Netzach, new Dictionary<string, int>[4] },
					{ Sephirah.Tiphereth, new Dictionary<string, int>[8] },
					{ Sephirah.Gebura, new Dictionary<string, int>[4] },
					{ Sephirah.Chesed, new Dictionary<string, int>[4] },
					{ Sephirah.Binah, new Dictionary<string, int>[4] },
					{ Sephirah.Hokma, new Dictionary<string, int>[4] },
					{ Sephirah.Kether, new Dictionary<string, int>[8] },
				};
				foreach (JToken abnormalitySave in this.save["creatures"]["creatureList"] as JArray)
				{
					if (abnormalitySave.Value<string>("sefiraNum") == "0") continue;  // TODO: handle backup agents
					var sephirah = Sephirah.GetSephirahByGameIndex(abnormalitySave.Value<string>("sefiraNum"));
					int index = Sephirah.GetContainmentUnitIndexByGameEntryNodeId(abnormalitySave.Value<string>("entryNodeId"));
					abnormalities[sephirah][index] = new Dictionary<string, int>()
					{
						{ "id", (int)abnormalitySave.Value<long>("metadataId") },
						{ "index", (int)abnormalitySave.Value<long>("instanceId") },
					};
				}
				foreach (Sephirah s in Sephirah.AllWithoutDaat)
				{
					var abnormalitiesList = this.ret["sephiroth"][s.ToLowerString()]["abnormalities"] as JArray;
					foreach (Dictionary<string, int> abnormality in abnormalities[s])
					{
						if (abnormality == null) break;
						abnormalitiesList.Add(JObject.FromObject(abnormality));
					}
				}

				return this;
			}

			/*
				Ignored field(s):
				- "currentSefira": Not necessary (Inferable, from key).
				- "sefira": Not necessary (Inferable, from key).
				- "name": Not necessary (Inferable, from "customName" and "nameId").
				- "baseMovement": Always 0.
				- "baseMaxHp": Always 0.
				- "baseMaxMental": Always 0.
				- "isUniqueCredit": Not necessary (Inferable, from "customName").
				- "uniqueScriptIndex": Not necessary (Inferable, from "customName").
				- "isAce": Not necessary (Inferable, from all agents).
				- "gifts":
					- "giftTypeIdList": Not necessary (Inferable, from "lockState").
				- "history":
					- "workSuccess": Always 0.
					- "physicalDamage": Always 0.
					- "mentalDamage": Always 0.
					- "deathByCreature": Always 0.
					- "panicByCreature": Always 0.
					- "deathByWorker": Always 0.
					- "panic": Always 0.
					- "creatureDamage": Always 0.
					- "workerDamage": Always 0.
					- "panicWorkerDamage": Always 0.
					- "suppressDamage": Always 0.
					- "disposition": Always 0.
				- "spriteSet":
					- "bInit": Always False.
					- "AttachmentHair": Always {0, 0}.
					- "EyeColor":
						- "a": always 1.0.
					- "HairColor":
						- "a": always 1.0.
			*/
			public DayRetBuilder AddAgents()
			{
				foreach (Sephirah s in Sephirah.AllWithoutDaat)
				{
					this.ret["sephiroth"][s.ToLowerString()]["agents"] = new JArray();
				}

				foreach (JToken agentSave in this.save["agents"]["agentList"] as JArray)
				{
					var sephirah = Sephirah.GetSephirahByGameIndex(agentSave.Value<string>("sefira"));
					var sephirahAgents = this.ret["sephiroth"][sephirah.ToLowerString()]["agents"] as JArray;

					var agentRet = new JObject();

					// basic info
					agentRet["index"] = (int)agentSave.Value<long>("instanceId");
					agentRet["continuousServiceDay"] = agentSave["continuousServiceDay"];

					// stats
					agentRet["stats"] = new JObject();
					agentRet["stats"]["fortitude"] = agentSave["primaryStat"]["hp"];
					agentRet["stats"]["prudence"] = agentSave["primaryStat"]["mental"];
					agentRet["stats"]["temperance"] = agentSave["primaryStat"]["work"];
					agentRet["stats"]["justice"] = agentSave["primaryStat"]["battle"];

					// titles
					agentRet["titles"] = new JObject();
					agentRet["titles"]["primary"] = agentSave["prefix"];  // int, no cast
					agentRet["titles"]["secondary"] = agentSave["suffix"];  // int, no cast

					// egos
					agentRet["egos"] = new JObject();
					agentRet["egos"]["weapon"] = agentSave["weaponId"];
					agentRet["egos"]["armor"] = agentSave["armorId"];
					agentRet["egos"]["gifts"] = new JObject();

					//// gifts
					// TODO: How about a kiss? ;)
					foreach (KeyValuePair<string, JToken> kvp in agentSave["gifts"]["lockState"] as JObject)
					{
						int partId = int.Parse(kvp.Key);
						string typeIndex = (partId / 100).ToString();
						string partIndex = (partId % 100).ToString();
						if (agentRet["egos"]["gifts"][partIndex] == null)
						{
							agentRet["egos"]["gifts"][partIndex] = new JObject();
						}
						if (agentRet["egos"]["gifts"][partIndex][typeIndex] == null)
						{
							agentRet["egos"]["gifts"][partIndex][typeIndex] = new JObject();
						}

						agentRet["egos"]["gifts"][partIndex][typeIndex]["id"] = kvp.Value["id"];  // int, no cast
						agentRet["egos"]["gifts"][partIndex][typeIndex]["isLocked"] = kvp.Value["state"];
					}
					foreach (KeyValuePair<string, JToken> kvp in agentSave["gifts"]["displayState"] as JObject)
					{
						int partId = int.Parse(kvp.Key);
						string typeIndex = (partId / 100).ToString();
						string partIndex = (partId % 100).ToString();
						if (agentRet["egos"]["gifts"][partIndex] == null)  // These are necessary! Not all displayState composed to a gitf due to a bug! See `UnitEGOgiftSpace.ReleaseGift(EGOgiftModel) : void @06003662`
						{
							agentRet["egos"]["gifts"][partIndex] = new JObject();
						}
						if (agentRet["egos"]["gifts"][partIndex][typeIndex] == null)
						{
							agentRet["egos"]["gifts"][partIndex][typeIndex] = new JObject();
						}

						agentRet["egos"]["gifts"][partIndex][typeIndex]["isVisible"] = kvp.Value;
					}

					// history
					agentRet["history"] = new JObject();
					agentRet["history"]["dayCount"] = agentSave["history"]["historyworkDay"];
					agentRet["history"]["InterruptedByPanicWorkCount"] = agentSave["history"]["workFail"];
					// agentRet["history"]["promotionVal"] = agentSave["history"]["promotionVal"];  // TODO: Those poor guys remember how many day they ACTUALLY experienced by add 3 to this and add 6 to this somehow... Let's put this later.

					//// PE-BOX
					agentRet["history"]["pebox"] = new JObject();
					agentRet["history"]["pebox"]["instinct"] = agentSave["history"]["workCubeCounts"]["R"];
					agentRet["history"]["pebox"]["insight"] = agentSave["history"]["workCubeCounts"]["W"];
					agentRet["history"]["pebox"]["attachment"] = agentSave["history"]["workCubeCounts"]["B"];
					agentRet["history"]["pebox"]["repression"] = agentSave["history"]["workCubeCounts"]["P"];

					// custom
					agentRet["custom"] = new JObject();
					agentRet["custom"]["isCustom"] = agentSave["iscustom"];

					//// name
					agentRet["custom"]["name"] = new JObject();
					if (agentSave["customName"] != null)
					{
						agentRet["custom"]["name"]["customName"] = agentSave["customName"];
					}
					agentRet["custom"]["name"]["id"] = agentSave["nameId"];  // int, no cast
					//// appearance
					agentRet["custom"]["appearance"] = new JObject();
					agentRet["custom"]["appearance"]["eyes"] = new JObject();
					agentRet["custom"]["appearance"]["eyes"]["color"] = this.GetAppearanceColorRet(agentSave as JObject, "EyeColor");
					agentRet["custom"]["appearance"]["eyes"]["normal"] = this.GetAppearanceSpriteRet(agentSave as JObject, "Eye");
					agentRet["custom"]["appearance"]["eyes"]["close"] = this.GetAppearanceSpriteRet(agentSave as JObject, "EyeClose");
					agentRet["custom"]["appearance"]["eyes"]["panic"] = this.GetAppearanceSpriteRet(agentSave as JObject, "EyePanic");
					agentRet["custom"]["appearance"]["eyes"]["dead"] = this.GetAppearanceSpriteRet(agentSave as JObject, "EyeDead");
					agentRet["custom"]["appearance"]["brow"] = new JObject();
					agentRet["custom"]["appearance"]["brow"]["normal"] = this.GetAppearanceSpriteRet(agentSave as JObject, "EyeBrow");
					agentRet["custom"]["appearance"]["brow"]["battle"] = this.GetAppearanceSpriteRet(agentSave as JObject, "BattleEyeBrow");
					agentRet["custom"]["appearance"]["brow"]["panic"] = this.GetAppearanceSpriteRet(agentSave as JObject, "PanicEyeBrow");
					agentRet["custom"]["appearance"]["mouth"] = new JObject();
					agentRet["custom"]["appearance"]["mouth"]["normal"] = this.GetAppearanceSpriteRet(agentSave as JObject, "Mouth");
					agentRet["custom"]["appearance"]["mouth"]["battle"] = this.GetAppearanceSpriteRet(agentSave as JObject, "BattleMouth");
					agentRet["custom"]["appearance"]["mouth"]["panic"] = this.GetAppearanceSpriteRet(agentSave as JObject, "PanicMouth");
					agentRet["custom"]["appearance"]["hair"] = new JObject();
					agentRet["custom"]["appearance"]["hair"]["color"] = this.GetAppearanceColorRet(agentSave as JObject, "HairColor");
					agentRet["custom"]["appearance"]["hair"]["frontHair"] = this.GetAppearanceSpriteRet(agentSave as JObject, "FrontHair");
					agentRet["custom"]["appearance"]["hair"]["rearHair"] = this.GetAppearanceSpriteRet(agentSave as JObject, "RearHair");

					sephirahAgents.Add(agentRet);
				}

				return this;
			}

			JObject GetAppearanceSpriteRet(JObject agentSave, string part)
			{
				var ret = new JObject();

				// TODO: replace the secondary index to single index.
				ret["srcIndex"] = (int)agentSave["spriteSet"][part].Value<long>("a");
				ret["innerIndex"] = agentSave["spriteSet"][part]["b"];

				return ret;
			}

			JObject GetAppearanceColorRet(JObject agentSave, string part)
			{
				var ret = new JObject();

				ret["r"] = agentSave["spriteSet"][part]["r"];
				ret["g"] = agentSave["spriteSet"][part]["g"];
				ret["b"] = agentSave["spriteSet"][part]["b"];
				ret["a"] = 1.0;

				return ret;
			}
		}

		protected override JObject Reorganize(Dictionary<string, object> rawSave)
		{
			var ret = new JObject();
			var days = rawSave["dayList"] as Dictionary<int, Dictionary<string, object>>;
			foreach (KeyValuePair<int, Dictionary<string, object>> kvp in days)
			{
				var day = kvp.Value;
				var abnormalities = day["creatures"] as Dictionary<string, object>;
				foreach (Dictionary<string, object> abnormality in abnormalities["creatureList"] as List<Dictionary<string, object>>)
				{
					abnormality.Remove("basePosition");
				}
			}
			var save = JObject.FromObject(rawSave);

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
			ret["days"]["current"] = this.GetDayRet(save["dayList"][currentDay.ToString()] as JObject);
			ret["days"]["memoryRepository"] = this.GetDayRet(save["dayList"][memoryRepositoryDay.ToString()] as JObject);

			return ret;
		}

		private JObject GetDayRet(JObject save)
		{
			return new DayRetBuilder(save).AddBasicInfo()
			                              .AddExpansionLevel()
			                              .AddAbnormalities()
			                              .AddAgents()
			                              .Build();
		}
	}

	class GlobalSaveSerializer : SaveSerializerBase
	{
		public GlobalSaveSerializer() : base("saveGlobal170808.dat", "global.json")
		{
		}

		private class RetBuilder
		{
			private JObject ret;
			private JObject save;

			public RetBuilder(JObject save)
			{
				this.save = save;
				this.ret = new JObject();
				this.ret["abnormalities"] = new JObject();
				this.ret["trackers"] = new JObject();
				this.ret["egos"] = new JObject();
				this.ret["researches"] = new JObject();
				this.ret["missions"] = new JObject();
				this.ret["sephiroth"] = new JObject();
			}

			public JObject Build()
			{
				return this.ret;
			}

			private static JArray GetInfo(JObject abnormality)
			{
				var ret = new JArray();
				for (int i = 0; abnormality["care_" + i] != null; i++)
				{
					ret.Add(abnormality["care_" + i]);
				}
				return ret;
			}

			/*
				Ignored field(s):
				- "observeProgress": Not necessary (Inferable, See `Lobotomy save.md`).
			*/
			public RetBuilder AddAbnormalities()
			{
				foreach (KeyValuePair<string, JToken> kvp in this.save["observe"]["observeList"] as JObject)
				{
					int id = int.Parse(kvp.Key);
					string type = new Abnormality(id).Type;
					var abnormality = kvp.Value as JObject;

					this.ret["abnormalities"][id.ToString()] = new JObject();
					var abnormalityRet = this.ret["abnormalities"][id.ToString()];

					if (type == "normal")
					{
						abnormalityRet["pebox"] = abnormality["cubeNum"];
						abnormalityRet["info"] = new JObject();
						abnormalityRet["info"]["basic"] = abnormality["stat"];
						abnormalityRet["info"]["escape"] = abnormality["defense"];
						abnormalityRet["info"]["tips"] = RetBuilder.GetInfo(abnormality);
						abnormalityRet["info"]["work"] = new JObject();
						abnormalityRet["info"]["work"]["fortitude"] = abnormality["work_r"];
						abnormalityRet["info"]["work"]["prudence"] = abnormality["work_w"];
						abnormalityRet["info"]["work"]["temperance"] = abnormality["work_b"];
						abnormalityRet["info"]["work"]["justice"] = abnormality["work_p"];
					}
					else if (type == "tool_channeled")
					{
						abnormalityRet["usageTime"] = abnormality["totalKitUseTime"];
						// abnormalityRet["usageCount"] = abnormality["totalKitUseCount"];
						abnormalityRet["info"] = RetBuilder.GetInfo(abnormality);
					}
					else if (type == "tool_equippable")
					{
						abnormalityRet["usageTime"] = abnormality["totalKitUseTime"];
						abnormalityRet["usageCount"] = abnormality["totalKitUseCount"];
						abnormalityRet["info"] = RetBuilder.GetInfo(abnormality);
					}
					else if (type == "tool_single")
					{
						abnormalityRet["usageCount"] = abnormality["totalKitUseCount"];
						abnormalityRet["info"] = RetBuilder.GetInfo(abnormality);
					}
				}

				return this;
			}

			/*
				Ignored field(s):
				- "tutorialDone": Always false.
				- "nextUnitInstanceId": Always 1.
			*/
			public RetBuilder AddTrackers()
			{
				this.ret["trackers"]["day1ClearCount"] = this.save["etcData"]["day1clearCount"];
				// this.ret["trackers"]["isTutorialDone"] = this.save["etcData"]["tutorialDone"];
				this.ret["trackers"]["farthestDay"] = this.save["etcData"]["unlockedMaxDay"];
				this.ret["trackers"]["ending"] = new JObject();
				this.ret["trackers"]["ending"]["isAEndingCompleted"] = this.save["etcData"]["ending1Done"];
				this.ret["trackers"]["ending"]["isBEndingCompeleted"] = this.save["etcData"]["ending2Done"];
				this.ret["trackers"]["ending"]["isCEndingCompeleted"] = this.save["etcData"]["ending3Done"];
				this.ret["trackers"]["ending"]["isTrueEndingCompeleted"] = this.save["etcData"]["trueEndingDone"];
				this.ret["trackers"]["ending"]["isHiddenEndingCompeleted"] = this.save["etcData"]["hiddenEndingDone"];

				return this;
			}

			public RetBuilder AddEgos()
			{
				this.ret["egos"]["nextIndex"] = this.save["inventory"]["nextInstanceId"];
				this.ret["egos"]["info"] = new JObject();
				foreach (JToken ego in this.save["inventory"]["equips"])
				{
					string id = ego["equipTypeId"].Value<int>().ToString();
					if (this.ret["egos"]["info"][id] == null)
					{
						this.ret["egos"]["info"][id] = new JObject();
						this.ret["egos"]["info"][id]["count"] = 0;
						this.ret["egos"]["info"][id]["index"] = new JArray();
					}

					this.ret["egos"]["info"][id]["count"] = this.ret["egos"]["info"][id]["count"].Value<int>() + 1;
					(this.ret["egos"]["info"][id]["index"] as JArray).Add(ego["equipInstanceId"]);
				}
				return this;
			}

			public RetBuilder AddResearches()
			{
				// There is a little room of improvement...
				foreach (Sephirah s in Sephirah.All)
				{
					if (s == Sephirah.Kether || s == Sephirah.Daat)
					{
						continue;
					}
					this.ret["researches"][s.ToLowerString()] = new JArray { false, false, false };
				}
				Dictionary<int, tuple<string, int>> mp = {
					{ 1, { "malkut", 0 } },
					{ 2, { "malkut", 1 } },
					{ 103, { "malkut", 2 } },

					{ 3, { "yesod", 0 } },
					{ 4, { "yesod", 1 } },
					{ 5, { "yesod", 2 } },

					{ 6, { "netzach", 0 } },
					{ 7, { "netzach", 1 } },
					{ 203, { "netzach", 2 } },

					{ 8, { "hod", 0 } },
					{ 9, { "hod", 1 } },
					{ 10, { "hod", 2 } },

					{ 501, { "tiphereth", 0 } },
					{ 502, { "tiphereth", 1 } },
					{ 503, { "tiphereth", 2 } },

					{ 701, { "geburah", 0 } },
					{ 702, { "geburah", 1 } },
					{ 703, { "geburah", 2 } },

					{ 801, { "chesed", 0 } },
					{ 802, { "chesed", 1 } },
					{ 803, { "chesed", 2 } },

					{ 901, { "binah", 0 } },
					{ 902, { "binah", 1 } },
					{ 903, { "binah", 2 } },

					{ 1001, { "chokhmah", 0 } },
					{ 1002, { "chokhmah", 1 } },
					{ 1003, { "chokhmah", 2 } }
				};
				
			}
		}

		protected override JObject Reorganize(Dictionary<string, object> rawSave)
		{
			var save = JObject.FromObject(rawSave);

			return new RetBuilder(save).AddAbnormalities()
			                           .AddTrackers()
			                           .AddEgos()
			                           .Build();
		}
	}
}
