using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpClientCommunication
{
    public class ClientCommunication
    {
        // IRL the client will ofcause not connect to the server with a local IP.
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        public async Task<string> ConnectToServer(string request)
        {
            int port = 13000;
            string server = GetLocalIPAddress();
            TcpClient client = null;

            while (client == null)
            {
                try
                {
                    client = new TcpClient(server, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not connect to server. Check if the server is running \nError message: {ex.Message}");
                    //throw new Exception($"Could not connect to server. Check if the server is running \nError message: {ex.Message}");
                }
            }
            NetworkStream netStream = client.GetStream();

            if (client.GetStream().CanWrite && client.GetStream().CanRead)
            {
                using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.Unicode))
                using (StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.Unicode))
                {
                    string response = "";
                    // While connected the client first sends the xString, and afterwards receives a response.
                    // the response can either be a new xml string or a confirmation message.
                    if (client.Connected)
                    {
                        await writer.WriteLineAsync(request);
                        await writer.FlushAsync();

                        response = await reader.ReadLineAsync();
                        client.Close();
                        return response;
                    }
                }
            }
            else
            {
                // if the client cant write the stream
                if (!client.GetStream().CanWrite) throw new Exception("Client cannot write");
                // else the client cant read the stream
                else throw new Exception("Client cannot read");
            }

            return "";
        }
    }
}
