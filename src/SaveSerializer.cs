using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

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
		// TODO: add func SerializeToJson

		// TODO: add func DeserializeJson

		private Dictionary<string, object> DeserializeDat(string path)
		{
			FileStream stream = File.OpenRead(path);
			var data = new BinaryFormatter().Deserialize(stream) as Dictionary<string, object>;
			stream.Close();
			return data;
		}
	}
}