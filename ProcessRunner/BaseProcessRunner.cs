using NanoDNA.ProcessRunner.Enums;
using NanoDNA.ProcessRunner.Results;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Provides the base implementation for running system processes.
    /// </summary>
    public abstract class BaseProcessRunner : IProcessRunner
    {
        /// <summary>
        /// Default exit code when a process fails to run or is null.
        /// </summary>
        private const int FAILED_TO_RUN_EXIT_CODE = -1;

        /// <summary>
        /// Instance of the Class Logger for the class.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public string ApplicationName => StartInfo.FileName;

        /// <inheritdoc />
        public ProcessStartInfo StartInfo { get; protected set; }

        /// <inheritdoc />
        public string WorkingDirectory => StartInfo.WorkingDirectory;

        /// <inheritdoc />
        public string[] STDOutput => _stdOutput.ToArray();

        /// <inheritdoc />
        public string[] STDError => _stdError.ToArray();

        /// <summary>
        /// Gets whether <see cref="STDOutput"/> is redirected to the console.
        /// </summary>
        public bool STDOutputRedirect => StartInfo.RedirectStandardOutput;

        /// <summary>
        /// Gets whether <see cref="STDError"/> is redirected to the console.
        /// </summary>
        public bool STDErrorRedirect => StartInfo.RedirectStandardError;

        /// <summary>
        /// Stores the standard output messages from the executed process.
        /// </summary>
        protected List<string> _stdOutput { get; set; }

        /// <summary>
        /// Stores the standard error messages from the executed process.
        /// </summary>
        protected List<string> _stdError { get; set; }

        /// <summary>
        /// Occurs when the standard output of the process receives data.
        /// </summary>
        public event DataReceivedEventHandler? STDOutputReceived;

        /// <summary>
        /// Occurs when the standard error of the process receives data.
        /// </summary>
        public event DataReceivedEventHandler? STDErrorReceived;

        /// <summary>
        /// Default constructore initializing a new Instance of the <see cref="BaseProcessRunner"/> class.
        /// </summary>
        private BaseProcessRunner()
        {
            STDOutputReceived = null;
            STDErrorReceived = null;

            _stdOutput = new List<string>();
            _stdError = new List<string>();

            StartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Logger.Trace("Initialized with default settings.");
        }

        /// <summary>
        /// Initializes a new Instance of the <see cref="BaseProcessRunner"/> class.
        /// </summary>
        /// <param name="applicationName">Name of the application to execute</param>
        /// <param name="workingDirectory">Working directory for the process, defaults to the current directory if unspecified</param>
        /// <param name="stdOutputRedirect">Whether to redirect the standard output, defaults to false if unspecified</param>
        /// <param name="stdErrorRedirect">Whether to redirect the standard error, defaults to false if unspecified</param>
        protected BaseProcessRunner(string applicationName, string workingDirectory = "", bool stdOutputRedirect = true, bool stdErrorRedirect = true) : this()
        {
            if (string.IsNullOrEmpty(workingDirectory))
                Initialize(new ProcessStartInfo
                {
                    FileName = applicationName,
                    RedirectStandardOutput = stdOutputRedirect,
                    RedirectStandardError = stdErrorRedirect,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            else
                Initialize(new ProcessStartInfo
                {
                    FileName = applicationName,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = stdOutputRedirect,
                    RedirectStandardError = stdErrorRedirect,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

            if (string.IsNullOrEmpty(workingDirectory))
                Logger.Debug($"Initialized with Following Info (Application : {applicationName}, STDOutRedirect : {stdOutputRedirect}, STDErrRedirect : {stdErrorRedirect})");
            else
                Logger.Debug($"Initialized with Following Info (Application : {applicationName}, Working Directory : {workingDirectory}, STDOutRedirect : {stdOutputRedirect}, STDErrRedirect : {stdErrorRedirect})");
        }

        /// <summary>
        /// Initializes the <see cref="BaseProcessRunner"/> by setting the <see cref="StartInfo"/> property and validating the application availability.
        /// </summary>
        /// <param name="startInfo">Starting configuration of the <see cref="BaseProcessRunner"/></param>
        /// <exception cref="NotSupportedException">Thrown if the application does not exist, is inaccessible or not supported on the platform</exception>
        private void Initialize(ProcessStartInfo startInfo)
        {
            StartInfo = startInfo;

            if (!IsApplicationAvailable(StartInfo.FileName))
            {
                Logger.Error($"Application '{StartInfo.FileName}' not found on the system.");
                throw new NotSupportedException($"Application '{StartInfo.FileName}' not found on the system.");
            }

            if (!string.IsNullOrEmpty(StartInfo.WorkingDirectory) && !Directory.Exists(StartInfo.WorkingDirectory))
            {
                Logger.Error($"Working directory '{StartInfo.WorkingDirectory}' does not exist.");
                throw new DirectoryNotFoundException($"Working directory '{StartInfo.WorkingDirectory}' does not exist.");
            }
        }

        /// <summary>
        /// Initializes a new Instance of the <see cref="BaseProcessRunner"/> class with a specified <see cref="ProcessStartInfo"/>.
        /// </summary>
        /// <param name="startInfo">The process start configuration for execution</param>
        protected BaseProcessRunner(ProcessStartInfo startInfo) : this()
        {
            Initialize(startInfo);
        }

        /// <inheritdoc/>
        public void SetStandardOutputRedirect(bool redirect)
        {
            StartInfo.RedirectStandardOutput = redirect;

            Logger.Debug($"STD Output Redirect : {redirect}");
        }

        /// <inheritdoc/>
        public void SetStandardErrorRedirect(bool redirect)
        {
            StartInfo.RedirectStandardError = redirect;

            Logger.Debug($"STD Error Redirect : {redirect}");
        }

        /// <inheritdoc/>
        public void SetWorkingDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Logger.Error($"Directory does not exist: {path}");
                throw new DirectoryNotFoundException("Directory does not exist: " + path);
            }

            StartInfo.WorkingDirectory = path;

            Logger.Debug($"Working Directory : {path}");
        }

        /// <summary>
        /// Saves the standard output from the process internally.
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="data">Data received by the event</param>
        protected void SaveSTDOutput(object sender, DataReceivedEventArgs data)
        {
            string? output = data.Data;

            if (output == null)
                return;

            Logger.Info($"STDOutput : {output}");

            _stdOutput.Add(output);
        }

        /// <summary>
        /// Saves the standard error from the process internally.
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="data">Data received by the event</param>
        protected void SaveSTDError(object sender, DataReceivedEventArgs data)
        {
            string? output = data.Data;

            if (output == null)
                return;

            Logger.Info($"STDError : {output}");

            _stdError.Add(output);
        }

        /// <inheritdoc/>
        public virtual Result<ProcessResult> Run(string args)
        {
            StartInfo.Arguments = args;

            string command = $"{ApplicationName} {args}";

            Logger.Info($"Running Command : {command}");

            using (Process? process = Process.Start(StartInfo)) //Process.Start(StartInfo)
            {
                if (process == null)
                {
                    Logger.Error($"Process was Null : {command}");
                    return new Result<ProcessResult>(new ProcessResult(ProcessStatus.Failed, FAILED_TO_RUN_EXIT_CODE), "Process is null");
                }

                process.OutputDataReceived += STDOutputReceived;
                process.ErrorDataReceived += STDErrorReceived;
                process.OutputDataReceived += (sender, data) => SaveSTDOutput(sender, data);
                process.ErrorDataReceived += (sender, data) => SaveSTDError(sender, data);

                //process.Start();

                /* if (process.HasExited)
                     _stdOutput.AddRange(process.StandardOutput.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries));
                 else
                 {*/
                if (STDOutputRedirect)
                    process.BeginOutputReadLine();

                if (STDErrorRedirect)
                    process.BeginErrorReadLine();
                //}

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Logger.Info($"Successfully Ran Command : {command}");
                    return new Result<ProcessResult>(new ProcessResult(ProcessStatus.Success, process.ExitCode), $"Command executed successfully: {command}");
                }

                Logger.Error($"Command exited with code {process.ExitCode}: {command}");

                return new Result<ProcessResult>(new ProcessResult(ProcessStatus.Failed, process.ExitCode), $"Command ran and failed : {command}");
            }
        }

        /// <inheritdoc/>
        public virtual async Task<Result<ProcessResult>> RunAsync(string args) => await Task.Run(() => this.Run(args));

        /// <inheritdoc/>
        public virtual bool TryRun(string args) => this.Run(args).Content.Status == ProcessStatus.Success;

        /// <inheritdoc/>
        public virtual async Task<bool> TryRunAsync(string args)
        {
            Result<ProcessResult> result = await this.RunAsync(args);
            return result.Content.Status == ProcessStatus.Success;
        }

        /// <inheritdoc/>
        public bool IsApplicationAvailable(string applicationName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = applicationName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process? process = Process.Start(startInfo))
            {
                if (process == null)
                    return false;

                process.WaitForExit();

                return process.ExitCode == 0;
            }
        }
    }
}