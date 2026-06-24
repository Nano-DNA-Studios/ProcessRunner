using System;
using System.IO;
using NUnit.Framework;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using NanoDNA.AutomationResults;
using System.Collections.Generic;

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

            Assert.That(TestRunner.IsApplicationAvailable(application), Is.True);

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
        }

        /// <summary>
        /// Tests the constructor of <see cref="BaseProcessRunner(string, string, bool, bool)"/> with a valid application name and working directory.
        /// </summary>
        [Test]
        public void ConstructorWithValidApplicationAndWorkingDirectory()
        {
            string application = GetOSDefaultApplication();
            string workingDirectory = Directory.GetCurrentDirectory();

            Assert.That(TestRunner.IsApplicationAvailable(application), Is.True);

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

            Assert.That(TestRunner.IsApplicationAvailable(application), Is.True);

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

            Assert.That(TestRunner.IsApplicationAvailable(application), Is.True);

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

            Result<int> result = runner.Run(DEFAULT_APPLICATION_COMMAND);

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Success));
            Assert.That(result.Data, Is.EqualTo(0));
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

            Result<int> result = runner.Run(DEFAULT_APPLICATION_COMMAND);

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Success));
            Assert.That(result.Data, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.Run(string)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail.
        /// </summary>
        [Test]
        public void RunDefaultFail()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<int> result = runner.Run(DEFAULT_APPLICATION_FAIL_COMMAND);

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Data, Is.Not.EqualTo(0));
            Assert.That(runner.STDError, Is.Not.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> method of <see cref="BaseProcessRunner"/> to run a default command asynchronously.
        /// </summary>
        [Test]
        public void RunAsyncDefault()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<int> result = runner.RunAsync(DEFAULT_APPLICATION_COMMAND).Result;

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Success));
            Assert.That(result.Data, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Not.Empty);
            Assert.Contains(DEFAULT_PROCESS_OUTPUT, runner.STDOutput);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> method of <see cref="BaseProcessRunner"/> to run a command without redirecting output asynchronously.
        /// </summary>
        [Test]
        public void RunAsyncNoRedirect()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION, stdOut: false);

            Result<int> result = runner.RunAsync(DEFAULT_APPLICATION_COMMAND).Result;

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Success));
            Assert.That(result.Data, Is.EqualTo(0));
            Assert.That(runner.STDOutput, Is.Empty);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail asynchronously.
        /// </summary>
        [Test]
        public void RunAsyncDefaultFail()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);

            Result<int> result = runner.RunAsync(DEFAULT_APPLICATION_FAIL_COMMAND).Result;

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Data, Is.Not.EqualTo(0));
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
        /// Tests the <see cref="BaseProcessRunner.TryRunAsync(string, CancellationToken)"/> method of <see cref="BaseProcessRunner"/> to run a default command asynchronously.
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
        /// Tests the <see cref="BaseProcessRunner.TryRunAsync(string, CancellationToken)"/> method of <see cref="BaseProcessRunner"/> to run a command without redirecting output asynchronously.
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
        /// Tests the <see cref="BaseProcessRunner.TryRunAsync(string, CancellationToken)"/> method of <see cref="BaseProcessRunner"/> to run a command that will fail asynchronously.
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
            Assert.That(TestRunner.IsApplicationAvailable(DEFAULT_VALID_APPLICATION), Is.True);
        }

        /// <summary>
        /// Tests the <see cref="BaseProcessRunner.IsApplicationAvailable(string)"/> method of <see cref="BaseProcessRunner"/> to check if it returns False for invalid applications.
        /// </summary>
        [Test]
        public void IsApplicationAvailableInvalid()
        {
            Assert.That(TestRunner.IsApplicationAvailable(DEFAULT_NON_EXISTENT_APPLICATION), Is.False);
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> runs to completion successfully when cancellation is not requested.
        /// </summary>
        [Test]
        public async Task RunAsyncWithoutCancellation()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);
            CancellationToken token = CancellationToken.None;

            Result<int> result = await runner.RunAsync(DEFAULT_APPLICATION_COMMAND, token);

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Success));
            Assert.That(result.Data, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> exits immediately when provided a pre-cancelled token.
        /// </summary>
        [Test]
        public async Task RunAsyncPreCancelledToken()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            Result<int> result = await runner.RunAsync(DEFAULT_APPLICATION_COMMAND, cts.Token);

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Cancelled));
            Assert.That(result.Data, Is.EqualTo(-1));
            Assert.That(result.Message, Does.Contain("canceled"));
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> cancels a long-running process gracefully.
        /// </summary>
        [Test]
        public async Task RunAsyncGracefulCancellation()
        {
            string longRunningApp = OperatingSystem.IsWindows() ? "cmd.exe" : "sleep";
            string longRunningArgs = OperatingSystem.IsWindows() ? "/k" : "10";

            //string longRunningArgs = OperatingSystem.IsWindows() ? "-n 10 127.0.0.1" : "10";
            //string longRunningApp = OperatingSystem.IsWindows() ? "ping" : "sleep";

            TestRunner runner = new TestRunner(longRunningApp);
            using CancellationTokenSource cts = new CancellationTokenSource();

            Task<Result<int>> runTask = runner.RunAsync(longRunningArgs, cts.Token);
            await Task.Delay(250);
            cts.Cancel();

            Result<int> result = await runTask;

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Cancelled));
            Assert.That(result.Data, Is.EqualTo(-1));
            Assert.That(result.Message, Does.Contain("canceled"));
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.RunAsync(string, CancellationToken)"/> forcefully kills a process tree if it refuses to close gracefully within the timeout.
        /// </summary>
        [Test]
        public async Task RunAsyncForcefulCancellationTimeout()
        {
            //string longRunningArgs = OperatingSystem.IsWindows() ? "-n 10 127.0.0.1" : "10";
            string longRunningApp = OperatingSystem.IsWindows() ? "ping" : "sleep";
            string longRunningArgs = OperatingSystem.IsWindows() ? "-n 10 127.0.0.1" : "-c \"trap '' SIGTERM; sleep 10\"";

            //string app = OperatingSystem.IsWindows() ? "cmd.exe" : "bash";
            //string args = OperatingSystem.IsWindows()
            //    ? "/c \"@echo off & choice /t 10 /d y > nul\""
            //    : "-c \"trap '' SIGTERM; sleep 10\"";

            TestRunner runner = new TestRunner(longRunningApp);
            using CancellationTokenSource cts = new CancellationTokenSource();

            Task<Result<int>> runTask = runner.RunAsync(longRunningArgs, cts.Token);
            await Task.Delay(250);
            cts.Cancel();

            Result<int> result = await runTask;

            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Data, Is.EqualTo(-1));
            Assert.That(result.Message, Does.Contain("killed forcefully"));
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.TryRunAsync(string, CancellationToken)"/> returns True when execution finishes cleanly without cancellation.
        /// </summary>
        [Test]
        public async Task TryRunAsyncWithoutCancellation()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);
            CancellationToken token = CancellationToken.None;

            bool success = await runner.TryRunAsync(DEFAULT_APPLICATION_COMMAND, token);

            Assert.That(success, Is.True);
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.TryRunAsync(string, CancellationToken)"/> returns False immediately if the token is already canceled.
        /// </summary>
        [Test]
        public async Task TryRunAsyncPreCancelledToken()
        {
            TestRunner runner = new TestRunner(DEFAULT_VALID_APPLICATION);
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            bool success = await runner.TryRunAsync(DEFAULT_APPLICATION_COMMAND, cts.Token);

            Assert.That(success, Is.False);
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.TryRunAsync(string, CancellationToken)"/> catches cancellation and returns False cleanly.
        /// </summary>
        [Test]
        public async Task TryRunAsyncHandlesCancellation()
        {
            string longRunningArgs = OperatingSystem.IsWindows() ? "-n 10 127.0.0.1" : "10";
            string longRunningApp = OperatingSystem.IsWindows() ? "ping" : "sleep";

            TestRunner runner = new TestRunner(longRunningApp);
            using CancellationTokenSource cts = new CancellationTokenSource();

            Task<bool> runTask = runner.TryRunAsync(longRunningArgs, cts.Token);
            await Task.Delay(250);
            cts.Cancel();

            bool success = await runTask;

            Assert.That(success, Is.False);
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.StandardOutputStream"/> and <see cref="BaseProcessRunner.StandardErrorStream"/>
        /// correctly expose the underlying stream readers.
        /// </summary>
        [Test]
        public void StandardStreamsAreExposedCorrectly()
        {
            TestRunner runner = new TestRunner(GetOSDefaultApplication());

            Assert.That(runner.StandardOutputStream, Is.Not.Null);
            Assert.That(runner.StandardErrorStream, Is.Not.Null);
            Assert.That(runner.StandardOutputStream, Is.InstanceOf<StreamReader>());
            Assert.That(runner.StandardErrorStream, Is.InstanceOf<StreamReader>());
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.STDOutput"/> cleanly extracts and parses lines written to the stream.
        /// </summary>
        [Test]
        public void STDOutputExtractsLinesFromMemoryStreamSafely()
        {
            TestRunner runner = new TestRunner(GetOSDefaultApplication());

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes($"First line{Environment.NewLine}Second line{Environment.NewLine}");

            // Write directly to the underlying memory stream layout
            runner.StandardOutputStream.BaseStream.Write(bytes, 0, bytes.Length);

            string[] extractedLines = runner.STDOutput;

            Assert.That(extractedLines, Is.Not.Null);
            Assert.That(extractedLines.Length, Is.EqualTo(2));
            Assert.That(extractedLines, Contains.Item("First line"));
            Assert.That(extractedLines, Contains.Item("Second line"));
        }

        /// <summary>
        /// Tests that <see cref="BaseProcessRunner.STDError"/> cleanly extracts and parses lines written to the error stream.
        /// </summary>
        [Test]
        public void STDErrExtractsLinesFromMemoryStreamSafely()
        {
            TestRunner runner = new TestRunner(GetOSDefaultApplication());

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes($"Error log 1{Environment.NewLine}Error log 2{Environment.NewLine}");

            // Write directly to the underlying memory stream layout
            runner.StandardErrorStream.BaseStream.Write(bytes, 0, bytes.Length);

            string[] extractedLines = runner.STDError;

            Assert.That(extractedLines, Is.Not.Null);
            Assert.That(extractedLines.Length, Is.EqualTo(2));
            Assert.That(extractedLines, Contains.Item("Error log 1"));
            Assert.That(extractedLines, Contains.Item("Error log 2"));
        }

        /// <summary>
        /// Verifies that thread-safe reading from the stream handles concurrent access gracefully.
        /// </summary>
        [Test]
        public void GetLinesFromStreamHandlesConcurrentReads()
        {
            TestRunner runner = new TestRunner(GetOSDefaultApplication());
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes($"Line 1{Environment.NewLine}Line 2{Environment.NewLine}");

            runner.StandardOutputStream.BaseStream.Write(bytes, 0, bytes.Length);

            List<Task<string[]>> readTasks = new List<Task<string[]>>();

            for (int i = 0; i < 10; i++)
            {
                readTasks.Add(Task.Run(() => runner.STDOutput));
            }

            Task.WaitAll(readTasks.ToArray());

            foreach (Task<string[]> task in readTasks)
            {
                Assert.That(task.Result.Length, Is.EqualTo(2));
                Assert.That(task.Result, Contains.Item("Line 1"));
            }
        }
    }
}