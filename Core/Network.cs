using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static BlackHole.Core.DefaultLogger;

namespace BlackHole.Core
{
    public delegate void NetworkOnRecv(INetwork rNetwork, byte[] rBuffer, uint nSize);
    public delegate void NetworkOnConnected(INetwork rNetwork);
    public delegate void NetworkOnDisconnected(INetwork rNetwork);
    public delegate void NetworkOnError(INetwork rNetwork, string err);

    public interface INetwork
    {
        event NetworkOnRecv         OnRecv;
        event NetworkOnConnected    OnConnected;
        event NetworkOnDisconnected OnDisconnected;
        event NetworkOnError        OnError;
    }
    public interface INetworkClient : INetwork
    {
        void Connect(string ip, uint port);

        void Send(byte[] rBuffer, uint nSize);
    }
    public interface INetworkServer : INetwork
    {
        void Start(uint port);

        void SendTo(INetwork rClient, byte[] rBuffer, uint nSize);
    }

    public class SimulationNetwork
    {
        public bool ImmediateMode { get; set; }

        public INetworkClient CreateClient()
        {
            return new SimulationClient() { Network = this };
        }
        public INetworkServer GetOrCreateServer(string ip, uint port)
        {
            var rServerKey = ip + port.ToString();

            if (!this.Servers.TryGetValue(rServerKey, out SimulationServer rServer))
            {
                rServer = new SimulationServer() { Network = this, IP = ip, Port = port };
                this.Servers.Add(rServerKey, rServer);
            }

            return rServer;
        }

        public void Update()
        {
            foreach (var rPostAction in this.PostProcess)
                rPostAction();
            this.PostProcess.Clear();
        }

        public void AddPostAction(Action rPostAction)
        {
            if (!this.ImmediateMode)
                this.PostProcess.Add(rPostAction);
            else
                rPostAction();
        }

        protected List<Action> PostProcess = new List<Action>();
        protected Dictionary<string, SimulationServer> Servers = new Dictionary<string, SimulationServer>();


        protected class SimulationClient : INetworkClient
        {
            public SimulationNetwork Network { get; set; }

            public event NetworkOnRecv OnRecv;
            public event NetworkOnConnected OnConnected;
            public event NetworkOnDisconnected OnDisconnected;
            public event NetworkOnError OnError;

            public void DoOnRecv(INetwork rNetwork, byte[] rBuffer, uint nSize)
            {
                if (this.OnRecv != null)
                    this.OnRecv(rNetwork, rBuffer, nSize);
            }
            public void DoOnConnected(INetwork rNetwork)
            {
                if (this.OnConnected != null)
                    this.OnConnected(rNetwork);

                Log("Client(Server:{0}:{1}) Connected！", this.BindServer.IP, this.BindServer.Port);
            }
            public void DoOnDisconnected(INetwork rNetwork)
            {
                if (this.OnDisconnected != null)
                    this.OnDisconnected(rNetwork);
            }
            public void DoOnError(INetwork rNetwork, string err)
            {
                if (this.OnError != null)
                    this.OnError(rNetwork, err);
            }

            public void Connect(string ip, uint port)
            {
                this.BindServer = (SimulationServer)this.Network.GetOrCreateServer(ip, port);

                this.Network.AddPostAction(() => {
                    this.BindServer.DoOnConnected(this);
                });
            }

            public void Send(byte[] rBuffer, uint nSize)
            {
                this.Network.AddPostAction(() => {
                    this.BindServer.DoOnRecv(this, rBuffer, nSize);
                });
            }

            protected SimulationServer BindServer;
        }
        protected class SimulationServer : INetworkServer
        {
            public SimulationNetwork Network { get; set; }
            public string IP { get; set; }
            public uint Port { get; set; }


            public event NetworkOnRecv OnRecv;
            public event NetworkOnConnected OnConnected;
            public event NetworkOnDisconnected OnDisconnected;
            public event NetworkOnError OnError;

            public void DoOnRecv(INetwork rNetwork, byte[] rBuffer, uint nSize)
            {
                if (this.OnRecv != null)
                    this.OnRecv(rNetwork, rBuffer, nSize);

                Log("Server OnRecv！");
            }
            public void DoOnConnected(INetwork rNetwork)
            {
                if (this.OnConnected != null)
                    this.OnConnected(rNetwork);

                Log("Server OnConnected！");
            }
            public void DoOnDisconnected(INetwork rNetwork)
            {
                if (this.OnDisconnected != null)
                    this.OnDisconnected(rNetwork);

                Log("Server OnDisconnected！");
            }
            public void DoOnError(INetwork rNetwork, string err)
            {
                if (this.OnError != null)
                    this.OnError(rNetwork, err);
            }

            public void SendTo(INetwork rClient, byte[] rBuffer, uint nSize)
            {
                var rSimulationClient = (SimulationClient)rClient;

                this.Network.AddPostAction(() => {
                    rSimulationClient.DoOnRecv(rSimulationClient, rBuffer, nSize);
                });
            }

            public void Start(uint port)
            {
            }
        }
    }
}