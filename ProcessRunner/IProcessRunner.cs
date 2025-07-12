using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NanoDNA.ProcessRunner.Results;

namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Represents a contract for executing system processes.
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>
        /// Gets the application name that is executed by the process runner.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// Gets the process start information used to when executed.
        /// </summary>
        public ProcessStartInfo StartInfo { get; }

        /// <summary>
        /// Gets the working directory where the process is executed from the <see cref="StartInfo"/>
        /// </summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Gets the standard output messages from the executed process.
        /// </summary>
        public string[] STDOutput { get; }

        /// <summary>
        /// Gets the standard error messages from the executed process.
        /// </summary>
        public string[] STDError { get; }

        /// <summary>
        /// Sets whether the standard output should be redirected to the console.
        /// </summary>
        /// <param name="redirect">The state of the redirect</param>
        public void SetStandardOutputRedirect(bool redirect);

        /// <summary>
        /// Sets whether the standard error should be redirected to the console.
        /// </summary>
        /// <param name="redirect">The state of the redirect</param>
        public void SetStandardErrorRedirect(bool redirect);

        /// <summary>
        /// Sets the working directory for the process execution.
        /// </summary>
        /// <param name="path">Path to the directory</param>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified path does not exist</exception>"
        public void SetWorkingDirectory(string path);

        /// <summary>
        /// Runs a process using the provided arguments and the current <see cref="StartInfo"/> configuration.
        /// </summary>
        /// <param name="args">Arguments for the process</param>
        /// <returns>A <see cref="ProcessResult"/> containing the exit code, execution status and an optional message describing the outcome</returns>
        public Result<ProcessResult> Run(string args);

        /// <summary>
        /// Runs the process asynchronously using the provided arguments.
        /// </summary>
        /// <param name="args">Arguments for the process</param>
        /// <returns>An awaitable task with a result of <see cref="ProcessResult"/> containing the exit code, execution status and an optional message describing the outcome</returns>
        public Task<Result<ProcessResult>> RunAsync(string args);

        /// <summary>
        /// Tries to run the process with the provided arguments and returns a boolean indicating success or failure.
        /// </summary>
        /// <param name="args">Arguments for the process</param>
        /// <returns>True if the process succeeded, False otherwise</returns>
        public bool TryRun(string args);

        /// <summary>
        /// Tries to run the process asynchronously with the provided arguments and returns a boolean indicating success or failure.
        /// </summary>
        /// <param name="args">Arguments for the process</param>
        /// <returns>An awaitable task with a result of True if the process succeeded, False otherwise</returns>
        public Task<bool> TryRunAsync(string args);

        /// <summary>
        /// Checks if the specified application is available on the system.
        /// </summary>
        /// <param name="applicationName">Name of the Application</param>
        /// <returns>True if the Application Exists on the Device</returns>
        public bool IsApplicationAvailable(string applicationName);
    }
}
