using Godot;
using System;

public class CharEditGlobal : Node
{

    public static Godot.Collections.Dictionary<string, Godot.Collections.Dictionary> meshs_shapes = new Godot.Collections.Dictionary<string, Godot.Collections.Dictionary>();
    // Declare member variables here. Examples:
    // private int a = 2;f
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Load shapes");
        var file = new File();

        file.OpenCompressed("res://char_edit/shapes.dat", File.ModeFlags.Read);

        Godot.Collections.Dictionary shapes = (Godot.Collections.Dictionary) file.GetVar();
        foreach(System.Collections.DictionaryEntry x in shapes)
        {
            var tf = new Godot.Collections.Dictionary();
            
            foreach(System.Collections.DictionaryEntry i in (Godot.Collections.Dictionary) x.Value)
            {
                tf.Add(i.Key, i.Value);
            }

            meshs_shapes.Add(x.Key.ToString(), tf);
          
        }
        file.Close();
    }
}
