using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSBot
{
    public class RSS_Log
    {
        private Dictionary<string, DateTime> DB;

        public RSS_Log()
        {
            DB = new Dictionary<string, DateTime>();

            ReadFromFile();
        }

        public bool CheckKey(string key)
        {
            return DB.ContainsKey(key);
        }

        public void Log(string key, DateTime time)
        {
            if(!DB.ContainsKey(key))
            DB.Add(key, time);
        }

        private void ReadFromFile()
        {
            string text = "";
            try
            {
                text = File.ReadAllText(Directory.GetCurrentDirectory() + "/Log.txt");
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("Log.txt not found, creating...");
                string path = Directory.GetCurrentDirectory() + "/Log.txt";
                File.Create(path).Close();
            }

            string[] pieces = text.Split(new char[] {'/', ';'});
            for(int i = 0; i < pieces.Length; i+=2)
            {
                if (pieces[i] == "")
                    break;
                
                long tmplong;
                //bool b = DateTime.TryParse(pieces[i + 1], out tmp);
                bool b = long.TryParse(pieces[i + 1], out tmplong);
                
                if (!b)
                {
                    continue;
                }
                    
                DB.Add(pieces[i], DateTime.FromFileTime(tmplong));
            }
        }

        public void WriteToFile()
        {
            string contents = "";
            foreach(var v in DB)
            {
                contents += v.Key + "/";
                contents += v.Value.ToFileTime() + ";";
            }
            File.WriteAllText(Directory.GetCurrentDirectory() + "/Log.txt", contents);
        }
    }
}
