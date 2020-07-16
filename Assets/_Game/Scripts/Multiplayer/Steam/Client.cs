using Steamworks;
using Steamworks.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public class Client : Common
    {
        public bool Connected { get; private set; }
        public bool Error { get; private set; }

        private event Action<byte[], int> OnReceivedData;
        private event Action OnConnected;
        private event Action OnDisconnected;

        private TimeSpan ConnectionTimeout;

        private SteamId hostSteamID = new SteamId();
        private TaskCompletionSource<Task> connectedComplete;
        private CancellationTokenSource cancelToken;

        private Client(FizzySteamworks transport) : base(transport)
        {
            ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
        }

        public static Client CreateClient(FizzySteamworks transport, string host)
        {
            Client c = new Client(transport);

            c.OnConnected += () => transport.OnClientConnected.Invoke();
            c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
            c.OnReceivedData += (data, channel) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), channel);

            if (SteamManager.Initialized)
            {
                c.Connect(host);
            }
            else
            {
                Debug.LogError("SteamWorks not initialized");
                c.OnConnectionFailed(new SteamId());
            }

            return c;
        }

        private async void Connect(string host)
        {
            cancelToken = new CancellationTokenSource();

            try
            {
                hostSteamID = new SteamId() { Value = UInt64.Parse(host) };
                connectedComplete = new TaskCompletionSource<Task>();

                OnConnected += SetConnectedComplete;

                SendInternal(hostSteamID, InternalMessages.CONNECT);

                Task connectedCompleteTask = connectedComplete.Task;

                if (await Task.WhenAny(connectedCompleteTask, Task.Delay(ConnectionTimeout, cancelToken.Token)) != connectedCompleteTask)
                {
                    Debug.LogError($"Connection to {host} timed out.");
                    OnConnected -= SetConnectedComplete;
                    OnConnectionFailed(hostSteamID);
                    NetworkManager.singleton.TimedOut();
                }

                OnConnected -= SetConnectedComplete;
            }
            catch (FormatException)
            {
                Debug.LogError($"Connection string was not in the right format. Did you enter a SteamId?");
                Error = true;
                OnConnectionFailed(hostSteamID);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Error = true;
                OnConnectionFailed(hostSteamID);
            }
            finally
            {
                if (Error)
                {
                    OnConnectionFailed(new SteamId());
                }
            }

        }

        public void Disconnect()
        {
            Debug.Log("Sending Disconnect message");
            SendInternal(hostSteamID, InternalMessages.DISCONNECT);
            Dispose();
            cancelToken?.Cancel();

            WaitForClose(hostSteamID);
        }

        private void SetConnectedComplete() => connectedComplete.SetResult(connectedComplete.Task);

        protected override void OnReceiveData(byte[] data, SteamId clientSteamID, int channel)
        {
            if (clientSteamID != hostSteamID)
            {
                Debug.LogError("Received a message from an unknown");
                return;
            }

            OnReceivedData.Invoke(data, channel);
        }

        protected override void OnNewConnection(SteamId resultID)
        {
            if (hostSteamID == resultID)
            {
                SteamNetworking.AcceptP2PSessionWithUser(resultID);
            }
            else
            {
                Debug.LogError("P2P Acceptance Request from unknown host ID.");
            }
        }

        protected override void OnReceiveInternalData(InternalMessages type, SteamId clientSteamID)
        {
            switch (type)
            {
                case InternalMessages.ACCEPT_CONNECT:
                    Connected = true;
                    OnConnected.Invoke();
                    Debug.Log("Connection established.");
                    break;
                case InternalMessages.DISCONNECT:
                    Connected = false;
                    Debug.Log("Disconnected.");
                    OnDisconnected.Invoke();
                    break;
                default:
                    Debug.Log("Received unknown message type");
                    break;
            }
        }

        public bool Send(byte[] data, int channelId) => Send(hostSteamID, data, channelId);

        protected override void OnConnectionFailed(SteamId remoteId) => OnDisconnected.Invoke();
    }
}