using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3DEmulator
{
    public class PictureWindow:Window
    {
        //public static async Task ShowPicture(BitmapImage img)
        //{
        //    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Constants.picturePort));
        //    img.CopyPixels()
        //    BitConverter.GetBytes(img.PixelWidth).Concat(BitConverter.GetBytes(img.PixelHeight)).Concat(BitConverter.GetBytes(img.))
        //    socket.Send
        //}
        Image img = new Image();
        Socket socket;
        void Send(Socket client, byte[]data)
        {
            data = BitConverter.GetBytes(data.Length).Concat(data).ToArray();
            System.Diagnostics.Trace.Assert(client.Send(data) == data.Length);
        }
        byte[] Receive(Socket client)
        {
            byte[] data = new byte[2];
            System.Diagnostics.Trace.Assert(client.Receive(data) == data.Length);
            int length = BitConverter.ToInt32(data, 0);
            data = new byte[length];
            System.Diagnostics.Trace.Assert(client.Receive(data) == data.Length);
            return data;
        }
        async void Start()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    using (var client = socket.Accept())
                    {
                        var data = Receive(client);
                        int width = BitConverter.ToInt32(data, 0), height = BitConverter.ToInt32(data, 2), stride = BitConverter.ToInt32(data, 4);
                        var bmpSrc = BitmapSource.Create(width, height, 0, 0, new PixelFormat(), null, data, stride);
                        Dispatcher.InvokeAsync(new Action(() =>
                        {
                            this.Width = width;
                            this.Height = height;
                            img.Source = bmpSrc;
                        }));
                        data = Encoding.UTF8.GetBytes("OK");
                        Send(client, data);
                        client.Close();
                    }
                }
            });
        }
        public PictureWindow(int port)
        {
            this.Content = img;
            this.WindowStyle = WindowStyle.None;
            this.Width = 100;
            this.Height = 100;
            this.Left = 1000;
            this.Top = 300;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            socket.Listen(int.MaxValue);
            Start();
        }
    }
}
