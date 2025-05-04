namespace LabAudio.Api.Components;

using LabAudio.Api.Core;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    private ReferenceHub hub = null!;

    private void Awake()
    {
        this.hub = this.GetComponent<ReferenceHub>();
    }

    private void OnDestroy()
    {
        DummyPlayerManager.Singleton.ReturnPlayerDummies(this.hub);
    }
}