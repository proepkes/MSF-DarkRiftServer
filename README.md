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

99% of the code is/will be ported from the really awesome (but no longer continued? :() Master-Server-Framework: https://github.com/alvyxaz/barebones-masterserver


Take a look at Example.config for Plugin- and Database-configuration
