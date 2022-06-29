using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WirelessUSG
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public EventWaitHandle ProgramStarted { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "WirelessUSG", out createNew);


            if (!createNew)
            {
                App.Current.Shutdown();
                Environment.Exit(0);
            }
            base.OnStartup(e);


            DispatcherUnhandledException += App_DispatcherUnhandledException;

            SplashScreen splashScreen = new SplashScreen("Splash.png");
            splashScreen.Show(true, true);

            USPreferences prefs = USPreferences.GetInstance();
            int language = 0;
            string languageInt = prefs.GetString("language", "0");
            int.TryParse(languageInt, out language);

            switch (language)
            {
                case 0:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
                    break;
                case 1:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-TW");
                    break;
                case 2:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                    break;
                default:
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                    break;
            }
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
        }
    }
}
