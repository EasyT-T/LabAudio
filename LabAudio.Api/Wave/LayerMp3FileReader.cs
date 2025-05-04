namespace LabAudio.Api.Wave;

using System.IO;
using NAudio.Wave;
using NLayer.NAudioSupport;

public class LayerMp3FileReader : Mp3FileReaderBase
{
    public LayerMp3FileReader(string mp3FileName) : base(mp3FileName, CreateFrameDecompressor)
    {
    }

    public LayerMp3FileReader(Stream inputStream) : base(inputStream, CreateFrameDecompressor)
    {
    }

    protected LayerMp3FileReader(Stream inputStream, bool ownInputStream) : base(inputStream, CreateFrameDecompressor, ownInputStream)
    {
    }

    private static IMp3FrameDecompressor CreateFrameDecompressor(WaveFormat mp3Format)
    {
        return new Mp3FrameDecompressor(mp3Format);
    }
}