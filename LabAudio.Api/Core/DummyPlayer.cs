namespace LabAudio.Api.Core;

using LabAudio.Api.Components;
using Mirror;
using UnityEngine;

public class DummyPlayer
{
    public ReferenceHub Observer { get; }
    public GameObject GameObject { get; }
    public DummyPlayerComponent DummyComponent { get; }
    public ReferenceHub DummyHub { get; }
    public bool Destroyed { get; private set; }

    internal DummyPlayer(ReferenceHub target, DummyPlayerComponent dummyComponent)
    {
        this.Observer = target;
        this.GameObject = dummyComponent.gameObject;
        this.DummyComponent = dummyComponent;
        this.DummyHub = dummyComponent.gameObject.GetComponent<ReferenceHub>();
    }

    internal void Spawn()
    {
        this.Observer.connectionToClient.AddToObserving(this.DummyHub.netIdentity);
    }

    internal void Despawn()
    {
        this.DummyHub.netIdentity.ClearObservers();
        this.DummyHub.netIdentity.connectionToClient.RemoveOwnedObject(this.DummyHub.netIdentity);
        this.Observer.connectionToClient.Send(new ObjectDestroyMessage
            { netId = this.DummyHub.networkIdentity.netId });
    }

    internal void Destroy()
    {
        this.DummyHub.Network_playerId = new RecyclablePlayerId(0);
        this.Despawn();
        this.DummyHub.connectionToClient.Disconnect();
        Object.Destroy(this.DummyComponent.gameObject);
        Object.DestroyImmediate(this.DummyComponent.gameObject);

        this.Destroyed = true;
    }
}