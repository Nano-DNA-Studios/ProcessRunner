using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NanoDNA.ProcessRunner
{
    internal partial class CommandRunner
    {
        /// <summary>
        /// Specifies the Set of Values to use when Starting a Process.
        /// </summary>
        private ProcessStartInfo _processStartInfo { get; set; }

        /// <summary>
        /// Enum Defining the CLI Process Application the Command will run through.
        /// </summary>
        public ProcessApplication Application { get; set; }

        /// <summary>
        /// Toggles the Standard Output Redirect to the Console.
        /// </summary>
        private bool _outputRedirect = true;

        /// <summary>
        /// Standard Output of the Process.
        /// </summary>
        private List<string> _standardOutput = new List<string>();

        /// <summary>
        /// Standard Output of the Process.
        /// </summary>
        public string[] StandardOutput { get => _standardOutput.ToArray(); }

        /// <summary>
        /// Standard Error of the Process.
        /// </summary>
        private List<string> _standardError = new List<string>();

        /// <summary>
        /// Standard Error of the Process.
        /// </summary>
        public string[] StandardError { get => _standardError.ToArray(); }

        /// <summary>
        /// Initializes a new Instance of <see cref="CommandRunner"/> with a Specified Process Application   Specific Console Process Handler Constructor, Lets the User Define the Process Application using the <see cref="ProcessApplication"/>.
        /// </summary>
        public CommandRunner(ProcessApplication application)
        {
            _processStartInfo = new ProcessStartInfo();

            Application = application;
            _processStartInfo.FileName = GetApplicationPath(application);
            _processStartInfo.RedirectStandardOutput = true;
            _processStartInfo.RedirectStandardError = true;
            _processStartInfo.CreateNoWindow = true;
            _processStartInfo.UseShellExecute = false;
            _standardOutput = new List<string>();
        }

        /// <summary>
        /// Default Console Process Handler Constructor, Lets the User Define the Process Application using their <see cref="String"/> Name.
        /// </summary>
        /// <param name="application">Name of the Application as a String</param>
        public CommandRunner(string application)
        {
            _processStartInfo = new ProcessStartInfo();

            if (Enum.TryParse(application, out ProcessApplication app))
            {
                Application = app;
                _processStartInfo.FileName = GetApplicationPath(app);
            }
            else
                SetOSDefaultApp();

            _processStartInfo.FileName = application;
            _processStartInfo.RedirectStandardOutput = true;
            _processStartInfo.RedirectStandardError = true;
            _processStartInfo.CreateNoWindow = true;
            _processStartInfo.UseShellExecute = false;
            _standardOutput = new List<string>();
        }

        /// <summary>
        /// Default Console Process Handler Constructor, Automatically defines the Application based on the OS.
        /// </summary>
        public CommandRunner()
        {
            _processStartInfo = new ProcessStartInfo();
            _processStartInfo.RedirectStandardOutput = true;
            _processStartInfo.RedirectStandardError = true;
            _processStartInfo.CreateNoWindow = true;
            _processStartInfo.UseShellExecute = false;
            _standardOutput = new List<string>();

            SetOSDefaultApp();
        }

        /// <summary>
        /// Defines the Default Application based on the OS.
        /// </summary>
        private void SetOSDefaultApp()
        {
            if (OperatingSystem.IsWindows())
            {
                Application = ProcessApplication.CMD;
            }
            else if (OperatingSystem.IsLinux())
            {
                Application = ProcessApplication.Bash;
            }
            else if (OperatingSystem.IsMacOS())
            {
                Application = ProcessApplication.Sh;
            }
            else
                throw new NotSupportedException($"Unsupported OS");

            _processStartInfo.FileName = GetApplicationPath(Application);
        }

        /// <summary>
        /// Gets the Default Application based on the OS.
        /// </summary>
        /// <returns>Default <see cref="ProcessApplication"/> for the OS</returns>
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
        /// Toggles the Standard Output Redirect Option
        /// </summary>
        /// <param name="redirectState"> The state of the Toggle, True = Display the Standard Output in Console, False = Don't display Output </param>
        public void SetOutputRedirect(bool redirectState)
        {
            _outputRedirect = redirectState;
        }

        /// <summary>
        /// Changes the Working Directory to the Specified Directory.
        /// </summary>
        /// <param name="directory"> The Directory from which the Command will be run </param>
        public void ChangeWorkingDirectory(string directory)
        {
            if (Directory.Exists(directory))
                _processStartInfo.WorkingDirectory = directory;
            else
                Console.WriteLine("Directory does not exist: " + directory);
        }

        /// <summary>
        /// Returns the File Name / Executable of the Application based on the ProcessApplication Enum.
        /// </summary>
        /// <param name="application"> The Application Selected </param>
        /// <returns> The Executable / File Name of the Application </returns>
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
        /// Returns the Arguments for the Application based on the ProcessApplication Enum.
        /// </summary>
        /// <param name="application"> Application that will run the Process </param>
        /// <param name="command"> The Commands to run </param>
        /// <returns> The Arguments that will be passed to the Application </returns>
        private string GetApplicationArguments(ProcessApplication application, string command)
        {
            if (OperatingSystem.IsWindows())
            {
                switch (application)
                {
                    case ProcessApplication.CMD:
                        return $"/c {command}";
                    case ProcessApplication.PowerShell:
                        return $"-Command {command}";
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                switch (application)
                {
                    case ProcessApplication.Bash:
                    case ProcessApplication.Sh:
                        return $"-c \"{command}\"";
                }
            }

            return command;
        }

        /// <summary>
        /// Runs a Process with the given command.
        /// </summary>
        /// <param name="command"> The Command Passed to the CMD </param>
        public void RunProcess(string command)
        {
            _processStartInfo.Arguments = GetApplicationArguments(Application, command);

            using (Process process = new Process())
            {
                process.StartInfo = _processStartInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();

                    if (line == null)
                        continue;

                    _standardOutput.Add(line);

                    if (_outputRedirect)
                        Console.WriteLine(line);
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    if (_outputRedirect)
                        Console.WriteLine($"Error: {process.StandardError.ReadToEnd()}");
                }
            }
        }
    }
}
