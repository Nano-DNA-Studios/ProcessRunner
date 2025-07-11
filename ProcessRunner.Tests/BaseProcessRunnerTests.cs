using NUnit.Framework;
using System.Diagnostics;
using System;
using System.IO;
using NanoDNA.ProcessRunner.Results;
using NanoDNA.ProcessRunner.Enums;

namespace NanoDNA.ProcessRunner.Tests
{
    /// <summary>
    /// Unit Tests for the <see cref="BaseProcessRunner"/> class.
    /// </summary>
    internal class BaseProcessRunnerTests : BaseUnitTest
    {
        /// <summary>
        /// Dummy class that inherits <see cref="BaseProcessRunner"/>
        /// </summary>
        private class TestRunner : BaseProcessRunner
        {
            /// <summary>
            /// Default Constructor for the <see cref="TestRunner"/> class.
            /// </summary>
            /// <param name="application">Name of the application to execute</param>
            /// <param name="workingDirectory">Working directory for the process, defaults to the current directory if unspecified</param>
            /// <param name="stdOut">Whether to redirect the standard output, defaults to false if unspecified</param>
            /// <param name="stdErr">Whether to redirect the standard error, defaults to false if unspecified</param>
            public TestRunner(string application, string workingDirectory = "", bool stdOut = true, bool stdErr = true)
                : base(application, workingDirectory, stdOut, stdErr) { }

