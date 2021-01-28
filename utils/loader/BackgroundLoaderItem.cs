using Godot;
using System;
using System.Collections.Generic;
using Game;

public class BackgroundLoaderItem : BackgroundLoaderBaseItem
{

    [Signal]
    public delegate void CompleteLoadEvent(Resource value);

    [Signal]
    public delegate void LoaderUpdateEvent(float progress);

    [Signal]
    public delegate void LoaderErrorEvent();



    public BackgroundLoaderItem(string _path) : base(_path)
    {
    }

    public override void CallOnLoaderComplete()
    {
        EmitSignal("CompleteLoadEvent", resource);
    }

    public override void CallOnLoaderUpdate(float progress)
    {
        EmitSignal("LoaderUpdateEvent", progress);
    }
    public override void CallOnLoaderError()
    {
        EmitSignal("LoaderErrorEvent");
    }
}