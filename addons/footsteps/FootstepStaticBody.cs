using Godot;
using System;

[Tool]
public class FootstepStaticBody : StaticBody
{
    [Export]
    public FootstepTypes footstepType = FootstepTypes.CONCRETE;
}