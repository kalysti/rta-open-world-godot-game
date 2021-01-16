using Godot;
using System;

[Tool]
public class UIAcceptDialog : ConfirmationDialog
{

    [Signal]
    public delegate void onAccept(bool result);

    public bool result = false;

    public override void _Ready()
    {
        Connect("confirmed", this, "onCofirm");

        GetCancel().Connect("pressed", this, "onCancel");
        GetCloseButton().Connect("pressed", this, "onCancel");

        result = false;
    }

    public void onCofirm()
    {
        result = true;
        EmitSignal(nameof(onAccept), true);

    }
    public void onCancel()
    {

        result = false;
        EmitSignal(nameof(onAccept), false);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
