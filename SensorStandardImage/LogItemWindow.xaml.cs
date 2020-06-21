using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SensorStandardImage
{
    /// <summary>
    /// Interaction logic for LogItemWindow.xaml
    /// </summary>
    public partial class LogItemWindow : Window
    {

        public object Item
        {
            get { return (object)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(object), typeof(LogItemWindow), new PropertyMetadata(null));


        public LogItemWindow()
        {
            InitializeComponent();
        }

        public LogItemWindow(object item) : this()
        {
            Item = item;
        }
    }
}
