using NUnit.Framework;
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using NanoDNA.ProcessRunner.Results;
using NanoDNA.ProcessRunner.Enums;

namespace NanoDNA.ProcessRunner.Tests
{
    internal class BaseProcessRunnerTests
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

        private string GetOSDefaultApplication()
        {
            if (OnAppropriateOS(PlatformOperatingSystem.Windows))
                return "cmd";
            else if (OnAppropriateOS(PlatformOperatingSystem.Unix))
                return "bash";
            else if (OnAppropriateOS(PlatformOperatingSystem.OSX))
                return "sh";
            else
                throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        private string GetInvalidOSDirectory()
        {
            if (OnAppropriateOS(PlatformOperatingSystem.Windows))
                return "C:\\non_existent_dir";
            else if (OnAppropriateOS(PlatformOperatingSystem.Unix) || OnAppropriateOS(PlatformOperatingSystem.OSX))
                return "/non_existent_dir";
            else
                throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        private string GetValidOSDirectory()
        {
            if (OnAppropriateOS(PlatformOperatingSystem.Windows))
                return Directory.GetCurrentDirectory();
            else if (OnAppropriateOS(PlatformOperatingSystem.Unix) || OnAppropriateOS(PlatformOperatingSystem.OSX))
                return Directory.GetCurrentDirectory();
            else
                throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        private class TestRunner : BaseProcessRunner
        {
            public TestRunner(string application, string workingDirectory = "", bool stdOut = false, bool stdErr = false)
                : base(application, workingDirectory, stdOut, stdErr) { }

            public TestRunner(ProcessStartInfo info) : base(info) { }
        }

        [Test]
        public void ConstructorWithValidApplication()
        {
            string application = GetOSDefaultApplication();

            TestRunner runner = new TestRunner(application);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.Empty);
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.False);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.False);
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.Empty);
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.False);
            Assert.That(runner.STDErrorRedirect, Is.False);
            Assert.That(runner.IsApplicationAvailable(application), Is.True);
        }

        [Test]
        public void ConstructorWithValidApplicationAndWorkingDirectory()
        {
            string application = GetOSDefaultApplication();
            string workingDirectory = Directory.GetCurrentDirectory();

            TestRunner runner = new TestRunner(application, workingDirectory);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.EqualTo(workingDirectory));
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.False);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.False);
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.EqualTo(workingDirectory));
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.False);
            Assert.That(runner.STDErrorRedirect, Is.False);
            Assert.That(runner.IsApplicationAvailable(application), Is.True);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void ConstructorWithRedirects(bool stdOut, bool stdErr)
        {
            string application = GetOSDefaultApplication();

            TestRunner runner = new TestRunner(application, stdOut: stdOut, stdErr: stdErr);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.Empty);
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.EqualTo(stdOut));
            Assert.That(runner.StartInfo.RedirectStandardError, Is.EqualTo(stdErr));
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.Empty);
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.EqualTo(stdOut));
            Assert.That(runner.STDErrorRedirect, Is.EqualTo(stdErr));
            Assert.That(runner.IsApplicationAvailable(application), Is.True);
        }

        [Test]
        [TestCase("non_existent_app")]
        [TestCase(null)]
        public void ConstructorWithInvalidApplication(string applicationName)
        {
            Assert.Throws<ArgumentException>(() => new TestRunner(applicationName));
        }

        [Test]
        public void ConstructorWithInvalidWorkingDirectory()
        {
            string application = GetOSDefaultApplication();
            string invalidDirectory = GetInvalidOSDirectory();

            Assert.Throws<DirectoryNotFoundException>(() => new TestRunner(application, invalidDirectory));
        }


        [Test]
        public void ConstructorWithStartInfo ()
        {
            string application = GetOSDefaultApplication();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = application,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            TestRunner runner = new TestRunner(startInfo);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.Empty);
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.False);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.False);
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.Empty);
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.False);
            Assert.That(runner.STDErrorRedirect, Is.False);
            Assert.That(runner.IsApplicationAvailable(application), Is.True);
        }

        [Test]
        public void ConstructorWithInvalidApplicationStartInfo()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "non_existent_app",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Assert.Throws<ArgumentException>(() => new TestRunner(startInfo));
        }

        [Test]
        public void ConstructorWithInvalidWorkingDirectoryStartInfo()
        {
            string application = GetOSDefaultApplication();
            string invalidDirectory = GetInvalidOSDirectory();

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = application,
                WorkingDirectory = invalidDirectory,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Assert.Throws<DirectoryNotFoundException>(() => new TestRunner(startInfo));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void SetSTDOutputRedirect (bool startStatus, bool endStatus)
        {
            TestRunner testRunner = new TestRunner(GetOSDefaultApplication(), stdOut: startStatus);

            Assert.That(testRunner.STDOutputRedirect, Is.EqualTo(startStatus));
            Assert.That(testRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(startStatus));

            testRunner.SetStandardOutputRedirect(endStatus);

            Assert.That(testRunner.STDOutputRedirect, Is.EqualTo(endStatus));
            Assert.That(testRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(endStatus));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void SetSTDErrorRedirect(bool startStatus, bool endStatus)
        {
            TestRunner testRunner = new TestRunner(GetOSDefaultApplication(), stdErr: startStatus);

            Assert.That(testRunner.STDErrorRedirect, Is.EqualTo(startStatus));
            Assert.That(testRunner.StartInfo.RedirectStandardError, Is.EqualTo(startStatus));

            testRunner.SetStandardErrorRedirect(endStatus);

            Assert.That(testRunner.STDErrorRedirect, Is.EqualTo(endStatus));
            Assert.That(testRunner.StartInfo.RedirectStandardError, Is.EqualTo(endStatus));
        }

        [Test]
        public void SetValidWorkingDirectory()
        {
            string workingDirectory = GetValidOSDirectory();
            TestRunner testRunner = new TestRunner(GetOSDefaultApplication());

            Assert.That(testRunner.WorkingDirectory, Is.Empty);
            Assert.That(testRunner.StartInfo.WorkingDirectory, Is.Empty);

            testRunner.SetWorkingDirectory(workingDirectory);

            Assert.That(testRunner.WorkingDirectory, Is.EqualTo(workingDirectory));
            Assert.That(testRunner.StartInfo.WorkingDirectory, Is.EqualTo(workingDirectory));
        }

        [Test]
        public void SetInvalidWorkingDirectory()
        {
            string invalidDirectory = GetInvalidOSDirectory();
            TestRunner testRunner = new TestRunner(GetOSDefaultApplication());

            Assert.That(testRunner.WorkingDirectory, Is.Empty);
            Assert.That(testRunner.StartInfo.WorkingDirectory, Is.Empty);

            Assert.Throws<DirectoryNotFoundException>(() => testRunner.SetWorkingDirectory(invalidDirectory));
        }

        public void RunDefault()
        {
            string application = "echo";
            TestRunner runner = new TestRunner(application);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.Empty);
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.False);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.False);
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.Empty);
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.False);
            Assert.That(runner.STDErrorRedirect, Is.False);
            Assert.That(runner.IsApplicationAvailable(application), Is.True);

            Result<ProcessResult> result = runner.Run("Hello");

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(result.Content.ExitCode, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Not.Empty);
            Assert.Contains("Hello", runner.STDOutput);
        }




        /*[Test]
        public void SetRedirects()
        {
            var runner = new TestRunner("cmd.exe");

            runner.SetStandardOutputRedirect(true);
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.True);

            runner.SetStandardErrorRedirect(true);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.True);
        }

        [Test]
        public void SetWorkingDirectoryValid()
        {
            var runner = new TestRunner("cmd.exe");
            string path = Directory.GetCurrentDirectory();

            runner.SetWorkingDirectory(path);

            Assert.That(runner.WorkingDirectory, Is.EqualTo(path));
        }

        [Test]
        public void SetWorkingDirectoryInvalid()
        {
            var runner = new TestRunner("cmd.exe");
            string path = "C:/non_existent_dir";

            Assert.Throws<DirectoryNotFoundException>(() => runner.SetWorkingDirectory(path));
        }

        [Test]
        public void ApplicationAvailable()
        {
            var runner = new TestRunner("cmd.exe");
            Assert.That(runner.IsApplicationAvailable("cmd.exe"), Is.True);
        }

        [Test]
        public void RunCommandSuccess()
        {
            var runner = new TestRunner("cmd.exe", true, true);
            var result = runner.Run("echo hello");

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(runner.STDOutput, Does.Contain("hello"));
        }

        [Test]
        public void RunCommandFail()
        {
            var runner = new TestRunner("cmd.exe", true, true);
            var result = runner.Run("invalid_command_xyz");

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Failed));
            Assert.That(runner.STDError.Length, Is.GreaterThan(0));
        }

        [Test]
        public async Task RunAsyncCommand()
        {
            var runner = new TestRunner("cmd.exe", true, true);
            var result = await runner.RunAsync("echo async");

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(runner.STDOutput, Does.Contain("async"));
        }

        [Test]
        public void TryRunSuccess()
        {
            var runner = new TestRunner("cmd.exe", true, true);
            var result = runner.TryRun("echo try");
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TryRunAsyncSuccess()
        {
            var runner = new TestRunner("cmd.exe", true, true);
            var result = await runner.TryRunAsync("echo tryAsync");
            Assert.That(result, Is.True);
        }*/
    }
}
