using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System;

namespace NanoDNA.ProcessRunner.Tests
{
    internal class ProcessRunnerTests
    {
        /// <summary>
        /// Checks if the Current Operating System is the same as the one passed in
        /// </summary>
        /// <param name="OS">Operating System we are checking for</param>
        /// <returns>True if the OS's Match, False otherwise</returns>
        private bool OnAppropriateOS(PlatformOperatingSystem OS)
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

        [Test]
        [TestCase("cmd.exe", PlatformOperatingSystem.Windows)]
        [TestCase("/bin/bash", PlatformOperatingSystem.Unix)]
        [TestCase("/bin/sh", PlatformOperatingSystem.Unix)]
        [TestCase("powershell.exe", PlatformOperatingSystem.Windows)]
        public void ProcessRunnerConstructor_ValidApplication (string applicationName, PlatformOperatingSystem OS)
        {
            if (!OnAppropriateOS(OS))
            {
                Assert.Throws<NotSupportedException>(() => new ProcessRunner(applicationName));
                return;
            }

            ProcessRunner processRunner = new ProcessRunner(applicationName);

            Assert.IsNotNull(processRunner);
            Assert.That(processRunner.ApplicationName, Is.EqualTo(applicationName));
        }
    }
}
