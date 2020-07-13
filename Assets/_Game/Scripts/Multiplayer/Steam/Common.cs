using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public abstract class Common
    {
        private P2PSend[] channels;
        private int internal_ch => channels.Length;

        protected enum InternalMessages : byte
        {
            CONNECT,
            ACCEPT_CONNECT,
            DISCONNECT
        }

        protected readonly FizzySteamworks transport;

        protected Common(FizzySteamworks transport)
        {
            channels = transport.Channels;

            SteamNetworking.OnP2PConnectionFailed = OnConnectFail;
            SteamNetworking.OnP2PSessionRequest = OnNewConnection;

            this.transport = transport;
        }

        protected void Dispose()
        {
            SteamNetworking.OnP2PConnectionFailed = null;
            SteamNetworking.OnP2PSessionRequest = null;
        }

        protected abstract void OnNewConnection(SteamId steamId);

        private void OnConnectFail(SteamId id, P2PSessionError error)
        {
            OnConnectionFailed(id);
            CloseP2PSessionWithUser(id);

            switch ((int)error)
            {
                case 1:
                    throw new Exception("Connection failed: The target user is not running the same game.");
                case 2:
                    throw new Exception("Connection failed: The local user doesn't own the app that is running.");
                case 3:
                    throw new Exception("Connection failed: Target user isn't connected to Steam.");
                case 4:
                    throw new Exception("Connection failed: The connection timed out because the target user didn't respond.");
                default:
                    throw new Exception("Connection failed: Unknown error.");
            }
        }

        protected void SendInternal(SteamId target, InternalMessages type) => SteamNetworking.SendP2PPacket(target, new byte[] { (byte)type }, 1, internal_ch, P2PSend.Reliable);
        protected bool Send(SteamId host, byte[] msgBuffer, int channel)
        {
            return SteamNetworking.SendP2PPacket(host, msgBuffer, msgBuffer.Length, channel, channels[channel]);
        }
        private bool Receive(out SteamId clientSteamID, out byte[] receiveBuffer, int channel)
        {
            if (SteamNetworking.IsP2PPacketAvailable(channel))
            {
                P2Packet? packet = SteamNetworking.ReadP2PPacket(channel);

                if (packet.HasValue)
                {
                    receiveBuffer = packet.Value.Data;
                    clientSteamID = packet.Value.SteamId;

                    return true;
                }
            }

            receiveBuffer = null;
            clientSteamID = new SteamId();
            return false;
        }

        protected void CloseP2PSessionWithUser(SteamId clientSteamID) => SteamNetworking.CloseP2PSessionWithUser(clientSteamID);

        protected void WaitForClose(SteamId SteamId) => transport.StartCoroutine(DelayedClose(SteamId));
        private IEnumerator DelayedClose(SteamId SteamId)
        {
            yield return null;
            CloseP2PSessionWithUser(SteamId);
        }

        public void ReceiveData()
        {
            try
            {
                while (Receive(out SteamId clientSteamID, out byte[] internalMessage, internal_ch))
                {
                    if (internalMessage.Length == 1)
                    {
                        OnReceiveInternalData((InternalMessages)internalMessage[0], clientSteamID);
                        return; // Wait one frame
                    }
                    else
                    {
                        Debug.Log("Incorrect package length on internal channel.");
                    }
                }

                for (int chNum = 0; chNum < channels.Length; chNum++)
                {
                    while (Receive(out SteamId clientSteamID, out byte[] receiveBuffer, chNum))
                    {
                        OnReceiveData(receiveBuffer, clientSteamID, chNum);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected abstract void OnReceiveInternalData(InternalMessages type, SteamId clientSteamID);
        protected abstract void OnReceiveData(byte[] data, SteamId clientSteamID, int channel);
        protected abstract void OnConnectionFailed(SteamId remoteId);
    }
}