using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Zamak;
using FireSharp.Response;
using FireSharp.EventStreaming;

namespace Steel_Engine.Monel
{
    public class MonelClient
    {
        public static string clientName = "NONE";
        private static string joinLocation;
        private static string noticeLocation;
        private static bool connecting = false;
        private static string attemptedPassword;
        public static bool connected;

        public static void Init()
        {
            InfoManager.SetWindowTitle("Steel - Monel (Client)");
            //InfoManager.executingArgs[3]); // the 4th arg is the requested name
            MonelLinkManager.client.OnAsync("", (sender, args, context) =>
            {
                HandleChanges(sender, args, context);
            });
        }

        public static void Login(string accountName, string password)
        {
            Console.WriteLine("------------LOGIN------------");
            // check if account exists
            FirebaseResponse response = MonelLinkManager.client.Get("/Accounts/Broadcasts");

            string _noticeLocation = "";
            bool accountExists = false;

            response.Body.Replace("{","").Replace("}","").Replace("\"","");
            string[] parts = response.Body.Split(',');
            foreach (string part in parts)
            {
                string[] split = part.Split(':');
                string key = split[0];
                string value = split[1];

                if (value == accountName)
                {
                    _noticeLocation = key;
                    accountExists = true;
                }
            }

            if (!accountExists)
            {
                Console.WriteLine("ERROR: Account doesn't exist! Create an account or get lost!");
            }

            // check password

            FirebaseResponse accountData = MonelLinkManager.client.Get("/Accounts/"+accountName);

            Console.WriteLine(accountData.Body);

            // confirm connection
            // set notice location
            Console.WriteLine("------------LOGIN------------");
        }

        public static void CreateAccount(string accountName, string password)
        {
            connecting = true;
            PushResponse response = MonelLinkManager.client.Push("/Accounts/Broadcasts/JoinRequests", accountName);
            string filter = response.Body.Replace("}","");
            string[] parts = filter.Split(':');
            string location = parts[1].Replace("\"","");
            clientName = accountName;
            MonelLinkManager.client.Set("Accounts/"+accountName, password);
            joinLocation = location;
            attemptedPassword = password;
        }

        public static void HandleChanges(object sender, ValueAddedEventArgs args, object context)
        {
            Console.WriteLine($"Broadcast recieved: {args.Data} at location {args.Path}");

            if (args.Path.StartsWith("/Accounts/Broadcasts/JoinRequests/" + joinLocation))
            {
                FirebaseResponse response = MonelLinkManager.client.Get(args.Path);
                Console.WriteLine("Body: " + response.Body);
                if (response.Body.Replace("\"","") == "%INVALID NAME%-Name already taken!")
                {
                    Console.WriteLine("+------------------------------------------------------------------------------------------+");
                    Console.WriteLine("|ERROR: Invalid Username - It has already been taken, please try again with a new username.|");
                    Console.WriteLine("+------------------------------------------------------------------------------------------+");
                    // clean up join request
                    MonelLinkManager.client.Delete(args.Path);
                    // confirm connection
                    connecting = false;
                    connected = false;
                    // close application
                    InfoManager.CloseWindow();
                }
                if (response.Body.Replace("\"", "") == "%Account Created Successfully!%")
                {
                    Console.WriteLine("Connection established successfully.");
                    // confirm connection
                    connecting = false;
                    connected = true;
                    MonelLinkManager.client.Set("/Accounts/" + clientName, attemptedPassword);
                    // clean up join request
                    MonelLinkManager.client.Delete(args.Path);
                }
            }
        }

        public static bool SendData(params string[] data)
        {
            try
            {
                MonelLinkManager.SendData("/Accounts/Broadcasts/"+clientName, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Tick(float deltaTime)
        {
        }
    }
}