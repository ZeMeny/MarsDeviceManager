using System;

namespace MrsDeviceManager.Core
{
    /// <summary>
    /// Global Variables
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Gets or sets the intervals between KeepAlive requests (Minimum 1 second)
        /// </summary>
        public static TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the connection timeout
        /// </summary>
        public static TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the intervals between reconnection attempts
        /// </summary>
        public static TimeSpan ReconnectionInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets weather to validate outgoing messages
        /// </summary>
        public static bool ValidateMessages { get; set; } = true;
    }
}
