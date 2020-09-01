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
            device.MessageSent += Device_MessageSent;

			Console.WriteLine("Connecting...");
			Console.WriteLine("Press any key to exit");
			device.Connect();

			Console.ReadKey(true);

			device.Disconnect();
		}

        private static void Device_MessageSent(object sender, MrsMessage e)
		{
			Device device = (Device)sender;
			Console.WriteLine($"{DateTime.Now} - {e.MrsMessageType} Message Sent to {device.DeviceIP}:{device.DevicePort}");
		}

        private static void Device_MessageReceived(object sender, MrsMessage e)
        {
			Device device = (Device)sender;
			Console.WriteLine($"{DateTime.Now} - {e.MrsMessageType} Message Received from {device.DeviceIP}:{device.DevicePort}");			
		}
	}
}
