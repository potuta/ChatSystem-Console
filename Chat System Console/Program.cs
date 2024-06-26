using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Program 
    {
        private static TcpListener server = new TcpListener(IPAddress.Any, 16969);
        private static TcpClient client = new TcpClient();
        private static List<TcpClient> clientList = new List<TcpClient>();
        private static List<StreamWriter> SWList = new List<StreamWriter>();
        private static StreamReader SR;
        private static StreamWriter SW;
        private static string remoteAddress;
        private static byte[] buffer = new byte[1024];

        static void Main(string[] args)
        {
            Console.Title = "Server";
            server.Start();

            Console.WriteLine("Waiting for connection");
            server.BeginAcceptTcpClient(new AsyncCallback(AcceptConnections), null);
            Console.ReadLine();

            Console.WriteLine("Press enter to start chatting");
            for (int i = 0; i < clientList.Count; i++)
            {
                while (clientList[i].Connected)
                {
                    clientList[i].Client.BeginReceive(buffer, 0 , buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), client);
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        Console.Write("You: ");
                        SendData();
                    }
                }
            }
        }

        private static void AcceptConnections(IAsyncResult ar)
        {
            try
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                client = server.EndAcceptTcpClient(ar);
                clientList.Add(client);
                SWList.Add(new StreamWriter(client.GetStream()));
                Console.WriteLine("Client " + client.Client.RemoteEndPoint + " connected");
                client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), client);
                server.BeginAcceptTcpClient(new AsyncCallback(AcceptConnections), null);
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void ReceiveData(IAsyncResult ar)
        {
            TcpClient client = (TcpClient)ar.AsyncState;
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].Connected)
                {
                    client = clientList[i];
                }
            }
            int received = client.Client.EndReceive(ar);
            byte[] dataBuffer = new byte[received];
            Array.Copy(buffer, dataBuffer, received);
            remoteAddress = client.Client.RemoteEndPoint.ToString();
            string receivedText = Encoding.ASCII.GetString(dataBuffer);
            Console.Write(remoteAddress + ": " + receivedText);
            client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), client);
        }

        private static void SendData()
        {
            while (true)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].Connected)
                    {
                        SW = new StreamWriter(clientList[i].GetStream());
                        SW.AutoFlush = true;
                        SW.WriteLine(Console.ReadLine());
                        Console.Write("You: ");
                    }
                }
            }
        }

        private static void LoopAcceptConnection()
        {
            try
            {
                client = server.AcceptTcpClient();
                Console.WriteLine("Client "+ client.Client.RemoteEndPoint +" connected");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}