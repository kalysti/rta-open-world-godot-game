using Godot;
using System;
using System.Collections.Generic;
using Game;

public class BackgroundLoaderObjectItem : BackgroundLoaderBaseItem
{
    public delegate void CompleteLoadEvent(Resource resource, WorldObject worldObject);
    public event CompleteLoadEvent OnLoaderComplete;

    public WorldObject gameObject = null;
    public BackgroundLoaderObjectItem(WorldObject _object) : base(_object.getResourcePath())
    {
        gameObject = _object;
    }
    public override void CallOnLoaderComplete()
    {
        OnLoaderComplete?.Invoke(resource, gameObject);
    }

    public override void CallOnLoaderUpdate(float progress)
    {
        
    }
    public override void CallOnLoaderError()
    {
        
    }
}