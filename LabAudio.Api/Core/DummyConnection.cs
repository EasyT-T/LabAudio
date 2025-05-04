namespace LabAudio.Api.Core;

using System;
using Mirror;

public class DummyConnection() : NetworkConnectionToClient(0)
{
    public override string address => "localhost";

    public override void Send(ArraySegment<byte> segment, int channelId = 0)
    {
    }

    public override void Disconnect()
    {
    }

    public override void SendToTransport(ArraySegment<byte> segment, int channelId = 0)
    {
    }

    public override void UpdatePing()
    {
    }
}