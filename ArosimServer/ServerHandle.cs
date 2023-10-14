using System;


namespace ArosimServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string userName = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfully and is now active {fromClient}.");
            if(fromClient != clientIdCheck)
            {
                Console.WriteLine($"User {userName} (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }
            // TODO: send user into simulation
        }

        internal static void UDPTestReceived(int fromClient, Packet packet)
        {
            string _msg = packet.ReadString();

            Console.WriteLine($"Received packet via UDP. Contains message: {_msg}");
        }

        internal static void ClientControlCommand(int fromClient, Packet packet)
        {
            //TODO: Get commands from ClientControl and send to Simulator
            string command = packet.ReadString();

            Console.WriteLine($"ClientControl sends: {command}");

            ServerSend.SimulatorCommand(command);

        }

        internal static void AllCoordinates(int fromClient, Packet packet)
        {
            //TODO: Get all coordinates string from Simulator and send to ClientControl
            string coords_string = packet.ReadString();

            //Console.WriteLine($"Simulator sends: {coords_string.Split(';')[0]}");
            //Console.WriteLine($"Simulator sends: {coords_string}");

            ServerSend.AllCoordinates(coords_string);

        }
    }
}