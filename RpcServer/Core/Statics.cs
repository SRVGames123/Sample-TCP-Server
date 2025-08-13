using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ModalStrikeServer.RpcServer.Core {
    public class EnumMapper<TP, TO> {
        private static readonly Dictionary<TP, TO> OriginalMap;

        private static readonly Dictionary<TO, TP> ProtoMap;

        static EnumMapper() {
            Type protoType = typeof(TP);
            Type originalType = typeof(TO);
            OriginalMap = Enum.GetValues(typeof(TO)).Cast<TO>().ToDictionary((TO tp) => (TP)Enum.Parse(protoType, tp.ToString()));
            ProtoMap = Enum.GetValues(typeof(TP)).Cast<TP>().ToDictionary((TP tp) => (TO)Enum.Parse(originalType, tp.ToString()));
        }

        public static TO ToOriginal(TP proto) {
            return OriginalMap[proto];
        }

        public static TP ToProto(TO original) {
            return ProtoMap[original];
        }

        public static TO[] ToOriginal(TP[] proto) {
            return proto.Select(ToOriginal).ToArray();
        }

        public static TP[] ToProto(TO[] original) {
            return original.Select(ToProto).ToArray();
        }
    }
    public class Token { public ObjectId playerId; public string gameCode; public string gameVersion; }

    public static class Statics {
        public static Dictionary<TcpClient, string> Users = new();

        public static ConcurrentDictionary<string, string> PlayerTokens = new();
        
        public static ServerStatus ServerStatus;
    }
}
