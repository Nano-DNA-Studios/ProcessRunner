using System;
using System.Diagnostics;
using NLog;

namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Used to run CLI Commands through the specified <see cref="ProcessApplication"/>.
    /// </summary>
    public class CommandRunner : BaseProcessRunner
    {
        /// <summary>
        /// NLog Logger instance for the Class. Used to Log various levels of Information within the Library
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Process Application the Command will run through.
        /// </summary>
        public ProcessApplication Application { get; }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="application">Process Application the Command will run through.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property</param>
        public CommandRunner(ProcessApplication application, bool stdOutRedirect = true, bool stdErrRedirect = true) : base(GetApplicationPath(application), stdOutRedirect, stdErrRedirect)
        {
            Application = application;
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using the String Name of the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="applicationName">String Name of the Process Application Enum the Command will run through.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property</param>
        public CommandRunner(string applicationName, bool stdOutRedirect = true, bool stdErrRedirect = true) : base(GetApplicationPath(applicationName), stdOutRedirect, stdErrRedirect)
        {
            Application = GetApplicationFromName(applicationName);
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the default Process Application based on the devices Operating System.
        /// </summary>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="BaseProcessRunner.STDOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="BaseProcessRunner.STDError"/> Property</param>
        public CommandRunner(bool stdOutRedirect = true, bool stdErrRedirect = true) : base(GetApplicationPath(), stdOutRedirect, stdErrRedirect)
        {
            Application = GetDefaultOSApplication();
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the <see cref="System.Diagnostics.ProcessStartInfo"/> provided by the User.
        /// </summary>
        /// <param name="startInfo">Process Info defined by the User</param>
        public CommandRunner(ProcessStartInfo startInfo) : base(startInfo)
        {
            Application = GetApplicationFromName(GetApplicationPath(startInfo.FileName));
        }

        private static ProcessApplication GetApplicationFromName(string applicationName)
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

        private static string GetApplicationPath() => GetApplicationPath(GetDefaultOSApplication());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        private static string GetApplicationPath(string applicationName) => GetApplicationPath(GetApplicationFromName(applicationName));

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
        /// Gets the Arguments that will be passed to the Application based on the Operating System.
        /// </summary>
        /// <param name="application"> <see cref="ProcessApplication"/> that will run the Command.</param>
        /// <param name="command">Command to Run.</param>
        /// <returns>Argument to run the Command through it's respective <see cref="ProcessApplication"/></returns>
        private string GetApplicationArguments(ProcessApplication application, string command)
        {
            switch (application)
            {
                case ProcessApplication.CMD:
                    Logger.Info("Inputting Command");
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
        public override ProcessResult Run(string args)
        {
            Logger.Info("Command Runner Run");
            return base.Run(GetApplicationArguments(Application, args));
        }
    }
}