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

        /// <summary>
        /// Delegate function used to pass the <see cref="SaveSTDOutput(byte[], int, StringBuilder)"/> and <see cref="SaveSTDError(byte[], int, StringBuilder)"/> functions to the <see cref="CreateWriterTask(Stream, StreamChunkProcessor, DataReceivedEventHandler?)"/> function
        /// </summary>
        /// <param name="buffer">Buffer of bytes to write</param>
        /// <param name="bytesRead">Number of bytes to write</param>
        /// <param name="lineBuilder">The converted string line to construct from the bytes</param>
        private delegate void StreamChunkProcessor(byte[] buffer, int bytesRead, StringBuilder lineBuilder);

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

        /// <summary>
        /// Saves the Standard Output from the process internally and invokes the receiver
        /// </summary>
        /// <param name="buffer">Buffer of bytes to write</param>
        /// <param name="count">Number of bytes to write</param>
        /// <param name="lineBuilder">The converted string line to construct from the bytes</param>
        protected void SaveSTDOutput(byte[] buffer, int count, StringBuilder lineBuilder)
        {
            Logger.Trace("Saving Data to STD Output");

            if (count <= 0)
                return;

            lock (_outputLock)
            {
                _stdOutput.Write(buffer, 0, count);
            }

            if (STDOutputReceived?.GetInvocationList().Length > 0)
                ParseAndInvokeLine(buffer, count, lineBuilder, line => STDOutputReceived?.Invoke(this, CreateEventArgs(line)));
        }

        /// <summary>
        /// Saves the Standard Error from the process internally and invokes the receiver
        /// </summary>
        /// <param name="buffer">Buffer of bytes to write</param>
        /// <param name="count">Number of bytes to write</param>
        /// <param name="lineBuilder">The converted string line to construct from the bytes</param>
        protected void SaveSTDError(byte[] buffer, int count, StringBuilder lineBuilder)
        {
            Logger.Trace("Saving Data to STD Error");

            if (count <= 0)
                return;

            lock (_errorLock)
            {
                _stdError.Write(buffer, 0, count);
            }

            if (STDErrorReceived?.GetInvocationList().Length > 0)
                ParseAndInvokeLine(buffer, count, lineBuilder, line => STDErrorReceived?.Invoke(this, CreateEventArgs(line)));
        }

        /// <summary>
        /// Parses a raw byte buffer chunk into text strings line-by-line, omitting carriage returns, and dispatches them to a parsing action upon encountering a newline character.
        /// </summary>
        /// <param name="buffer">The raw byte array chunk received from the process stream.</param>
        /// <param name="count">The number of valid bytes to read from the buffer.</param>
        /// <param name="lineBuilder">The persistent string builder instance used to store and aggregate incomplete text fragments across chunks.</param>
        /// <param name="onLineParsed">The callback action to execute with the string data whenever a complete line is parsed.</param>
        private void ParseAndInvokeLine(byte[] buffer, int count, StringBuilder lineBuilder, Action<string> onLineParsed)
        {
            Logger.Trace($"Parsing and Invoking line");

            string textChunk = Encoding.UTF8.GetString(buffer, 0, count);

            for (int i = 0; i < textChunk.Length; i++)
            {
                char c = textChunk[i];

                if (c == '\n' || c == '\r')
                {
                    if (lineBuilder.Length == 0)
                        continue;

                    onLineParsed(lineBuilder.ToString());
                    lineBuilder.Clear();
                }
                else
                    lineBuilder.Append(c);
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="DataReceivedEventArgs"/> instance by reflecting into its internal constructor to pass line data.
        /// </summary>
        /// <param name="lineData">The string data representing the parsed line to attach to the event arguments.</param>
        /// <returns>A new non-null instance of <see cref="DataReceivedEventArgs"/> containing the line data.</returns>
        private DataReceivedEventArgs CreateEventArgs(string? lineData)
        {
            Logger.Trace("Creating Event Args for Invoke");

            object[] data = new object[0];

            if (!string.IsNullOrEmpty(lineData))
                data = new object[] { lineData };

            return (DataReceivedEventArgs)Activator.CreateInstance(
                typeof(DataReceivedEventArgs),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                data,
                null
            )!;
        }

        /// <summary>
        /// Creates a new Task to read from the specified process stream, process chunks via a delegate, and invoke the associated data handler.
        /// </summary>
        /// <param name="stream">The source process stream to read data from.</param>
        /// <param name="processor">The chunk processor delegate responsible for handling the byte buffer and updating the string builder state.</param>
        /// <param name="handler">The data received event handler to invoke when a full line or remaining text block is parsed.</param>
        /// <returns>A running Task instance that performs the asynchronous stream reading loop.</returns>
        private Task CreateWriterTask(Stream stream, StreamChunkProcessor processor, DataReceivedEventHandler? handler)
        {
            Logger.Trace("Creating new Write task instance");

            return Task.Run(async () =>
            {
                StringBuilder lineBuilder = new StringBuilder();
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    processor.Invoke(buffer, bytesRead, lineBuilder);
                }

                if (lineBuilder.Length > 0 && handler?.GetInvocationList().Length > 0)
                    handler?.Invoke(this, CreateEventArgs(lineBuilder.ToString()));
            });
        }

        /// <summary>
        /// Safely awaits background streaming tasks with a hard timeout to prevent deadlocks during abnormal process aborts.
        /// </summary>
        private async Task SafeAwaitStreamsAsync(List<Task> streamTasks)
        {
            Logger.Trace("Waiting for the Writer Streams");

            try
            {
                Task streamWaitTask = Task.WhenAll(streamTasks);
                Task timeoutFallback = Task.Delay(2000);

                Task completedTask = await Task.WhenAny(streamWaitTask, timeoutFallback).ConfigureAwait(false);

                if (completedTask == timeoutFallback)
                    Logger.Warn("Stream synchronization tasks timed out during fallback termination.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred wrapping up process stream threads.");
            }
        }

        /// <inheritdoc/>
        public virtual Result<int> Run(string args, TimeSpan? timeout = null)
        {
            string command = $"{ApplicationName} {args}";
            StartInfo.Arguments = args;

            Logger.Debug($"Running Command : {command}");

            using (Process? process = Process.Start(StartInfo))
            {
                if (process == null)
                {
                    Logger.Error($"Process was Null : {command}");
                    return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, "Process is null");
                }

                List<Task> streamTasks = new List<Task>();

                if (STDOutputRedirect)
                    streamTasks.Add(CreateWriterTask(process.StandardOutput.BaseStream, SaveSTDOutput, STDOutputReceived));

                if (STDErrorRedirect)
                    streamTasks.Add(CreateWriterTask(process.StandardError.BaseStream, SaveSTDError, STDErrorReceived));

                bool exited = true;

                if (timeout != null && timeout.HasValue)
                    exited = process.WaitForExit(timeout.Value);
                else
                    process.WaitForExit();

                if (!exited)
                {
                    Logger.Warn($"Process timed out after {timeout}. Force killing process tree: {command}");

                    process.Kill(entireProcessTree: true);
                    process.WaitForExit();
                    Task.WaitAll(SafeAwaitStreamsAsync(streamTasks));

                    return new Result<int>(ResultStatus.Cancelled, FAILED_TO_RUN_EXIT_CODE, $"Command timed out: {command}");
                }

                Task.WaitAll(SafeAwaitStreamsAsync(streamTasks));

                if (process.ExitCode == 0)
                {
                    Task.WaitAll(streamTasks.ToArray());
                    Logger.Debug($"Successfully Ran Command : {command}");
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

            Logger.Debug($"Running Command : {command}");

            using (Process? process = Process.Start(StartInfo))
            {
                if (process == null)
                {
                    Logger.Error($"Process was Null : {command}");
                    return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, "Process is null");
                }

                List<Task> streamTasks = new List<Task>();

                if (STDOutputRedirect)
                    streamTasks.Add(CreateWriterTask(process.StandardOutput.BaseStream, SaveSTDOutput, STDOutputReceived));

                if (STDErrorRedirect)
                    streamTasks.Add(CreateWriterTask(process.StandardError.BaseStream, SaveSTDError, STDErrorReceived));

                try
                {
                    await process.WaitForExitAsync(cancellationToken);
                    await SafeAwaitStreamsAsync(streamTasks);
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

                    if (graceCondition || killErrorCondition)
                    {
                        string condition = graceCondition ? "did not exit within the grace period" : "cancellation resulted in an error";

                        Logger.Warn($"Process {condition}. Force killing process tree: {command}");

                        process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync(CancellationToken.None);
                        await SafeAwaitStreamsAsync(streamTasks);

                        return new Result<int>(ResultStatus.Error, FAILED_TO_RUN_EXIT_CODE, $"Command was canceled and was killed forcefully: {command}");
                    }

                    return new Result<int>(ResultStatus.Cancelled, FAILED_TO_RUN_EXIT_CODE, $"Command was canceled and exited gracefully: {command}");
                }

                if (process.ExitCode == 0)
                {
                    Logger.Debug($"Successfully Ran Command : {command}");
                    return new Result<int>(ResultStatus.Success, process.ExitCode, $"Command executed successfully: {command}");
                }

                Logger.Error($"Command exited with code {process.ExitCode}: {command}");

                return new Result<int>(ResultStatus.Error, process.ExitCode, $"Command ran and failed: {command}");
            }
        }

        /// <inheritdoc/>
        public virtual bool TryRun(string args, TimeSpan? timeout = null)
        {
            Logger.Trace("Running TryRun");
            return this.Run(args, timeout).Status == ResultStatus.Success;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> TryRunAsync(string args, CancellationToken cancellationToken = default)
        {
            Logger.Trace("Running TryRunAsync");
            Result<int> result = await this.RunAsync(args, cancellationToken);
            return result.Status == ResultStatus.Success;
        }

        /// <summary>
        /// Sends the appropriate signal to the Process to gracefully cancel it
        /// </summary>
        /// <param name="process">Process to Cancel</param>
        /// <returns>Graceful cancellation task to be run</returns>
        private async Task CancelProcessGracefully(Process process)
        {
            Logger.Trace("Cancelling the Process Gracefully");

            if (!OperatingSystem.IsWindows())
            {
                Logger.Debug("Sending SIGTERM Signal");

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

            Logger.Debug("Sending Ctrl+C Command");

            process.StandardInput.WriteLine("\x3");
            process.StandardInput.Close();

            await process.WaitForExitAsync();
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

        /// <summary>
        /// Extracts the array of bytes from the given memory stream in a thread-safe manner.
        /// </summary>
        /// <param name="stream">The source memory stream to read from.</param>
        /// <param name="lockObject">The synchronization object used to secure access to the stream.</param>
        /// <returns>An array of bytes representing the data from the stream</returns>
        private byte[] GetBytesFromStream(MemoryStream stream, object lockObject)
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