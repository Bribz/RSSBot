using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace RSSBot
{
    public class RSS_Reader
    {
        private string URI;
        private RSS_Log log;


        public RSS_Reader()
        {
            log = new RSS_Log();
            
            //Load();
        }

        public void ChangeURI(string uri)
        {
            URI = uri;
        }

        public List<ImagePostData> Load()
        {

            SyndicationFeed feed;
            try
            {
                feed = SyndicationFeed.Load(XmlReader.Create(URI));
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception Thrown: " + e.Message);
                return null;
            }
            

            List<ImagePostData> returnList = new List<ImagePostData>();

            foreach (var item in feed.Items)
            {
                string imgUrl = "";
                TextSyndicationContent tsc = (TextSyndicationContent)item.Content;
                
                var match = Regex.Match(tsc.Text, "a href=\"https://cdn.awwni.me/(?<1>.*?)\"", RegexOptions.ExplicitCapture);
                if (match.Success && match.Groups[1].Value != "")
                    imgUrl = "https://cdn.awwni.me/"+ match.Groups[1].Value;
                else
                {
                    match = Regex.Match(tsc.Text, "a href=\"https://i.redd.it/(?<1>.*?)\"", RegexOptions.ExplicitCapture);
                    if (match.Success && match.Groups[1].Value != "")
                        imgUrl = "https://i.redd.it/" + match.Groups[1].Value;
                    else
                        continue;
                }

                //Console.WriteLine("{2}:{0}:{1}", item.Title.Text, imgUrl, item.Id);
                if(!log.CheckKey(item.Id))
                {
                    Console.WriteLine("{2}:{0}:{1}", item.Title.Text, imgUrl, item.Id);
                    returnList.Add(new ImagePostData() { Title = item.Title.Text, URL = imgUrl, Author = item.Authors[0].Name });
                    log.Log(item.Id, DateTime.Now);
                }
            }

            return returnList;
        }

        ~RSS_Reader()
        {
            log.WriteToFile();
        }
    }
}

