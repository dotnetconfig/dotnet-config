# dotnet-eventgrid

[![dotnet-eventgrid](https://img.shields.io/nuget/v/dotnet-eventgrid.svg?color=royalblue&label=dotnet-eventgrid)](https://nuget.org/packages/dotnet-eventgrid)

[dotnet-eventgrid](https://raw.github.com/kzu/dotnet-eventgrid) is a dotnet global tool to 
monitor and filter real-time event from Azure EventGrid, delivered through Azure SignalR.

The tool allows configuring filters so that you can monitor the specific events comming through 
the SignalR connection like:

```gitconfig
[eventgrid]
    url = https://events.mygrid.com
    filter = path=MyApp/**/Login
    filter = eventType=*System*
```

You can also specify whether you want to see certain event properties rendered or not in the console:

```gitconfig
[eventgrid]
    # properties to include in the event rendering
    include = EventType
    include = Subject

    # properties to exclude from event rendering
    exclude = data
```

The tool is quite awesome in action, and after you have your desired *.netconfig* in place, you can 
just run `eventgrid` without parameters and enjoy it right-away:

![EventGrid tool in action](https://raw.github.com/kzu/dotnet-eventgrid/master/img/eventgrid.gif)