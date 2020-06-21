using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SensorStandard;
using SensorStandard.MrsTypes;

namespace MarsDeviceManager
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
