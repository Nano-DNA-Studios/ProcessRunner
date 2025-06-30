using NUnit.Framework;
using NanoDNA.ProcessRunner.Enums;
using NanoDNA.ProcessRunner.Results;

namespace NanoDNA.ProcessRunner.Tests
{
    internal class ProcessResultTests
    {
       /* [Test]
        public void Constructor_SetsStatusAndExitCode()
        {
            var result = new ProcessResult(ProcessStatus.Success, 0);

            Assert.That(result.Status, Is.EqualTo(ProcessStatus.Success));
            Assert.That(result.ExitCode, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_AllStatusValues()
        {
            var success = new ProcessResult(ProcessStatus.Success, 0);
            var failed = new ProcessResult(ProcessStatus.Failed, 1);
            var didNotRun = new ProcessResult(ProcessStatus.DidNotRun, -1);

            Assert.Multiple(() =>
            {
                Assert.That(success.Status, Is.EqualTo(ProcessStatus.Success));
                Assert.That(failed.Status, Is.EqualTo(ProcessStatus.Failed));
                Assert.That(didNotRun.Status, Is.EqualTo(ProcessStatus.DidNotRun));
            });
        }*/
    }
}
