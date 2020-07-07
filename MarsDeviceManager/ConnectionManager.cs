using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace MarsDeviceManager
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
			connectedDevices = new List<Device>();
			deviceCfgTime = new Dictionary<Device, DateTime>();

			instance = this;
			connectionTimer = new Timer(Globals.KeepAliveInterval.TotalMilliseconds);
			connectionTimer.Elapsed += ConnectionTimer_Elapsed;
		}

		#endregion


		#region / / / / /  Private fields  / / / / /

		private readonly Timer connectionTimer;
		private readonly List<Device> connectedDevices;
		private readonly Dictionary<Device, DateTime> deviceCfgTime;
		private readonly object syncToken = new object();

		#endregion


		#region / / / / /  Properties  / / / / /

		public IEnumerable<Device> ConnectedDevices => connectedDevices;

		#endregion


		#region / / / / /  Private methods  / / / / /

		private void ConnectionTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (syncToken)
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
						deviceCfgTime.Add(device, DateTime.Now);
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
				else if ((DateTime.Now - deviceCfgTime[device]).TotalSeconds >= Globals.ReconnectionInterval.TotalSeconds)
				{
					try
					{
						deviceCfgTime[device] = DateTime.Now;
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
				try
				{
					device.KeepAlive();
				}
				catch (Exception ex)
				{
					// ignore
					Console.WriteLine(ex);
				}
				if (deviceCfgTime.ContainsKey(device))
				{
					deviceCfgTime.Remove(device);
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
			if (deviceCfgTime.ContainsKey(device))
			{
				deviceCfgTime[device] = DateTime.Now;
			}
			else
			{
				deviceCfgTime.Add(device, DateTime.Now);
			}
			if (connectedDevices.Contains(device) == false)
			{
				lock (syncToken)
				{
					connectedDevices.Add(device);
				}
			}
			if (connectionTimer.Enabled == false)
			{
				connectionTimer.Start();
			}
		}

		public void RemoveDevice(Device device)
		{
			if (connectedDevices.Contains(device))
			{
				lock (syncToken)
				{
					connectedDevices.Remove(device);
				}
				if (connectedDevices.Count == 0)
				{
					connectionTimer.Stop();
				}
			}
			if (deviceCfgTime.ContainsKey(device))
			{
				deviceCfgTime.Remove(device);
			}
		}

		#endregion
	}
}
