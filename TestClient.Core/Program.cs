using MrsDeviceManager.Core;
using SensorStandard.Core;
using System;
using System.Net;
using System.Threading;

namespace TestClient.Core
{
	class Program
	{
		private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

		static void Main(string[] args)
		{
			Globals.ValidateMessages = false;
			Device device = new Device("192.168.43.105", 41000, "127.0.0.1", 12000, "MarsLab");
			device.MessageReceived += Device_MessageReceived;
            device.MessageSent += Device_MessageSent;
            device.Disconnected += Device_Disconnected;
			Console.WriteLine("Connecting...");
			Console.WriteLine("Press any key to exit");
			device.Connect();

			Console.CancelKeyPress += (o, e) =>
			{
				Console.WriteLine("Exit");
				// Allow the main thread to continue and exit...
				WaitHandle.Set();

				device.Disconnect();
			};
			// wait until Set method calls
			WaitHandle.WaitOne();
		}

		private static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}

			return string.Empty;
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
