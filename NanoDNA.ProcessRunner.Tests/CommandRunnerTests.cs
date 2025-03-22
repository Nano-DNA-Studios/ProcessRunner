using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NanoDNA.ProcessRunner.Tests
{
    /// <summary>
    /// Defines all Tests Needed for the <see cref="CommandRunner"/> Class
    /// </summary>
    internal class CommandRunnerTests
    {
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
        //[TestCase("Bash", PlatformOperatingSystem.OSX)]
        //[TestCase("Sh", PlatformOperatingSystem.OSX)]
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
        //[TestCase(ProcessApplication.Bash, PlatformOperatingSystem.OSX)]
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX)]
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
        /// <param name="application">Application as a <see cref="ProcessApplication"/></param>
        /// <param name="OS">Operating System to Test on</param>
        [Test]
        [TestCase("CMD", PlatformOperatingSystem.Windows)]
        [TestCase("PowerShell", PlatformOperatingSystem.Windows)]
        [TestCase("Bash", PlatformOperatingSystem.Unix)]
        [TestCase("Sh", PlatformOperatingSystem.Unix)]
        //[TestCase("Bash", PlatformOperatingSystem.OSX)]
        //[TestCase("Sh", PlatformOperatingSystem.OSX)]
        public void ContructorStringRedirects(string applicationName, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(applicationName));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(applicationName, true, true);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(applicationName, true, false);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(false));

            commandRunner = new CommandRunner(applicationName, false, true);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(applicationName, false, false);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(false));
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
        //[TestCase(ProcessApplication.Bash, PlatformOperatingSystem.OSX)]
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX)]
        public void ContructorEnumRedirects(ProcessApplication application, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application, true, true);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(application, true, false);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(true));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(false));

            commandRunner = new CommandRunner(application, false, true);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(true));

            commandRunner = new CommandRunner(application, false, false);

            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardOutput, Is.EqualTo(false));
            Assert.That(commandRunner.ProcessStartInfo.RedirectStandardError, Is.EqualTo(false));
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
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "bin/", true)]
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "bin10/", false)]
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
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "echo hello", "hello")]
        public void RunCommandOutput(ProcessApplication application, PlatformOperatingSystem OS, string command, string output)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            commandRunner.RunCommand(command);

            Assert.That(commandRunner.StandardOutput.Length, Is.GreaterThan(0));
            Assert.That(commandRunner.StandardError.Length, Is.EqualTo(0));
            Assert.That(commandRunner.StandardOutput[0], Is.EqualTo(output));
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
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "echoe hello")]
        public void RunCommandError(ProcessApplication application, PlatformOperatingSystem OS, string command)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);
            commandRunner.RunCommand(command, true, true);

            Assert.That(commandRunner.StandardOutput.Length, Is.EqualTo(0));
            Assert.That(commandRunner.StandardError.Length, Is.GreaterThan(0));

            string errorOutput = string.Join("\n", commandRunner.StandardError).ToLower();
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
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "echo hello", "hello")]
        public async Task RunCommandAsyncOutput(ProcessApplication application, PlatformOperatingSystem OS, string command, string output)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            await commandRunner.RunCommandAsync(command);

            Assert.That(commandRunner.StandardOutput.Length, Is.GreaterThan(0));
            Assert.That(commandRunner.StandardError.Length, Is.EqualTo(0));
            Assert.That(commandRunner.StandardOutput[0], Is.EqualTo(output));
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
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "echoe hello")]
        public async Task RunCommandAsyncFail(ProcessApplication application, PlatformOperatingSystem OS, string command)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new CommandRunner(application));
                return;
            }

            CommandRunner commandRunner = new CommandRunner(application);

            await commandRunner.RunCommandAsync(command);

            Assert.That(commandRunner.StandardOutput.Length, Is.EqualTo(0));
            Assert.That(commandRunner.StandardError.Length, Is.GreaterThan(0));

            string errorOutput = string.Join("\n", commandRunner.StandardError).ToLower();
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
        //[TestCase(ProcessApplication.Sh, PlatformOperatingSystem.OSX, "/bin/sh", "echo hello")]
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

            commandRunner.RunCommand(command);

            Assert.That(commandRunner.StandardOutput.Length, Is.GreaterThan(0));
            Assert.That(commandRunner.StandardError.Length, Is.EqualTo(0));
            Assert.That(commandRunner.StandardOutput[0], Is.EqualTo("hello"));
        }
    }
}