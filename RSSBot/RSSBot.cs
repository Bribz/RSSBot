using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using HigLabo;
using Discord.Commands;
using System.Threading;

namespace RSSBot
{
    public class RSSBot
    {
        private DiscordClient _client;
        private Server _Server;
        private CommandService commands;
        private Thread feedThread;
        private RSS_Reader _reader;
        private ServerData serverData;
        private bool running;

        public RSSBot()
        {
            serverData = ServerDataReader.GetServerData();

            if(serverData.ReaderFeeds == null)
            {
                serverData.ReaderFeeds = new List<string>();
            }

            if(serverData.ReaderFeeds.Count == 0)
            {
                serverData.ReaderFeeds.Add("awwnime");
            }

            _client = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            //Register Commands
            _client.UsingCommands(x =>
            {
                x.PrefixChar = '~';
                x.AllowMentionPrefix = false;
            });

            RegisterCommands();

            _reader = new RSS_Reader();
            ChangeFeed(serverData.ReaderFeeds[0]);


            try
            {

                _client.ExecuteAndWait(async () =>
                {
                    await _client.Connect(serverData.Token, TokenType.Bot);
                    await Task.Delay(2000);

                    _Server = _client.GetServer(serverData.ServerID);

                    Console.WriteLine("Done Connecting. Starting main thread");
                    running = true;

                    ThreadStart feedThreadst = new ThreadStart(UpdateLoop);
                    feedThread = new Thread(feedThreadst);
                    feedThread.Start();
                });
            }

            catch(Exception ex)
            { 
                if(ex is UnauthorizedAccessException)
                {
                    Console.WriteLine($"ERROR: {ex.Message}, did you correctly configure the ServerData.config file?");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine($"ERROR: {ex.Message}!");
                    Console.ReadKey();
                }
            }

            
        }

        private void ChangeFeed(string URI)
        {
            _reader.ChangeURI("https://www.reddit.com/r/" + URI + "/new/.rss");
        }

        private async void RegisterCommands()
        {
            commands = _client.GetService<CommandService>();

            commands.CreateCommand("refresh")
                .Alias(new string[] { "load" })
                .Description("Poll for new Images")
                .Do(async e =>
                {
                    if (e.Channel.Id != serverData.ChannelID) return;

                    var postData = _reader.Load();

                    if (postData == null)
                    {
                        Console.WriteLine("Post Data is null, command failed");
                    }

                    else if (postData.Count > 0)
                    {
                        for (int i = postData.Count - 1; i >= 0; i--)
                        {
                            await _Server.GetChannel(serverData.ChannelID).SendMessage("**[ " + postData[i].Author + " ]:  " + postData[i].Title + "**\n" + postData[i].URL + "\n");
                            Thread.Sleep(8000);
                        }
                    }
                    await e.Message.Delete();
                });

            commands.CreateCommand("addfeed")
                .Alias(new string[] { "add", "feed" })
                .Description("Add a feed to the collection of polled reddit feeds. Use just the name of the subreddit.")
                .Parameter("subredditname", ParameterType.Required)
                .Do(async e =>
                {
                    if (serverData == null || serverData.ReaderFeeds == null)
                    {
                        await e.Message.Delete();
                        return;
                    }

                    var s = e.GetArg(0).Replace(" ", "").Replace("-", "").ToLower();
                    if (s.Length <= 0 || s.Length > 21)
                    {
                        await e.Channel.SendMessage(e.User.NicknameMention + "... Failed to add subreddit!");
                        return;
                    }

                    serverData.ReaderFeeds.Add(s);

                    await e.Channel.SendMessage(e.User.NicknameMention + "... Complete!");
                });

            commands.CreateCommand("removefeed")
                .Alias(new string[] { "remove", "rem" })
                .Description("Remove a feed from the collection of polled reddit feeds. Use just the name of the subreddit.")
                .Parameter("subredditname", ParameterType.Required)
                .Do(async e =>
                {
                    if (serverData == null || serverData.ReaderFeeds == null)
                    {
                        await e.Message.Delete();
                        return;
                    }

                    var s = e.GetArg(0).ToLower();

                    if (!serverData.ReaderFeeds.Contains(s))
                    {
                        await e.Channel.SendMessage(e.User.NicknameMention + "... Failed to remove subreddit, not in list!");
                        return;
                    }
                    else
                    {
                        serverData.ReaderFeeds.Remove(s);
                    }

                    serverData.ReaderFeeds.Add(s);

                    await e.Channel.SendMessage(e.User.NicknameMention + "... Complete!");
                });

            commands.CreateCommand("shutdown")
                .Description("Shutdown bot")
                .Do(async e =>
                {
                    if (e.Channel.Id != serverData.ChannelID) return;
                    if (!e.User.ServerPermissions.Administrator) return;

                    running = false;

                    await e.Channel.SendMessage(e.User.NicknameMention + "\n Goodbye!");

                    ServerDataReader.WriteServerData(serverData);

                    await _client.Disconnect();
                });

            commands.CreateCommand("ping")
                .Do(async e =>
                {
                    if (e.Channel.Id != serverData.ChannelID) return;

                    await e.Channel.SendMessage(e.User.Mention + " Pong!");
                });
        }

        private void SendMessages(List<ImagePostData> data)
        {
            Task.Run(async () =>
            {
                for(int i = data.Count-1; i >= 0; i--)
                {
                    await _Server.GetChannel(serverData.ChannelID).SendMessage("**[ " + data[i].Author + " ]:  " + data[i].Title + "**\n" + data[i].URL + "\n");
                    Thread.Sleep(8000);


                }
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private async void DisconnectEvent()
        {
            await _client.Connect(serverData.Token, TokenType.Bot);
            await Task.Delay(2000);

            _Server = _client.GetServer(serverData.ServerID);

            Console.WriteLine("Done reconnecting. Starting main thread");
            running = true;

            ThreadStart feedThreadst = new ThreadStart(UpdateLoop);
            feedThread = new Thread(feedThreadst);
            feedThread.Start();
        }

        private void UpdateLoop()
        {
            while (running)
            {
                if(_client.State == ConnectionState.Disconnected)
                {
                    DisconnectEvent();
                }

                List<ImagePostData> totalPostData = new List<ImagePostData>();

                foreach (var str in serverData.ReaderFeeds)
                {
                    ChangeFeed(str);
                    var postData = _reader.Load();
                    if(postData != null && postData.Count > 0)
                    {
                        foreach(var data in postData)
                        {
                            totalPostData.Add(data);
                        }
                    }
                }

                if (totalPostData == null)
                {
                    Console.WriteLine("Post Data is null, continuing...");
                }
                else if (totalPostData.Count > 0)
                {
                    Console.WriteLine("Found {0} Posts", totalPostData.Count);
                    SendMessages(totalPostData);
                }

                Console.WriteLine("Sleep Started");
                Thread.Sleep(10*60000);
                Console.WriteLine("Sleep Done");
            }
            feedThread.Abort();
        }
    }
}
