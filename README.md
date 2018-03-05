## AutoJob - A modular and thread safe execution engine built on C#

[![N|Solid](https://raw.githubusercontent.com/nleabcdef/BatchJob/master/common/AutoJob.png)](https://github.com/nleabcdef/BatchJob)

AutoJob is a modular and thread safe execution engine for .Net, which enables developers to define sequential and/or parallel executable units (task or class) and execute them as defined.
With AutoJob, any integration code will become modular, easy to maintain and quickly built by its reusable infrastructure include definition and runtime support.

AutoJob’s fluent based interfaces, allow developers to create integrations with modular design and assemble them as repeatable or sequential and parallel tasks. And out of the box run time host will allow them to execute as configured.

# Configuration features, include
  - Creating workflow of jobs using fluent interface 
  - How to handle error/failures while executing jobs
		a) StopOrExitJob or ContinueOn	
		b) Data/Context sharing types could 
  - Execution/Runtime type, either sequential or parallel
  - Error handling and logging configurations allow to use any framework of developers/app choice with adapter pattern

# Runtime support, include
  - Thread safe execution of jobs
  - Workflow types supported, nested, retry, repeatable, sequential and parallel
  - Common and plug and play error handling and logging modules
  - Supports both syn and async job execution
  - Out of the box, in-memory Data/Context sharing

> Since it provides module code design, definitely developers could avoid ‘God class’ anti-pattern
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

//build integraion workflow - sequential by default - parallel also be possible
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
    //to simulate the retry when job failed
    throw new Exception("making the job failed");
    return new JobResult(JobStatus.Completed);
});

//make the job as retry, when error/exception is anticipated
var retry = _processFile.ConvertToRetry(3, TimeSpan.FromMilliseconds(500));

ErrorHandle.Logger = NullLogger.Instance; //disable the default console logging

var rResult = retry.Doable(); // run retry job

Console.WriteLine(rResult.Status); //CompletedWithError
foreach (var r in retry.RetryResults)
    Console.WriteLine(r.Error.Message); //print retry error/excpetion message
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
