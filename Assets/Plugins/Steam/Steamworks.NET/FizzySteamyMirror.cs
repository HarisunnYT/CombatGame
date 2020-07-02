using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class FizzySteamyMirror : Transport
{
    // events for the client
    public event Action OnClientConnect;
    public event Action<byte[]> OnClientData;
    public event Action<Exception> OnClientError;
    public event Action OnClientDisconnect;

    // events for the server
    public event Action<int> OnServerConnect;
    public event Action<int, byte[]> OnServerData;
    public event Action<int, Exception> OnServerError;
    public event Action<int> OnServerDisconnect;

    protected FizzySteam.Client client = new FizzySteam.Client();
    protected FizzySteam.Server server = new FizzySteam.Server();

    public FizzySteamyMirror()
    {
        // dispatch the events from the server
        server.OnConnected += (id) => OnServerConnect?.Invoke(id);
        server.OnDisconnected += (id) => OnServerDisconnect?.Invoke(id);
        server.OnReceivedData += (id, data) => OnServerData?.Invoke(id, data);
        server.OnReceivedError += (id, exception) => OnServerError?.Invoke(id, exception);

        // dispatch events from the client
        client.OnConnected += () => OnClientConnect?.Invoke();
        client.OnDisconnected += () => OnClientDisconnect?.Invoke();
        client.OnReceivedData += (data) => OnClientData?.Invoke(data);
        client.OnReceivedError += (exception) => OnClientError?.Invoke(exception);

        Debug.Log("FizzySteamyMirror initialized!");
    }

    // client
    public override bool ClientConnected() { return client.Connected; }
    public virtual void ClientConnect(string address, int port) { client.Connect(address); }
    public virtual void ClientSend(int channelId, byte[] data) { client.Send(data, channelId); }
    public override void ClientDisconnect() { client.Disconnect(); }

    // server
    public override bool ServerActive() { return server.Active; }
    public override void ServerStart()
    {
        server.Listen();
    }

    public virtual void ServerStartWebsockets(string address, int port, int maxConnections)
    {
        Debug.LogError("FizzySteamyMirror.ServerStartWebsockets not possible!");
    }

    public virtual void ServerSend(int connectionId, int channelId, byte[] data) { server.Send(connectionId, data, channelId); }

    public override bool ServerDisconnect(int connectionId)
    {
        return server.Disconnect(connectionId);
    }

    public virtual bool GetConnectionInfo(int connectionId, out string address) { return server.GetConnectionInfo(connectionId, out address); }
    public override void ServerStop() { server.Stop(); }

    // common
    public override void Shutdown()
    {
        client.Disconnect();
        server.Stop();
    }

    public override int GetMaxPacketSize(int channelId)
    {
        switch (channelId)
        {
            case Channels.DefaultUnreliable:
                return 1200; //UDP like - MTU size.

            case Channels.DefaultReliable:
                return 1048576; //Reliable message send. Can send up to 1MB of data in a single message.

            default:
                Debug.LogError("Unknown channel so uknown max size");
                return 0;
        }
    }

    public override bool Available()
    {
        throw new NotImplementedException();
    }

    public override void ClientConnect(string address)
    {
        throw new NotImplementedException();
    }

    public override bool ClientSend(int channelId, ArraySegment<byte> segment)
    {
        throw new NotImplementedException();
    }

    public override Uri ServerUri()
    {
        throw new NotImplementedException();
    }

    public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment)
    {
        throw new NotImplementedException();
    }

    public override string ServerGetClientAddress(int connectionId)
    {
        throw new NotImplementedException();
    }
}