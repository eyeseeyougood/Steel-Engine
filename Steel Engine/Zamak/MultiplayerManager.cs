using Steel_Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Steel_Engine.Monel;
using Steel_Engine.Zamak;

namespace Zamak
{
    public enum MultiplayerType
    {
        Zamak,
        Monel
    }

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
        public static MultiplayerType multiplayerType;
        public static ClientType clientType;
        public static bool connectedToServer;
        public static string playerName;

        public static void CleanUp()
        {
            if (!isMultiplayer || multiplayerType != MultiplayerType.Zamak)
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
            Enum.TryParse<ClientType>(args[1], out ClientType result);
            clientType = result;

            if (multiplayerType == MultiplayerType.Monel)
            {
                MonelLinkManager.Init();
            }

            if (clientType == ClientType.Server && multiplayerType == MultiplayerType.Zamak)
            {
                ServerMain.Init();
            }
            else if (clientType == ClientType.Client && multiplayerType == MultiplayerType.Zamak)
            {
                playerName = args[3];
                ClientMain.Init();
            }
            else if (clientType == ClientType.Server && multiplayerType == MultiplayerType.Monel)
            {
                MonelServer.Init();
            }
            else if (clientType == ClientType.Client && multiplayerType == MultiplayerType.Monel)
            {
                MonelClient.Init();
            }

            return 0;
        }

        public static void Tick(float deltaTime)
        {
            if (isMultiplayer)
            {
                if (clientType == ClientType.Server && multiplayerType == MultiplayerType.Zamak)
                {
                    ServerMain.Tick(deltaTime);
                }
                else if (clientType == ClientType.Client && multiplayerType == MultiplayerType.Zamak)
                {
                    ClientMain.Tick(deltaTime);
                }
                else if (clientType == ClientType.Server && multiplayerType == MultiplayerType.Monel)
                {
                    MonelServer.Tick(deltaTime);
                }
                else if (clientType == ClientType.Client && multiplayerType == MultiplayerType.Monel)
                {
                    MonelClient.Tick(deltaTime);
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