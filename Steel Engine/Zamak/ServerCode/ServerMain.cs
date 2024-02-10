using Steel_Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Zamak;

namespace Steel_Engine.Zamak
{
    public static class ServerMain
    {
        private static Dictionary<IPAddress, string> connectedClients = new Dictionary<IPAddress, string>();

        private static UdpClient udpClient;
        private static IPEndPoint remoteIpEndPoint;

        private static Dictionary<string, object[]> relayPackets = new Dictionary<string, object[]>();

        public static void Init()
        {
            udpClient = new UdpClient(9999);
            remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            SceneManager.ChangeClearColour(new OpenTK.Mathematics.Vector3(221, 148, 255) / 255f);
            InfoManager.SetWindowTitle("Steel - Zamak (Server)");
        }

        public static void CleanUp()
        {
            udpClient.Dispose();
        }

        public static bool CheckNameValidity(string name)
        {
            return !connectedClients.ContainsValue(name);
        }

        public static void DataRecieved(byte[] data, Dictionary<IPAddress, string> currentClients, out Dictionary<IPAddress, string> newClients, ref IPEndPoint rep)
        {
            object[] objectData = ObjectSerialiser.DeserialiseBytes(data);
            Dictionary<IPAddress, string> cl = new Dictionary<IPAddress, string>();
            foreach (KeyValuePair<IPAddress, string> cli in currentClients)
            {
                cl.Add(cli.Key, cli.Value);
            }
            if (objectData[0].GetType() == typeof(string))
            {
                string obj = (string)objectData[0];
                switch (obj) // is this vulnerable to injection?
                {
                    case "[{Join RQ}]":
                        // its a join game request
                        // so follow the join game request packet standard
                        if (!cl.ContainsValue((string)objectData[1]))
                        {
                            cl.Add(rep.Address, (string)objectData[1]);
                            udpClient.Send(ObjectSerialiser.SerialiseObjects(new object[] { $"Join attempt successfull, new client ({objectData[1]})" }), rep);
                        }
                        break;
                    case "[{RelayZC RQ}]":
                        // its a relay packet request
                        // so follow the relay packet request packet standard
                        relayPackets.Add((string)objectData[1], objectData);
                        break;
                    case "[{PacheckZC RQ}]":
                        // its a check packet request
                        // so follow the check packet request packet standard
                        if (relayPackets.ContainsKey(connectedClients[rep.Address]))
                        {
                            udpClient.Send(ObjectSerialiser.SerialiseObjects(relayPackets[connectedClients[rep.Address]]), rep);
                        }
                        break;
                }
            }
            newClients = cl;
        }

        public static void CheckForData(ref Dictionary<IPAddress, string> clients)
        {
            byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
            DataRecieved(receiveBytes, clients, out Dictionary<IPAddress, string> newClients, ref remoteIpEndPoint);
            clients = newClients;
        }

        public static void Tick(float deltaTime)
        {
            Task.Run(() => CheckForData(ref connectedClients)); // check for data on a new thread because checking for data blocks the thread
            Console.WriteLine(connectedClients.Count);
        }
    }
}