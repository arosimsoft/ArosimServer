using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ArosimServer
{
    class Server
    {
        public static int MaxUsers { private set; get; }
        public static int PortConnection { private set; get; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static Dictionary<int, PacketHandler> packetHandlers;


        private static TcpListener tcpListener;

        private static UdpClient udpListener;


        public static void Start(int maxUsers, int portConnection)
        {
            MaxUsers = maxUsers;
            PortConnection = portConnection;

            Console.WriteLine("Starting server...");
            InitializeServerData();
            
            // TCP Connection
            tcpListener = new TcpListener(IPAddress.Any, PortConnection);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            
            // UDP Connection
            udpListener = new UdpClient(portConnection);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on port {PortConnection}");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection fom {client.Client.RemoteEndPoint}...");

            for(int i=1; i<= MaxUsers; i++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void InitializeServerData()
        {
            for(int i=1; i<=MaxUsers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.updTestReceived, ServerHandle.UDPTestReceived },
                { (int)ClientPackets.ClientControlCommand, ServerHandle.ClientControlCommand },
                { (int)ClientPackets.AllCoordinates, ServerHandle.AllCoordinates }

            };
            Console.WriteLine("Initialized packets.");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if(data.Length < 4)
                {
                    return;
                }

                using(Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();

                    if(clientId == 0)
                    {
                        return;
                    }

                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if(clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientId].udp.HandleData(packet);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        internal static void SendUDPData(IPEndPoint endPoint, Packet packet)
        {
            try
            {
                if (endPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {endPoint} via UDP: {ex}");
            }
        }
    }
}
