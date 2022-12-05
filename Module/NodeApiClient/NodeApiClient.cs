using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using VMP_CNR.Module.Logging;

namespace VMP_CNR
{
    public class NodeAPIClient
    {
        private const string URL = "http://127.0.0.1:1337/";

        public static void SendApiRequest(string command, Dictionary<string, string> paras)
        {
            Logger.Debug("API TO:" + URL + command);
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(paras);

                client.PostAsync(URL + command, content);
            }
        }
    }
}