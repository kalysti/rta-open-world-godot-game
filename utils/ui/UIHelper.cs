using Godot;
using System;
using System.Threading.Tasks;

public class UIHelper : Node
{
    public override void _Ready()
    {
        Helper = this;
    }
    public void getTheme()
    {
        var uiAcceptScene = (PackedScene)ResourceLoader.Load("res://utils/ui/controls/UIAcceptDialog.tscn");
    }
    public async Task<bool> AcceptDialog(string title, string message)
    {
        var uiAcceptScene = (PackedScene)ResourceLoader.Load("res://utils/ui/controls/UIAcceptDialog.tscn");
        UIAcceptDialog dialog = (UIAcceptDialog)uiAcceptScene.Instance();

        AddChild(dialog);
        dialog.WindowTitle = title;
        dialog.DialogText = message;
        dialog.PopupCentered();

        await ToSignal(dialog, "onAccept");

        RemoveChild(dialog);

        return dialog.result;
    }
    public async Task<bool> ErrorDialog(string title, string message)
    {
        var uiAcceptScene = (PackedScene)ResourceLoader.Load("res://utils/ui/controls/UIErrorDialog.tscn");
        UIErrorDialog dialog = (UIErrorDialog)uiAcceptScene.Instance();

        AddChild(dialog);
        dialog.WindowTitle = title;
        dialog.DialogText = message;
        dialog.PopupCentered();

        await ToSignal(dialog, "onAccept");

        RemoveChild(dialog);

        return dialog.confirmed;
    }

    public static UIHelper Helper { get; set; }
}
