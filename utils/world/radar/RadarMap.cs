using Godot;
using System;
using Game;
public class RadarMap : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    [Export]
    public NodePath myPosPath;

    private Sprite myPos = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var test = GetNode(myPosPath);
        myPos = (Sprite)GetNode(myPosPath);

        //reset to default
        (FindNode("radar") as Control).AnchorLeft = 0f;
        (FindNode("radar") as Control).AnchorTop = 0f;

        (FindNode("radar") as Control).AnchorRight = 1.0f;
        (FindNode("radar") as Control).AnchorBottom = 1.0f;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        var player = (GetParent().GetParent() as Player);
        var vec2 = (FindNode("3d_camera") as Camera).UnprojectPosition(player.GetPlayerPosition());
        if (myPos != null)
        {
            myPos.Position = vec2;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton && Visible)
        {
            InputEventMouseButton emb = (InputEventMouseButton)@event;
            if (emb.IsPressed())
            {
                var scale = (FindNode("radar") as Control).RectScale;
                if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                {
                    scale.x += 0.1f;
                    scale.y += 0.1f;
                }
                if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                {
                    scale.x -= 0.1f;
                    scale.y -= 0.1f;
                }

                scale.x = Mathf.Clamp(scale.x, 1.0f, 5.0f);
                scale.y = Mathf.Clamp(scale.y, 1.0f, 5.0f);
                
                (FindNode("radar") as Control).RectPivotOffset = (FindNode("radar") as Control).RectSize / 2;
                (FindNode("radar") as Control).RectScale = scale;
            }
        }
    }

}
