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
			device.MessageReceived += Device_MessageReceived;
			Console.WriteLine("Connecting...");
			Console.WriteLine("Press any key to exit");
			device.Connect();
			Console.ReadKey(true);
			device.Disconnect();
		}

        private static void Device_MessageReceived(object sender, MrsMessage e)
        {
			Device device = (Device)sender;
			if (e.IsValid(out Exception exception))
			{
				Console.WriteLine($"{e.MrsMessageType} Received from {device.DeviceIP}:{device.DevicePort}");
			}
			else
			{
				throw exception;
			}
		}
	}
}
