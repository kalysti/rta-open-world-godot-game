using Godot;
using System;
using Game;

public class CharSlider : Label
{
    public Color[] vertex_groups = null;

    [Signal]
    public delegate void change_morph(string text, float value);
    

    public void SetSlider(double value)
    {
        (GetNode("HSlider") as Slider).Value = value;
    }

    private void _on_HSlider_value_changed(float value)
    {
        EmitSignal(nameof(change_morph), Text, value);
    }

    /*
    extends Label
signal change_morph(text,value)
export var vertex_groups = PoolColorArray()

func _on_HSlider_value_changed(value):
	emit_signal("change_morph",text,value)

    */
}
