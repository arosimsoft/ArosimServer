using System;
using System.Net;
using System.Net.Sockets;

namespace ArosimServer
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public TCP tcp;
        public UDP udp;

        public Client(int clientID)
        {
            id = clientID;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;

            public TCP(int tcpId)
            {
                id = tcpId;
            }

            public void Connect(TcpClient tcpSocket)
            {
                socket = tcpSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                // Send welcome packet
                ServerSend.Welcome(id, "Welcome to the server");

            }

            public void SendData(Packet packet)
            {
                try
                {
                    if(socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error sending data to user {id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    int byteLenght = stream.EndRead(ar);
                    if(byteLenght<=0)
                    {
                        // Disconnect
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLenght];
                    Array.Copy(receiveBuffer, data, byteLenght);

                    // Handle data
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {ex}");
                    // Disconnect
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedData.SetBytes(data);

                if(receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if(packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using(Packet packet = new Packet(packetBytes)){
                            int packetId = packet.ReadInt();
                            Server.packetHandlers[packetId](id, packet);
                        }
                    });

                    packetLength = 0;
                    if(receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if(packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if(packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }
    
    
        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int id)
            {
                this.id = id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
                ServerSend.UDPTest(id);
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
            
        }

        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            tcp.Disconnect();
            udp.Disconnect();
        }

    }
}
