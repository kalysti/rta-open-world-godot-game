
#if TOOLS

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[Tool]
public class CustomFootstepPlugin : EditorPlugin
{
    
    public override void _EnterTree()
    {

        var script = GD.Load<Script>("res://addons/footsteps/FootstepStaticBody.cs");
        var texture = GD.Load<Texture>("res://addons/footsteps/icons/footstep.png");

        AddCustomType("FootstepStaticBody", "StaticBody", script, texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("FootstepStaticBody");
    }
}
#endif