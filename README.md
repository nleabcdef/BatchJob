## AutoJob - A modular and thread safe execution engine built on C#
![Build status](https://nle-abcdef.visualstudio.com/_apis/public/build/definitions/679e92cd-8c0a-4ac1-987c-5eb2b0116bcb/2/badge)  [![NuGet](https://img.shields.io/nuget/v/BatchProcess.AutoJob.svg)](https://www.nuget.org/packages/BatchProcess.AutoJob) ![License Apache](https://img.shields.io/github/license/nleabcdef/BatchJob.svg)

[![N|Solid](https://raw.githubusercontent.com/nleabcdef/BatchJob/master/common/AutoJob.png)](https://github.com/nleabcdef/BatchJob)

AutoJob is a modular and thread safe execution engine for .Net, which enables developers to define sequential and/or parallel executable units (task or class) and execute them as defined.
With AutoJob, any integration code will become modular, easy to maintain and quickly built by its reusable infrastructure include definition and runtime support.

AutoJob’s fluent based interfaces, allow developers to create integrations with modular design and assemble them as repeatable or sequential and parallel tasks. And out of the box run time host will allow them to execute as configured.

# Configuration features, include
  - Config workflow of jobs using fluent interface. 
		* Build you own integration jobs/tasks, and make or assemble them as workflow
  - Configure Message hooks, to subscribe job's push notifications. 
		* using out of the box notification manager and hook handlers
  - For extensibility, Configure you own IserviceProvider to override out of the box Service Repo.
  - Configure error handling feature on the fly with every workflow/integration job.
		* either StopOrExitJob or ContinueOn 
  - Configure Data/Context sharing types, between execution of jobs.
		* Parent, First, Previous or NoSharing
  - Configure integration/workflow's runtime as either sequential, parallel, nested/mixed.
  - Compatible with your own, Error handling and logging implementations.
		* To use any framework of developers/app choice with adapter pattern

# Runtime support, include
  - Thread safe execution of jobs.
  - Out of the box support for message hook subscriptions.
		* better jobs execution management and Async status reporting
  - Workflow types supported, nested, retry, repeatable, sequential and parallel.
  - Common and plug and play error handling and logging modules.
  - Supports both syn and async job execution.
  - Out of the box, in-memory Data/Context sharing.
  - Extendable ServiceRepo, to override out of the box IserviceProvider implementaions.
		* works well with internal IserviceProvider implementaion

> Since it provides modular design, definitely developers could avoid ‘God class’ anti-pattern
	https://sourcemaking.com/antipatterns/the-blob

# Quick start: 
The following examples illustrate simple usage as quick start tutorial

Using
```cs
using BatchProcess.AutoJob;
using BatchProcess.AutoJob.Extensions;
using BatchProcess.Shared;
using System;
using System.Threading.Tasks;
```
Example - Download data from an API call, using two steps,
```cs
IAutomatedJob _getAuthToken = InlineJob.GetDefault((ctx) =>
{
    //call actual api and get Auth token
    var token = Guid.NewGuid().ToString();
    
    //store the token in to Context store
    ctx.SetValue(token, "token-key");
    //Console.WriteLine(token);

    return new JobResult(JobStatus.Completed);
});

IAutomatedJob _download = InlineJob.GetDefault((ctx) =>
{
    //get Auth token from Context store
    var token = ctx.GetValue<string>("token-key");
    //Console.WriteLine(token);

    // download the data using Auth token
    Task.Delay(200).Wait();

    return new JobResult(JobStatus.Completed);
});

//build integration workflow - sequential by default - parallel also be possible
IAutomatedJob _integraion = Builder.BuildWorkflow(InlineJob.GetJobId())
    .WithOption(WhenFailure.StopOrExitJob, ShareContext.previous)
    .AddJob(_getAuthToken)
    .ThenAdd(_download)
    .NothingElse().Create();

var result = _integraion.Doable(); //run the integration job

Console.WriteLine(result.Status.ToString()); //Completed
```
 
Example - Job with retry functionality
```cs
IAutomatedJob _processFile = InlineJob.GetDefault((ctx) =>
{
    //to simulate the retry when job failed. expect break-point hit in VS IDE with Debug configuration
    throw new Exception("making the job failed"); 
    return new JobResult(JobStatus.Completed);
});

//make the job as retry, when error/exception is anticipated
//retry max of 3 times with 500 ms inerval
var retry = _processFile.ConvertToRetry(3, TimeSpan.FromMilliseconds(500));

ErrorHandle.Logger = NullLogger.Instance; //disable the default console logging

var rResult = retry.Doable(); // run retry job

Console.WriteLine(rResult.Status); //CompletedWithError
foreach (var r in retry.RetryResults)
    Console.WriteLine(r.Error.Message); //print retry error/excpetion message
```

***Example`3 - Message hook/subscription and handling - for better status reporting

Using
```cs
using BatchProcess.AutoJob;
using BatchProcess.AutoJob.Extensions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
```

```cs
//setup Download job
IAutomatedJob _download = InlineJob.GetDefault((ctx) =>
{
    //push notification
    ctx.PushReportToHookAsync(ctx.ParentJobId,
        new MessageHook("Download Started", "Info", MessageType.Info));

    // download the data
    Task.Delay(200).Wait();

    //push notification
    ctx.PushReportToHookAsync(ctx.ParentJobId,
        new MessageHook("Download Completed", "Info", MessageType.Info));

    return new JobResult(JobStatus.Completed);
});

//get notification manager, from service repo
var manager = ServiceRepo.Instance.GetServiceOf<INotificationManager<JobId>>();

//setup Message hooks
var hanlder = new AggregateHandler(); // bring your own hook/message handler
manager.RegisterHook(_download.Id, MessageType.Info, hanlder);

var result = _download.Doable(); //execute the job

Console.WriteLine(result.Status.ToString()); //Completed

//wait for handler to aggregate the message hooks, since push is ASync
Task.Delay(500).Wait();

//print the messages recieved
hanlder.MessagesRecieved.ToList().ForEach(m =>
{
    Console.WriteLine(m.Item2.ToString());
});
```
```cs
/// <summary>
/// Sample notification handler, to aggregate messages when it arraives
/// </summary>
public class AggregateHandler : IHookHandler<JobId>
{
    public string Id => "id-of-AggregateHandler";
    public string Name => "Aggregate-Handler";
    public IReadOnlyCollection<Tuple<JobId, MessageHook>> MessagesRecieved => _messages.AsReadOnly();

    protected List<Tuple<JobId, MessageHook>> _messages { get; set; }

    public void DoHandle(JobId sender, MessageHook message)
    {
        lock (_messages)
        {
            _messages.Add(new Tuple<JobId, MessageHook>(sender, message));
        }
    }

    public AggregateHandler()
    {
        _messages = new List<Tuple<JobId, MessageHook>>();
    }
}
```

 - more samples yet to be uploaded.

# Library Dependencies – build for
	.NET Standard 2.0
	
# Supported .NET framework
	.NET Core 2.0
	.NET Framework 4.6.1 
	Mono 5.4

> Please read the following Microsoft document, for .NET Standard 2.0 and its platform support
	https://docs.microsoft.com/en-us/dotnet/standard/net-standard
	https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md

# Installation
### from nuget
 > https://www.nuget.org/packages/BatchProcess.AutoJob

 ###### Please note, Message Hook feature is not yet deployed to nuget.
 ###### referring Example`3 above. Its only available in dev branch which is aggregated for upcoming minor release.

 - using Package Manager
 ```ps
PM> Install-Package BatchProcess.AutoJob -Version 0.1.2
 ```
 
 - using .NET CLI
 ```ps
> dotnet add package BatchProcess.AutoJob --version 0.1.2
 ```

# License : Apache License 2.0
 - read more here http://www.apache.org/licenses/ 
