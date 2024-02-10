using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.Monel
{
    public abstract class MonelLink
    {
        public abstract IFirebaseConfig CreateConfig();
    }

    public static class MonelLinkManager
    {
        public static IFirebaseClient client { get; private set; }

        private static IFirebaseConfig firebaseConfiguration;

        public static MonelLink monelLink;

        public static void Init()
        {
            if (monelLink == null)
            {
                throw new Exception("The monel link must be set before attempting to initialise the MonelLinkManager.");
            }

            firebaseConfiguration = monelLink.CreateConfig();

            client = new FireSharp.FirebaseClient(firebaseConfiguration);
        }

        public static async Task SendData(string path, params string[] data)
        {
            await client.PushAsync(path, data);
        }
    }
}