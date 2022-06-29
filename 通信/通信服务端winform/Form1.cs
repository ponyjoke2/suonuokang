using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace 通信服务端winform
{
    public partial class Form1 : Form
    {
        public static Socket ReceiveSocket;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//在其他线程中可以调用主窗体控件
            int port = 34567;
            IPAddress ip = IPAddress.Any;
            //udp通信
            ReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//使用指定的地址簇协议、套接字类型和通信协议   <br>            ReceiveSocket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);  //有关套接字设置
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            ReceiveSocket.Bind(endPoint); //绑定IP地址和端口号
        }
        
        private void button1_Click(object sender, EventArgs e)
        {

            Thread t = new Thread(job);
            t.Start();
            t.IsBackground = true;//规定t为后台线程后台线程会在前台线程结束时结束
            Bitmap bitmap=new Bitmap(20,30);
            for (int i = 0; i < 20; i++) {
                for (int j = 0; j < 30; j++) { 
                    
                }
            }
        }

        public void job() {
           
            try
            {


                byte[] receive = new byte[32];
                while (true)
                {
                    int len = ReceiveSocket.Receive(receive);
                    this.Invoke((Action)delegate ()
                    {

                         textBox1.Text = Encoding.ASCII.GetString(receive).Substring(0, 5);

                    });
                    Console.WriteLine("接收到消息：" + Encoding.ASCII.GetString(receive).Substring(0, 5) + "，接收数据长度：" + len + "字节");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ReceiveSocket.Close();
            }
        }
    }
}
