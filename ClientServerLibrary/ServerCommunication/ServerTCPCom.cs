using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServerCommunication
{
    public class ServerCommunication
    {
        IHandleRequest func;

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        // the methods returns a string the program can write to the console.
        public async void ServerStartListening()
        {
            while (true)
            {
                MyTcpListener server = null;
                try
                {
                    //the port the TcpListener will use. Not sure if this is the best port to use.
                    int port = 13000;
                    string serveradd = GetLocalIPAddress();
                    IPAddress localAddr = IPAddress.Parse(serveradd);
                    Console.WriteLine(serveradd);

                    server = new MyTcpListener(localAddr, port);
                    Console.WriteLine("Server starting...");
                    // Starts listening for client request
                    server.Start();

                    // Uses the protected 'active' TcpListener property to determine how long to listen
                    while (true /*server.Active*/)
                    {
                        TcpClient client = null;

                        try
                        {
                            Console.WriteLine("Listening for client request...");

                            // Perform a blocking call to accept request
                            //client = server.AcceptTcpClient();
                            client = await server.AcceptTcpClientAsync();
                            Console.WriteLine("Connected!");

                            using (StreamReader reader = new StreamReader(client.GetStream(), Encoding.Unicode))
                            using (StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.Unicode))
                            {
                                string receivedData = "";
                                string response = "";

                                if (client.Connected)
                                {
                                    //receivedData = reader.ReadLine();
                                    receivedData = await reader.ReadLineAsync();
                                    Console.Write("Received data: ");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(receivedData);
                                    Console.ForegroundColor = ConsoleColor.Gray;

                                    response = func.HandleRequest(receivedData);
                                    await writer.WriteLineAsync(response);
                                    Console.WriteLine("Response ready to send");
                                    await writer.FlushAsync();
                                    Console.Write("Response sent: ");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(response + "\n");
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        finally
                        {
                            // Shutdown and end connection
                            // Client closes connection.
                            //client?.Close();
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"SocketException: {ex}");
                }
                finally
                {
                    server.Stop();
                }
            }
        }
        public ServerCommunication(IHandleRequest handleRequest)
        {
            func = handleRequest;
        }
    }
}
