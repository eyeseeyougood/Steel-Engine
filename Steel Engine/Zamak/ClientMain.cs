using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Steel_Engine;

namespace Zamak
{
    public static class ClientMain
    {
        private static UdpClient udpClient;
        private static IPEndPoint clientEP;

        public static void Init()
        {
            IPAddress serverIP = IPAddress.Parse(InfoManager.executingArgs[2]);
            udpClient = new UdpClient();
            udpClient.Connect(serverIP, 9999);
            clientEP = new IPEndPoint(serverIP, 0);
            SceneManager.ChangeClearColour(new OpenTK.Mathematics.Vector3(193, 255, 148) / 255f);
            InfoManager.SetWindowTitle("Steel - Zamak (Client)");
            object[] objectData = new object[2];
            objectData[0] = "[{Join RQ}]";
            objectData[1] = InfoManager.executingArgs[3]; // the 4th arg is the requested name
            SendData(objectData);
        }

        public static void SendData(params object[] data)
        {
            byte[] byteData = ObjectSerialiser.SerialiseObjects(data);
            udpClient.Send(byteData, byteData.Length).ToString();
        }

        public static void CleanUp()
        {
            udpClient.Dispose();
        }

        public static void CheckForData()
        {
            byte[] receiveBytes = udpClient.Receive(ref clientEP);
            Console.WriteLine(ObjectSerialiser.DeserialiseBytes(receiveBytes)[0]);
        }

        public static void Tick(float deltaTime)
        {
            Task.Run(() => CheckForData());
            object[] pckt = new object[]
            {
                "[{PacheckZC RQ}]"
            };

            SendData(pckt);
        }
    }
}
