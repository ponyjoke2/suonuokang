using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Drawing.Imaging;

namespace 图片通信服务端winform
{
    public partial class Form1 : Form
    {
        public Socket SocketWatch1=null;   //图片
        public Thread threadWatch1 = null; // 图片，负责监听客户端连接请求的线程；
        public Socket SocketWatch2 = null;  //信息
        public Thread threadWatch2 = null; // 信息，负责监听客户端连接请求的线程；
       
        /**
        * 构造函数中初始化Socket
        */
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//在其他线程中可以调用主窗体控件
            
            //使用tcp通信，注意参数设置为SocketType.Stream, ProtocolType.Tcp
            int port1 = 34568;
            IPAddress ip1 = IPAddress.Any;
            SocketWatch1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//使用指定的地址簇协议、套接字类型和通信协议   <br>            ReceiveSocket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);  //有关套接字设置
            IPEndPoint endPoint1 = new IPEndPoint(ip1, port1);
            SocketWatch1.Bind(endPoint1); //绑定IP地址和端口号
            SocketWatch1.Listen(3);
            
            int port2 = 34569;
            IPAddress ip2 = IPAddress.Any;
            SocketWatch2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//使用指定的地址簇协议、套接字类型和通信协议   <br>            ReceiveSocket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);  //有关套接字设置
            IPEndPoint endPoint2 = new IPEndPoint(ip2, port2);
            SocketWatch2.Bind(endPoint2); //绑定IP地址和端口号
            SocketWatch2.Listen(3);

           
        }
        /**
        * 按下button1开始监听
        */
        private void button1_Click(object sender, EventArgs e)
        {
            threadWatch1 = new Thread(WatchConnecting1);
            threadWatch1.IsBackground = true;
            threadWatch1.Start();
            threadWatch2 = new Thread(WatchConnecting2);
            threadWatch2.IsBackground = true;
            threadWatch2.Start();
            button1.Text = "已启动";

        }

        void WatchConnecting1()
        {
            while (true)  // 持续不断的监听客户端的连接请求；
            {
                // 开始监听客户端连接请求，Accept方法会阻断当前的线程；
                Socket socketConnection = SocketWatch1.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字；
                Thread thr = new Thread(job1);
                thr.IsBackground = true;
                thr.Start(socketConnection);
                
            }
        }
        void WatchConnecting2()
        {
            while (true)  // 持续不断的监听客户端的连接请求；
            {
                // 开始监听客户端连接请求，Accept方法会阻断当前的线程；              
                Socket socketConnection = SocketWatch2.Accept(); // 一旦监听到一个客户端的请求，就返回一个与该客户端通信的 套接字；
                Thread thr = new Thread(job2);
                thr.IsBackground = true;
                thr.Start(socketConnection);

            }
        }
        
        /**
         * 这是csdn上抄的方法
         * 可以将灰度数组转为bitmap
         */
        public static Bitmap ToGrayBitmap(byte[] rawValues, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定  
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// 获取图像参数  
            int stride = bmpData.Stride;  // 扫描线的宽度  
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙  
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置  
            int scanBytes = stride * height;// 用stride宽度，表示这是内存区域的大小  

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组  
            int posScan = 0, posReal = 0;// 分别设置两个位置指针，指向源数组和目标数组  
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存  

            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描  
                for (int y = 0; y < width; y++)
                {
                    pixelValues[posScan++] = rawValues[posReal++];
                }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”  
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
            ////将数据从一维托管 8 位无符号整数数组复制到非托管内存指针。
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域  

            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度  
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bmp.Palette = tempPalette;
            //bmp = new Bitmap(bmp, 704, 436);

            //// 算法到此结束，返回结果  
            return bmp;
        }

        /**
        *接收并显示图片
        */
        public void job1(object socketConnection)
        {
            try
            {
                Socket ReceiveSocket = socketConnection as Socket;
                //缓冲区大小不能超过接收到的数据大小的2倍,1227782,2455564
                byte[] receive = new byte[1500000];
                byte[] widthBytes = new byte[4];
                byte[] heightBytes = new byte[4];
                while (true)
                {
                    //接收数据，要接收的字节数不可超过缓冲区大小
                    ReceiveSocket.Receive(widthBytes, 0, 3, SocketFlags.None);
                    ReceiveSocket.Receive(heightBytes, 0, 3, SocketFlags.None);

                    
                    //这里需要线程休眠一段时间，不然接收数据有误，下一行代码会出现字符串输入格式不正确的异常。
                    //原因不明
                    Thread.Sleep(2);
                    //获取图像的宽度和高度
                    int width = int.Parse(Encoding.ASCII.GetString(widthBytes).Substring(0, 3));
                    int height = int.Parse(Encoding.ASCII.GetString(heightBytes).Substring(0, 3));
                    byte[] image = new byte[width * height * 4];
                    ReceiveSocket.Receive(receive, 0, width * height * 4, SocketFlags.None);

                    Thread.Sleep(2);
                    //截取其中的图像数据
                    Array.Copy(receive, 0, image, 0, width * height * 4);
                    //转成灰度数组
                    byte[] image2 = new byte[width * height];
                    for (int i = 0; i < image.Length; i++)
                    {
                        if (i % 4 == 0)
                        {
                            image2[i / 4] = image[i];
                        }
                    }
                    //灰度数组转bitmap
                    Bitmap bitmap = ToGrayBitmap(image2, width, height);

                    //前端显示图像,pictureBox的SizeMode要选择StretchImage
                    this.Invoke((Action)delegate ()
                    {

                        pictureBox1.Image = bitmap;

                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //ReceiveSocket.Close();
            }
            
        }

        /**
         * 接收并显示深度，宽高比，面积
         */

        public void job2(object socketConnection)
        {
            try
            {
                Socket ReceiveSocket = socketConnection as Socket;
                byte[] receive = new byte[1024];
                while (true)
                {
                    int len = ReceiveSocket.Receive(receive);
                    String str = Encoding.ASCII.GetString(receive);
                    String[] stringArray = str.Split('#');
                    double depth = Convert.ToDouble(stringArray[0]);
                    if (depth < 0)
                    {
                        this.Invoke((Action)delegate ()
                        {
                            textBox1.Text = "请正确贴合";
                            textBox2.Text = "请正确贴合";
                            textBox3.Text = "请正确贴合";
                            textBox4.Text = "请正确贴合";
                        });
                    }
                    else
                    {
                        //前端显示深度
                        this.Invoke((Action)delegate ()
                        {
                            if (stringArray[0].Length > 4) {
                                textBox1.Text = stringArray[0].Substring(0,4);
                            }
                            else { 
                                textBox1.Text = stringArray[0];
                            }
                            if (stringArray[1].Length > 4)
                            {
                                textBox2.Text = stringArray[1].Substring(0, 4);
                            }
                            else
                            {
                                textBox2.Text = stringArray[1];
                            }
                            if (stringArray[2].Length > 4)
                            {
                                textBox3.Text = stringArray[2].Substring(0, 4);
                            }
                            else
                            {
                                textBox3.Text = stringArray[2];
                            }
                            if (stringArray[3].Length > 5)
                            {
                                textBox4.Text = stringArray[3].Substring(0, 5);
                            }
                            else
                            {
                                textBox4.Text = stringArray[3];
                            }
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //ReceiveSocket.Close();
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

