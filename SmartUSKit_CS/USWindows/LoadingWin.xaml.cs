﻿using System;
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
    /// LoadingWin.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWin : Window
    {
        public Action DoWork;
        public LoadingWin()
        {
            InitializeComponent();
        }
        public LoadingWin(Action doWork)
        {
            InitializeComponent();
            DoWork = doWork;
            this.Loaded += LoadCirclePage_Loaded;
        }

        private void LoadCirclePage_Loaded(object sender, RoutedEventArgs e)
        {
            DoWork.BeginInvoke(CloseThis, null);
        }

        private void CloseThis(IAsyncResult ar)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                this.ProgressLabel.Content = text;
            }
        }
        public void Show(String State)
        {
            this.Text = State;
            this.ShowDialog();
        }
    }
}
