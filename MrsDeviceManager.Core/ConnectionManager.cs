using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace MrsDeviceManager.Core
{
	internal class ConnectionManager
	{
		#region / / / / /  Singleton  / / / / /

		private static ConnectionManager instance;
		public static ConnectionManager Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}
				return new ConnectionManager();
			}
		}

		private ConnectionManager()
		{
			_connectedDevices = new List<Device>();
			_deviceCfgTime = new Dictionary<Device, DateTime>();

			instance = this;
			_connectionTimer = new Timer(Globals.KeepAliveInterval.TotalMilliseconds);
			_connectionTimer.Elapsed += ConnectionTimer_Elapsed;
		}

		#endregion


		#region / / / / /  Private fields  / / / / /

		private readonly Timer _connectionTimer;
		private readonly List<Device> _connectedDevices;
		private readonly Dictionary<Device, DateTime> _deviceCfgTime;
		private readonly object _syncToken = new object();

		#endregion


		#region / / / / /  Properties  / / / / /

		public IEnumerable<Device> ConnectedDevices => _connectedDevices;

		#endregion


		#region / / / / /  Private methods  / / / / /

		private void ConnectionTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (_syncToken)
			{
				Parallel.ForEach(ConnectedDevices, CheckDeviceConnection); 
			}
		}

		private void CheckDeviceConnection(Device device)
		{
			TimeSpan delta = DateTime.Now - device.LastConnectionTime;
			if (delta >= Globals.ConnectionTimeout)
			{
				// if first time losing connection
				if (device.State == DeviceState.Connected)
				{
					device.State = DeviceState.Reconnecting;
					try
					{
						_deviceCfgTime.Add(device, DateTime.Now);
						device.RaiseDisconnected();
						device.SendConfigRequest();
					}
					catch (Exception ex)
					{
						// ignore
						Console.WriteLine(ex);
					}
				}
				// if already reconnecting and time (by seconds) to send cfg 
				else if ((DateTime.Now - _deviceCfgTime[device]).TotalSeconds >= Globals.ReconnectionInterval.TotalSeconds)
				{
					try
					{
						_deviceCfgTime[device] = DateTime.Now;
						device.SendConfigRequest();
					}
					catch (Exception ex)
					{
						// ignore
						Console.WriteLine(ex);
					}
				}
			}
			else
			{
				device.State = DeviceState.Connected;
				if (_deviceCfgTime.ContainsKey(device))
				{
					device.RaiseConnected();
					_deviceCfgTime.Remove(device);
				}
				try
				{
					device.KeepAlive();
				}
				catch (Exception ex)
				{
					// ignore
					Console.WriteLine(ex);
				}
			}
		}

		#endregion


		#region / / / / /  Public methods  / / / / /

		public void AddDevice(Device device)
		{
			device.State = DeviceState.Reconnecting;
			try
			{
				device.SendConfigRequest();
			}
			catch 
			{
				// ignore
			}

			// add to reconnection watch
			if (_deviceCfgTime.ContainsKey(device))
			{
				_deviceCfgTime[device] = DateTime.Now;
			}
			else
			{
				_deviceCfgTime.Add(device, DateTime.Now);
			}
			if (_connectedDevices.Contains(device) == false)
			{
				lock (_syncToken)
				{
					_connectedDevices.Add(device);
				}
			}
			if (_connectionTimer.Enabled == false)
			{
				_connectionTimer.Start();
			}
		}

		public void RemoveDevice(Device device)
		{
			if (_connectedDevices.Contains(device))
			{
				lock (_syncToken)
				{
					_connectedDevices.Remove(device);
				}
				if (_connectedDevices.Count == 0)
				{
					_connectionTimer.Stop();
				}
			}
			if (_deviceCfgTime.ContainsKey(device))
			{
				_deviceCfgTime.Remove(device);
			}
		}

		#endregion
	}
}
