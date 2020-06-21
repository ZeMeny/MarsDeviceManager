namespace MarsDeviceManager
{
    /// <summary>
    /// Contains all the connection states of a <see cref="Device"/>
    /// </summary>
    public enum DeviceState
    {
        /// <summary>
        /// No connection been mae with the <see cref="Device"/>
        /// </summary>
        Disconnected,
        /// <summary>
        /// <see cref="Device"/> is connected
        /// </summary>
        Connected,
        /// <summary>
        /// Attempting to Reconnect to the <see cref="Device"/>
        /// </summary>
        Reconnecting
    }
}
