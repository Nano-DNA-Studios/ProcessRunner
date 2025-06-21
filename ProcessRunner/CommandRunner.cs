using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NanoDNA.ProcessRunner
{
    /// <summary>
    /// Used to run CLI Commands through the specified <see cref="ProcessApplication"/>.
    /// </summary>
    public class CommandRunner
    {
        /// <summary>
        /// Specifies the Set of Values to use when Starting a Process.
        /// </summary>
        public ProcessStartInfo ProcessStartInfo { get; private set; }

        /// <summary>
        /// Process Application the Command will run through.
        /// </summary>
        public ProcessApplication Application { get; set; }

        /// <summary>
        /// Toggles the Standard Output Redirect and Stores in the <see cref="StandardOutput"/> Property.
        /// </summary>
        private bool _stdOutputRedirect { get; set; }

        /// <summary>
        /// Toggles the Standard Error Redirect and Stores in the <see cref="StandardError"/> Property.
        /// </summary>
        private bool _stdErrorRedirect { get; set; }

        /// <summary>
        /// Standard Output of the Process.
        /// </summary>
        private List<string> _standardOutput { get; set; }

        /// <summary>
        /// Standard Output of the Process.
        /// </summary>
        public string[] StandardOutput { get => _standardOutput.ToArray(); }

        /// <summary>
        /// Standard Error of the Process.
        /// </summary>
        private List<string> _standardError { get; set; }

        /// <summary>
        /// Standard Error of the Process.
        /// </summary>
        public string[] StandardError { get => _standardError.ToArray(); }

        /// <summary>
        /// Working Directory where the Command will be executed.
        /// </summary>
        public string WorkingDirectory { get => ProcessStartInfo.WorkingDirectory; }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="application">Process Application the Command will run through.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="StandardOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="StandardError"/> Property</param>
        public CommandRunner(ProcessApplication application, bool stdOutRedirect = true, bool stdErrRedirect = true)
        {
            Application = application;
            _stdOutputRedirect = stdOutRedirect;
            _stdErrorRedirect = stdErrRedirect;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            ProcessStartInfo = new ProcessStartInfo();
            ProcessStartInfo.FileName = GetApplicationPath(application);
            ProcessStartInfo.RedirectStandardOutput = _stdOutputRedirect;
            ProcessStartInfo.RedirectStandardError = _stdErrorRedirect;
            ProcessStartInfo.CreateNoWindow = true;
            ProcessStartInfo.UseShellExecute = false;
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using the String Name of the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="applicationName">String Name of the Process Application Enum the Command will run through.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="StandardOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="StandardError"/> Property</param>
        public CommandRunner(string applicationName, bool stdOutRedirect = true, bool stdErrRedirect = true)
        {
            ProcessStartInfo = new ProcessStartInfo();

            if (Enum.TryParse(applicationName, out ProcessApplication app))
            {
                Application = app;
                ProcessStartInfo.FileName = GetApplicationPath(app);
            }
            else
                throw new ArgumentException($"Invalid Process Application: {applicationName}");

            _stdOutputRedirect = stdOutRedirect;
            _stdErrorRedirect = stdErrRedirect;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            ProcessStartInfo.FileName = applicationName;
            ProcessStartInfo.RedirectStandardOutput = _stdOutputRedirect;
            ProcessStartInfo.RedirectStandardError = _stdErrorRedirect;
            ProcessStartInfo.CreateNoWindow = true;
            ProcessStartInfo.UseShellExecute = false;
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the default Process Application based on the devices Operating System.
        /// </summary>
        /// <param name="stdOutRedirect">Redirect the Standard Output and Store in the <see cref="StandardOutput"/> Property.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error and Store in the <see cref="StandardError"/> Property</param>
        public CommandRunner(bool stdOutRedirect = true, bool stdErrRedirect = true)
        {
            _stdOutputRedirect = stdOutRedirect;
            _stdErrorRedirect = stdErrRedirect;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            ProcessStartInfo = new ProcessStartInfo();
            ProcessStartInfo.RedirectStandardOutput = _stdOutputRedirect;
            ProcessStartInfo.RedirectStandardError = _stdErrorRedirect;
            ProcessStartInfo.CreateNoWindow = true;
            ProcessStartInfo.UseShellExecute = false;

            SetOSDefaultApp();
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the <see cref="System.Diagnostics.ProcessStartInfo"/> provided by the User.
        /// </summary>
        /// <param name="processStartInfo">Process Info defined by the User</param>
        public CommandRunner(ProcessStartInfo processStartInfo)
        {
            ProcessStartInfo = processStartInfo;

            _stdOutputRedirect = processStartInfo.RedirectStandardOutput;
            _stdErrorRedirect = processStartInfo.RedirectStandardError;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            SetOSDefaultApp();
        }

        /// <summary>
        /// Defines the Default Application based on the devices OS.
        /// </summary>
        private void SetOSDefaultApp()
        {
            if (OperatingSystem.IsWindows())
                Application = ProcessApplication.CMD;
            else if (OperatingSystem.IsLinux())
                Application = ProcessApplication.Bash;
            else if (OperatingSystem.IsMacOS())
                Application = ProcessApplication.Sh;
            else
                throw new NotSupportedException($"Unsupported OS");

            ProcessStartInfo.FileName = GetApplicationPath(Application);
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
        /// Sets the Standard Output Redirect Option
        /// </summary>
        /// <param name="redirectState"> The state of the Toggle, True = Display the Standard Output in Console, False = Don't display Output.</param>
        public void SetStandardOutputOutputRedirect(bool redirectState)
        {
            _stdOutputRedirect = redirectState;
            ProcessStartInfo.RedirectStandardOutput = redirectState;
        }

        /// <summary>
        /// Sets the Standard Error Redirect Option
        /// </summary>
        /// <param name="redirectState"> The state of the Toggle, True = Display the Standard Error in Console, False = Don't display Output.</param>
        public void SetStandardErrorOutputRedirect(bool redirectState)
        {
            _stdErrorRedirect = redirectState;
            ProcessStartInfo.RedirectStandardError = redirectState;
        }

        /// <summary>
        /// Sets the Working Directory where the Command will be executed.
        /// </summary>
        /// <param name="directory">The path to the Working Directory for the Process.</param>
        public void SetWorkingDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("Directory does not exist: " + directory);

            ProcessStartInfo.WorkingDirectory = directory;
        }

        /// <summary>
        /// Returns the Executable File Name or Path for the given <see cref="ProcessApplication"/> based on the current Operating System.
        /// </summary>
        /// <param name="application">The selected Application to retrieve the Executable path for.</param>
        /// <returns>The Executable File Name or Full Path of the specified Application.</returns>
        /// <exception cref="NotSupportedException">Thrown if the specified Application is not supported on the current Operating System.</exception>
        private string GetApplicationPath(ProcessApplication application)
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

        /// <summary>
        /// Tries to Run a Command but doesn't throw an Exception if it fails.
        /// </summary>
        /// <param name="command">The Command to be run through the <see cref="ProcessApplication"/>.</param>
        /// <param name="displaySTDOutput">Display the Standard Output in the Console.</param>
        /// <param name="displaySTDError">Display the Standard Error in the Console.</param>
        public bool TryRunCommand(string command, bool displaySTDOutput = false, bool displaySTDError = false)
        {
            try
            {
                RunCommand(command, displaySTDOutput, displaySTDError);
                return true;
            }
            catch (Exception ex)
            {
                if (displaySTDError)
                    Console.WriteLine($"Error Running Command: {ex.Message} \n {ex.StackTrace}");

                return false;
            }
        }

        /// <summary>
        /// Runs a Command through the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="command">The Command to be run through the <see cref="ProcessApplication"/>.</param>
        /// <param name="displaySTDOutput">Display the Standard Output in the Console.</param>
        /// <param name="displaySTDError">Display the Standard Error in the Console.</param>
        public void RunCommand(string command, bool displaySTDOutput = false, bool displaySTDError = false)
        {
            ProcessStartInfo.Arguments = GetApplicationArguments(Application, command);

            using (Process? process = Process.Start(ProcessStartInfo))
            {
                if (process == null)
                    return;

                process.OutputDataReceived += (s, e) => STDOutputReceived(s, e, displaySTDOutput);
                process.ErrorDataReceived += (s, e) => STDErrorReceived(s, e, displaySTDError);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception($"Command exited with code {process.ExitCode}: {command}");
            }
        }

        /// <summary>
        /// Handles receiving Standard Output from the Process.
        /// </summary>
        /// <param name="sender">Object Sending the Event</param>
        /// <param name="data">Data Sent by the Object</param>
        /// <param name="displaySTDOutput">Toggle for displaying the STD Output to the Users Console</param>
        private void STDOutputReceived(object sender, DataReceivedEventArgs data, bool displaySTDOutput)
        {
            string? output = data.Data;

            if (output == null)
                return;

            _standardOutput.Add(output);

            if (displaySTDOutput)
                Console.WriteLine(output);
        }

        /// <summary>
        /// Handles receiving Standard Error from the Process.
        /// </summary>
        /// <param name="sender">Object Sending the Event</param>
        /// <param name="data">Data Sent by the Object</param>
        /// <param name="displaySTDError">Toggle for displaying the STD Error to the Users Console</param>
        private void STDErrorReceived(object sender, DataReceivedEventArgs data, bool displaySTDError)
        {
            string? output = data.Data;

            if (output == null)
                return;

            _standardError.Add(output);

            if (displaySTDError)
                Console.WriteLine(output);
        }

        /// <summary>
        /// Tries to Run a Command but doesn't throw an Exception if it fails.
        /// </summary>
        /// <param name="command">The Command to be run through the <see cref="ProcessApplication"/>.</param>
        /// <param name="displaySTDOutput">Display the Standard Output in the Console.</param>
        /// <param name="displaySTDError">Display the Standard Error in the Console.</param>
        public async Task<bool> TryRunCommandAsync(string command, bool displaySTDOutput = false, bool displaySTDError = false)
        {
            try
            {
                await RunCommandAsync(command, displaySTDOutput, displaySTDError);
                return true;
            }
            catch (Exception ex)
            {
                if (displaySTDError)
                    Console.WriteLine($"Error Running Command: {ex.Message} \n {ex.StackTrace}");

                return false;
            }
        }

        /// <summary>
        /// Runs a Command through the <see cref="ProcessApplication"/> Asynchronously.
        /// </summary>
        /// <param name="command">The Command to be run through the <see cref="ProcessApplication"/>.</param>
        /// <param name="displaySTDOutput">Display the Standard Output in the Console.</param>
        /// <param name="displaySTDError">Display the Standard Error in the Console.</param>
        public async Task RunCommandAsync(string command, bool displaySTDOutput = false, bool displaySTDError = false)
        {
            ProcessStartInfo.Arguments = GetApplicationArguments(Application, command);
            _standardOutput.Clear();
            _standardError.Clear();

            await Task.Run(() =>
            {
                using (Process? process = Process.Start(ProcessStartInfo))
                {
                    if (process == null)
                        return;

                    process.OutputDataReceived += (s, e) => STDOutputReceived(s, e, displaySTDOutput);
                    process.ErrorDataReceived += (s, e) => STDErrorReceived(s, e, displaySTDError);

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        throw new Exception($"Command exited with code {process.ExitCode}: {command}");
                }
            });
        }
    }
}