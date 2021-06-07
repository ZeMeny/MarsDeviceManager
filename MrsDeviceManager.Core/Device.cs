using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MrsDeviceManager.Core.Extensions;
using SensorStandard.Core;
using SensorStandard.Core.MrsTypes;
using SoapCore;

namespace MrsDeviceManager.Core
{
	/// <summary>
	/// Class that represents a mars device
	/// </summary>
	public class Device
	{
		#region / / / / /  Private fields  / / / / /

		private SubscriptionTypeType[] _subscriptions;
		private readonly ConnectionManager _connectionManager = ConnectionManager.Instance;
		private IHost _host;
		private SNSR_STDSOAPPortClient _client;

		#endregion


		#region / / / / /  Properties  / / / / /

		/// <summary>
		/// Gets the IP address of the device server
		/// </summary>
		public string DeviceIP { get; }

		/// <summary>
		/// Gets the port of the device server
		/// </summary>
		public int DevicePort { get; }

		/// <summary>
		/// Gets the local server IP address
		/// </summary>
		public string NotificationIP { get; }

		/// <summary>
		/// Gets the local server port
		/// </summary>
		public int NotificationPort { get; }

		/// <summary>
		/// Gets the current connection state of the device
		/// </summary>
		public DeviceState State { get; internal set; }

		/// <summary>
		/// Gets the last time a message was received from the device
		/// </summary>
		public DateTime LastConnectionTime { get; private set; }

		/// <summary>
		/// Gets the sensors under this device
		/// </summary>
		public IEnumerable<Sensor> Sensors { get; private set; }

		/// <summary>
		/// Gets the device configuration
		/// </summary>
		public DeviceConfiguration Configuration { get; private set; }

		/// <summary>
		/// Gets the full device status
		/// </summary>
		public DeviceStatusReport FullDeviceStatus { get; private set; }

		/// <summary>
		/// Gets the last status received
		/// </summary>
		public DeviceStatusReport LastDeviceStatus { get; private set; }

		/// <summary>
		/// Gets the device identification
		/// </summary>
		public DeviceIdentificationType DeviceIdentification => Configuration?.DeviceIdentification;

		/// <summary>
		/// Gets the currently active camera
		/// </summary>
		public Sensor CurrentCamera { get; private set; }

		/// <summary>
		/// Gets or sets the requestor identification in outgoing messages
		/// </summary>
		public string RequestorID { get; set; }

		#endregion


		#region / / / / /  Consructors  / / / / /

		/// <summary>
		/// Creates new Instance of the <see cref="Device"/> Class
		/// </summary>
		/// <param name="ip">Device Server IP Aderss</param>
		/// <param name="port">Device Server Port</param>
		/// <param name="notificationIp">Local Server IP Aderss</param>
		/// <param name="notificationPort">Local Server Port</param>
		/// <param name="requestorID">requestor identification</param>
		public Device(string ip, int port, string notificationIp, int notificationPort, string requestorID)
		{
			DeviceIP = ip;
			DevicePort = port;
			NotificationIP = notificationIp;
			NotificationPort = notificationPort;
			RequestorID = requestorID;

			Sensors = new List<Sensor>();
			State = DeviceState.Disconnected;
		}

		/// <summary>
		/// Creates new Instance of the <see cref="Device"/> Class
		/// </summary>
		/// <param name="settings">device connection settings</param>
		public Device(DeviceSettings settings) : this(settings.DeviceIP, settings.DevicePort,
			settings.DeviceNotificationIP, settings.DeviceNotificationPort, settings.RequestorID)
		{
		}

		/// <summary>
		/// Disconnect and destroy the device
		/// </summary>
		~Device()
		{
			Disconnect();
		}

		#endregion


		#region / / / / /  Private methods  / / / / /

		private void InitClient()
		{
			var binding = CreateBindingConfig();
			var endpoint = new EndpointAddress($"http://{DeviceIP}:{DevicePort}/SNSR_STD-SOAP");
			_client = new SNSR_STDSOAPPortClient(binding, endpoint);
		}

		private void InitHost()
		{
			var marsService = new MarsService();
			marsService.CommandMessage += MarsService_CommandMessage;
			marsService.DeviceConfiguration += MarsService_DeviceConfiguration;
			marsService.DeviceStatus += MarsService_DeviceStatus;
			marsService.DeviceIndication += MarsService_DeviceIndication;
			marsService.DeviceSubscription += MarsService_DeviceSubscription;
			_host = CreateHost(marsService);
			_host.RunAsync();
		}

