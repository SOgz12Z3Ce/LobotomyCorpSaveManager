using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LobotomyCorpSaveManager.Exceptions;

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
		static Dictionary<string, string> languageMap = new Dictionary<string, string>  // convert to BCP 47
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

			ret.Add("masterVolume", save["masterVolume"]);
			ret.Add("bgmVolume", save["bgmVolume"]);
			ret.Add("enableTooltip", save["tooltip"]);
			ret.Add("enableDlcAbnormalities", save["dlcCreatureOn"]);
			ret.Add("logIndex", save["logIndex"]);
			ret.Add("language", languageMap[save["language"].ToString()]);

			return ret;
		}

		class EtcSaveSerializer : SaveSerializerBase
		{
			public EtcSaveSerializer() : base("etc170808.dat", "etc.json")
			{
			}

			protected override JObject Reorganize(Dictionary<string, object> save)
			{
				var ret = new JObject();

				ret.Add("isCoreSuppressionTutorialPlayed", save["sefirabossTutorialPlayed"]);
				ret.Add("isCoreSuppressionTutorialPlayed", save["sefirabossTutorialPlayed"]);
				ret.Add("isKetherCoreSuppression1Completed", save["e0"]);
				ret.Add("isKetherCoreSuppression2Completed", save["e1"]);
				ret.Add("isKetherCoreSuppression3Completed", save["e2"]);
				ret.Add("isKetherCoreSuppression4Completed", save["e3"]);
				ret.Add("isKetherCoreSuppression5Completed", save["e4"]);
				ret.Add("extractedAbnormalitiesIdQueue", save["waitingCreature"]);

				return ret;
			}
		}
	}
}
