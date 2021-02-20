using Godot;
using System;
using System.Collections.Generic;

using System.Linq;

[Tool]
public class NewSceneTest : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var orig = GetNode<MeshInstance>("orig");
        var tights = GetNode<MeshInstance>("tights");
        var result = GetNode<MeshInstance>("result");

        var tmpMesh = new ArrayMesh();
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        var tool = new MeshDataTool();
        tool.CreateFromSurface((ArrayMesh)tights.Mesh, 0);

        var tool2 = new MeshDataTool();
        tool2.CreateFromSurface((ArrayMesh)orig.Mesh, 0);

        List<Vector3> vertices = new List<Vector3>();
        for (int v = 0; v < tool2.GetVertexCount(); v++)
        {
            vertices.Add(tool2.GetVertex(v));
        }

        for (int v = 0; v < tool.GetVertexCount(); v++)
        {
            //  surfaceTool.AddNormal(tool.GetVertexNormal(v));
            //  surfaceTool.AddColor(tool.GetVertexColor(v));
            // surfaceTool.AddUv(tool.GetVertexUv(v));
            //  surfaceTool.AddUv2(tool.GetVertexUv2(v));
            //  surfaceTool.AddTangent(tool.GetVertexTangent(v));

            var newVer = tool.GetVertex(v);
            var replace = vertices.OrderBy(df => newVer.DistanceTo(df)).FirstOrDefault();

            if (replace != null && replace != Vector3.Zero  && replace.DistanceTo(newVer) > 0.03f)
            {
                GD.Print("replace" + newVer + " by dist " + replace.DistanceTo(newVer));
                surfaceTool.AddVertex(replace);
            }
            else
                surfaceTool.AddVertex(newVer);
        }

        for (int fc = 0; fc < tool.GetFaceCount(); fc++)
        {
            for (var i = 0; i <= 2; i++)
            {
                var ind = tool.GetFaceVertex(fc, i);
                surfaceTool.AddIndex(ind);
            }
        }


        surfaceTool.Commit(tmpMesh);
        result.Mesh = tmpMesh;
    }


}
