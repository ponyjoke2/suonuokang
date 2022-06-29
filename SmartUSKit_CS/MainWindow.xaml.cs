using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using ShaderEffectLibrary;
using SmartUSKit_CS.USViewers;
using System.Threading;
using SmartUSKit_CS.LanguageFiles;
using System.Windows.Interop;

using SmartUSKit;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Reflection;
using SmartUSKit_CS.USMainPages;
using System.Windows.Markup;
using SmartUSKit_CS.Operations;
using SmartUSKit_CS.Converters;
using SmartUSKit_CS.USWindows;
using SmartUSKit.Enums;
using SmartUSKit_CS.ExtensionMethods;
using SmartUSKit_CS.USTools;

namespace SmartUSKit_CS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public partial class MainWindow : Page
    {
        public MainWindow()
        {
            InitializeComponent();
            ultrasoundPage = UltrasoundPage.GetInstance();
            Ultrasoundframe.Content = ultrasoundPage;
        }

        UltrasoundPage ultrasoundPage;

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            //设置程序的优先级为AboveNormal
            var process1 = System.Diagnostics.Process.GetCurrentProcess();
            process1.PriorityClass = ProcessPriorityClass.RealTime;
        }
       
        private void Window_Closed(object sender, RoutedEventArgs e)
        {
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CloseHandler != null)
            {
                CloseHandler();
            }
        }

        public event Action MinimizeHandler;
        public event Action CloseHandler;

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            //this.WindowState = WindowState.Minimized;
            if (MinimizeHandler != null)
            {
                MinimizeHandler();
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        #region 提供给外部程序使用
        private static MainWindow Instance = null;
        public static MainWindow GetInstance()
        {
            if (Instance == null)
            {
                Instance = new MainWindow();
            }
            return Instance;
        }

        public void Dispose()
        {
            ultrasoundPage.Dispose();
        }
        #endregion
    }
}



