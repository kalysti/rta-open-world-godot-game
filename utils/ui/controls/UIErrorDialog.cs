using Godot;
using System;

public class UIErrorDialog : AcceptDialog
{
    [Signal]
    public delegate void onConfirm(bool result);

    public bool confirmed = false;

    public override void _Ready()
    {
        Connect("confirmed", this, "onCofirm");

        GetCloseButton().Connect("pressed", this, "onCancel");

        confirmed = false;
    }

    public void onCofirm()
    {
        confirmed = true;
        EmitSignal(nameof(onConfirm), true);

    }
    public void onCancel()
    {

        confirmed = false;
        EmitSignal(nameof(onConfirm), false);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
