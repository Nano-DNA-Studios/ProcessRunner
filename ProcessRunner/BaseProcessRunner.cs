using NLog;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using NanoDNA.AutomationResults;
using System.Collections.Generic;

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
        /// Synchronization object used to serialize access to the standard output memory stream.
        /// </summary>
        private readonly object _outputLock = new object();

        /// <summary>
        /// Synchronization object used to serialize access to the standard error memory stream.
        /// </summary>
        private readonly object _errorLock = new object();

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
        public string[] STDOutput => GetLinesFromStream(_stdOutput, _outputLock);

        /// <inheritdoc />
        public string[] STDError => GetLinesFromStream(_stdError, _errorLock);

        /// <inheritdoc />
        public byte[] STDOutputBytes => GetBytesFromStream(_stdOutput, _outputLock);

        /// <inheritdoc />
        public byte[] STDErrorBytes => GetBytesFromStream(_stdError, _errorLock);

        /// <inheritdoc />
        public bool STDOutputRedirect => StartInfo.RedirectStandardOutput;

        /// <inheritdoc />
        public bool STDErrorRedirect => StartInfo.RedirectStandardError;

        /// <inheritdoc />
        public StreamReader StandardOutputReader { get; private set; }

        /// <inheritdoc />
        public StreamReader StandardErrorReader { get; private set; }

        /// <inheritdoc />
        public BinaryReader StandardOutputBinaryReader { get; private set; }

        /// <inheritdoc />
        public BinaryReader StandardErrorBinaryReader { get; private set; }

        /// <summary>
        /// Stores the standard output messages from the executed process.
        /// </summary>
        protected MemoryStream _stdOutput { get; set; }

        /// <summary>
        /// Stores the standard error messages from the executed process.
        /// </summary>
        protected MemoryStream _stdError { get; set; }

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

            _stdOutput = new MemoryStream();
            _stdError = new MemoryStream();

            StandardOutputReader = new StreamReader(_stdOutput, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            StandardErrorReader = new StreamReader(_stdError, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);

            StandardOutputBinaryReader = new BinaryReader(_stdOutput, Encoding.UTF8, leaveOpen: true);
            StandardErrorBinaryReader = new BinaryReader(_stdError, Encoding.UTF8, leaveOpen: true);

            StartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Logger.Trace("Initialized Base Process Runner with default settings");
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
                    RedirectStandardInput = true,
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
                    RedirectStandardInput = true,
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

            Logger.Trace("Base Process Runner initialized with Process Start Info");
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

        ///// <summary>
        ///// Saves the standard output from the process internally.
        ///// </summary>
        ///// <param name="sender">Object sending the event</param>
        ///// <param name="data">Data received by the event</param>
        //protected void SaveSTDOutput(object sender, DataReceivedEventArgs data)
        //{
        //    string? output = data.Data;
        //    if (output == null)
        //        return;

        //    lock (_outputLock)
        //    {
        //        byte[] bytes = Encoding.UTF8.GetBytes(output + Environment.NewLine);
        //        _stdOutput.Write(bytes, 0, bytes.Length);
        //    }
        //}

        ///// <summary>
        ///// Saves the standard error from the process internally.
        ///// </summary>
        ///// <param name="sender">Object sending the event</param>
        ///// <param name="data">Data received by the event</param>
        //protected void SaveSTDError(object sender, DataReceivedEventArgs data)
        //{
        //    string? output = data.Data;
        //    if (output == null)
        //        return;

        //    lock (_errorLock)
        //    {
        //        byte[] bytes = Encoding.UTF8.GetBytes(output + Environment.NewLine);
        //        _stdError.Write(bytes, 0, bytes.Length);
        //    }
        //}

        /// <inheritdoc/>
        public virtual Result<int> Run(string args)
        {
            string command = $"{ApplicationName} {args}";
            StartInfo.Arguments = args;

            Logger.Info($"Running Command : {command}");

            using (Process? process = Process.Start(StartInfo))
            {
                if (process == null)
                {
                    Logger.Error($"Process was Null : {command}");
                    return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, "Process is null");
                }

                process.OutputDataReceived += STDOutputReceived;
                process.ErrorDataReceived += STDErrorReceived;
                //process.OutputDataReceived += (sender, data) => SaveSTDOutput(sender, data);
                //process.ErrorDataReceived += (sender, data) => SaveSTDError(sender, data);

                //if (STDOutputRedirect)
                //    process.BeginOutputReadLine();

                //if (STDErrorRedirect)
                //    process.BeginErrorReadLine();

                Task outputCopyTask = STDOutputRedirect ? process.StandardOutput.BaseStream.CopyToAsync(_stdOutput) : Task.CompletedTask;
                Task errorCopyTask = STDErrorRedirect ? process.StandardError.BaseStream.CopyToAsync(_stdError) : Task.CompletedTask;

                process.WaitForExit();

                Task.WaitAll(outputCopyTask, errorCopyTask);

                //if (STDOutputRedirect)
                //    process.StandardOutput.BaseStream.CopyTo(_stdOutput);

                //if (STDErrorRedirect)
                //    process.StandardError.BaseStream.CopyTo(_stdError);

                if (process.ExitCode == 0)
                {
                    Logger.Info($"Successfully Ran Command : {command}");
                    return new Result<int>(ResultStatus.Success, process.ExitCode, $"Command executed successfully: {command}");
                }

                Logger.Error($"Command exited with code {process.ExitCode}: {command}");

                return new Result<int>(ResultStatus.Error, process.ExitCode, $"Command ran and failed : {command}");
            }
        }

        /// <inheritdoc/>
        public virtual async Task<Result<int>> RunAsync(string args, CancellationToken cancellationToken = default)
        {
            Logger.Trace("Running RunAsync");

            string command = $"{ApplicationName} {args}";
            StartInfo.Arguments = args;

            Logger.Info($"Running Command : {command}");

            using (Process? process = Process.Start(StartInfo))
            {
                if (process == null)
                {
                    Logger.Error($"Process was Null : {command}");
                    return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, "Process is null");
                }

                process.OutputDataReceived += STDOutputReceived;
                process.ErrorDataReceived += STDErrorReceived;
                //process.OutputDataReceived += (sender, data) => SaveSTDOutput(sender, data);
                //process.ErrorDataReceived += (sender, data) => SaveSTDError(sender, data);

                //if (STDOutputRedirect)
                //    process.BeginOutputReadLine();

                //if (STDErrorRedirect)
                //    process.BeginErrorReadLine();

                Task outputCopyTask = STDOutputRedirect ? process.StandardOutput.BaseStream.CopyToAsync(_stdOutput, cancellationToken) : Task.CompletedTask;
                Task errorCopyTask = STDErrorRedirect ? process.StandardError.BaseStream.CopyToAsync(_stdError, cancellationToken) : Task.CompletedTask;

                try
                {
                    await process.WaitForExitAsync(cancellationToken);
                }
                catch
                {
                    Logger.Warn($"Cancellation requested for command: {command}");

                    if (process.HasExited)
                        return new Result<int>(ResultStatus.Cancelled, FAILED_TO_RUN_EXIT_CODE, $"Command was canceled and has exited: {command}");

                    Task gracePeriodTask = Task.Delay(5000);
                    Task killTask = CancelProcessGracefully(process);
                    
                    Task completedKillTask = await Task.WhenAny(killTask, gracePeriodTask);

                    bool graceCondition = completedKillTask == gracePeriodTask && !process.HasExited;
                    bool killErrorCondition = completedKillTask == killTask && killTask.Exception != null;

                    if (graceCondition)
                    {
                        Logger.Warn($"Process did not exit within the grace period. Force killing process tree: {command}");

                        process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync(CancellationToken.None);

                        return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, $"Command was canceled and was killed forcefully: {command}");
                    }

                    if (killErrorCondition)
                    {
                        Logger.Warn($"Process cancellation resulted in an error. Force killing process tree: {command}");

                        process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync(CancellationToken.None);

                        return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, $"Command was canceled and was killed forcefully: {command}");
                    }

                    return new Result<int>(ResultStatus.Cancelled, FAILED_TO_RUN_EXIT_CODE, $"Command was canceled and exited gracefully: {command}");
                }

                await Task.WhenAll(outputCopyTask, errorCopyTask);

                //if (STDOutputRedirect)
                //    await process.StandardOutput.BaseStream.CopyToAsync(_stdOutput, cancellationToken);

                //if (STDErrorRedirect)
                //    await process.StandardError.BaseStream.CopyToAsync(_stdError, cancellationToken);

                if (process.ExitCode == 0)
                {
                    Logger.Info($"Successfully Ran Command : {command}");
                    return new Result<int>(ResultStatus.Success, process.ExitCode, $"Command executed successfully: {command}");
                }

                Logger.Error($"Command exited with code {process.ExitCode}: {command}");

                return new Result<int>(ResultStatus.Error, process.ExitCode, $"Command ran and failed: {command}");
            }
        }

        /// <summary>
        /// Sends the appropriate signal to the Process to gracefully cancel it
        /// </summary>
        /// <param name="process">Process to Cancel</param>
        /// <returns>Graceful cancellation task to be run</returns>
        private async Task CancelProcessGracefully (Process process)
        {
            if (!OperatingSystem.IsWindows())
            {
                Logger.Info("Sending SIGTERM Signal");

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "kill",
                    Arguments = $"-15 {process.Id}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? killProcess = Process.Start(startInfo))
                {
                    if (killProcess == null)
                    {
                        Logger.Error($"Kill Process was Null");
                        throw new Exception("Kill Process Was Null");
                    }

                    await killProcess.WaitForExitAsync();
                }

                return;
            }

            Logger.Info("Sending Ctrl+C Command");

            process.StandardInput.WriteLine("\x3");
            process.StandardInput.Close();

            await process.WaitForExitAsync();
        }

        /// <inheritdoc/>
        public virtual bool TryRun(string args)
        {
            Logger.Trace("Running TryRun");
            return this.Run(args).Status == ResultStatus.Success;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryRunAsync(string args, CancellationToken cancellationToken = default)
        {
            Logger.Trace("Running TryRunAsync");
            Result<int> result = await this.RunAsync(args, cancellationToken);
            return result.Status == ResultStatus.Success;
        }

        /// <summary>
        /// Extracts individual text lines from a given memory stream in a thread-safe manner.
        /// </summary>
        /// <param name="stream">The source memory stream to read from.</param>
        /// <param name="lockObject">The synchronization object used to secure access to the stream.</param>
        /// <returns>An array of strings representing the lines extracted from the stream.</returns>
        private string[] GetLinesFromStream(MemoryStream stream, object lockObject)
        {
            Logger.Trace("Getting String lines from Stream");

            lock (lockObject)
            {
                if (stream.Length == 0)
                    return Array.Empty<string>();

                List<string> lines = new List<string>();

                long originalPosition = stream.Position;
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
                {
                    while (!reader.EndOfStream)
                    {
                        string? line = reader.ReadLine();

                        if (line != null)
                            lines.Add(line);
                    }
                }

                stream.Position = originalPosition;

                return lines.ToArray();
            }
        }

        private byte[] GetBytesFromStream (MemoryStream stream, object lockObject)
        {
            Logger.Trace("Getting Raw Bytes from Stream");

            lock (lockObject)
            {
                if (stream == null || stream.Length == 0)
                    return new byte[0];

                return stream.ToArray();
            }
        }

        /// <inheritdoc/>
        public static bool IsApplicationAvailable(string applicationName)
        {
            Logger.Trace("Verifying if Application is Available");

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