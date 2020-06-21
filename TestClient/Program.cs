using MarsDeviceManager;
using MarsDeviceManager.Extensions;
using SensorStandard;
using System;
using SensorStandard.MrsTypes;

namespace TestClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Globals.ValidateMessages = false;
			Device device = new Device("127.0.0.1", 13001, "127.0.0.1", 11001, "MarsLab");
			device.ConfigurationReceived += Device_ConfigurationReceived;
			device.StatusReportReceived += Device_StatusReportReceived;
			device.Connect();
			Device device2 = new Device("127.0.0.1", 13002, "127.0.0.1", 11002, "MarsLab");
			device2.ConfigurationReceived += Device_ConfigurationReceived;
			device2.StatusReportReceived += Device_StatusReportReceived;
			//device2.Connect();
			Console.WriteLine("Device Connected");

			
			Console.ReadKey(true);
			device.Disconnect();
			device2.Disconnect();
		}

		private static void Device_StatusReportReceived(object sender, DeviceStatusReport e)
		{
			Device device = (Device)sender;
			if (e.IsValid(out Exception exception))
			{
				Console.WriteLine($"Status Report Received from {device.DeviceIP}:{device.DevicePort}");
			}
			else
			{
				throw exception;
			}
		}

		private static void Device_ConfigurationReceived(object sender, DeviceConfiguration e)
		{
			Device device = (Device)sender;
			if (e.IsValid(out Exception exception))
			{
				Console.WriteLine($"Device Configuration Received from {device.DeviceIP}:{device.DevicePort}");
			}
			else
			{
				throw exception;
			}
		}
	}
}
