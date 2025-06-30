using NanoDNA.ProcessRunner.Enums;
using System;

namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Used as a Wrapper for the <see cref="BaseProcessRunner"/> to run a Processes through.
    /// </summary>
    internal class ProcessRunner : BaseProcessRunner
    {
        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using the String Name of the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="applicationName">String Name of the Process Application Enum the Command will run through.</param>
        /// <param name="workingDirectory">Working Directory to run the Command in. Defaults to the current directory if not specified.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property. Default is true if unspecified</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property. Default is true if unspecified</param>
        public ProcessRunner(string applicationName, string workingDirectory = "", bool stdOutRedirect = true, bool stdErrRedirect = true) : base(applicationName, workingDirectory, stdOutRedirect, stdErrRedirect)
        {
            ApplicationExists(applicationName);
        }

        /// <summary>
        /// Verifies that the specified application exists on the system.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ApplicationExists(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException("Application name cannot be null or empty.", nameof(applicationName));

            if (!IsApplicationAvailable(applicationName))
                throw new ArgumentException($"Application '{applicationName}' not found on the system.", nameof(applicationName));
        }
    }
}