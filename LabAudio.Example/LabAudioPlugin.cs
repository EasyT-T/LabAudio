namespace LabAudio.Example;

using System.Linq;
using Exiled.API.Features;
using LabAudio.Api;
using LabAudio.Api.Core;
using Server = Exiled.Events.Handlers.Server;

public class LabAudioPlugin : Plugin<LabAudioConfig>
{
    public override void OnEnabled()
    {
        CosturaUtil.Initialize();
        Server.RoundStarted += () => Player.List.ToArray().ForEach(x => AudioPlayerManager.Singleton.Play(x.ReferenceHub, "D:\\music.ogg", new PlaybackProperty(10, true)));
    }
}