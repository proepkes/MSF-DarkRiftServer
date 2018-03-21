# MSF-DarkRiftServer

[![patreon_logo](https://user-images.githubusercontent.com/1029673/28471651-be40a2ea-6e35-11e7-9b01-e1b4a7d533b3.png)](https://www.patreon.com/proepkes) 
[![Discord](https://img.shields.io/discord/413156098993029120.svg)](https://discord.gg/F9hJhcX) 

### Highlevel-view:

[![architecture](https://i.imgur.com/x4XIuvF.png)](https://i.imgur.com/x4XIuvF.png)

### Current plugins & their dependencies

[![Plugins](https://i.imgur.com/6rARE7u.png)](https://i.imgur.com/6rARE7u.png)

### Instructions:

1. Place DarkRiftClient.dll, DarkRiftServer.dll and DarkRift.dll into the directory "Lib"
1. Extract "DarkRift Server.rar" into the directory "Deploy"
1. Open "TundraServerPlugins.sln" in Visual Studio
1. Right-click on solution -> "Restore NuGet-Packages"
1. Build solution
1. Replace the contents of "Server.config" with the contents of "MasterServerExample.config" and configure to your needs
1. Open "Deploy/Spawner/Spawner.exe.config" and configure the "ExecutablePath"
1. Run DarkRift.Server.Console.exe and then run Spawner/Spawner.exe

### Warning:

Even though this project is programmed with great care, I take no responsibility for any (security) issues. It's still in the very early development and there will be breaking changes every now and then. This project will be field-tested by my other projects.
