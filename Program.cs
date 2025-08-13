using System.Net;
using System.Net.Sockets;
using ModalStrikeServer.MongoDb.Core;
using ModalStrikeServer.RpcServer.Core;
using ModalStrikeServer.RpcServer.Utilities;
using ModalStrikeServer.RpcServer.Utilities.CustomLogger;

public enum ServerStatus
{
    Production,
    OnlyApprovedUsers,
    OnlyDev
}

class Program
{
    public static async Task Main(string[] args)
    {
        Logger.Debug($"Select server connection type:\nProduction - 0\nOnlyApprovedUsers - 1\nOnlyDev - 2");
      
        var typeValue = Console.ReadLine();
        
        if (int.TryParse(typeValue, out var typeStatus)) 
        {
            if (typeStatus > 4)
            {
                Logger.Debug("Incorect input. Please restart the server.");
                return;
            }
            var selectedType = (ServerStatus)typeStatus;
            
            Statics.ServerStatus = selectedType;
            
            Logger.Debug($"You selected server type: {selectedType}");
        }
        else
            Logger.Debug("Incorect input. Please restart the server.");
        
        Logger.Debug("Initializing database...");
        UnitOfWork.Initialize();
        
        await new ServerService(2222).LoopClients();
    }
}

public class ServerService {
    private TcpListener _server;

    public static string GetLocalIPAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach(var ip in host.AddressList) {
            if(ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public ServerService(int port) {
        _server = new TcpListener(IPAddress.Any, port);
        _server.Start();

        var logger = new ServiceLogger(typeof(ServerService));
        logger.Debug($"Server started. Address: {GetLocalIPAddress()}:{port}");
    }

    public async Task LoopClients() {
        while(true) {
            var newClient = await _server.AcceptTcpClientAsync();
            var a = Task.Run(() => HandleNewClient(newClient));
        }
    }

    private async Task HandleNewClient(TcpClient client) {
        var user = new TCPServer(client);
        
        await user.HandleClient();
    }
}