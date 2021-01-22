using Godot;
using System;

public class ButtonDialog : AcceptDialog
{
	[Signal]
	public delegate void onNewKey(string _key, InputEvent _event);

	public string  key  = null;
	public   InputEvent incomingEvent = null;
	public override void _Ready()
	{
		DialogText = "Press a key to continue";
	}
	public override void _Input(InputEvent inputEvent)
	{
		if(!Visible)
			return ;

		if(inputEvent is InputEventKey || inputEvent is InputEventMouseButton)
		{
			if(incomingEvent == null)
			{
				incomingEvent = inputEvent;
				updateLabel(inputEvent);
			}
		}
	}

	private void _on_button_dialog_confirmed()
	{
		if(incomingEvent != null)
			EmitSignal(nameof(onNewKey), key, incomingEvent);
	}


	private void _on_button_dialog_draw()
	{
		
		incomingEvent = null;
		DialogText = "Press a key to continue";
	}


	private void updateLabel(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey)
		{
			var t = (InputEventKey)inputEvent;
			var readFormat = OS.GetScancodeString(t.GetScancodeWithModifiers());

			DialogText = readFormat;
		}
		else if (inputEvent is InputEventMouseButton)
		{
			var t = (InputEventMouseButton)inputEvent;
			var buttonName = (ButtonList)t.ButtonIndex;
			
			DialogText = buttonName.ToString();
		}
	}
}






