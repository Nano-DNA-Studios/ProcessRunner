using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NanoDNA.ProcessRunner.Enums;
using NanoDNA.ProcessRunner.Results;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace NanoDNA.ProcessRunner.Tests
{
    /// <summary>
    /// Defines all Tests Needed for the <see cref="CommandRunner"/> Class
    /// </summary>
    internal class CommandRunnerTests : BaseUnitTest
    {
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.Sh, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.PowerShell, PlatformOperatingSystem.Windows)]
        public void CommandRunnerConstructor_ProcessApplication(ProcessApplication application, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            Assert.That(commandRunner, Is.Not.Null);
            Assert.That(commandRunner.Application, Is.EqualTo(application));
            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));
        }

        [Test]
        public void CommandRunnerConstructor_DefaultApplication()
        {
            CommandRunner commandRunner = new CommandRunner();

            Assert.That(commandRunner, Is.Not.Null);
            Assert.That(commandRunner.StartInfo.FileName, Is.EqualTo(GetOSDefaultApplication()));
            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));
        }

        [Test]
        [TestCase("cmd.exe", PlatformOperatingSystem.Windows)]
        [TestCase("bash", PlatformOperatingSystem.Unix)]
        [TestCase("sh", PlatformOperatingSystem.Unix)]
        [TestCase("powershell.exe", PlatformOperatingSystem.Windows)]
        public void CommandRunnerConstructor_ApplicationName(string applicationName, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(applicationName: applicationName));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(applicationName: applicationName);

            Assert.That(commandRunner, Is.Not.Null);
            Assert.That(commandRunner.StartInfo.FileName, Is.EqualTo(applicationName));
            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));
        }

        [Test]
        [TestCase("cmd.exe", PlatformOperatingSystem.Windows)]
        [TestCase("/bin/bash", PlatformOperatingSystem.Unix)]
        [TestCase("/bin/sh", PlatformOperatingSystem.Unix)]
        [TestCase("powershell.exe", PlatformOperatingSystem.Windows)]
        public void CommandRunnerConstructor_StartInfo(string applicationName, PlatformOperatingSystem OS)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = applicationName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(startInfo));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(startInfo);

            Assert.That(commandRunner, Is.Not.Null);
            Assert.That(commandRunner.StartInfo.FileName, Is.EqualTo(applicationName));
            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));
        }

        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX)]
        public void CommandRunnerGetDefaultOSApplication(ProcessApplication application, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            Assert.That(CommandRunner.GetDefaultOSApplication(), Is.EqualTo(application), "Default Platform Application does not match");
        }

        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.Sh, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.PowerShell, PlatformOperatingSystem.Windows)]
        public void CommandRunnerRun(ProcessApplication application, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner();

            Result<ProcessResult> result = commandRunner.Run(DEFAULT_PROCESS_COMMAND);

            Assert.That(result, Is.Not.Null, "Command Run Result should not be null");
            Assert.That(result.Content, Is.Not.Null, "Command Result Content should not be null");
            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success), $"Command Result Status should be {ProcessStatus.Success}");
            Assert.That(commandRunner.STDOutput.Length, Is.GreaterThan(0), "STDOutput should not be empty");
            Assert.That(commandRunner.STDError.Length, Is.EqualTo(0), "STDError should be empty");
            Assert.That(commandRunner.STDOutput[0], Is.EqualTo(DEFAULT_PROCESS_OUTPUT), $"STDOutput does not match expected output : {DEFAULT_PROCESS_OUTPUT}");
        }


        /*
        /// <summary>
        /// Checks if the Current Operating System is the same as the one passed in
        /// </summary>
        /// <param name="OS">Operating System we are checking for</param>
        /// <returns>True if the OS's Match, False otherwise</returns>
        private bool OnAppropriateOS(PlatformOperatingSystem OS)
        {
            switch (OS)
            {
                case PlatformOperatingSystem.Windows:
                    return OperatingSystem.IsWindows();

                case PlatformOperatingSystem.Unix:
                    return OperatingSystem.IsLinux();

                case PlatformOperatingSystem.OSX:
                    return OperatingSystem.IsMacOS();
            }

            return false;
        }

        /// <summary>
        /// Tests the <see cref="CommandRunner.GetDefaultOSApplication"/> Method
        /// </summary>
        [Test]
        public void GetDefaultApplication()
        {
            if (OperatingSystem.IsWindows())
                Assert.That(CommandRunner.GetDefaultOSApplication(), Is.EqualTo(ProcessApplication.CMD));

            if (OperatingSystem.IsLinux())
                Assert.That(CommandRunner.GetDefaultOSApplication(), Is.EqualTo(ProcessApplication.Bash));

            if (OperatingSystem.IsMacOS())
                Assert.That(CommandRunner.GetDefaultOSApplication(), Is.EqualTo(ProcessApplication.Sh));
        }

        /// <summary>
        /// Tests if the Class Exists through a ProcessStartInfo Constructor
        /// </summary>
        [Test]
        public void ContructorProcessInfoExists()
        {
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            CommandRunner commandRunner = new CommandRunner(info);

            Assert.That(commandRunner, Is.Not.Null);
        }

        /// <summary>
        /// Tests if the Class Exists through an Empty Constructor
        /// </summary>
        [Test]
        public void ContructorEmptyExists()
        {
            CommandRunner commandRunner = new CommandRunner();

            Assert.That(commandRunner, Is.Not.Null);
        }

        /// <summary>
        /// Tests if the Application is set to the Default Application
        /// </summary>
        [Test]
        public void ConstructorEmptyDefaultApplication()
        {
            CommandRunner commandRunner = new CommandRunner();

            if (OperatingSystem.IsWindows())
                Assert.That(commandRunner.Application, Is.EqualTo(ProcessApplication.CMD));

            if (OperatingSystem.IsLinux())
                Assert.That(commandRunner.Application, Is.EqualTo(ProcessApplication.Bash));

            if (OperatingSystem.IsMacOS())
                Assert.That(commandRunner.Application, Is.EqualTo(ProcessApplication.Sh));
        }

        /// <summary>
        /// Tests if the Class Exists through a String Constructor
        /// </summary>
        /// <param name="applicationName">Name of the Application</param>
        /// <param name="OS">Operating System to Test on</param>
        [Test]
        [TestCase("CMD", PlatformOperatingSystem.Windows)]
        [TestCase("PowerShell", PlatformOperatingSystem.Windows)]
        [TestCase("Bash", PlatformOperatingSystem.Unix)]
        [TestCase("Sh", PlatformOperatingSystem.Unix)]
        public void ContructorStringExists(string applicationName, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(applicationName));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(applicationName);

            Assert.That(commandRunner, Is.Not.Null);
        }

        /// <summary>
        /// Tests if the Class Exists through an Enum Constructor
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.PowerShell, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.Sh, PlatformOperatingSystem.Unix)]
        public void ContructorEnumExists(ProcessApplication application, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            Assert.That(commandRunner, Is.Not.Null);
        }

        /// <summary>
        /// Tests if the Redirects are set correctly for the String Constructor
        /// </summary>
        /// <param name="applicationName">Name of the Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        [Test]
        [TestCase("CMD", PlatformOperatingSystem.Windows)]
        [TestCase("PowerShell", PlatformOperatingSystem.Windows)]
        [TestCase("Bash", PlatformOperatingSystem.Unix)]
        [TestCase("Sh", PlatformOperatingSystem.Unix)]
        public void ContructorStringRedirects(string applicationName, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(applicationName));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(applicationName, true, true);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(applicationName, true, false);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(false));

            commandRunner = new CommandRunner(applicationName, false, true);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(applicationName, false, false);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(false));
        }

        /// <summary>
        /// Tests if the Redirects are set correctly for the Enum Constructor
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.PowerShell, PlatformOperatingSystem.Windows)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix)]
        [TestCase(ProcessApplication.Sh, PlatformOperatingSystem.Unix)]
        public void ContructorEnumRedirects(ProcessApplication application, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application, true, true);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(application, true, false);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(false));

            commandRunner = new CommandRunner(application, false, true);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(application, false, false);

            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(false));
        }

        /// <summary>
        /// Tests if the Working Directory
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        /// <param name="path">Work Directory Path</param>
        /// <param name="pass">Toggle to expect the Test to pass</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, @"C:\\Users", true)]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, @"C:\\Users10", false)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "/bin", true)]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "/bin10", false)]
        public void SetWorkingDirectory(ProcessApplication application, PlatformOperatingSystem OS, string path, bool pass)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            if (!pass)
            {
                Assert.Throws<DirectoryNotFoundException>(() => commandRunner.SetWorkingDirectory(path));
                return;
            }

            commandRunner.SetWorkingDirectory(path);

            Assert.That(commandRunner.WorkingDirectory, Is.EqualTo(path));
        }

        /// <summary>
        /// Tests if Running a Command will record STDOutput
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        /// <param name="command">Command to run</param>
        /// <param name="output">Expected STDOutput</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, "echo hello", "hello")]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "echo hello", "hello")]
        public void RunCommandOutput(ProcessApplication application, PlatformOperatingSystem OS, string command, string output)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            commandRunner.Run(command);

            Assert.That(commandRunner.STDOutput.Length, Is.GreaterThan(0));
            Assert.That(commandRunner.STDError.Length, Is.EqualTo(0));
            Assert.That(commandRunner.STDOutput[0], Is.EqualTo(output));
        }

        /// <summary>
        /// Tests if Running a Command will record STDError
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        /// <param name="command">Command to run</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, "echoe hello")]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "echoe hello")]
        public void RunCommandError(ProcessApplication application, PlatformOperatingSystem OS, string command)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);
            
            ProcessResult result = commandRunner.Run(command).Content;

            if (result.Status != ProcessStatus.Failed)
                Assert.Fail($"Command was supposed to fail : {command}");
            
            Assert.That(commandRunner.STDOutput.Length, Is.EqualTo(0));
            Assert.That(commandRunner.STDError.Length, Is.GreaterThan(0));

            string errorOutput = string.Join("\n", commandRunner.STDError).ToLower();
            Assert.That(errorOutput, Does.Contain("not").And.Contain("found").Or.Contain("recognized"));
        }

        /// <summary>
        /// Tests if Running a Async Command will record STDOutput
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        /// <param name="command">Command to run</param>
        /// <param name="output">Expected STDOutput</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, "echo hello", "hello")]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "echo hello", "hello")]
        public async Task RunCommandAsyncOutput(ProcessApplication application, PlatformOperatingSystem OS, string command, string output)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            Result<ProcessResult> result = await commandRunner.RunAsync(command);

            if (result.Content.Status == ProcessStatus.Failed)
                Assert.Fail($"Command was supposed to succeed : {command}\n{result.Message}");

            Assert.That(commandRunner.STDOutput.Length, Is.GreaterThan(0));
            Assert.That(commandRunner.STDError.Length, Is.EqualTo(0));
            Assert.That(commandRunner.STDOutput[0], Is.EqualTo(output));
        }
        
        /// <summary>
        /// Tests if Running a Async Command will record STDError
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        /// <param name="command">Command to run</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, "echoe hello")]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "echoe hello")]
        public async Task RunCommandAsyncFail(ProcessApplication application, PlatformOperatingSystem OS, string command)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            ProcessResult result = (await commandRunner.RunAsync(command)).Content;

            if (result.Status != ProcessStatus.Failed)
                Assert.Fail($"Command was supposed to fail : {command}");

            Assert.That(result.Status, Is.EqualTo(ProcessStatus.Failed));
            Assert.That(commandRunner.STDOutput.Length, Is.EqualTo(0));
            Assert.That(commandRunner.STDError.Length, Is.GreaterThan(0));

            string errorOutput = string.Join("\n", commandRunner.STDError).ToLower();
            Assert.That(errorOutput, Does.Contain("not").And.Contain("found").Or.Contain("recognized"));
        }

        /// <summary>
        /// Tests if we can run a command with a custom ProcessStartInfo
        /// </summary>
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        /// <param name="fileName">File Name of the CLI Application</param>
        /// <param name="command">Command to Run</param>
        [Test]
        [TestCase(ProcessApplication.CMD, PlatformOperatingSystem.Windows, "cmd.exe", "echo hello")]
        [TestCase(ProcessApplication.Bash, PlatformOperatingSystem.Unix, "/bin/bash", "echo hello")]
        public void CustomProcessStartInfo(ProcessApplication application, PlatformOperatingSystem OS, string fileName, string command)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            CommandRunner commandRunner = new CommandRunner(info);

            commandRunner.Run(command);

            Assert.That(commandRunner.STDOutput.Length, Is.GreaterThan(0));
            Assert.That(commandRunner.STDError.Length, Is.EqualTo(0));
            Assert.That(commandRunner.STDOutput[0], Is.EqualTo("hello"));
        }*/
    }

}