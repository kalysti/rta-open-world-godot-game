using Godot;
using System;
using System.Collections.Generic;

public abstract class BackgroundLoaderBaseItem : Node
{


    public ResourceInteractiveLoader loader = null;

    public string path = null;
    public Resource resource = null;

    public BackgroundLoaderBaseItem(string _path)
    {
        path = _path;
    }
    public abstract void CallOnLoaderComplete();
    public abstract void CallOnLoaderUpdate(float progress);
    public abstract void CallOnLoaderError();
 
}