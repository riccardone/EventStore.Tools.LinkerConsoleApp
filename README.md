# LinkerConsoleApp
Simple .Net Core console app to replicate data from a production EventStore to a local instance. This is an example of Data Redundancy using [Linker nuget package](https://github.com/riccardone/EventStore.Tools.Linker) and replicate EventStore instances or clusters across different networks. Edit appsettings.json file to set origin and destination info. 

Get the [latest release](https://github.com/riccardone/EventStore.Tools.LinkerConsoleApp/releases). Edit the appsettings.json to configure your linked EventStore's.  
  
Open a command line, cd the folder and run the app: 
```> dotnet LinkerConsoleApp.dll```
  
# Configuration 
This app make use of the appsettings.json file to configure Linked EventStore's. You can have as many Links as you need and as your running machine allow you to run. Even when you are replicating at full speed, the Linker logic make use of **backpressure** tecnique in order to not take the full amount of CPU and Memory available.  
You can have the same but swapping the Origin and Destination in order to configure a **multi master** replication. You can have the same Origin in different Links replicating data to different destination for a **Fan-Out** solution. You can have different Origins linked with the same Destination for a **Fan-In** solution.  
Following is an example of simple configuration for **Data Redundancy** between one Origin and one Destination with an exclude filter.

```javascript
{
  "links": [
    {
      "origin": {
        "connectionString": "tcp://localhost:1112",
        "user": "admin",
        "pass": "changeit",
        "connectionName": "origin-01"
      },
      "destination": {
        "connectionString": "tcp://localhost:2112",
        "user": "admin",
        "pass": "changeit",
        "connectionName": "destination-01"
      },
      "filters": [
        {
          "filterType": "stream",
          "value": "diary-input",
          "filterOperation": "exclude"
        },
        {
          "filterType": "stream",
          "value": "*",
          "filterOperation": "include"
        }
      ]
    }
  ]
}
```
