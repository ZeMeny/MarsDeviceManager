namespace MrsDeviceManager.Core
{
    /// <summary>
    /// A Class that contains <see cref="Device"/> connection info
    /// </summary>
    public class DeviceSettings
    {
        /// <summary>
        /// Get or sets the Device IP Address
        /// </summary>
        public string DeviceIP { get; set; }
        /// <summary>
        /// Gets or sets the Device Port
        /// </summary>
        public int DevicePort { get; set; }
        /// <summary>
        /// Gets or sets the device callback ip address
        /// </summary>
        public string DeviceNotificationIP { get; set; }
        /// <summary>
        /// Gets or sets the device callback port
        /// </summary>
        public int DeviceNotificationPort { get; set; }
        /// <summary>
        /// Gets or sets the name of the current device manager
        /// </summary>
        public string RequestorID { get; set; }
    }
}