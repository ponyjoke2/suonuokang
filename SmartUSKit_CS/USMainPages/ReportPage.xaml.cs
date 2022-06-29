using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using SamrtUSExamination.USWindows;
//using SamrtUSExamination.Views;
using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.USWindows;
using SmartUSKit_CS.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Windows.Threading;

namespace SmartUSKit_CS.USMainPages
{
    /// <summary>
    /// ReportPage1.xaml 的交互逻辑
    /// </summary>
    public partial class ReportPage1 : Page
    {
        public static bool textGetFocus = false;
        public static Process process;
        // public 
        public static Dispatcher ReportDispatcher;
        public static TextBox textBox = new TextBox();

        public static bool isshowing = false;

        public static ChromiumWebBrowser Browser;

        public delegate void CloseWindowDelegate();
        public static event CloseWindowDelegate CloseWindowEventHandler;

        public ReportPage1()
        {
            InitializeComponent();
            Back2HomeButton.Content = Properties.Resources.Back1;
            SetreportButton.Content = Properties.Resources.Settings;
            SavereportButton.Content = Properties.Resources.Save;
            PrintreportButton.Content = Properties.Resources.Print;

            USPreferences thePrefs = USPreferences.GetInstance();
            SmartUSKit_CS.Model.USExamination.GetInstance().report.organization = thePrefs.GetString("Hospital", "Hospital");

            lstCategories.ItemsSource = MainWindow.UltrasoundImages;
            Browser = new ChromiumWebBrowser();
            Browser.Name = "Browser";
            Browser.TouchUp += Browser_TouchUp;
            viewContainer.Children.Add(Browser);

            ReportDispatcher = this.Dispatcher;
            USPreferences prefs = USPreferences.GetInstance();
            int language = int.Parse(prefs.GetString("language", "2"));
            if (language == 2)
            {
                Browser.Address = Environment.CurrentDirectory + @"\report_doc_en.html";
            }
            else
            {
                Browser.Address = Environment.CurrentDirectory + @"\report_doc.html";
            }
            //this.Browser.MenuHandler = new MenuHandler();   //右键菜单
            //ReportUtil.setmWebView(Browser);
            //ReportUtil.ShowTipsHandler += ShowTips;
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;//新cefsharp绑定需要优先申明
            JsEvent_ReportPage jsEvent = new JsEvent_ReportPage();
            Browser.RegisterJsObject("jsObj", jsEvent, new CefSharp.BindingOptions() { CamelCaseJavascriptNames = false });

            //var ccc = Browser.GetBrowser();
            ReportPage1.CloseWindowEventHandler += this.CloseWindow;
        }

