using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace 通信服务端
{
    class Program
    {
        
        static void Main(string[] args)
        {
            int port = 34567;
            IPAddress ip = IPAddress.Any;
            Socket ReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//使用指定的地址簇协议、套接字类型和通信协议   <br>            ReceiveSocket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);  //有关套接字设置
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            ReceiveSocket.Bind(endPoint); //绑定IP地址和端口号
            //ReceiveSocket.Listen(100);  //设定最多有10个排队连接请求
            Console.WriteLine("建立连接");
            try
            {


                byte[] receive = new byte[32];
                while (true)
                {
                    /*
                    Socket socket = ReceiveSocket.Accept();
                    Console.WriteLine("ReceiveSocket.Accept()");
                    int len=socket.Receive(receive);
                    */
                    int len =ReceiveSocket.Receive(receive);
                    Console.WriteLine("接收到消息：" + Encoding.ASCII.GetString(receive).Substring(0,5)+"，接收数据长度："+len+"字节");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ex:{ex}");
            }
            finally {
                //socket.Close();
                ReceiveSocket.Close();
            }

            

        }
    }
}
