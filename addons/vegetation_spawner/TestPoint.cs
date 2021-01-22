using Godot;
using System;

[Tool]
public class TestPoint : ImmediateGeometry
{
  
    public override void _Process(float delta)
    {
        Clear();
        Begin(Mesh.PrimitiveType.Triangles, null);
        AddSphere(15, 15, 1f, false);
        End();
    }


}