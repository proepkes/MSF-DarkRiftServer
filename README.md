# MSF-DarkRiftServer

[![discord_logo](https://discordapp.com/assets/2c21aeda16de354ba5334551a883b481.png)](https://discordapp.com/invite/F9hJhcX)

[![patreon_logo](https://user-images.githubusercontent.com/1029673/28471651-be40a2ea-6e35-11e7-9b01-e1b4a7d533b3.png)](https://www.patreon.com/proepkes)

### Instructions:

See "Example.config" in the root directory for Plugin- and Database-configuration

Make sure to edit PostBuild-Events of every containing project to fit your needs. By default, the builds are copied to "E:\UnityProjects\DarkRiftServer\Plugins". The build of "Utils"-project should be copied to the Unity-Assets, so you can access the MessageTags:
1. Right-click on every project
1. Select "Properties" (last entryin the context menu)
1. Select "Build Events" (on the left)
1. Edit the path of the xcopy-target

For each project you have to set the correct dll-paths:
1. Right-click on References
1. Add-Reference...
1. Browse to [Darkrift.dll, DarkRiftServer.dll, DarkRiftClient.dll] (depending on the project's usings)
1. Cick "OK"

Final steps:
1. Right-click on solution
1. Restore NuGet-Packages
1. Build solution
1. If you set up the build-events correctly, all plugins & dependencies will be automatically copied to DarkRiftServer's Plugin-folder
1. Setup finished, for a basic usage you can take a look at the project "TundraClient". The MonoBehaviourExample is an outcommented Unity-Monobehaviour ready to use in Unity.

99% of the code is/will be ported from the really awesome (but no longer continued? :() Master-Server-Framework: https://github.com/alvyxaz/barebones-masterserver

### Warning:

Even though this project is programmed with great care, I take no responsibility for any (security) issues. It's still in the very early development and there will be breaking changes every now and then. This project will be field-tested by my other projects.
