using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarsDeviceManager;
using SensorStandard;
using SensorStandard.MrsTypes;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Wpf;
using File = SensorStandard.MrsTypes.File;

namespace SensorStandardImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Device _device;
        private bool _validate;
        private bool _showKeepAlive;

        public string IP
        {
            get { return (string)GetValue(IPProperty); }
            set { SetValue(IPProperty, value); }
        }

        public static readonly DependencyProperty IPProperty =
            DependencyProperty.Register(nameof(IP), typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register(nameof(Port), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int NotificationPort
        {
            get { return (int)GetValue(NotificationPortProperty); }
            set { SetValue(NotificationPortProperty, value); }
        }

        public static readonly DependencyProperty NotificationPortProperty =
            DependencyProperty.Register(nameof(NotificationPort), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public MainWindow()
        {
            InitializeComponent();
            IP = GetLocalIPAddress();
            Port = 13001;
            NotificationPort = 20000;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _device?.Disconnect();
            _device = new Device(IP, Port, 
                GetLocalIPAddress(), NotificationPort, "MarsLab");

            Globals.ValidateMessages = false;

            _device.MessageSent += Device_MessageSent;
            _device.MessageReceived += Device_MessageReceived;
            _device.Disconnected += Device_Disconnected;

            _device.Connect();
        }

        private void Device_MessageReceived(object sender, MrsMessage e)
        {
            if (_validate && e.IsValid(out var exception) == false)
            {
                MessageBox.Show("Invalid message!\n\n" + exception.Message);
            }
            if (e is DeviceStatusReport status)
            {
                if (_showKeepAlive || status.Items != null && status.Items.OfType<SensorStatusReport>().Any(x => x.Item != null))
                {
                    AddLogItem("Status Report Received", e.ToXml());
                }
                var picture = status.Items?.OfType<SensorStatusReport>().FirstOrDefault(x => x.PictureStatus != null)?.PictureStatus;
                if (picture?.MediaFile != null && picture.MediaFile.Length > 0)
                {
                    File file = picture.MediaFile[0];
                    var stream = new MemoryStream(file.File1);

                    var dir = new DirectoryInfo(@"C:\Program Files (x86)\VideoLAN\VLC");
                    VlcControl.SourceProvider.CreatePlayer(dir);
                    VlcControl.SourceProvider.MediaPlayer.Play(stream);
                    //if (file.ItemElementName == ItemChoiceType3.NameJPEG)
                    //{
                    //    JoinUiThread(() => { VlcControl.SourceProvider.VideoSource = LoadImage(file.File1); });
                    //}
                    //else if (file.ItemElementName == ItemChoiceType3.NameMP4)
                    //{
                    //    var stream = new MemoryStream(file.File1);

                    //    var dir = new DirectoryInfo(@"C:\Program Files (x86)\VideoLAN\VLC");
                    //    VlcControl.SourceProvider.CreatePlayer(dir);
                    //    VlcControl.SourceProvider.MediaPlayer.Play(stream); 
                    //}
                }
            }
            else if (e is DeviceIndicationReport indication)
            {
                AddLogItem("Indication Report Received", e.ToXml());
                if (indication.Items.OfType<SensorIndicationReport>().ElementAt(0).IndicationType[0].Item is VideoAnalyticDetectionType detectionType)
                {
                    var imageData = detectionType.Picture?.ElementAt(0).File1;
                    if (imageData != null)
                    {
                        try
                        {
                            if (VlcControl.SourceProvider.MediaPlayer == null)
                            {
                                var dir = new DirectoryInfo(@"C:\Program Files (x86)\VideoLAN\VLC");
                                VlcControl.SourceProvider.CreatePlayer(dir);
                            }

                            var stream = new MemoryStream(imageData);
                            VlcControl.SourceProvider.MediaPlayer.Play(stream);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error Displaying Media\n\n" + ex);
                        }
                    }
                }
            }
            else
            {
                AddLogItem($"{e.MrsMessageType} Received", e.ToXml());
            }
        }

        private void Device_MessageSent(object sender, MrsMessage e)
        {
            if (e is CommandMessage commandMessage && commandMessage.Command.Item is SimpleCommandType simple &&
                simple == SimpleCommandType.KeepAlive)
            {
                if (_showKeepAlive == false)
                {
                    return;
                }
            }

            AddLogItem($"{e.MrsMessageType} Sent", e.ToXml());
        }

        private void Device_Disconnected(object sender, EventArgs e)
        {
            AddLogItem("Disconnected");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _device?.Disconnect();
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

        private void AddLogItem(string message, object content = null)
        {
            JoinUiThread(() =>
            {
                var item = new ListViewItem { Content = $"{DateTime.Now.ToLongTimeString()} - {message}" };
                if (content != null)
                {
                    item.MouseDoubleClick += (sender, args) =>
                    {
                        var logItemWindow = new LogItemWindow(content);
                        logItemWindow.Show();
                    };
                }
                LogList.Items.Add(item);
                ScrollViewer.ScrollToEnd();
            });
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

        private async void JoinUiThread(Action action)
        {
            await Dispatcher.InvokeAsync(action);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _device?.Disconnect();
            Environment.Exit(0);
        }

        private void ValidateCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            // cant use binding because of thread issue
            _validate = true;
        }

        private void ValidateCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            // cant use binding because of thread issue
            _validate = false;
        }

        private void KeepAliveCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            // cant use binding because of thread issue
            _showKeepAlive = true;
        }

        private void KeepAliveCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            // cant use binding because of thread issue
            _showKeepAlive = false;
        }
    }
}
