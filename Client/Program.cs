using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Client
{
    public class Program
    {
        private static TcpClient client = new TcpClient();
        private static StreamReader SR;
        private static StreamWriter SW;
        private static string remoteAddress;
        private static byte[] buffer = new byte[1024];

        static void Main(string[] args)
        {
            Console.Title = "Client";

            Console.WriteLine("Connecting to server");
            LoopConnect();

            Console.WriteLine("Press enter to start chatting");
            while (client.Connected)
            {
                client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), client);
                if (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    //Console.Write("You: ");
                    //SendData();

                    Console.Write("You: ");
                    string message = Console.ReadLine();
                    byte[] dataBuffer = Encoding.ASCII.GetBytes(message);
                    client.Client.BeginSend(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), client);
                }
            }
        }

        private static void SendCallBack(IAsyncResult ar)
        {
            while (true)
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                while (client.Connected)
                {
                    client.Client.EndSend(ar);
                    string message = Console.ReadLine();
                    byte[] dataBuffer = Encoding.ASCII.GetBytes(message);
                    Console.Write("You: ");
                    client.Client.BeginSend(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), client);
                    break;
                }
                break;
            }
        }

        private static void ReceiveData(IAsyncResult ar)
        {
            while (true)
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                while (client.Connected)
                {
                    int received = client.Client.EndReceive(ar);
                    byte[] dataBuffer = new byte[received];
                    Array.Copy(buffer, dataBuffer, received);
                    SR = new StreamReader(client.GetStream());
                    remoteAddress = client.Client.RemoteEndPoint.ToString();
                    string receivedText = Encoding.ASCII.GetString(dataBuffer);
                    Console.WriteLine(remoteAddress + ": " + receivedText);
                    client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), client);
                    break;
                }
                break;
            }
        }

        private static void SendData()
        {
            while (true)
            {
                while (client.Connected)
                {
                    SW = new StreamWriter(client.GetStream());
                    SW.AutoFlush = true;
                    SW.WriteLine(Console.ReadLine());
                    Console.Write("You: ");
                }
            }
        }

        private static void LoopConnect()
        {
            int attempts = 0;
            try
            {
                while(!client.Connected)
                {
                    attempts++;
                    client.Connect(IPAddress.Parse("112.204.108.171"), 16969);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Connected to server " + client.Client.RemoteEndPoint);
        }

    }
}