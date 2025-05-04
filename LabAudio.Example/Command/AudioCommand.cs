namespace LabAudio.Example.Command;

using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;
using LabAudio.Api.Core;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AudioCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var filename = arguments.At(0);
        AudioPlayerManager.Singleton.Play(Player.Get(sender).ReferenceHub, filename);

        response = "Done!";
        return true;
    }

    public string Command => "audio";
    public string[] Aliases => [];
    public string Description => "Play a sound.";
}