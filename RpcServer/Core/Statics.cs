using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ModalStrikeServer.RpcServer.Core {
    public static class Statics {
        public static Dictionary<TcpClient, string> Users = new();

        public static ConcurrentDictionary<string, string> PlayerTokens = new();
        
        public static ServerStatus ServerStatus;
    }
}