		private IHost CreateHost(MarsService marsService) =>
			Host.CreateDefaultBuilder()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseUrls($"http://{NotificationIP}:{NotificationPort}");
					webBuilder.UseStartup(_ => new Startup(marsService));
				}).Build();

		private BasicHttpBinding CreateBindingConfig()
		{
			return new()
			{
				CloseTimeout = TimeSpan.FromMinutes(1),
				OpenTimeout = TimeSpan.FromMinutes(1),
				SendTimeout = TimeSpan.FromMinutes(1),
				ReceiveTimeout = TimeSpan.FromMinutes(10),
				AllowCookies = false,
				BypassProxyOnLocal = false,
				MaxBufferSize = 65535 * 1000,
				MaxBufferPoolSize = 524288 * 1000,
				MaxReceivedMessageSize = 65535 * 1000,
				TextEncoding = Encoding.UTF8,
				TransferMode = TransferMode.Buffered,
				UseDefaultWebProxy = true,
				Security = new BasicHttpSecurity
				{
					Mode = BasicHttpSecurityMode.None,
					Transport = new HttpTransportSecurity
					{
						ClientCredentialType = HttpClientCredentialType.None,
						ProxyCredentialType = HttpProxyCredentialType.None
					},
					Message = new BasicHttpMessageSecurity
					{
						ClientCredentialType = BasicHttpMessageCredentialType.UserName
					}
				},
			};
		}

		private IEnumerable<Sensor> InitSensors(IEnumerable<SensorConfiguration> configurations)
		{
			if (configurations == null)
			{
				//throw new ArgumentNullException(nameof(configurations), "SensorConfiguration cannot be null");
				return new Sensor[0];
			}
			List<Sensor> sensors = new List<Sensor>();
			foreach (SensorConfiguration config in configurations)
			{
				Sensor sensor = new Sensor
				{
					Configuration = config,
				};
				sensors.Add(sensor);
			}
			return sensors;
		}

		private Sensor GetCurrentCamera(DeviceStatusReport deviceStatus)
		{
			Sensor currentCamera = null;
			VideoSwitchStatus videoSwitch = (VideoSwitchStatus)deviceStatus.Items?.OfType<SensorStatusReport>().FirstOrDefault(x => x.Item is VideoSwitchStatus)?.Item;

			if (videoSwitch?.VideoChannel.Length > 0)
			{
				VideoChannelType firstChannel = videoSwitch.VideoChannel[0];
				if (firstChannel.Item is SensorTypeType sensorType)
				{
					currentCamera = Sensors.FirstOrDefault(x => x.SensorIdentification.SensorType == sensorType);
				}
				else if (firstChannel.Item is SensorIdentificationType sensorIdentification)
				{
					currentCamera = Sensors.FirstOrDefault(x => x.SensorIdentification.Equals(sensorIdentification));
				}
			}
			return currentCamera;
		}

		private void MarsService_DeviceSubscription(object sender, DeviceSubscriptionConfiguration e)
		{
			if (Globals.ValidateMessages && !e.IsValid(out var ex))
			{
				throw ex;
			}

			LastConnectionTime = DateTime.Now;
			MessageReceived?.Invoke(this, e);
		}

		private void MarsService_DeviceStatus(object sender, DeviceStatusReport e)
		{
			if (Globals.ValidateMessages && !e.IsValid(out var ex))
			{
				throw ex;
			}

			LastConnectionTime = DateTime.Now;
			Sensor temp = GetCurrentCamera(e);
			if (temp != null)
			{
				CurrentCamera = temp;
			}

			if (e.Items != null)
			{
				UpdateSensorData(e);
			}

			// update the full status report
			FullDeviceStatus = FullDeviceStatus.UpdateValues(e);

			LastDeviceStatus = e;

			MessageReceived?.Invoke(this, e);
		}

		private void MarsService_DeviceIndication(object sender, DeviceIndicationReport e)
		{
			if (Globals.ValidateMessages && !e.IsValid(out var ex))
			{
				throw ex;
			}

			LastConnectionTime = DateTime.Now;
			MessageReceived?.Invoke(this, e);
		}

		private void MarsService_CommandMessage(object sender, CommandMessage e)
		{
			if (Globals.ValidateMessages && !e.IsValid(out var ex))
			{
				throw ex;
			}

			LastConnectionTime = DateTime.Now;
			MessageReceived?.Invoke(this, e);
		}

		private void MarsService_DeviceConfiguration(object sender, DeviceConfiguration e)
		{
			if (Globals.ValidateMessages && !e.IsValid(out var ex))
			{
				throw ex;
			}

			LastConnectionTime = DateTime.Now;
			Configuration = e;
			Sensors = InitSensors(e?.SensorConfiguration);

			MessageReceived?.Invoke(this, e);

			SubscriptionTypeType[] subscriptionTypes = _subscriptions ?? new[]
			{
					SubscriptionTypeType.Configuration,
					SubscriptionTypeType.OperationalIndication,
					SubscriptionTypeType.TechnicalStatus
				};
			SendSubscriptionRequest(subscriptionTypes);
		}

		private void UpdateSensorData(DeviceStatusReport statusReport)
		{
			foreach (var deviceStatus in statusReport.Items.OfType<DeviceStatusReport>())
			{
				UpdateSensorData(deviceStatus);
			}

			var sensorStatus = statusReport.Items.OfType<SensorStatusReport>();
			var sensorBit = statusReport.Items.OfType<DetailedSensorBITType>();
			foreach (var sensor in Sensors)
			{
				// find sensor status
				// ReSharper disable once PossibleMultipleEnumeration
				var matchingSensorStatus =
					sensorStatus.FirstOrDefault(x => x.SensorIdentification.Equals(sensor.SensorIdentification));
				if (matchingSensorStatus?.Item != null)
				{
					sensor.SensorStatus = matchingSensorStatus;
				}

				// ReSharper disable once PossibleMultipleEnumeration
				var matchingSensorBit = sensorBit.FirstOrDefault(x => x.SensorIdentification.Equals(sensor.SensorIdentification));
				if (matchingSensorBit != null)
				{
					sensor.SensorBit = matchingSensorBit;
				}
			}
		}

		internal void RaiseDisconnected()
		{
			Disconnected?.Invoke(this, EventArgs.Empty);
		}

		internal void RaiseConnected()
		{
			Connected?.Invoke(this, EventArgs.Empty);
		}

		#endregion


		#region / / / / /  Public methods  / / / / /

		/// <summary>
		/// Start connection (Configuration and KeepAlive)
		/// </summary>
		public void Connect(SubscriptionTypeType[] subscriptions = null)
		{
			InitClient();
			InitHost();

			_subscriptions = subscriptions;

			_connectionManager.AddDevice(this);
		}

        /// <summary>
        /// Stop connection
        /// </summary>
        public void Disconnect()
        {
	        _host.StopAsync();
			_client.Close();

			_connectionManager.RemoveDevice(this);
			if (State == DeviceState.Connected)
			{
				SendSubscriptionRequest(Array.Empty<SubscriptionTypeType>());
				State = DeviceState.Disconnected;
				Disconnected?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Send GeoGoto command
		/// </summary>
		/// <param name="latitude">Latitude in degree</param>
		/// <param name="longitude">Longitude in degree</param>
		/// <param name="altitude">Altitue in meters</param>
		/// <param name="altitudeReference">Altitude reference</param>
		/// <param name="sensor">Target Sensor (default is the active camera)</param>
		public void GeoGoto(double latitude, double longitude, double altitude,
			AltitudeReferenceType altitudeReference = AltitudeReferenceType.MSL, Sensor sensor = null)
		{
			Sensor pedestal = Sensors.FirstOrDefault(x => x.SensorIdentification.SensorType == SensorTypeType.Pedestal);

			if (pedestal != null)
			{
				CommandType command = new CommandType
				{
					Item = new ScriptCommandType
					{
						GEOGoToCommand = new LocationCommandType
						{
							SimpleCommand = SimpleCommandType.GEOGoTo,
							Point = new[]
							{
								new Point
								{
									Item = new LocationType
									{
										Item = new GeodeticLocation
										{
											Altitude = new AltitudeType
											{
												Reference = altitudeReference,
												Units = DistanceUnitsType.Meters,
												Value = altitude
											},
											Latitude = new Latitude
											{
												Units = LatLonUnitsType.DecimalDegrees,
												Value = latitude
											},
											Longitude = new Longitude
											{
												Units = LatLonUnitsType.DecimalDegrees,
												Value = longitude
											},
											Datum = DatumType.WGS84
										}
									}
								}
							}
						},
						PedestalSensorIdentification = pedestal.SensorIdentification,
						OpticalSensorIdentification =
							sensor != null ? sensor.SensorIdentification : CurrentCamera.SensorIdentification
					}
				};
				SendCommandMessage(command);
			}
			else
			{
				throw new Exception("No pedestal found on this device");
			}

		}

		/// <summary>
		/// Send relative GoTo command
		/// </summary>
		/// <param name="azimuth">Azimuth in degree</param>
		/// <param name="elevation">Elevation in degree</param>
		/// <param name="range">Range in meteres</param>
		/// <param name="sensor">Target Sensor (default is the active camera)</param>
		public void Goto(double azimuth, double elevation, double range = 0, Sensor sensor = null)
		{
			Sensor pedestal = Sensors.FirstOrDefault(x => x.SensorIdentification.SensorType == SensorTypeType.Pedestal);
			if (pedestal == null || CurrentCamera == null && sensor == null)
			{
				throw new Exception("No valid sensors found on this device");
			}

			CommandType command = new CommandType
			{
				Item = new ScriptCommandType
				{
					GoToCommand = new LocationCommandType
					{
						SimpleCommand = SimpleCommandType.GoTo,
						Point = new[]
						{
							new Point
							{
								Item = new RelativeLocationType
								{
									Azimuth = new AzimuthType
									{
										Units = AngularUnitsType.Mils,
										Value = Math.Round(UnitConverter.DegreeToMils(azimuth), 2)
									},
									Elevation = new ElevationAngle
									{
										Units = AngularUnitsType.Mils,
										Value = Math.Round(UnitConverter.DegreeToMils(elevation), 2)
									},
									Range = range > 0 ? new Distance
									{
										Units = DistanceUnitsType.Meters,
										Value = range
									} : null
								}
							}
						}
					},
					PedestalSensorIdentification = pedestal.SensorIdentification,
					OpticalSensorIdentification = sensor != null ? sensor.SensorIdentification : CurrentCamera.SensorIdentification
				}
			};
			SendCommandMessage(command);
		}

		/// <summary>
		/// Send Move command to the pedestal
		/// </summary>
		/// <param name="horizonatlVelocity">Horizontal Velocity</param>
		/// <param name="verticalVelocity">Vertical Velocity</param>
		public void Move(double horizonatlVelocity, double verticalVelocity)
		{
			Sensor pedestal = Sensors.FirstOrDefault(x => x.SensorIdentification.SensorType == SensorTypeType.Pedestal);
			if (pedestal != null)
			{
				CommandType command = new CommandType
				{
					Item = new LocationCommandType
					{
						ItemsElementName = new[]
						{
							ItemsChoiceType.HorizontalVelocity,
							ItemsChoiceType.VerticalVelocity,
						},
						Items = new[]
						{
							new AngularSpeed
							{
								Units = AngularSpeedUnitsType.MilsPerSecond,
								Value = horizonatlVelocity
							},
							new AngularSpeed
							{
								Units = AngularSpeedUnitsType.MilsPerSecond,
								Value = verticalVelocity
							}
						},
						SimpleCommand = SimpleCommandType.Move
					}
				};
				SendCommandMessage(command, pedestal);
			}
			else
			{
				throw new Exception("No Pedestal found on this device");
			}
		}

		/// <summary>
		/// Send Zoom command
		/// </summary>
		/// <param name="value">Zoom value</param>
		/// <param name="sensor">Target Sensor (default is the active camera)</param>
		public void Zoom(double value, Sensor sensor = null)
		{
			CommandType command = new CommandType
			{
				Item = new OpticalCommandType
				{
					SimpleCommand = SimpleCommandType.Zoom,
					ItemElementName = ItemChoiceType1.Value,
					OperationSpecified = true,
					Operation = value > 0 ? OperationType.Plus : OperationType.Minus,
					ControlSpecified = true,
					Control = ControlType.Manual,
					Item = value
				}
			};
			SendCommandMessage(command, sensor ?? CurrentCamera);
		}

		/// <summary>
		/// Send Focus command
		/// </summary>
		/// <param name="value">Focus Value</param>
		/// <param name="sensor">Target Sensor (default is the active camera)</param>
		public void Focus(double value, Sensor sensor = null)
		{
			CommandType command = new CommandType
			{
				Item = new OpticalCommandType
				{
					SimpleCommand = SimpleCommandType.Focus,
					ItemElementName = ItemChoiceType1.Value,
					OperationSpecified = true,
					Operation = value > 0 ? OperationType.Plus : OperationType.Minus,
					ControlSpecified = true,
					Control = ControlType.Manual,
					Item = value
				}
			};
			SendCommandMessage(command, sensor ?? CurrentCamera);
		}

		/// <summary>
		/// Send KeepAlive command to the device
		/// </summary>
		public void KeepAlive()
		{
			var commandType = new CommandType
			{
				Item = SimpleCommandType.KeepAlive
			};
			SendCommandMessage(commandType);
		}

		/// <summary>
		/// Send Stop command
		/// </summary>
		/// <param name="sensor">Target Sensor (default is the device itself)</param>
		public void Stop(Sensor sensor = null)
		{
			var command = new CommandType();
			var cameraTypes = new List<SensorTypeType>
			{
				SensorTypeType.DayCameraBW,
				SensorTypeType.DayCameraColor,
				SensorTypeType.FLIR
			};

			if (sensor == null)
			{
				command.Item = SimpleCommandType.Stop;
			}
			else if (sensor.Configuration.SimpleCommandConfiguration.Contains(SimpleCommandType.Stop))
			{
				// if sensor is EO sensor
				if (cameraTypes.Contains(sensor.Configuration.SensorIdentification.SensorType))
				{
					command.Item = new OpticalCommandType
					{
						SimpleCommand = SimpleCommandType.Stop,
					};
				}
				// if it is a pedestal
				else if (sensor.Configuration.SensorIdentification.SensorType == SensorTypeType.Pedestal)
				{
					command.Item = new LocationCommandType
					{
						SimpleCommand = SimpleCommandType.Stop
					};
				}
				else
				{
					command.Item = SimpleCommandType.Stop;
				}
			}

			SendCommandMessage(command, sensor);
		}

		/// <summary>
		/// Send Turn On command
		/// </summary>
		/// <param name="sensor">Target Senosr (default is the active camera)</param>
		public void TurnOn(Sensor sensor = null)
		{
			CommandType command = new CommandType
			{
				Item = SimpleCommandType.On
			};
			SendCommandMessage(command, sensor);
		}

		/// <summary>
		/// Send Turn Off command
		/// </summary>
		/// <param name="sensor">Target Senosr (default is the active camera)</param>
		public void TurnOff(Sensor sensor = null)
		{
			CommandType command = new CommandType
			{
				Item = SimpleCommandType.Off
			};
			SendCommandMessage(command, sensor);
		}

		/// <summary>
		/// Send Switch Channels command
		/// </summary>
		/// <param name="channels">channel configuration</param>
		/// <param name="sensor">Target Senosr (default is the active camera)</param>
		public void SwitchChannels(VideoChannelType[] channels, Sensor sensor = null)
		{
			CommandType command = new CommandType
			{
				Item = new VideoSwitchCommandType
				{
					SimpleCommand = SimpleCommandType.Set,
					VideoChannel = channels,
				}
			};
			SendCommandMessage(command, sensor);
		}

		/// <summary>
		/// Send Custom command
		/// </summary>
		/// <param name="commandType">Command to be sent</param>
		/// <param name="sensor">Target Sensor (default is the device itself)</param>
		public void SendCommandMessage(CommandType commandType, Sensor sensor = null)
		{
			var commandMessage = new CommandMessage
			{
				DeviceIdentification = DeviceIdentification,
				RequestorIdentification = RequestorID,
				Command = commandType,
				SensorIdentification = sensor?.SensorIdentification,
			};
			Console.WriteLine($"Sending Command Message ({commandType.Item}) to {DeviceIP}:{DevicePort}");

			if (Globals.ValidateMessages)
			{
				if (commandMessage.IsValid(out Exception ex))
				{
					_client.doCommandMessage(commandMessage);
					MessageSent?.Invoke(this, commandMessage);
				}
				else
				{
					throw ex;
				}
			}
			else
			{
				_client.doCommandMessage(commandMessage);
				MessageSent?.Invoke(this, commandMessage);
			}
		}

		/// <summary>
		/// Send Device Configuration Request
		/// </summary>
		public void SendConfigRequest()
		{
			var configuration = new DeviceConfiguration
			{
				MessageTypeSpecified = true,
				MessageType = MessageType.Request,
				RequestorIdentification = RequestorID,
				NotificationServiceIPAddress = NotificationIP,
				NotificationServicePort = NotificationPort.ToString()
			};
			Console.WriteLine($"Sending Device configuration to {DeviceIP}:{DevicePort}");

			if (Globals.ValidateMessages)
			{
				if (configuration.IsValid(out Exception ex))
				{
					_client.BegindoDeviceConfiguration(configuration, null, null);
					MessageSent?.Invoke(this, configuration);
				}
				else
				{
					throw ex;
				}
			}
			else
			{
				_client.BegindoDeviceConfiguration(configuration, null, null);
				MessageSent?.Invoke(this, configuration);
			}
		}

		/// <summary>
		/// Send subscription request
		/// </summary>
		/// <param name="subscriptionTypes">Subscriptions Configuration</param>
		public void SendSubscriptionRequest(SubscriptionTypeType[] subscriptionTypes)
		{
			var deviceSubscription = new DeviceSubscriptionConfiguration
			{
				DeviceIdentification = DeviceIdentification,
				MessageTypeSpecified = true,
				MessageType = MessageType.Request,
				SubscriptionType = subscriptionTypes,
				ExecutionStatusSpecified = false,
				RequestorIdentification = RequestorID,
			};

			Console.WriteLine($"Sending Device Subscription to {DeviceIP}:{DevicePort}");

			if (Globals.ValidateMessages)
			{
				if (deviceSubscription.IsValid(out Exception ex))
				{
					_client.BegindoDeviceSubscriptionConfiguration(deviceSubscription, null, null);
					MessageSent?.Invoke(this, deviceSubscription);
				}
				else
				{
					throw ex;
				}
			}
			else
			{
				_client.BegindoDeviceSubscriptionConfiguration(deviceSubscription, null, null);
				MessageSent?.Invoke(this, deviceSubscription);
			}
		}

		/// <summary>
		/// Determine weather the <see cref="Device"/> has the same connection info of another <see cref="Device"/>
		/// </summary>
		/// <param name="obj">Other <see cref="Device"/></param>
		/// <returns>True if both have the connection info, otherwise false</returns>
		public override bool Equals(object obj)
		{
			Device device = (Device)obj;
			if (device != null)
			{
				return device.DeviceIP == DeviceIP && device.DevicePort == DevicePort;
			}
			return false;
		}

		/// <summary>
		/// Calls the <see cref="object.Equals(object)"/> method
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}

		#endregion


		#region / / / / /  Events  / / / / /

		/// <summary>
		/// Occurs When a <see cref="MrsMessage"/> is Received
		/// </summary>
		public event EventHandler<MrsMessage> MessageReceived;

		/// <summary>
		/// Occurs When a <see cref="MrsMessage"/> is Sent
		/// </summary>
		public event EventHandler<MrsMessage> MessageSent;

		/// <summary>
		/// Occurs After a Connection Timeout
		/// </summary>
		public event EventHandler Disconnected;

		/// <summary>
		/// Occurs When a new Connection is Established
		/// </summary>
		public event EventHandler Connected;

        #endregion


        #region / / / / /  Nested classes  / / / / /

        private sealed class Startup
        {
	        private readonly MarsService _marsService;

	        public Startup(MarsService marsService)
	        {
		        _marsService = marsService;
	        }

	        public void ConfigureServices(IServiceCollection services)
	        {
		        services.AddSingleton(_marsService);
		        services.AddSoapCore();
		        services.AddControllers();
			}

			public void Configure(IApplicationBuilder app)
			{
				app.UseRouting();
		        app.UseEndpoints(endpoints =>
		        {
			        endpoints.UseSoapEndpoint<MarsService>("/SNSR_STD-SOAP", new BasicHttpBinding(), SoapSerializer.XmlSerializer);
			        endpoints.MapControllers();
		        });
			}
		}

        #endregion
    }
}
