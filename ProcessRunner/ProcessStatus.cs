
namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Stores the possible statuses of a process execution.
    /// </summary>
    public enum ProcessStatus
    {
        /// <summary>
        /// Indicates the process executed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Indicates the process execution failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates the process did not run, either due to an error or because it was not started.
        /// </summary>
        DidNotRun,
    }
}
