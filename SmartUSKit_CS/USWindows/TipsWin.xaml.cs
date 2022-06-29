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

namespace SmartUSKit_CS.USWindows
{
    /// <summary>
    /// TipsWin.xaml 的交互逻辑
    /// </summary>
    public partial class TipsWin : Window
    {
        public TipsWin()
        {
            InitializeComponent();
            SaveButton.Content = SmartUSKit_CS.Properties.Resources.Save;
            this.Title = SmartUSKit_CS.Properties.Resources.Tips;
        }
        public TipsWin(string tips)
        {
            InitializeComponent();
            TipsLabel.Content = tips;
        }
        private void Savebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
