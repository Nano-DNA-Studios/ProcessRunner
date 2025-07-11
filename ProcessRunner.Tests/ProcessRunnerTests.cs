using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System;

namespace NanoDNA.ProcessRunner.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="ProcessRunner"/> class.
    /// </summary>
    internal class ProcessRunnerTests : BaseUnitTest
    {
        /// <summary>
        /// Tests the constructor of <see cref="ProcessRunner(string, string, bool, bool)"/> with a valid application name.
        /// </summary>
        /// <param name="applicationName">Name of the Application</param>
        /// <param name="OS">Operating System to Test on</param>
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
