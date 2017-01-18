# RSSBot
Discord bot serving as a subreddit crawler, posts images found.

///////////////////////
// CHITOGEKID RSSBOT //
//  AKA --- MOEBOT   //
//   VERSION: 1.4    //
///////////////////////

The bot is a simple managed Reddit crawler. It finds images, it posts images. 
This bot tends to work best with pages like /r/awwnime and/or /r/pics

Currently it only runs on Windows platforms, but that may change in the future.

Please direct any bugs or notices to Bribz for further handling.

INSTALLATION:
1: Discord Application Proceess:
Create a Bot User over at https://discordapp.com/developers/applications/
Follow the standard procedures of the OAUTH2 process of authorizing Discord bots. 
Note the Bot's Token and set aside for later

2: In Client:
Make sure your account settings are set in Developer Mode in the Discord Client. 
You can change this if you need to under User Settings > Appearance > Developer Mode
Right click on the Server at the top left and Copy ID. Set this ID aside for later.
Right click on the Text Channel you want the bot to post in. Set this ID aside for later.

3: ServerData.config:
In the case that you have grabbed the entire source repo, you will need to build/generate an executable, and run it. 
Users that skip this step will find that ServerData.config and Log.txt will both be absent from the build directory. 
For users that simply downloaded the premade zip archive, this is no issue. You should already have a ServerData.config file. Open it.
At this point, fill in the details of the ServerData.config file. These will be the Tokens/IDs you set aside previously. 
Make sure you place the Token between quotation marks, as this is read as a string.
At this point, if you would like to add additional feeds to the list, you may. However, this can also be done via command in the client -while the bot is running.

Done!
