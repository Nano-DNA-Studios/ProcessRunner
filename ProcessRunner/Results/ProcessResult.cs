using NanoDNA.ProcessRunner.Enums;
using NLog;

namespace NanoDNA.ProcessRunner.Results
{
    /// <summary>
    /// Represents the result of a process execution.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// Logger instance for the CommandRunner class, used for logging errors and information for debugging and transparency.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the exit code of the process.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Indicates the status of the process execution, whether it was successful, failed, or did not run.
        /// </summary>
        public ProcessStatus Status { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessResult"/> class with the specified exit code, success status, and optional message.
        /// </summary>
        /// <param name="exitCode">The exit code of the execution</param>
        /// <param name="status">The success status of the execution</param>
        public ProcessResult(ProcessStatus status, int exitCode)
        {
            Status = status;
            ExitCode = exitCode;

            Logger.Debug($"Process Result Created : (Status={status}, ExitCode={exitCode})");
        }
    }
}
