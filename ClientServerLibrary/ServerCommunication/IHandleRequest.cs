using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServerCommunication
{
    public interface IHandleRequest
    {
        string HandleRequest(string receivedData);
    }
}