            /// <summary>
            /// Constructor for the <see cref="TestRunner"/> class using a <see cref="ProcessStartInfo"/> object.
            /// </summary>
            /// <param name="info">Process Start Info Instance</param>
            public TestRunner(ProcessStartInfo info) : base(info) { }
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with a valid application name.
        /// </summary>
        [Test]
        public void ConstructorWithValidApplication()
        {
            string application = GetOSDefaultApplication();

            TestRunner runner = new TestRunner(application);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.Empty);
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.True);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.True);
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.Empty);
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.True);
            Assert.That(runner.STDErrorRedirect, Is.True);
            Assert.That(runner.IsApplicationAvailable(application), Is.True);
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with a valid application name and working directory.
        /// </summary>
        [Test]
        public void ConstructorWithValidApplicationAndWorkingDirectory()
        {
            string application = GetOSDefaultApplication();
            string workingDirectory = Directory.GetCurrentDirectory();

            TestRunner runner = new TestRunner(application, workingDirectory);

            Assert.That(runner.StartInfo.FileName, Is.EqualTo(application));
            Assert.That(runner.StartInfo.WorkingDirectory, Is.EqualTo(workingDirectory));
            Assert.That(runner.StartInfo.RedirectStandardOutput, Is.True);
            Assert.That(runner.StartInfo.RedirectStandardError, Is.True);
            Assert.That(runner.StartInfo.UseShellExecute, Is.False);
            Assert.That(runner.StartInfo.CreateNoWindow, Is.True);
            Assert.That(runner.StartInfo.Arguments, Is.Empty);

            Assert.That(runner.ApplicationName, Is.EqualTo(application));
            Assert.That(runner.WorkingDirectory, Is.EqualTo(workingDirectory));
            Assert.That(runner.STDOutput, Is.Empty);
            Assert.That(runner.STDError, Is.Empty);
            Assert.That(runner.STDOutputRedirect, Is.True);
            Assert.That(runner.STDErrorRedirect, Is.True);
            Assert.That(runner.IsApplicationAvailable(application), Is.True);
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with a valid application name and an invalid working directory.
        /// </summary>
        [Test]
        public void ConstructorWithValidApplicationAndInvalidWorkingDirectory()
        {
            string application = GetOSDefaultApplication();
            string workingDirectory = GetInvalidOSDirectory();

            Assert.Throws<DirectoryNotFoundException>(() => new TestRunner(application, workingDirectory));
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with redirects for standard output and error.
        /// </summary>
        /// <param name="stdOut">The Value to set STDOutput</param>
        /// <param name="stdErr">The Value to set STDError</param>
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

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with an invalid application name.
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        [Test]
        [TestCase(DEFAULT_NON_EXISTENT_APPLICATION)]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ConstructorWithInvalidApplication(string applicationName)
        {
            Assert.Throws<NotSupportedException>(() => new TestRunner(applicationName));
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with an invalid working directory.
        /// </summary>
        [Test]
        public void ConstructorWithInvalidWorkingDirectory()
        {
            string application = GetOSDefaultApplication();
            string invalidDirectory = GetInvalidOSDirectory();

            Assert.Throws<DirectoryNotFoundException>(() => new TestRunner(application, invalidDirectory));
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(ProcessStartInfo)"/> with a <see cref="ProcessStartInfo"/> object.
        /// </summary>
        [Test]
        public void ConstructorWithStartInfo()
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

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(ProcessStartInfo)"/> with an invalid <see cref="ProcessStartInfo"/> object.
        /// </summary>
        [Test]
        public void ConstructorWithInvalidApplicationStartInfo()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = DEFAULT_NON_EXISTENT_APPLICATION,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Assert.Throws<NotSupportedException>(() => new TestRunner(startInfo));
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(ProcessStartInfo)"/> with an invalid working directory in the <see cref="ProcessStartInfo"/> object.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.SetStandardOutputRedirect(bool)"/>  method of <see cref="BaseProcessRunner"/> to change the standard output redirect status.
        /// </summary>
        /// <param name="startStatus">The Starting Status of the STDOutput</param>
        /// <param name="endStatus">The Ending Status of the STDOutput</param>
        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void SetSTDOutputRedirect(bool startStatus, bool endStatus)
        {
            TestRunner testRunner = new TestRunner(GetOSDefaultApplication(), stdOut: startStatus);

            Assert.That(testRunner.STDOutputRedirect, Is.EqualTo(startStatus));
            Assert.That(testRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(startStatus));

            testRunner.SetStandardOutputRedirect(endStatus);

            Assert.That(testRunner.STDOutputRedirect, Is.EqualTo(endStatus));
            Assert.That(testRunner.StartInfo.RedirectStandardOutput, Is.EqualTo(endStatus));
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.SetStandardErrorRedirect(bool)"/> method of <see cref="BaseProcessRunner"/> to change the standard error redirect status.
        /// </summary>
        /// <param name="startStatus">The Starting Status of the STDError</param>
        /// <param name="endStatus">The Ending Status of the STDError</param>
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

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.SetWorkingDirectory(string)"/> method of <see cref="BaseProcessRunner"/> to set a valid working directory.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.SetWorkingDirectory(string)"/> method of <see cref="BaseProcessRunner"/> to set an invalid working directory.
        /// </summary>
        [Test]
        public void SetInvalidWorkingDirectory()
        {
            string invalidDirectory = GetInvalidOSDirectory();
            TestRunner testRunner = new TestRunner(GetOSDefaultApplication());

            Assert.That(testRunner.WorkingDirectory, Is.Empty);
            Assert.That(testRunner.StartInfo.WorkingDirectory, Is.Empty);

            Assert.Throws<DirectoryNotFoundException>(() => testRunner.SetWorkingDirectory(invalidDirectory));
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.Run(string)"/> method of <see cref="BaseProcessRunner"/> to run a default command.
        /// </summary>
        [Test]
        public void RunDefault()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<ProcessResult> result = runner.Run(DEFAULT_APPLICATION_COMMAND);

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(result.Content.ExitCode, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Not.Empty);
            Assert.Contains(DEFAULT_PROCESS_OUTPUT, runner.STDOutput);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.Run(string)"/> method of <see cref="BaseProcessRunner"/> to run a command without redirecting output.
        /// </summary>
        [Test]
        public void RunNoRedirect()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION, stdOut: false);

            Result<ProcessResult> result = runner.Run(DEFAULT_APPLICATION_COMMAND);

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(result.Content.ExitCode, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.Run(string)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail.
        /// </summary>
        [Test]
        public void RunDefaultFail()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<ProcessResult> result = runner.Run(DEFAULT_APPLICATION_FAIL_COMMAND);

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Failed));
            Assert.That(result.Content.ExitCode, Is.Not.EqualTo(0));
            Assert.That(runner.STDError, Is.Not.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.RunAsync(string)"/> method of <see cref="BaseProcessRunner"/> to run a default command asynchronously.
        /// </summary>
        [Test]
        public void RunAsyncDefault()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<ProcessResult> result = runner.RunAsync(DEFAULT_APPLICATION_COMMAND).Result;

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(result.Content.ExitCode, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Not.Empty);
            Assert.Contains(DEFAULT_PROCESS_OUTPUT, runner.STDOutput);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.RunAsync(string)"/> method of <see cref="BaseProcessRunner"/> to run a command without redirecting output asynchronously.
        /// </summary>
        [Test]
        public void RunAsyncNoRedirect()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION, stdOut: false);

            Result<ProcessResult> result = runner.RunAsync(DEFAULT_APPLICATION_COMMAND).Result;

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(result.Content.ExitCode, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.RunAsync(string)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail asynchronously.
        /// </summary>
        [Test]
        public void RunAsyncDefaultFail()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<ProcessResult> result = runner.RunAsync(DEFAULT_APPLICATION_FAIL_COMMAND).Result;

            Assert.That(result.Content.Status, Is.EqualTo(ProcessStatus.Failed));
            Assert.That(result.Content.ExitCode, Is.Not.EqualTo(0));
            Assert.That(runner.STDError, Is.Not.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.TryRun(string)"/> method of <see cref="BaseProcessRunner"/> to run a default command.
        /// </summary>
        [Test]
        public void TryRunDefault()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            bool result = runner.TryRun(DEFAULT_APPLICATION_COMMAND);

            Assert.That(result, Is.True);
            Assert.That(runner.STDOutput, Is.Not.Empty);
            Assert.Contains(DEFAULT_PROCESS_OUTPUT, runner.STDOutput);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.TryRun(string)"/> method of <see cref="BaseProcessRunner"/> to run a command without redirecting output.
        /// </summary>
        [Test]
        public void TryRunNoRedirect()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION, stdOut: false);

            bool result = runner.TryRun(DEFAULT_APPLICATION_COMMAND);

            Assert.That(result, Is.True);
            Assert.That(runner.STDOutput, Is.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.TryRun(string)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail.
        /// </summary>
        [Test]
        public void TryRunDefaultFail()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            bool result = runner.TryRun(DEFAULT_APPLICATION_FAIL_COMMAND);

            Assert.That(result, Is.False);
            Assert.That(runner.STDError, Is.Not.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.TryRunAsync(string)"/> method of <see cref="BaseProcessRunner"/> to run a default command asynchronously.
        /// </summary>
        [Test]
        public void TryRunAsyncDefault()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            bool result = runner.TryRunAsync(DEFAULT_APPLICATION_COMMAND).Result;

            Assert.That(result, Is.True);
            Assert.That(runner.STDOutput, Is.Not.Empty);
            Assert.Contains(DEFAULT_PROCESS_OUTPUT, runner.STDOutput);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.TryRunAsync(string)"/> method of <see cref="BaseProcessRunner"/> to run a command without redirecting output asynchronously.
        /// </summary>
        [Test]
        public void TryRunAsyncNoRedirect()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION, stdOut: false);

            bool result = runner.TryRunAsync(DEFAULT_APPLICATION_COMMAND).Result;

            Assert.That(result, Is.True);
            Assert.That(runner.STDOutput, Is.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.TryRunAsync(string)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail asynchronously.
        /// </summary>
        [Test]
        public void TryRunAsyncDefaultFail()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            bool result = runner.TryRunAsync(DEFAULT_APPLICATION_FAIL_COMMAND).Result;

            Assert.That(result, Is.False);
            Assert.That(runner.STDError, Is.Not.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.IsApplicationAvailable(string)"/> method of <see cref="BaseProcessRunner"/> to check if a valid application is available.
        /// </summary>
        [Test]
        public void IsApplicationAvailableValid()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);
            Assert.That(runner.IsApplicationAvailable(DEFAULT_VALID_APPLICATION), Is.True);
        }
    }
}