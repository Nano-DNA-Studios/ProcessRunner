using System;
using System.Diagnostics;
using NanoDNA.ProcessRunner.Enums;
using NanoDNA.ProcessRunner.Results;
using NLog;

namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Used as a Wrapper to simplify running Commands through a default supported <see cref="ProcessApplication"/> such as CMD, PowerShell, Bash, or Sh.
    /// </summary>
    public class CommandRunner : BaseProcessRunner
    {
        /// <summary>
        /// Logger instance for the CommandRunner class, used for logging errors and information for debugging and transparency.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Command Line Application the Command runs through.
        /// </summary>
        public ProcessApplication Application { get; }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="application">Process Application the Command will run through.</param>
        /// <param name="workingDirectory">Working Directory to run the Command in. Defaults to the current directory if not specified.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property. Default is true if unspecified</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property. Default is true if unspecified</param>
        public CommandRunner(ProcessApplication application, string workingDirectory = "", bool stdOutRedirect = true, bool stdErrRedirect = true) : base(GetApplicationPath(application), workingDirectory, stdOutRedirect, stdErrRedirect)
        {
            Application = application;
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using the String Name of the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="applicationName">String Name of the Process Application Enum the Command will run through.</param>
        /// <param name="workingDirectory">Working Directory to run the Command in. Defaults to the current directory if not specified.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property</param>
        public CommandRunner(string applicationName, string workingDirectory = "", bool stdOutRedirect = true, bool stdErrRedirect = true) : base(GetApplicationPath(applicationName), workingDirectory, stdOutRedirect, stdErrRedirect)
        {
            Application = GetApplicationFromNameOrPath(applicationName);
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the default Process Application based on the devices Operating System.
        /// </summary>
        /// <param name="workingDirectory">Working Directory to run the Command in. Defaults to the current directory if not specified.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property</param>
        public CommandRunner(string workingDirectory = "", bool stdOutRedirect = true, bool stdErrRedirect = true) : base(GetApplicationPath(), workingDirectory, stdOutRedirect, stdErrRedirect)
        {
            Application = GetDefaultOSApplication();
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the <see cref="System.Diagnostics.ProcessStartInfo"/> provided by the User.
        /// </summary>
        /// <param name="startInfo">Process Info defined by the User</param>
        public CommandRunner(ProcessStartInfo startInfo) : base(startInfo)
        {
            Application = GetApplicationFromNameOrPath(GetApplicationPath(startInfo.FileName));
        }

        /// <summary>
        /// Gets the <see cref="ProcessApplication"/> Enum from the String Name or Path to the Application.
        /// </summary>
        /// <param name="applicationName">String name of the Application</param>
        /// <returns><see cref="ProcessApplication"/> Enum corresponding to the Application Name</returns>
        /// <exception cref="ArgumentException">Thrown if the provided</exception>
        private static ProcessApplication GetApplicationFromNameOrPath(string applicationName)
        {
            if (Enum.TryParse(applicationName, out ProcessApplication app))
                return app;

            switch (applicationName.ToLower())
            {
                case "cmd.exe":
                    return ProcessApplication.CMD;
                case "powershell.exe":
                    return ProcessApplication.PowerShell;
                case "/bin/bash":
                    return ProcessApplication.Bash;
                case "/bin/sh":
                    return ProcessApplication.Sh;
            }

            Logger.Error($"Invalid Process Application: {applicationName}");
            throw new ArgumentException($"Invalid Process Application: {applicationName}");
        }

        /// <summary>
        /// Gets the devices Default <see cref="ProcessApplication"/> based on the Operating System.
        /// </summary>
        /// <returns>
        /// <para>The default <see cref="ProcessApplication"/> for the OS.</para>
        /// <para>  - Windows: <see cref="ProcessApplication.CMD"/></para>
        /// <para>  - Linux: <see cref="ProcessApplication.Bash"/></para>
        /// <para>  - MacOS: <see cref="ProcessApplication.Sh"/></para>
        /// </returns>
        public static ProcessApplication GetDefaultOSApplication()
        {
            if (OperatingSystem.IsWindows())
                return ProcessApplication.CMD;
            else if (OperatingSystem.IsLinux())
                return ProcessApplication.Bash;
            else if (OperatingSystem.IsMacOS())
                return ProcessApplication.Sh;
            else
                throw new NotSupportedException("Unsupported OS");
        }

        /// <summary>
        /// Gets the Path to the Application executable based on the devices Operating System default <see cref="ProcessApplication"/>.
        /// </summary>
        /// <returns>Path to the <see cref="ProcessApplication"/> executable</returns>
        private static string GetApplicationPath() => GetApplicationPath(GetDefaultOSApplication());

        /// <summary>
        /// Gets the Path to the Application executable using the string name of the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="applicationName">String name of the application</param>
        /// <returns>Path to the <see cref="ProcessApplication"/> executable</returns>
        private static string GetApplicationPath(string applicationName) => GetApplicationPath(GetApplicationFromNameOrPath(applicationName));

        /// <summary>
        /// Gets the Path to the Application executable based on the <see cref="ProcessApplication"/> Enum.
        /// </summary>
        /// <param name="application"><see cref="ProcessApplication"/> enum choice</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if the devices Operating System is not supported</exception>
        private static string GetApplicationPath(ProcessApplication application)
        {
            if (OperatingSystem.IsWindows())
            {
                switch (application)
                {
                    case ProcessApplication.CMD:
                        return "cmd.exe";
                    case ProcessApplication.PowerShell:
                        return "powershell.exe";
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                switch (application)
                {
                    case ProcessApplication.Bash:
                        return "/bin/bash";
                    case ProcessApplication.Sh:
                        return "/bin/sh";
                }
            }

            Logger.Error($"CLI Application not Supported : {application}");

            throw new NotSupportedException($"Command Line Application is not Supported : {application}");
        }

        /// <summary>
        /// Gets the formatted Arguments used by the Process Application to run Commands.
        /// </summary>
        /// <param name="application"> <see cref="ProcessApplication"/> that will run the Command.</param>
        /// <param name="command">Command to Run.</param>
        /// <returns>Argument to run the Command through it's respective <see cref="ProcessApplication"/></returns>
        private string GetApplicationArguments(ProcessApplication application, string command)
        {
            switch (application)
            {
                case ProcessApplication.CMD:
                    return $"/c {command}";
                case ProcessApplication.PowerShell:
                    return $"-Command \"{command}\"";
                case ProcessApplication.Bash:
                    return $"-c \"{command}\"";
                case ProcessApplication.Sh:
                    return $"-c \"{command}\"";
                default:
                    throw new NotSupportedException($"Unsupported Application: {application}");
            }
        }

        /// <inheritdoc/>
        public override Result<ProcessResult> Run(string args)
        {
            return base.Run(GetApplicationArguments(Application, args));
        }
    }
}