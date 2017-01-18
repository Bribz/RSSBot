using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace RSSBot
{
    public class ServerDataReader
    {

        public static ServerData GetServerData()
        {
            try
            {
                string data = File.ReadAllText(Directory.GetCurrentDirectory() + "/ServerData.config");

                return JsonConvert.DeserializeObject<ServerData>(data);
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("Server Data not found. Creating new file");

                string path = Directory.GetCurrentDirectory() + "/ServerData.config";
                File.CreateText(path).Close();

                File.WriteAllText(path, JsonConvert.SerializeObject(new ServerData("",0,0,new string[] {"awwnime"}), Formatting.Indented));

                string data = File.ReadAllText(Directory.GetCurrentDirectory() + "/ServerData.config");

                return JsonConvert.DeserializeObject<ServerData>(data);
            }
        }

        public static void WriteServerData(ServerData data)
        {
            string path = Directory.GetCurrentDirectory() + "/ServerData.config";

            if (!File.Exists(path))
            {
                File.CreateText(path).Close();
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
    
    public class ServerData
    {
        [JsonProperty("Token")]
        public string Token;
        [JsonProperty("ServerID")]
        public ulong ServerID;
        [JsonProperty("ChannelID")]
        public ulong ChannelID;
        [JsonProperty("Feeds")]
        public List<string> ReaderFeeds;

        [JsonConstructor]
        public ServerData()
        {
        }

        public ServerData(string token, ulong serverID, ulong channelID, string[] feeds)
        {
            Token = token;
            ServerID = serverID;
            ChannelID = channelID;
            ReaderFeeds = feeds.ToList();
        }

    }
}
