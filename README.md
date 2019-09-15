# LinkerConsoleApp
Simple .Net Core console app to replicate data from a production EventStore to a local instance. This is an example of Data Redundancy using [Linker nuget package](https://github.com/riccardone/EventStore.Tools.Linker) and replicate EventStore instances or clusters across different networks. Edit appsettings.json file to set origin and destination info. 

Get the [latest release](https://github.com/riccardone/EventStore.Tools.LinkerConsoleApp/releases). Edit the appsettings.json to configure your linked EventStore's.  
  
Open a command line, cd the folder and run the app: 
```> dotnet LinkerConsoleApp.dll```
