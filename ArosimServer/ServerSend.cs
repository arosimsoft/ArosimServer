using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArosimServer
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i= 1; i<= Server.MaxUsers; i++){
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for(int i=1; i<=Server.MaxUsers; i++)
            {
                if(i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxUsers; i++)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxUsers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }

        public static void UDPTest(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.udpTest))
            {
                packet.Write("A test packet for UDP.");

                SendUDPData(toClient, packet);
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SimulatorCommand(string command)
        {
            //TODO: Send Command from ClientControl to Simulator
            using (Packet packet = new Packet((int)ServerPackets.SimulatorCommand))
            {
                packet.Write(command);
                SendUDPData(1, packet);
            }

        }

        public static void AllCoordinates(string coord_string)
        {
            //TODO: Send All joint coordinates to ClientControl
            using (Packet packet = new Packet((int)ServerPackets.AllCoordinates))
            {
                packet.Write(coord_string);
                SendUDPData(2, packet);
            }
        }

    }
}
