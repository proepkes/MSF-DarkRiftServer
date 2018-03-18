# MSF-DarkRiftServer

Make sure to edit PostBuild-Events of every containing project to fit your needs. By default, the builds are copied to "E:\UnityProjects\DarkRiftServer\Plugins". The build of "Utils"-project should be copied to the Unity-Assets, so you can access the MessageTags:
1. Right-click on project
1. Properties (last entry)
1. Select "Build Events" (on the left)
1. Edit the path of the xcopy-target

For each project you have to set the correct dll-paths:
1. Right-click on References
1. Add-Reference...
1. Browse to [Darkrift.dll, DarkRiftServer.dll, DarkRiftClient.dll, UnityEngine.CoreModule.dll] (depending on the project's usings)
1. Cick "OK"

Final steps:
1. Right-click on solution
1. Restore NuGet-Packages
1. Build solution
1. If you set up the build-events correctly, all plugins will be automatically copied to DarkRiftServer's Plugin-folder
1. In the bin-folder of this project should be a "MySQL.Data.dll" and "System.Threading.Tasks.NET35.dll", copy them to the Plugin-folder too
1. Setup finished, for a basic usage you can look at the Example-Client

99% of the code is/will be ported from the really awesome (but no longer continued? :() Master-Server-Framework: https://github.com/alvyxaz/barebones-masterserver


Take a look at Example.config for Plugin- and Database-configuration
