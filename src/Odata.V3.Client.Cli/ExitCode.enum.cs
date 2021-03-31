using System;

namespace Odata.V3.Client.Cli
{
    /// <summary>
    /// Command line args parsing exit codes
    /// </summary>
    [Flags]
    public enum ExitCode
    {
        #region Errors

        /// <summary>
        /// Error code
        /// </summary>
        Error = -2,

        #endregion Errors

        /// <summary>
        /// Default exit code
        /// </summary>
        Default = 0,

        /// <summary>
        /// Ok code
        /// </summary>
        Ok = 1
    }
}
