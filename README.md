# NanoDNA.ProcessRunner
A C# Library that provides a simple interface for executing Subprocesses or Shell calls through code.

This library is designed to be cross-platform, supporting Windows, MacOS, and Linux. It allows developers to run external processes, capture their output, and handle errors in a straightforward manner.

It is particularly useful for automation tasks, scripting, and integrating with other applications or services that require command-line interaction.

This library is used by Nano-DNA-Studios and MrDNAlex to create Controller/Manager libraries for applications that don't have a native API or SDK, allowing for easier integration and automation of tasks.

# Requirements
- Windows, MacOS, or Linux Operating System
- .NET 8 or Later

# Installation / Download
This Framework can be installed using NuGet, Downloading the Self-Contained DLL's or Cloning through GitHub

## Install from NuGet
This library can be installed from the NuGet Package Manager, which is the recommended way to install it.

Alternatively, use the following command to install the Tool. Replace ``<version>`` with the appropriate version using ``0.0.0`` format.

```bash
dotnet add package NanoDNA.ProcessRunner --version <version>
```

## Download Self-Contained Builds
Visit the [``Release``](https://github.com/Nano-DNA-Studios/ProcessRunner/releases) Page and Download the Self-Contained Tars for your Target Platform and OS

## Clone and Build
Clone the latest state of the Repo and Build it locally.

```bash
git clone https://github.com/Nano-DNA-Studios/ProcessRunner
cd ProcessRunner
dotnet build
```

# Usage
The following shows examples of how to use the ProcessRunner library in your C# projects.

Ideally from these code snippets you can observe how powerful this could be when used correctly and use the library in your own projects. You can also extend functionality by creating your own custom runners.

## Different Run Commands and their use cases
There are 4 native ways to run commands using the ProcessRunner library in each Runner Class. They each have their use cases.

### Run
---
Default run commands, will return a ``Result<ProcessResult>``, the ``Result`` class stores a optional message and a Content object which is the ``ProcessResult`` class. The ``ProcessResult`` class contains a status from the Processes Execution, this is used to verify if it worked something went wrong, it also stores the exit code of the process.

### RunAsync
---
This is the same as the default run command, but it runs the command asynchronously. This is useful for long-running commands or when you want to avoid blocking the main thread.

### TryRun
---
This is a "simplified" version of the default run command. It returns a ``Boolean`` that automatically indicates if the process was successful or not. This can be used for quick commands that don't require detailed error handling or output processing. It is useful for simple commands where you only care about success or failure.

### TryRunAsync
---
This is the asynchronous version of the TryRun command. It returns a ``Boolean`` indicating if the process was successful or not, but it runs the command asynchronously. This is useful for long-running commands where you only care about success or failure without blocking the main thread.


## Run Commands as a Process (Process Runner)
The Process Runner class is used to run commands as a subprocess, it directly runs the specified applications executable. The class has QOL features making it easier to run and automate processes compared to the default .NET Process class.

The following example shows how to run the `dotnet help` command using the Process Runner class:

```csharp
//Create a Process Runner instance for the `dotnet` application
ProcessRunner processRunner = new ProcessRunner("dotnet");

//Run the command and get the result
Result<ProcessResult> result = processRunner.Run("help");

//Verify that the command was successful by checking the status or exit code
if (result.Content.Status == ProcessStatus.Success || result.Content.ExitCode == 0)
{
	Console.WriteLine("Successfully Ran \"dotnet help\"");

	foreach (string line in processRunner.STDOutput)
	{
		Console.WriteLine(line);
	}
}
else
{
	Console.WriteLine("Failed to run \"dotnet help\"");
	Console.WriteLine(result.Content.Error);
}
```

## Run Command through Default OS Shell Application (Command Runner)
The Command Runner class is used to run commands through the default shell application of the operating system. This is useful for commands that require shell features like piping or redirection.


The following example shows how to run the `echo Hello World` command using the Command Runner class:
```csharp
//Use the Default OS Shell Application to run a command (Picked automatically)
CommandRunner commandRunner = new CommandRunner();

//Run the command "echo Hello World"
Result<ProcessResult> result = commandRunner.Run("echo Hello World");

//Verify that the command was successful by checking the status or exit code
if (result.Content.Status == ProcessStatus.Success || result.Content.ExitCode == 0)
{
	Console.WriteLine("Successfully Ran \"echo Hello World\"")
	foreach (string line in commandRunner.STDOutput)
	{
		Console.WriteLine(line);
	}
} else
{
	Console.WriteLine("Failed to run \"echo Hello World\"");
	Console.WriteLine(result.Content.Status);
}}
```

## Making your own Custom Runner
You can create your own custom runner by inheriting from the ``BaseProcessRunner`` class. This allows you to customize the behavior of the runner, such as adding additional features or modifying the way commands are executed. All boilerplate is taken care of, allowing you to focus on custom execution logic or extra features.

# License
Individuals can use the Library under the MIT License

Groups and or Companies consisting of 5 or more people can Contact MrDNAlex through the email ``Mr.DNAlex.2003@gmail.com`` to License the Library for usage. 

# Support
For Additional Support, Contact MrDNAlex through the email : ``Mr.DNAlex.2003@gmail.com``.