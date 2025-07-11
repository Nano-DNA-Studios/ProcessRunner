using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System;
using System.IO;

namespace NanoDNA.ProcessRunner.Tests
{
    /// <summary>
    /// Provides and stores base functions and values for unit tests.
    /// </summary>
    internal class BaseUnitTest
    {
        /// <summary>
        /// The Default Process Command to run in the tests.
        /// </summary>
        protected const string DEFAULT_PROCESS_COMMAND = "echo Hello World";

        /// <summary>
        /// The Default Output to expect from a successful process run.
        /// </summary>
        protected const string DEFAULT_PROCESS_OUTPUT = "Hello World";

        /// <summary>
        /// The Default Name of a nonexistent application.
        /// </summary>
        protected const string DEFAULT_NON_EXISTENT_APPLICATION = "non_existent_app";

        /// <summary>
        /// The Default Application Command to run in the tests. Dependent on OS
        /// </summary>
        protected static readonly string DEFAULT_APPLICATION_COMMAND = OperatingSystem.IsWindows() ? "/c echo Hello World" : "-c echo Hello World";

        /// <summary>
        /// The Default Application Command to run in the tests that will fail. Dependent on OS
        /// </summary>
        protected static readonly string DEFAULT_APPLICATION_FAIL_COMMAND = OperatingSystem.IsWindows() ? "/c echoe Hello World" : "-c echoe Hello World";

        /// <summary>
        /// The Default Valid Application to run in the tests. Dependent on OS
        /// </summary>
        protected static readonly string DEFAULT_VALID_APPLICATION = OperatingSystem.IsWindows() ? "cmd.exe" : "/usr/bin/bash";

        /// <summary>
        /// Gets a Invalid Directory for the Current Operating System.
        /// </summary>
        /// <returns>The Path to a Non existent Directory</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if the Operating System is not supported</exception>
        protected string GetInvalidOSDirectory()
        {
            if (OnAppropriateOS(PlatformOperatingSystem.Windows))
                return "C:\\non_existent_dir";
            else if (OnAppropriateOS(PlatformOperatingSystem.Unix) || OnAppropriateOS(PlatformOperatingSystem.OSX))
                return "/non_existent_dir";
            else
                throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        /// <summary>
        /// Gets a Valid Directory for the Current Operating System.
        /// </summary>
        /// <returns>The Path to a Valid Directory</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if the Operating System is not supported</exception>
        protected string GetValidOSDirectory()
        {
            if (OnAppropriateOS(PlatformOperatingSystem.Windows))
                return Directory.GetCurrentDirectory();
            else if (OnAppropriateOS(PlatformOperatingSystem.Unix) || OnAppropriateOS(PlatformOperatingSystem.OSX))
                return Directory.GetCurrentDirectory();
            else
                throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        /// <summary>
        /// Gets the Default Application for the Current Operating System
        /// </summary>
        /// <returns>The Path to the Default OS Command Runner Application</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if the Application is not supported by the Library</exception>
        protected string GetOSDefaultApplication()
        {
            if (OnAppropriateOS(PlatformOperatingSystem.Windows))
                return "cmd.exe";
            else if (OnAppropriateOS(PlatformOperatingSystem.Unix))
                return "/bin/bash";
            else if (OnAppropriateOS(PlatformOperatingSystem.OSX))
                return "/bin/sh";
            else
                throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        /// <summary>
        /// Checks if the Current Operating System is the same as the one passed in
        /// </summary>
        /// <param name="OS">Operating System we are checking for</param>
        /// <returns>True if the OS's Match, False otherwise</returns>
        protected bool OnAppropriateOS(PlatformOperatingSystem OS)
        {
            switch (OS)
            {
                case PlatformOperatingSystem.Windows:
                    return OperatingSystem.IsWindows();

                case PlatformOperatingSystem.Unix:
                    return OperatingSystem.IsLinux();

                case PlatformOperatingSystem.OSX:
                    return OperatingSystem.IsMacOS();
            }

            return false;
        }
    }
}