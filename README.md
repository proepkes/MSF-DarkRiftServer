# MSF-DarkRiftServer

Make sure to edit PostBuild-Events of every containing project to fit your needs. By default, the Builds are copied to "E:\UnityProjects\DarkRiftServer\Plugins". The build of "Utils"-project should be copied to the Unity-Assets, so you can access the MessageTags.

For each project you have to set the correct dll-paths:
1. Rightclick on References
1. Add-Reference...
1. Browse to [Darkrift.dll, DarkRiftServer.dll, DarkRiftClient.dll, UnityEngine.CoreModule.dll] (depending on the project's requirement)
1. Cick "OK"

99% of the code is/will be ported from the really awesome (but no longer continued? :() Master-Server-Framework: https://github.com/alvyxaz/barebones-masterserver


Take a look at Example.config for Plugin- and Database-configuration
