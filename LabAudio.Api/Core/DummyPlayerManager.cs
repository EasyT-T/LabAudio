namespace LabAudio.Api.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using CentralAuth;
using JetBrains.Annotations;
using LabAudio.Api.Components;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

public class DummyPlayerManager
{
    public static DummyPlayerManager Singleton { get; } = new DummyPlayerManager();
    private static int dummyIdReduction = 2 * byte.MaxValue;

    [PublicAPI]
    public Dictionary<ReferenceHub, List<DummyPlayer>> Dummies { get; } = [];

    private readonly HashSet<ReferenceHub> managedHubs = [];

    private DummyPlayerManager()
    {
    }

    private static DummyPlayerComponent CreateDummy(string nickname)
    {
        var obj = Object.Instantiate(LiteNetLib4MirrorNetworkManager.singleton.playerPrefab);
        var hub = obj.GetComponent<ReferenceHub>();

        ReferenceHub.AllHubs.Remove(hub);
        ReferenceHub.HubsByGameObjects.Remove(obj);

        hub.netIdentity.isClient = true;
        hub.netIdentity.isServer = true;
        hub.netIdentity.connectionToClient = new DummyConnection();
        hub.netIdentity.netId = NetworkIdentity.GetNextNetworkId();

        hub.Network_playerId.Destroy();
        hub.Network_playerId = new RecyclablePlayerId(dummyIdReduction--);
        hub.authManager.NetworkSyncedUserId = PlayerAuthenticationManager.DedicatedId;
        hub.nicknameSync.Network_myNickSync = nickname;

        var component = obj.AddComponent<DummyPlayerComponent>();

        return component;
    }

    public DummyPlayer Spawn(string nickname)
    {
        if (!ReferenceHub.TryGetHostHub(out var hub))
        {
            throw new InvalidOperationException("Host hub not found");
        }

        return this.Spawn(hub, nickname);
    }

    public DummyPlayer Spawn(ReferenceHub observer, string nickname)
    {
        if (!this.managedHubs.Contains(observer))
        {
            observer.gameObject.AddComponent<PlayerComponent>();
            this.managedHubs.Add(observer);
        }

        var component = CreateDummy(nickname);
        component.SetObserver(observer);

        var dummy = new DummyPlayer(observer, component);
        dummy.Spawn();

        if (!this.Dummies.TryGetValue(observer, out var dummies))
        {
            dummies = [];
            this.Dummies.Add(observer, dummies);
        }

        dummies.Add(dummy);
        return dummy;
    }

    public void Return(GameObject gameObject)
    {
        var dummy = this.Dummies.Values.SelectMany(d => d).ToList().FirstOrDefault(x => x.GameObject == gameObject);

        if (dummy != null)
        {
            this.Return(dummy);
        }
    }

    public void Return(DummyPlayer dummy)
    {
        dummy.Destroy();
        this.Dummies[dummy.Observer].Remove(dummy);
    }

    internal void ReturnPlayerDummies(ReferenceHub referenceHub)
    {
        if (!this.Dummies.TryGetValue(referenceHub, out var dummies))
        {
            return;
        }

        foreach (var dummyPlayer in dummies)
        {
            dummyPlayer.Destroy();
        }

        this.Dummies.Remove(referenceHub);
    }
}