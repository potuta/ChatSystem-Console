using System.Net;
using System.Net.Sockets;
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
                    Console.Write("You: ");
                    SendData();
                }
            }
        }

        private static void ReceiveData(IAsyncResult ar)
        {
            TcpClient client = (TcpClient)ar.AsyncState;
            int received = client.Client.EndReceive(ar);
            byte[] dataBuffer = new byte[received];
            Array.Copy(buffer, dataBuffer, received);
            SR = new StreamReader(client.GetStream());
            remoteAddress = client.Client.RemoteEndPoint.ToString();
            string receivedText = Encoding.ASCII.GetString(dataBuffer);
            Console.Write(remoteAddress + ": " + receivedText);
            client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), client);
        }

        private static void SendData()
        {
            while (true)
            {
                SW = new StreamWriter(client.GetStream());
                SW.AutoFlush = true;
                SW.WriteLine(Console.ReadLine());
                Console.Write("You: ");
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