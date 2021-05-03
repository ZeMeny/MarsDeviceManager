using SensorStandard.Core.MrsTypes;

namespace MrsDeviceManager.Core
{
    /// <summary>
    /// Class that Represents a Mars Device component
    /// </summary>
    public class Sensor
    {
        #region / / / / /  Private fields  / / / / /



        #endregion


        #region / / / / /  Propertis  / / / / /

        /// <summary>
        /// Gets the Sensor Configuration
        /// </summary>
        public SensorConfiguration Configuration { get; internal set; }

        /// <summary>
        /// Gets the Updated Senosr Status
        /// </summary>
        public SensorStatusReport SensorStatus { get; internal set; }

        /// <summary>
        /// Gets the Sensor BIT Result
        /// </summary>
        public DetailedSensorBITType SensorBit { get; internal set; }

        /// <summary>
        /// Gets the Sensors Identification
        /// </summary>
        public SensorIdentificationType SensorIdentification
        {
            get
            {
                return Configuration?.SensorIdentification;
            }
        }

        #endregion


        #region / / / / /  Private methods  / / / / /



        #endregion


        #region / / / / /  Public methods  / / / / /



        #endregion
    }
}
