using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NativeWifi;
using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.Model;
using SmartUSKit_CS.USViewers;

namespace SmartUSKit_CS.USWindows
{
    /// <summary>
    /// PresetWindow1.xaml 的交互逻辑
    /// </summary>

    public partial class PresetWindow : Window
    {
        public PresetWindow()
        {
            InitializeComponent();

            USPreferences prefs = USPreferences.GetInstance();
            LanguageComboBox.SelectedIndex = int.Parse(prefs.GetString("language", "0"));

            this.SettingsLabel.Content = Properties.Resources.Settings;
            this.btnClose.Content = Properties.Resources.Close;
            this.ShowInfoLabel.Content = Properties.Resources.ShowInformation + ":";
            this.ReplayFrames.Content = Properties.Resources.ReplayFrames + ":";
            this.wifichannelLabel.Content = Properties.Resources.WiFiChannel + ":";
            this.btnSelect.Content = Properties.Resources.Select;
            this.languageLabel.Content = Properties.Resources.Language;

            this.txtNum.Content = Preset.GetInstance().ImageMaxAmount.ToString();

            if ((byte)(Preset.GetInstance().PatientVisible) == 0)
            {
                this.IsShowimformationChb.IsChecked = true;
                this.IsShowimformationChb.Content = Properties.Resources.On;
            }
            else
            {
                this.IsShowimformationChb.IsChecked = false;
                this.IsShowimformationChb.Content = Properties.Resources.Off;
            }

            try
            {
                using (WlanClient client = new WlanClient())
                {
                    if (client.Interfaces.Count() > 0)
                    {
                        channelLabel.Content = client.Interfaces[0].Channel.ToString();
                    }
                }
                //WlanClient client ;
                //for (int i = 0; i < 1; i++)
                //{
                //    client = new WlanClient();
                    
                //        channelLabel.Content = client.Interfaces[0].Channel.ToString();

                //    IDisposable disposable = client as IDisposable;
                //    disposable.Dispose();
                    
                //}
            }
            catch (Exception e)
            {
                //USLog.LogInfo(e.ToString());
                Debug.WriteLine(e.ToString());
            }
            bool is5g = false;
            try
            {

                USManager mgr = USManager.GetInstance(null);
                var probe = mgr.GetCurrentProbe();
                if (probe!=null)
                {
                    is5g = probe.Is5GProbe();
                }
            }
            catch (Exception) { }
            ls.Items.Clear();
            if (is5g)
            {
                #region 5G
                ls.Items.Add("5G CHANNEL 40");
                ls.Items.Add("5G CHANNEL 44");
                ls.Items.Add("5G CHANNEL 48");
                ls.Items.Add("5G CHANNEL 149");
                ls.Items.Add("5G CHANNEL 153");
                ls.Items.Add("5G CHANNEL 157");
                ls.Items.Add("5G CHANNEL 161");
                ls.Items.Add("5G CHANNEL 165");
                #endregion
            }
            #region 2.4G
            ls.Items.Add("2.4G CHANNEL 1");
            ls.Items.Add("2.4G CHANNEL 2");
            ls.Items.Add("2.4G CHANNEL 3");
            ls.Items.Add("2.4G CHANNEL 4");
            ls.Items.Add("2.4G CHANNEL 5");
            ls.Items.Add("2.4G CHANNEL 6");
            ls.Items.Add("2.4G CHANNEL 7");
            ls.Items.Add("2.4G CHANNEL 8");
            ls.Items.Add("2.4G CHANNEL 9");
            ls.Items.Add("2.4G CHANNEL 10");
            ls.Items.Add("2.4G CHANNEL 11");
            ls.Items.Add("2.4G CHANNEL 12");
            ls.Items.Add("2.4G CHANNEL 13");
            #endregion
        }
        string tag = string.Empty;

        //关闭
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var story = (Storyboard)this.Resources["HideWindow"];
            if (story != null)
            {
                story.Completed += delegate { Close(); };
                story.Begin(this);
            }


        }
        // +按钮
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            int n = int.Parse(txtNum.Content.ToString());
            if (n < 200)
            {
                txtNum.Content = 200.ToString();
                Preset.GetInstance().ImageMaxAmount = 200;
            }
            else if (n < 500)
            {
                txtNum.Content = 500.ToString();
                Preset.GetInstance().ImageMaxAmount = 500;
            }
            else
            {
                txtNum.Content = 1000.ToString();
                Preset.GetInstance().ImageMaxAmount = 1000;
            }
        }
        // -按钮
        private void BtnSub_Click(object sender, RoutedEventArgs e)
        {
            int n = int.Parse(txtNum.Content.ToString());
            if (n >= 1000)
            {
                txtNum.Content = 500.ToString();
                Preset.GetInstance().ImageMaxAmount = 500;
            }
            else if (n >= 500)
            {
                txtNum.Content = 200.ToString();
                Preset.GetInstance().ImageMaxAmount = 200;
            }
            else
            {
                txtNum.Content = 100.ToString();
                Preset.GetInstance().ImageMaxAmount = 100;
            }
        }
        public void SetWifiChannel(int WifiChannel)
        {
        }
        //选择按钮
        private void BtnSelected(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = ls.SelectedItem.ToString();
                var arrstr = str.Split(new string[1] { "CHANNEL" }, StringSplitOptions.RemoveEmptyEntries);
                arrstr.Last<string>().ToString();
                int select = Convert.ToInt32(arrstr.Last<string>().ToString());
                if (select.ToString() == channelLabel.Content.ToString())
                {
                    return;
                }
                try
                {
                    USManager mgr = USManager.GetInstance(null);
                    USDriver driver = mgr.GetCurrentDriver();
                    driver.SetWifiChannel(select);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show(Application.Current.MainWindow, Properties.Resources.ConnectedToDevice);
                    return;
                }
                MessageBox.Show(Application.Current.MainWindow, Properties.Resources.RestartAndReconnect);
                this.Close();
            }
            catch (Exception)
            {
            }
        }
        //ON打开
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.IsShowimformationChb.Content = Properties.Resources.On;
            Preset.GetInstance().PatientVisible = Visibility.Visible;
            Preset.GetInstance().InfoVisible = Visibility.Visible;
            USViewer.StartShowInformation(false);
        }
        //ON关闭
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            this.IsShowimformationChb.Content = Properties.Resources.Off;
            Preset.GetInstance().PatientVisible = Visibility.Hidden;
            Preset.GetInstance().InfoVisible = Visibility.Hidden;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox ls = sender as ListBox;
            ListBoxItem lstitem = ls.ItemContainerGenerator.ContainerFromItem(ls.SelectedItem) as ListBoxItem;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutInt("language", LanguageComboBox.SelectedIndex);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int index = 0;
            foreach (var item in ls.Items)
            {
                string str = item.ToString();
                if (str.EndsWith(" "+channelLabel.Content.ToString()))
                {
                    ls.SelectedIndex = index;
                    break;
                }
                index++;
            }
        }
    }
}
