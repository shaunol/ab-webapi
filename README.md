ab-webapi
=========

Proof of concept

Provides a web based frontend to apache benchmark

Built on top of .NET 4.5 and OWIN webstack, it runs on Windows and Linux.

Check App.config for configuration settings, including setting the path to ab.


Usage:
```
POST /ab
Content-Type: application/json

{
  "ScheduledDate": "2015-01-01T00:00:00Z",
  "Uri": "http://192.168.1.198:9100/values",
  "Requests": 1000,
  "Concurrency": 2
}
```

ScheduledDate is optional (nullable). If specified, the ab task is run at that time in the future. This is to help coordinate a cluster to all run at the same time, rather than an actual task scheduler for future tasks-as it holds a thread open (using Task.Delay).

I may tweak this to have ab nodes also be able pull jobs from the master rather than just be pushed jobs like this, but this way is far less complex as it's exposed as a simple synchronous operation.