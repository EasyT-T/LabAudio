namespace LabAudio.Example;

using Exiled.API.Interfaces;

public class LabAudioConfig : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = true;
}