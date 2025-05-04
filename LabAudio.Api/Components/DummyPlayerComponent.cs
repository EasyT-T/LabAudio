namespace LabAudio.Api.Components;

using UnityEngine;

public class DummyPlayerComponent : MonoBehaviour
{
    public ReferenceHub? Observer { get; private set; }

    internal void SetObserver(ReferenceHub target)
    {
        this.Observer = target;
    }
}