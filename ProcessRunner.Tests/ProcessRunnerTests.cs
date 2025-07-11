using NUnit.Framework;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System;

namespace NanoDNA.ProcessRunner.Tests
{
    internal class ProcessRunnerTests : BaseUnitTest
    {
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
