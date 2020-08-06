using MrsDeviceManager.Core;
using SensorStandard;
using System;

namespace TestClient.Core
{
	class Program
	{
		static void Main(string[] args)
		{
			Globals.ValidateMessages = false;
			Device device = new Device("127.0.0.1", 41000, "MarsLab");
			device.MessageReceived += Device_MessageReceived;
            device.MessageSent += Device_MessageSent;
            device.Disconnected += Device_Disconnected;
			Console.WriteLine("Connecting...");
			Console.WriteLine("Press any key to exit");
			device.Connect();

			while (Console.ReadKey(true).Key != ConsoleKey.Escape)
			{
				device.TurnOn();
			}
			device.Disconnect();
		}

        private static void Device_Disconnected(object sender, EventArgs e)
		{
			Device device = (Device)sender;
			Console.WriteLine($"Device ({device.DeviceIP}:{device.DevicePort}) Disconnected");
        }

        private static void Device_MessageSent(object sender, MrsMessage e)
        {
			Device device = (Device)sender;
			Console.WriteLine($"{e.MrsMessageType} Sent to {device.DeviceIP}:{device.DevicePort}");
		}

        private static void Device_MessageReceived(object sender, MrsMessage e)
		{
			Device device = (Device)sender;
			Console.WriteLine($"{e.MrsMessageType} Received from {device.DeviceIP}:{device.DevicePort}");
		}
	}
}
