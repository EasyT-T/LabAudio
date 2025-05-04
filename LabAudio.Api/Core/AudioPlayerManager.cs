namespace LabAudio.Api.Core;

using System.Collections.Generic;
using LabAudio.Api.Components;
using LabAudio.Api.Factory;

public class AudioPlayerManager
{
    public static AudioPlayerManager Singleton { get; } = new AudioPlayerManager();
    public uint MaxIdleDummies { get; set; } = 3;

    private readonly Dictionary<ReferenceHub, Queue<AudioPlayerComponent>> idlePlayers = [];

    public PlaybackContext Play(ReferenceHub player, string filename, PlaybackProperty? property = null)
    {
        property ??= PlaybackProperty.Default;

        if (!this.idlePlayers.TryGetValue(player, out var idles))
        {
            idles = new Queue<AudioPlayerComponent>();
            this.idlePlayers.Add(player, idles);
        }

        if (!idles.TryDequeue(out var audioPlayer))
        {
            var dummyPlayer = DummyPlayerManager.Singleton.Spawn(player, "Audio Player");
            audioPlayer = dummyPlayer.GameObject.AddComponent<AudioPlayerComponent>();
        }

        var stream = AudioFileReaderFactory.Singleton.CreateWaveStream(filename);
        var context = new PlaybackContext(player, audioPlayer, stream, this.OnContextStopped);
        context.Volume = property.Volume;
        context.Loop = property.Loop;

        audioPlayer.SetContext(context);

        return context;
    }

    internal void OnContextStopped(PlaybackContext context)
    {
        if (this.idlePlayers[context.Observer].Count < this.MaxIdleDummies)
        {
            this.idlePlayers[context.Observer].Enqueue(context.AudioPlayer);
        }
        else
        {
            DummyPlayerManager.Singleton.Return(context.AudioPlayer.gameObject);
        }
    }
}