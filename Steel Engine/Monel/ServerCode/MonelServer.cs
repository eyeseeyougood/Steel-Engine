using FireSharp.EventStreaming;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.Monel
{
    public static class MonelServer
    {
        public static List<string> connectedClients = new List<string>();

        public static void Init()
        {
            InfoManager.SetWindowTitle("Steel - Monel (Server)");

            MonelLinkManager.client.OnAsync("", (sender, args, context) =>
            {
                HandleChanges(sender, args, context);
            });
        }

        public static bool BroadcastNotice(params string[] data)
        {
            try
            {
                MonelLinkManager.SendData("/Accounts/Broadcasts/Server", data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool SendData(string path, params string[] data)
        {
            try
            {
                MonelLinkManager.SendData(path, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void HandleChanges(object sender, ValueAddedEventArgs args, object context)
        {
            Console.WriteLine($"--------------------------------------------------");
            Console.WriteLine($"");
            Console.WriteLine($"New Change Recieved from {sender}!");
            Console.WriteLine($"Recieved Data: {args.Data}, in location: {args.Path}");
            Console.WriteLine($"");

            if (args.Path.StartsWith("/Accounts/Broadcasts/JoinRequests"))
            {
                if (args.Data == "") { return; } // ignore the request the server sends to clear the join requests
                if (args.Data.StartsWith("%")) { return; } // ignore the notices the server sends to other clients
                FirebaseResponse response = MonelLinkManager.client.Get("/Accounts/Broadcasts/JoinRequests");
                Console.WriteLine($"");
                Console.WriteLine("Body: " + response.Body);
                Console.WriteLine("Status Code: " + response.StatusCode);
                Console.WriteLine($"");

                KeyValuePair<string, string>[] JoinRequests = ExtractJoinRequests(response.Body);

                foreach (KeyValuePair<string, string> request in JoinRequests)
                {
                    if (!connectedClients.Contains(request.Value))
                    {
                        Console.WriteLine("Connecting Client: " + request.Value);
                        connectedClients.Add(request.Value);
                        MonelLinkManager.client.PushAsync("/Accounts/Broadcasts", request.Value);

                        // Cleanup JoinRequest
                        MonelLinkManager.client.Delete("/Accounts/Broadcasts/JoinRequests/"+request.Key);
                    }
                    else
                    {
                        Console.WriteLine("Invalid name, attempted username: " + request.Value);
                        MonelLinkManager.client.PushAsync("/Accounts/Broadcasts/JoinRequests/"+request.Key, "%INVALID NAME%-Name already taken!");
                    }
                }
            }
            else if (args.Path.StartsWith("/Accounts/Broadcasts"))
            {
                FirebaseResponse response = MonelLinkManager.client.Get("/Accounts/Broadcasts");
                Console.WriteLine($"");
                Console.WriteLine("Body: " + response.Body);
                Console.WriteLine("Status Code: " + response.StatusCode);
                Console.WriteLine($"");

                KeyValuePair<string, string>[] JoinRequests = ExtractJoinRequests(response.Body);

                foreach (KeyValuePair<string, string> request in JoinRequests)
                {
                    connectedClients.Add(request.Value);
                    Console.WriteLine("Recieved client: " + request.Value);
                }
            }
            Console.WriteLine($"--------------------------------------------------");
        }

        private static KeyValuePair<string, string>[] ExtractJoinRequests(string data) // KVP<string_id, string_username>
        {
            // data example format: {"-Nq3cHThxcTyeDZIzDKU":["eyeseeyougood"],"-Nq3cxEJ_fc_ld6Yv1Pc":["eyeseeyougood"]}
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            string filtered1 = data.Replace("{","").Replace("}","");
            string[] parts = filtered1.Split(',');
            foreach (string part in parts)
            {
                string[] keyValue = part.Split(':');
                keyValue[0] = keyValue[0].Replace("\"","");
                keyValue[1] = keyValue[1].Replace("[","").Replace("]","").Replace("\"","");
                keyValuePairs.Add(keyValue[0], keyValue[1]);
            }

            return keyValuePairs.ToArray();
        }

        public static void Tick(float deltaTime)
        {
        }
    }
}
