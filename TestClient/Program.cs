using MarsDeviceManager;
using MarsDeviceManager.Extensions;
using SensorStandard;
using System;
using SensorStandard.MrsTypes;
using TestClient.Properties;

namespace TestClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var ip = Settings.Default.DeviceIP;
			var port = Settings.Default.DevicePort;
			var callbackIp = Settings.Default.CallbackIP;
			var callbackPort = Settings.Default.CallbackPort;
			var requestorId = Settings.Default.RequestorID;

			Globals.ValidateMessages = false;
			Device device = new Device(ip, port, callbackIp, callbackPort, requestorId);
			device.MessageReceived += Device_MessageReceived;

			var device2 = new Device(ip, port + 1, callbackIp, callbackPort + 1, requestorId);
			device2.MessageReceived += Device_MessageReceived;

			Console.WriteLine("Connecting...");
			Console.WriteLine("Press any key to exit");
			device.Connect();
			device2.Connect();

			Console.ReadKey(true);

			device.Disconnect();
			device2.Disconnect();
		}

        private static void Device_MessageReceived(object sender, MrsMessage e)
        {
			Device device = (Device)sender;
			if (e.IsValid(out Exception exception))
			{
				Console.WriteLine($"{e.MrsMessageType} received from {device.DeviceIP}:{device.DevicePort}");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Invalid message received from {device.DeviceIP}:{device.DevicePort}!\n{exception}");
				Console.ResetColor();
			}
		}
	}
}
