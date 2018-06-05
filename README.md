## Obsolete
As https://github.com/proepkes/SpeedDate is now open for public, this repository is rendered obsolete.

# MSF-DarkRiftServer

[![Discord](https://img.shields.io/discord/413156098993029120.svg)](https://discord.gg/F9hJhcX) 

### Highlevel-view:

[![architecture](https://i.imgur.com/x4XIuvF.png)](https://i.imgur.com/x4XIuvF.png)

### Instructions:

1. Extract "DarkRift Server.rar" into the directory "Deploy"
1. Copy DarkRiftClient.dll into the directory "Deploy/Lib"
1. Open "TundraServerPlugins.sln" in Visual Studio
1. Right-click on solution -> "Restore NuGet-Packages"
1. Check whether the settings in "Settings.settings" in the Project "Spawner" fit your needs
1. Right-click on Utils-Project -> "Properties" (last entry) -> select Build Events (on the left) and edit the xcopy-command so it copies the files into your Unity\Assets-directory
1. Build solution
1. Replace the contents of "Server.config" with the contents of "MasterServerExample.config" and configure accordingly
1. Run DarkRift.Server.Console.exe and then run Spawner.exe

### FAQ:

Deploying on Linux results in "System.ArgumentException: An item with the same key has already been added":
 - Delete DarkRiftServer.dll from Plugins/
 
### Resources:

https://darkriftnetworking.com/

https://github.com/alvyxaz/barebones-masterserver

### Warning:

Even though this project is programmed with great care, I take no responsibility for any (security) issues.

### Patron
[![patreon_logo](https://user-images.githubusercontent.com/1029673/28471651-be40a2ea-6e35-11e7-9b01-e1b4a7d533b3.png)](https://www.patreon.com/proepkes) 
