using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpServerCommunication;

namespace Server
{
    class ServerProgram
    {
        public static ServerCommunication serverCom = new ServerCommunication(new ServerFunc());
        static void Main(string[] args)
        {
            serverCom.ServerStartListening();
            Console.ReadLine();
        }
    }
}
