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
			JObject settingsSave = new SettingsSaveSerializer().Deserialize("tests/saves/live/Lobotomy170808state.dat");
			JObject etcSave = new EtcSaveSerializer().Deserialize("tests/saves/live/Lobotomy/etc170808.dat");
			JObject masterSave = new MasterSaveSerializer().Deserialize("tests/saves/live/Lobotomy/saveData170808.dat");

			Console.WriteLine(settingsSave);
			Console.WriteLine(etcSave);
			Console.WriteLine(masterSave);
		}
	}
}
