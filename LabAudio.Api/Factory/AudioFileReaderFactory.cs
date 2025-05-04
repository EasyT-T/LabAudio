namespace LabAudio.Api.Factory;

using System;
using System.Collections.Generic;
using System.IO;
using LabAudio.Api.Wave;
using NAudio.Vorbis;
using NAudio.Wave;

public class AudioFileReaderFactory
{
    public static AudioFileReaderFactory Singleton { get; } = new AudioFileReaderFactory();

    public delegate WaveStream WaveStreamCreator(string filename);

    private readonly Dictionary<string, WaveStreamCreator> extendStreamCreators =
        new Dictionary<string, WaveStreamCreator>
        {
            { ".ogg", filename => new VorbisWaveReader(filename) },
            { ".wav", filename => new WaveFileReader(filename) },
            { ".mp3", filename => new LayerMp3FileReader(filename) },
            { ".aiff", filename => new AiffFileReader(filename) },
            { ".aif", filename => new AiffFileReader(filename) },
        };

    private AudioFileReaderFactory()
    {
    }

    public WaveStream CreateWaveStream(string fileName)
    {
        var fileExtension = Path.GetExtension(fileName);

        if (!this.extendStreamCreators.TryGetValue(fileExtension, out var creator))
        {
            throw new NotSupportedException("File extension not support: " + fileExtension);
        }

        return creator(fileName);
    }
}