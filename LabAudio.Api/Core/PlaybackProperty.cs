namespace LabAudio.Api.Core;

public record PlaybackProperty(int Volume, bool Loop)
{
    public static PlaybackProperty Default { get; } = new PlaybackProperty(100, false);
}