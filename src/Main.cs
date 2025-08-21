using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LobotomyCorpSaveManager.SaveSerializer;

namespace LobotomyCorpSaveManager
{
	class Program
	{
		static void Main(string[] args)
		{
			JObject settings = new SettingsSaveSerializer().Deserialize("../tests/saves/live/d50-NoMemoryRepository/Lobotomy170808state.dat");
			Console.WriteLine(settings);
		}
	}
}
