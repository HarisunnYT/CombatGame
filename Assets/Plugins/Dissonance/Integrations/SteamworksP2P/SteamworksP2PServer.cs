using System;
using Dissonance.Networking;
using Steamworks;
using Steamworks.Data;

namespace Dissonance.Integrations.SteamworksP2P
{
    public class SteamworksP2PServer
        : BaseServer<SteamworksP2PServer, SteamworksP2PClient, SteamId>
    {
        private readonly SteamworksP2PCommsNetwork _network;

        private byte[] _receiveBuffer = new byte[4096];

        public SteamworksP2PServer(SteamworksP2PCommsNetwork network)
        {
            _network = network;
        }

        protected override void ReadMessages()
        {
            SteamId sender = new SteamId();
            uint size = 0;

            while (SteamNetworking.ReadP2PPacket(_receiveBuffer, ref size, ref sender, _network.P2PPacketChannelToServer))
            {
                NetworkReceivedPacket(sender, new ArraySegment<byte>(_receiveBuffer, 0, _receiveBuffer.Length));
            }
        }

        internal void PeerDisconnected(SteamId conn)
        {
            ClientDisconnected(conn);
        }

        protected override void SendReliable(SteamId connection, ArraySegment<byte> packet)
        {
            if (!_network.Send(connection, packet, P2PSend.Reliable, false))
                FatalError("Steam failed to send P2P Packet");
        }

        protected override void SendUnreliable(SteamId connection, ArraySegment<byte> packet)
        {
            _network.Send(connection, packet, P2PSend.UnreliableNoDelay, false);
        }
    }
}
