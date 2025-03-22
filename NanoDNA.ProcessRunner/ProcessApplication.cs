namespace NanoDNA.ProcessRunner
{
    internal partial class CommandRunner
    {
        /// <summary>
        /// Application that will handle the Command Runner Process.
        /// </summary>
        public enum ProcessApplication
        {
            /// <summary>
            /// Windows Command Prompt CMD Application
            /// </summary>
            CMD = 0,

            /// <summary>
            /// Windows PowerShell Application
            /// </summary>
            PowerShell = 1,

            /// <summary>
            /// Linux Bash Application
            /// </summary>
            Bash = 2,

            /// <summary>
            /// Linux/MacOS Shell Application
            /// </summary>
            Sh = 3,
        }
    }
}
