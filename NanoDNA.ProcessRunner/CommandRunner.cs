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
        private ProcessStartInfo _processStartInfo { get; set; }

        /// <summary>
        /// Process Application the Command will run through.
        /// </summary>
        public ProcessApplication Application { get; set; }

        /// <summary>
        /// Toggles the Standard Output Redirect to the Console.
        /// </summary>
        private bool _stdOutputRedirect { get; set; }

        /// <summary>
        /// Toggles the Standard Error Redirect to the Console.
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
        public string WorkingDirectory { get => _processStartInfo.WorkingDirectory; }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="application">Process Application the Command will run through.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output to the Console.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error to the Console.</param>
        public CommandRunner(ProcessApplication application, bool stdOutRedirect = false, bool stdErrRedirect = false)
        {
            Application = application;
            _stdOutputRedirect = stdOutRedirect;
            _stdErrorRedirect = stdErrRedirect;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            _processStartInfo = new ProcessStartInfo();
            _processStartInfo.FileName = GetApplicationPath(application);
            _processStartInfo.RedirectStandardOutput = _stdOutputRedirect;
            _processStartInfo.RedirectStandardError = _stdErrorRedirect;
            _processStartInfo.CreateNoWindow = true;
            _processStartInfo.UseShellExecute = false;
            
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> using the String Name of the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="application">String Name of the Process Application Enum the Command will run through.</param>
        /// <param name="stdOutRedirect">Redirect the Standard Output to the Console.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error to the Console.</param>
        public CommandRunner(string application, bool stdOutRedirect = false, bool stdErrRedirect = false)
        {
            _processStartInfo = new ProcessStartInfo();

            if (Enum.TryParse(application, out ProcessApplication app))
            {
                Application = app;
                _processStartInfo.FileName = GetApplicationPath(app);
            }
            else
                SetOSDefaultApp();

            _stdOutputRedirect = stdOutRedirect;
            _stdErrorRedirect = stdErrRedirect;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            _processStartInfo.FileName = application;
            _processStartInfo.RedirectStandardOutput = _stdOutputRedirect;
            _processStartInfo.RedirectStandardError = _stdErrorRedirect;
            _processStartInfo.CreateNoWindow = true;
            _processStartInfo.UseShellExecute = false;
        }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/>. Uses the default Process Application based on the devices Operating System.
        /// </summary>
        /// <param name="stdOutRedirect">Redirect the Standard Output to the Console.</param>
        /// <param name="stdErrRedirect">Redirect the Standard Error to the Console.</param>
        public CommandRunner(bool stdOutRedirect = false, bool stdErrRedirect = false)
        {
            _stdOutputRedirect = stdOutRedirect;
            _stdErrorRedirect = stdErrRedirect;
            _standardOutput = new List<string>();
            _standardError = new List<string>();

            _processStartInfo = new ProcessStartInfo();
            _processStartInfo.RedirectStandardOutput = _stdOutputRedirect;
            _processStartInfo.RedirectStandardError = _stdErrorRedirect;
            _processStartInfo.CreateNoWindow = true;
            _processStartInfo.UseShellExecute = false;

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

            _processStartInfo.FileName = GetApplicationPath(Application);
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
            _processStartInfo.RedirectStandardOutput = redirectState;
        }

        /// <summary>
        /// Sets the Standard Error Redirect Option
        /// </summary>
        /// <param name="redirectState"> The state of the Toggle, True = Display the Standard Error in Console, False = Don't display Output.</param>
        public void SetStandardErrorOutputRedirect(bool redirectState)
        {
            _stdErrorRedirect = redirectState;
            _processStartInfo.RedirectStandardError = redirectState;
        }

        /// <summary>
        /// Sets the Working Directory where the Command will be executed.
        /// </summary>
        /// <param name="directory">The path to the Working Directory for the Process.</param>
        public void SetWorkingDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("Directory does not exist: " + directory);

            _processStartInfo.WorkingDirectory = directory;
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
        /// Runs a Command through the <see cref="ProcessApplication"/>.
        /// </summary>
        /// <param name="command">The Command to be run through the <see cref="ProcessApplication"/>.</param>
        public void RunCommand(string command)
        {
            _processStartInfo.Arguments = GetApplicationArguments(Application, command);

            using (Process process = new Process())
            {
                process.StartInfo = _processStartInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream || !process.StandardError.EndOfStream)
                {
                    string stdOutLine = process.StandardOutput.ReadLine();
                    string stdErrLine = process.StandardError.ReadLine();

                    if (stdOutLine != null)
                    {
                        _standardOutput.Add(stdOutLine);

                        if (_stdOutputRedirect)
                            Console.WriteLine(stdOutLine);
                    }

                    if (stdErrLine != null)
                    {
                        _standardError.Add(stdErrLine);

                        if (_stdErrorRedirect)
                            Console.WriteLine(stdErrLine);
                    }
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    if (_stdErrorRedirect)
                        Console.WriteLine($"Runner Exited with an Error, Exit Code : {process.ExitCode}");
                }
            }
        }

        /// <summary>
        /// Runs a Command through the <see cref="ProcessApplication"/> Asynchronously.
        /// </summary>
        /// <param name="command">The Command to be run through the <see cref="ProcessApplication"/>.</param>
        public async Task RunCommandAsync(string command)
        {
            _processStartInfo.Arguments = GetApplicationArguments(Application, command);
            _standardOutput.Clear();
            _standardError.Clear();

            using (var process = new Process())
            {
                process.StartInfo = _processStartInfo;
                process.Start();

                var outputTask = Task.Run(async () =>
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        var line = await process.StandardOutput.ReadLineAsync();

                        if (line == null)
                            continue;

                        _standardOutput.Add(line);

                        if (_stdOutputRedirect)
                            Console.WriteLine(line);
                    }
                });

                var errorTask = Task.Run(async () =>
                {
                    while (!process.StandardError.EndOfStream)
                    {
                        var line = await process.StandardError.ReadLineAsync();

                        if (line == null)
                            continue;

                        _standardError.Add(line);

                        if (_stdErrorRedirect)
                            Console.WriteLine(line);
                    }
                });

                await Task.WhenAll(outputTask, errorTask);
                await process.WaitForExitAsync();

                if (process.ExitCode != 0 && _stdErrorRedirect)
                {
                    Console.WriteLine($"Runner exited with error. Exit Code: {process.ExitCode}");
                }
            }
        }
    }
}
