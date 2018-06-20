using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using BlackHole.Core;

using static BlackHole.Core.DefaultLogger;


namespace RPC
{
    public interface IConnectionRPC {}

    public abstract class Connection
    {
        protected abstract INetwork NetworkConnection { get; }

        public void AddImplement<T, ImplementType>()
            where T : IConnectionRPC
            where ImplementType : T
        {
            this.Connections[typeof(T)] = Reflect.TConstruct<T>(typeof(ImplementType), this.NetworkConnection);
        }

        public T GetRPCObject<T>()
            where T : IConnectionRPC
        {
            if (!this.Connections.TryGetValue(typeof(T), out var rConnectionRPC))
                return default(T);

            return (T)rConnectionRPC;
        }

        protected Dictionary<Type, IConnectionRPC> Connections = new Dictionary<Type, IConnectionRPC>();
    }

    public class ConnectionClient : Connection
    {
        protected override INetwork NetworkConnection
        {
            get { return this.Network; }
        }

        public INetworkClient Network { get; set; }

        public bool IsConnected { get; private set; }

        public bool Connect(string ip, uint port)
        {
            if (null == this.Network)
                return false;

            this.Network.OnConnected    += this.OnConnected;
            this.Network.OnRecv         += this.OnRecv;
            this.Network.OnDisconnected += this.OnDisconnected;
            this.Network.OnError        += this.OnError;
            this.Network.Connect(ip, port);

            return true;
        }

        protected void OnConnected(INetwork rClient)
        {
            this.IsConnected = true;
        }
        protected void OnDisconnected(INetwork rClient)
        {
            this.IsConnected = false;
        }
        protected void OnRecv(INetwork rClient, byte[] rBuffer, uint nSize)
        {

        }
        protected void OnError(INetwork rClient, string err)
        {

        }
    }
    public class ConnectionServer : Connection
    {
        protected override INetwork NetworkConnection
        {
            get { return this.Network; }
        }
        public INetworkServer Network { get; set; }

        public bool Initialize(uint port)
        {
            if (null == this.Network)
                return false;

            this.Network.OnConnected    += this.OnConnected;
            this.Network.OnRecv         += this.OnRecv;
            this.Network.OnDisconnected += this.OnDisconnected;
            this.Network.OnError        += this.OnError;
            this.Network.Start(port);

            return true;
        }

        protected void OnConnected(INetwork rClient)
        {

        }
        protected void OnDisconnected(INetwork rClient)
        {

        }
        protected void OnRecv(INetwork rClient, byte[] rBuffer, uint nSize)
        {

        }
        protected void OnError(INetwork rClient, string err)
        {

        }

    }
}

public interface IAccount : RPC.IConnectionRPC
{
    void Print(string value);
}

public class ClientIAccountProxy : IAccount
{
    public ClientIAccountProxy(INetworkClient rClient)
    {
        this.Network = rClient;
    }

    public void Print(string value)
    {
        var rMemoryStream   = new MemoryStream();
        var rWriter         = new BinaryWriter(rMemoryStream);
        rWriter.Write("IAccount.Print");
        rWriter.Write(value);

        var rBuffer = rMemoryStream.ToArray();
        this.Network.Send(rBuffer, (uint)rBuffer.Length);
    }

    protected INetworkClient Network;
}

public class ServerImplementIAccount : IAccount
{
    public void Print(string value)
    {
        Log("this way!");
    }
}

namespace BlackHoleRPC
{
    class Program
    {
        static SimulationNetwork    SimNetwork = new SimulationNetwork() { ImmediateMode = true };
        static RPC.ConnectionClient RPCClient = null;
        static RPC.ConnectionServer RPCServer = null;

        static void Initialize()
        {
            ImplementContainer.ImplementDefault<ILogger, ConsoleLogger>();

            RPCClient = new RPC.ConnectionClient() {
                Network = SimNetwork.CreateClient()
            };
            RPCServer = new RPC.ConnectionServer() {
                Network = SimNetwork.GetOrCreateServer("127.0.0.1", 4000)
            };

            RPCClient.AddImplement<IAccount, ClientIAccountProxy>();
        }
        static void Main(string[] args)
        {
            Initialize();


        }
    }
}
