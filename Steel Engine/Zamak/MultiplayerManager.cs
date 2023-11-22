using Steel_Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Zamak
{
    public enum ClientType
    {
        Host,
        Client,
        Server
    }

    public class DataPacket
    {
        public string recipient = "";
        private byte[] data;
        public object[] objectData;
    }

    public static class MultiplayerManager
    {// per runtime
        public static bool isMultiplayer;
        public static ClientType clientType;
        public static bool connectedToServer;
        public static string playerName;

        public static void CleanUp()
        {
            if (!isMultiplayer)
                return;

            if (clientType == ClientType.Server)
            {
                ServerMain.CleanUp();
            }
            else
            {
                ClientMain.CleanUp();
            }
        }

        public static int Init(params string[] args)
        {
            // the first arg is always client, server, or host
            Enum.TryParse<ClientType>(args[0], out ClientType result);
            clientType = result;

            if (clientType == ClientType.Server)
            {
                ServerMain.Init();
            }
            else
            {
                playerName = args[2];
                ClientMain.Init();
            }
            return 0;
        }

        public static void Tick(float deltaTime)
        {
            if (isMultiplayer)
            {
                if (clientType == ClientType.Server)
                {
                    ServerMain.Tick(deltaTime);
                }
                else
                {
                    ClientMain.Tick(deltaTime);
                }
            }
        }

        public static int AttemptConnection(uint serverIp, string nameRequest)
        {
            if (clientType == ClientType.Server)
                return 1; // 1 means invalid request - in this case a server is attempting to connect to another server / client
            return 0;
        }
    }
}