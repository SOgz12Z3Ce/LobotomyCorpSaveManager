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
}