using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServerCommunication
{
    class MyTcpListener : TcpListener
    {
        public MyTcpListener(IPAddress localaddr, int port) : base(localaddr, port)
        {
        }

        public new bool Active
        {
            get { return base.Active; }
        }
    }
}
