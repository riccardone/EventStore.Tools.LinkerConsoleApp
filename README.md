# Linker introduction
Simple .Net Core console app, ready for use, to replicate data from between separate EventStore instances/clusters. More info on this article [Cross Data Center Replication with Linker](http://www.dinuzzo.co.uk/2019/11/17/cross-data-center-replication-with-linker/).  
This is an example of Data Redundancy using [Linker nuget package](https://github.com/riccardone/EventStore.Tools.Linker) to replicate data from EventStore instances or clusters across different networks.   

Get the [latest release](https://github.com/riccardone/EventStore.Tools.LinkerConsoleApp/releases). Edit the appsettings.json to configure your linked EventStore's.  
  
Open a command line, cd the folder and run the app: 
```> dotnet LinkerConsoleApp.dll```
  
# Configuration Modes
This app make use of the appsettings.json file to configure Linked EventStore's. You can have as many Links as you need and as your running machine allow you to run. Even when you are replicating at full speed, the Linker logic make use of **backpressure** tecnique in order to not take the full amount of CPU and Memory available.  
## MultiMaster  
Configure two links swapping the same Origin and Destination in order to configure a **multi master** replication.  
## Fan-Out
Configure the same Origin in separate Links replicating data to different destination for a **Fan-Out** solution.  
## Fan-In
You can have separate Links with different Origins linked with the same Destination for a **Fan-In** solution.  

## Sample configuration  
Following is an example of configuration for **Data Redundancy** between one Origin and one Destination with an exclude filter.

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