        private void ShowTips()
        {
            try
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    TipsWin tipsWin = new TipsWin(Properties.Resources.Tip_4Images);
                    tipsWin.ShowDialog();
                    ReportUtil.TipsWinIsclosed = true;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public void CloseWindow()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    //this.Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Back2Homebtn_Click(object sender, RoutedEventArgs e)
        {
            //this.Visibility = Visibility.Hidden;
            //this.Close();
            //MainWindow.Instance.ShowHome();
        }
        private void Browser_TouchUp(object sender, TouchEventArgs e)
        {
            textGetFocus = true;
        }

        #region 将RenderTargetBitmap转换为byte数组
        public byte[] ReportImageBytes;
        public byte[] RTB2Bytes(RenderTargetBitmap bmp)
        {
            //RenderTargetBitmap bmp = new RenderTargetBitmap((int)this.viewContainer.ActualWidth - 1, (int)this.viewContainer.ActualHeight - 1, 96, 96, PixelFormats.Pbgra32);
            //bmp.Render(this.viewContainer);

            BitmapImage bmpR = SmartUSKit_CS.Converters.ImageConverter.ConvertRenderTargetBitmapToBitmapImage(bmp);
            Stream stm = bmpR.StreamSource;
            stm.Position = 0;
            BinaryReader br = new BinaryReader(stm);
            byte[] buff = br.ReadBytes((int)stm.Length);
            br.Close();
            ReportImageBytes = buff;
            stm.Dispose();

            return ReportImageBytes;
        }

        #endregion


        String json;
        /// <summary>
        /// 双击图片将图片传入到H5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Button button = (Button)sender;
            //System.Windows.Controls.Image image = (System.Windows.Controls.Image)button.Content;
            //RenderTargetBitmap renderTargetBitmap = (RenderTargetBitmap)image.Source;

            //记录当前传递的图片，根据H5返回的参数来决定是否加入到imageIndexiLst中去
            //ReportUtil.CurrentImage = renderTargetBitmap;
            #region 更新图片
            //imageforh5.Source = renderTargetBitmap;
            RenderTargetBitmap bmpForUpdate = new RenderTargetBitmap((int)this.viewContainerforh5.ActualWidth - 1, (int)this.viewContainerforh5.ActualHeight - 1, 96, 96, PixelFormats.Pbgra32);
            bmpForUpdate.Render(this.viewContainerforh5);
            #endregion

            PushImage2H5(RTB2Bytes(bmpForUpdate), "Select");
        }
        async public static void PushImage2H5(byte[] images, string imageInfo)
        {
            try
            {
                String base64 = bitmapToBase64(images);
                String[] myparams = { base64, imageInfo };
                string json = JSONComm.makeJSONWithFunction("loadRawImage", myparams);
                int length = json.Length;

                //这一步需要C#代码调用JavaScript
                await Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                GC.Collect();
            }


        }
        private void cmdView_Clicked(object sender, RoutedEventArgs e)
        {
            //Button cmd = (Button)sender;
            //DataRowView row = (DataRowView)cmd.Tag;
            //lstCategories.SelectedItem = row;

            // Alternate selection approach.
            //ListBoxItem item = (ListBoxItem)lstCategories.ItemContainerGenerator.ContainerFromItem(row);
            //item.IsSelected = true;

            //MessageBox.Show("You chose category #" + row["CategoryID"].ToString() + ": " + (string)row["CategoryName"]);
        }
        private void Printreportbtn_Click(object sender, RoutedEventArgs e)
        {
            PrintH5();
        }
        private void Savereportbtn_Click(object sender, RoutedEventArgs e)
        {
            //保存报告
            //ReportUtil.GetExaminationFromH5();
            SaveH5AsImage();
            //MessageBox.Show(SmartUSKit_CS.Properties.Resources.SaveSuccess, SmartUSKit_CS.Properties.Resources.Tips);
            //HidePage();

            //this.Close();
        }
        public static void SaveH5AsImage()
        {
            DoWhatNow = (int)DoWhat.SaveReportAsImage;
            //String base64 = SmartUSKit_CS.View.ReportUntil.bitmapToBase64(images);
            String[] myparams = { };
            string json = JSONComm.makeJSONWithFunction("save", myparams);

            //这一步需要C#代码调用JavaScript
            Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");

        }
        private void Setreportbtn_Click(object sender, RoutedEventArgs e)
        {
            ReportSettingWin reportSettingWin = new ReportSettingWin();
            reportSettingWin.ShowDialog();
        }
        #region JSONComm
        public static String makeJSONWithFunction(String function, String[] myparams)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                dict.Add("Function", function);
                if (myparams.Length == 1)
                {
                    dict.Add("Param", myparams[0]);
                }
                else if (myparams.Length == 2)
                {
                    dict.Add("Param1", myparams[0]);
                    dict.Add("Param2", myparams[1]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            string str = JsonConvert.SerializeObject(dict);
            return str;
        }
        public static string makeStringWithFunction(string function, string[] myparams)
        {
            string str = null;
            if (myparams.Length == 1)
            {
                str = "{\"Function\":\"" + function + "\",\"Param\":\"" + myparams[0] + "\"}";
            }
            else if (myparams.Length == 2)
            {
                str = "{\"Function\":\"" + function + "\",\"Param1\":\"" + myparams[0] + "\",Param2\":\"" + myparams[1] + "\"}";
            }
            return str;
        }
        public static void callWithJSON(String jsonObject)
        {
            string functionName = null;
            string param = null;
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(jsonObject);
                functionName = jo["Function"].ToString();
                param = jo["Param"].ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            switch (functionName)
            {
                case "saveReport":
                    {
                        saveReport(param);
                        //ReportPage1.CloseWindowEventHandler();
                    }
                    break;
                case "loadImage":
                    {
                        //loadImage();
                    }
                    break;
                case "addReportImageCallback":
                    {
                        addReportImageCallback(param);
                    }
                    break;
                case "deleteReportImageCallback":
                    {
                        deleteReportImageCallback(param);
                    }
                    break;
                case "onExamination":
                    {
                        onExamination(param);
                    }
                    break;
                case "updateExamination":
                    {
                        try
                        {
                            USPreferences prefs = USPreferences.GetInstance();
                            int language = int.Parse(prefs.GetString("language", "0"));
                            if (language == 2)
                            {
                                string patha = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                                StreamReader sr = new StreamReader(patha + @"obsandtips_en.json", Encoding.Unicode);
                                String line;
                                JObject o = (JObject)JToken.Parse(sr.ReadToEnd());
                                LoadListJson(o);
                            }
                            else if (language == 0)
                            {
                                string patha = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                                StreamReader sr = new StreamReader(patha + @"obsandtips.json", Encoding.Unicode);
                                String line;
                                JObject o = (JObject)JToken.Parse(sr.ReadToEnd());
                                LoadListJson(o);
                            }
                            else
                            {
                                string patha = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                                StreamReader sr = new StreamReader(patha + @"obsandtips.json", Encoding.Unicode);
                                String line;
                                JObject o = (JObject)JToken.Parse(sr.ReadToEnd());
                                LoadListJson(o);
                            }
                            //string patha = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                            //StreamReader sr = new StreamReader(patha + @"obsandtips.json", Encoding.Unicode);
                            //String line;
                            //JObject o = (JObject)JToken.Parse(sr.ReadToEnd());
                            //LoadListJson(o);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;
            }
        }
        #endregion
        #region ReportUntil
        private static int DoWhatNow = 0;
        private enum DoWhat
        {
            PrintReport = 0,
            SaveReportAsImage = 1
        }
        public delegate void ShowTipsDelegate();
        public static ShowTipsDelegate ShowTipsHandler;
        public static bool TipsWinIsclosed = true;
        public static void addReportImageCallback(string msg)
        {
            if (msg == "Select")
            {
                //SmartUSKit_CS.Model.USExamination.GetInstance().report.imageIndexiLst.Add(ReportUtil.CurrentImage);
            }
            if (msg == "-1")
            {
                //在此处调用STA内的弹窗，js无法直接调用
                if (TipsWinIsclosed)
                {
                    ShowTipsHandler();
                    TipsWinIsclosed = false;
                }

            }

        }

        internal static void deleteReportImageCallback(string param)
        {
            Debug.WriteLine("deleteReportImageCallback:" + param);
            try
            {
                //SmartUSKit_CS.Model.USExamination.GetInstance().report.imageIndexiLst.RemoveAt(int.Parse(param));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }
        internal static void onExamination(string param)
        {
            //在软件上点击保存会调用该方法
            //Debug.WriteLine("onExamination:" + param);
            JObject jsonObject = (JObject)JsonConvert.DeserializeObject(param);
            try
            {
                //MessageBox.Show(jsonObject["obs"].ToString()+jsonObject["tips"].ToString())  ;
                SmartUSKit_CS.Model.USExamination.GetInstance().obs = jsonObject["obs"].ToString();
                SmartUSKit_CS.Model.USExamination.GetInstance().tips = jsonObject["tips"].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public static void saveReport(String base64)
        {
            DoWhatNow = (int)DoWhat.SaveReportAsImage;
            byte[] bitmapBytes = ReportPage1.base64ToBitmap(base64);
            Debug.WriteLine($"bitmapBytes.Length:{bitmapBytes.Length}");
            using (MemoryStream ms = new MemoryStream(bitmapBytes))
            {
                using (Bitmap bitmap = new Bitmap(ms))
                {
                    try
                    {
                        //MemoryStream ms = new MemoryStream(bitmapBytes);
                        //Bitmap bmp = new Bitmap(ms);
                        //ms.Close();
                        //bitmap = bmp;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Base64StringToImage 转换失败\nException：" + ex.Message);
                    }
                    string patha = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\" + AppInfo.AppName;
                    // string patha = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\"+App.AppName;
                    if (!System.IO.Directory.Exists(patha))
                    {
                        System.IO.Directory.CreateDirectory(patha);
                    }
                    string report = "Report";
                    USPreferences prefs = USPreferences.GetInstance();
                    int language = int.Parse(prefs.GetString("language", "0"));
                    if (language == 2)
                    {
                        report = "Report";
                    }
                    else if (language == 1)
                    {
                        report = "報告";
                    }
                    else if (language == 0)
                    {
                        report = "报告";
                    }


                    if (DoWhatNow == (int)DoWhat.SaveReportAsImage)
                    {
                        //string filename = patha + "\\" + report + DateTime.Now.ToString("yyyyMMdd-HHmmss") + @".jpg";
                        //bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                        try
                        {
                            string filename = patha + "\\" + report + DateTime.Now.ToString("yyyyMMdd-HHmmss") + @".png";
                            bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                            MessageBox.Show(Properties.Resources.SaveSuccess);
                        }
                        catch (Exception)
                        {
                            throw new Exception("报告存文件出错！");
                        }

                    }
                    if (DoWhatNow == (int)DoWhat.PrintReport)
                    {
                        string filename = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "ReportForPrint" + @".jpg";
                        if (File.Exists(filename))
                        {
                            //删除原来的
                            File.Delete(filename);
                        }
                        bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ReportUtil.Print(filename);
                    }
                }
            }

            //Bitmap bitmap = null;

            CloseWindowEventHandler?.Invoke();
        }
        public static void LoadListJson(object list)
        {
            //DoWhatNow = (int)DoWhat.SaveReportAsImage;
            //String base64 = SmartUSKit_CS.View.ReportUntil.bitmapToBase64(images);
            object[] myparams = { list };
            string json = JSONComm.makeJSONWithFunction("loadListJson", myparams);

            //这一步需要C#代码调用JavaScript
            Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");

        }

        public static void PrintH5()
        {
            DoWhatNow = (int)DoWhat.PrintReport;
            //String base64 = SmartUSKit_CS.View.ReportUntil.bitmapToBase64(images);
            String[] myparams = { };
            string json = JSONComm.makeJSONWithFunction("save", myparams);

            //这一步需要C#代码调用JavaScript
            Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");

        }

        public static async void loadImage()
        {
            if (MainWindow.ReportImageBytes == "")
            {
                return;
            }
            //String base64 = (MainWindow.ReportImageBytes);

            String[] myparams = { MainWindow.ReportImageBytes };
            String json = ReportPage1.makeJSONWithFunction("loadRawImage", myparams);

            //这一步需要C#代码调用JavaScript
            await ReportPage1.Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");
        }

        // * bitmap转为base64
        // * @param bitmap
        // * @return
        // */
        public static string bitmapToBase64(byte[] bytes)
        {
            string base64 = "";
            try
            {
                base64 = System.Convert.ToBase64String(bytes);
            }
            catch (Exception)
            {
            }
            return base64;
        }

        // * base64转为bitmap  此方法的目的是为了接收html返回的截图，该截图会被保存到本地
        // * @param base64Data
        // * @return
        // */
        public static byte[] base64ToBitmap(string base64Data)
        {
            String headBase = "base64,";
            String newBase = base64Data.Substring(base64Data.IndexOf(headBase) + 7);
            char[] chars = newBase.ToCharArray();
            byte[] bytes = System.Convert.FromBase64CharArray(chars, 0, chars.Length);
            return bytes;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lstCategories.ItemsSource = MainWindow.UltrasoundImages;
            //PushJson2H5(SmartUSKit_CS.Model.USExamination.GetInstance());
        }
        public static void PushJson2H5(object jsonStr)
        {

            //String base64 = SmartUSKit_CS.View.ReportUntil.bitmapToBase64(images);
            object[] myparams = { jsonStr };
            //string json = JSONComm.makeJSONWithFunction("updateReportInfo", myparams);
            string json = JSONComm.makeJSONWithFunction("updateExamination", myparams);

            //这一步需要C#代码调用JavaScript
            //mWebView.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");
            var ccc = Browser.GetBrowser();
            Browser.GetBrowser().MainFrame.EvaluateScriptAsync("loadMTForJSWithJson('" + json + "')");

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                Action action = new Action(() =>
                 {
                     ReportPage1.CloseWindowEventHandler -= this.CloseWindow;
                     if (ReportPage1.Browser != null)
                     {



                     }
                     if (process != null)
                     {
                         process.Kill();
                     }
                     ReportPage1.isshowing = false;
                     ReportUtil.ShowTipsHandler -= ShowTips;
                 });
                action.BeginInvoke(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            //此处为了缩放图片，图片太大的话在向h5传递图片时会导致一些莫名的异常！！！
            Button button = (Button)sender;
            System.Windows.Controls.Image image = (System.Windows.Controls.Image)button.Content;
            BitmapSource renderTargetBitmap = (BitmapSource)image.Source;
            imageforh5.Source = renderTargetBitmap;
            double maxwidth = 750;
            if (renderTargetBitmap.PixelWidth > maxwidth)
            {
                viewContainerforh5.Width = maxwidth;
                double nwidth = renderTargetBitmap.PixelHeight * maxwidth / renderTargetBitmap.PixelWidth;
                viewContainerforh5.Height = nwidth;
            }
            //Panel.SetZIndex(imageforh5, 100);
        }
    }

    public class JsEvent_ReportPage
    {
        //TextBox textBox = new TextBox();
        public string MessageText { get; set; }
        public void ShowTest()
        {
            MessageBox.Show("this in c#.\n\r" + MessageText);
        }
        public void ShowTestArg(string ss)
        {
            MessageBox.Show("收到Js参数的调用\n\r" + ss);
        }

        //网页加载完时调用下面的方法
        public void loadJSForMTWithJson(string jsonObject)
        {
            if (jsonObject.Contains("updateExamination"))
            {
                ReportPage1.PushJson2H5(SmartUSKit_CS.Model.USExamination.GetInstance());
            }
            ReportPage1.callWithJSON(jsonObject);
            if (jsonObject.Contains("onExamination"))
            {
                //this.close
            }
        }
        public void GetFocus()
        {
            try
            {
                ReportPage1.ReportDispatcher.Invoke(() =>
                {
                    if (ReportPage1.textGetFocus)
                    {
                        //ReportWindow1.process = Process.Start(@"osk.exe");

                        string TabTipExecPath = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
                        ReportPage1.process = Process.Start(TabTipExecPath);
                        ReportPage1.textBox.Focus();
                        ReportPage1.textBox.Focusable = false;
                        //Debug.WriteLine("GetFocus");
                        ReportPage1.textGetFocus = false;
                    }
                });

            }
            catch (Exception ex)
            {
            }

        }
        public void LostFocus()
        {
            try
            {
                ReportPage1.ReportDispatcher.Invoke(() =>
                {
                    ReportPage1.textGetFocus = false;
                    Debug.WriteLine("LostFocus");
                    try
                    {
                        ReportPage1.process.Kill();
                    }
                    catch (Exception)
                    {
                    }
                });

            }
            catch (Exception es)
            {
                // MessageBox.Show(es.ToString());
                //throw;
            }

        }
    }

    /// <summary>
    /// cef菜单事件
    /// </summary>
    public class MenuHandler_ReportPage : CefSharp.IContextMenuHandler
    {

        void CefSharp.IContextMenuHandler.OnBeforeContextMenu(CefSharp.IWebBrowser browserControl, CefSharp.IBrowser browser, CefSharp.IFrame frame, CefSharp.IContextMenuParams parameters, CefSharp.IMenuModel model)
        {
            model.Clear();
        }

        bool CefSharp.IContextMenuHandler.OnContextMenuCommand(CefSharp.IWebBrowser browserControl, CefSharp.IBrowser browser, CefSharp.IFrame frame, CefSharp.IContextMenuParams parameters, CefSharp.CefMenuCommand commandId, CefSharp.CefEventFlags eventFlags)
        {
            //throw new NotImplementedException();
            return false;
        }

        void CefSharp.IContextMenuHandler.OnContextMenuDismissed(CefSharp.IWebBrowser browserControl, CefSharp.IBrowser browser, CefSharp.IFrame frame)
        {
            //throw new NotImplementedException();
        }

        bool CefSharp.IContextMenuHandler.RunContextMenu(CefSharp.IWebBrowser browserControl, CefSharp.IBrowser browser, CefSharp.IFrame frame, CefSharp.IContextMenuParams parameters, CefSharp.IMenuModel model, CefSharp.IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}
