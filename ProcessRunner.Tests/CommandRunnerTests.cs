using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NanoDNA.ProcessRunner.Enums;
using NanoDNA.ProcessRunner.Results;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace NanoDNA.ProcessRunner.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="CommandRunner"/> class.
    /// </summary>
    internal class CommandRunnerTests : BaseUnitTest
    {
        /// <summary>
        /// Tests the <see cref="CommandRunner(string, string, bool, bool)"/> constructor with a valid application name.
        /// </summary>
        /// <param name="application">Process Application Enum Instance</param>
        /// <param name="OS">Operating System to test on</param>
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

        /// <summary>
        /// Tests the <see cref="CommandRunner(string, bool, bool)"/> constructor with the parameters empty and using the default application.
        /// </summary>
        [Test]
        public void CommandRunnerConstructor_DefaultApplication()
        {
            CommandRunner commandRunner = new CommandRunner();

            Assert.That(commandRunner, Is.Not.Null);
            Assert.That(commandRunner.StartInfo.FileName, Is.EqualTo(GetOSDefaultApplication()));
            Assert.That(commandRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.StartInfo.RedirectStandardError, Is.EqualTo(true));
        }

        /// <summary>
        /// Tests the <see cref="CommandRunner(string, string, bool, bool)"/> constructor with a valid application name.
        /// </summary>
        /// <param name="applicationName">Name of the Application</param>
        /// <param name="OS">Operating System to test on</param>
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

        /// <summary>
        /// Tests the <see cref="CommandRunner(ProcessStartInfo)"/> constructor with a valid <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="applicationName">Name and Path to the Application</param>
        /// <param name="OS">Operating System to test on</param>
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

        /// <summary>
        /// Tests the <see cref="CommandRunner.GetDefaultOSApplication"/> method to ensure it returns the correct default application for the current operating system.
        /// </summary>
        /// <param name="application">Process Application Enum Instance</param>
        /// <param name="OS">Operating System to test on</param>
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

        /// <summary>
        /// Tests the <see cref="CommandRunner.Run(string)"/> method to ensure it runs a command and returns the expected result.
        /// </summary>
        /// <param name="application">Process Application Enum Instance</param>
        /// <param name="OS">Operating System to test on</param>
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
    }
}