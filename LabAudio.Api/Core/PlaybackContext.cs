namespace LabAudio.Api.Core;

using System;
using LabAudio.Api.Components;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using VoiceChat;

public class PlaybackContext : IDisposable
{
    public ReferenceHub Observer { get; }

    public bool Paused { get; private set; }
    public bool Stopped { get; private set; }

    public int Volume { get; set; }
    public bool Loop { get; set; }

    public TimeSpan TotalTime { get; }
    public TimeSpan CurrentTime => this.Stream.CurrentTime;

    internal WaveStream Stream { get; }
    internal ISampleProvider SampleProvider { get; }
    internal AudioPlayerComponent AudioPlayer { get; }

    private readonly Action<PlaybackContext>? onStopped;

    public PlaybackContext(ReferenceHub observer, AudioPlayerComponent audioPlayer, WaveStream stream, Action<PlaybackContext>? onStopped = null)
    {
        this.Observer = observer;
        this.Stream = stream;
        this.AudioPlayer = audioPlayer;

        this.SampleProvider = stream.ToSampleProvider();

        if (stream.WaveFormat.Channels != VoiceChatSettings.Channels)
        {
            this.SampleProvider = new StereoToMonoSampleProvider(this.SampleProvider);
        }

        if (stream.WaveFormat.SampleRate != VoiceChatSettings.SampleRate)
        {
            this.SampleProvider = new WdlResamplingSampleProvider(this.SampleProvider, VoiceChatSettings.SampleRate);
        }

        this.TotalTime = stream.TotalTime;
        this.onStopped = onStopped;
    }

    public void SetTime(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        if (time > this.TotalTime)
        {
            time = this.TotalTime;
        }

        this.Stream.CurrentTime = time;
    }

    public void Resume()
    {
        if (this.Stopped)
        {
            return;
        }

        this.Paused = false;
    }

    public void Pause()
    {
        this.Paused = true;
    }

    public void Stop()
    {
        if (this.Stopped)
        {
            return;
        }

        this.Stopped = true;
        this.Paused = true;

        this.Dispose();

        this.onStopped?.Invoke(this);
    }

    public void Dispose()
    {
        this.Stream.Dispose();

        GC.SuppressFinalize(this);
    }

    internal void ForceDestroy()
    {
        if (this.Stopped)
        {
            return;
        }

        this.Stopped = true;
        this.Paused = true;

        this.Dispose();
    }
}