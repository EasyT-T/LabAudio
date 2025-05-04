namespace LabAudio.Api.Components;

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LabAudio.Api.Core;
using PluginAPI.Core;
using UnityEngine;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

public class AudioPlayerComponent : MonoBehaviour
{
    public bool Activated { get; private set; }

    private ReferenceHub referenceHub = null!;

    private float allowedSamples;

    private PlaybackBuffer playbackBuffer = null!;

    private float[] sendBuffer = null!;
    private ConcurrentBag<Action> actionsQueue = null!;

    private volatile PlaybackContext? context;
    private OpusEncoder encoder = null!;

    public void SetContext(PlaybackContext? newContext)
    {
        var oldContext = Interlocked.Exchange(ref this.context, newContext);
        oldContext?.Stop();

        if (!this.Activated)
        {
            this.Activated = true;
            return;
        }

        this.playbackBuffer.Clear();
    }

    private void Awake()
    {
        this.referenceHub = this.gameObject.GetComponent<ReferenceHub>();
        this.playbackBuffer = new PlaybackBuffer();
        this.sendBuffer = new float[VoiceChatSettings.PacketSizePerChannel * VoiceChatSettings.Channels];
        this.actionsQueue = [];
        this.encoder = new OpusEncoder(OpusApplicationType.Voip);
    }

    private void Update()
    {
        try
        {
            for (var i = 0; i < 100.0f * Time.deltaTime; i++)
            {
                if (!this.actionsQueue.TryTake(out var action))
                {
                    break;
                }

                action.Invoke();
            }

            if (!this.Activated)
            {
                return;
            }


            if (this.context == null || this.context.Paused)
            {
                return;
            }

            if (this.context.Stopped)
            {
                return;
            }

            this.allowedSamples += VoiceChatSettings.SampleRate * VoiceChatSettings.Channels * Time.deltaTime;

            this.Playback();
        }
        catch (Exception e)
        {
            Log.Error("Encountered an error while playing an audio: " + e);
            this.SetContext(null);
        }
    }

    private void OnDestroy()
    {
        this.context?.ForceDestroy();
        this.playbackBuffer.Dispose();
        this.encoder.Dispose();
    }

    private void Playback()
    {
        if (this.context == null || this.context.Stopped || this.context.Paused)
        {
            return;
        }

        if (this.playbackBuffer.Length < this.sendBuffer.Length)
        {
            var cnt = this.context.SampleProvider.Read(this.playbackBuffer.Buffer, 0,
                this.playbackBuffer._bufferSize - this.playbackBuffer.Length);

            if (cnt == 0)
            {
                if (this.context.Loop)
                {
                    this.context.Stream.Seek(0, SeekOrigin.Begin);
                    this.playbackBuffer.Clear();
                }
                else
                {
                    this.actionsQueue.Add(this.context.Stop);
                    this.context = null;
                }

                return;
            }

            if (this.context.Volume != 100)
            {
                var factor = Mathf.Clamp(this.context.Volume, 0, 100) / 100.0f;
                Parallel.For(0, this.playbackBuffer._bufferSize, i => this.playbackBuffer.Buffer[i] *= factor);
            }

            this.playbackBuffer.ReadHead = this.playbackBuffer.HeadToIndex(this.playbackBuffer.ReadHead);
            this.playbackBuffer.WriteHead = cnt + this.playbackBuffer.ReadHead;
        }

        while (this.allowedSamples > this.sendBuffer.Length && this.playbackBuffer.Length >= this.sendBuffer.Length)
        {
            this.playbackBuffer.ReadTo(this.sendBuffer, this.sendBuffer.Length);

            var encodedBuffer = ArrayPool<byte>.Shared.Rent(VoiceChatSettings.MaxEncodedSize);
            var encodedLen = this.encoder.Encode(this.sendBuffer, encodedBuffer);
            this.actionsQueue.Add(() =>
                this.context.Observer.connectionToClient.Send(new VoiceMessage(this.referenceHub,
                    VoiceChatChannel.Proximity, encodedBuffer, encodedLen, false)));

            this.allowedSamples -= this.sendBuffer.Length;
        }
    }
}