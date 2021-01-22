using Godot;
using System;

public class KeyButton : HBoxContainer
{

	public string key = null;
	public string label = null;
	protected Label labelNode = null;
	protected Label keysNode = null;
	protected Button buttonNode = null;
	public ButtonDialog buttonDialog = null;	
	protected UserSettings userSettings = null;

	public override void _Ready()
	{
		labelNode = GetNode("label") as Label;
		keysNode = GetNode("keys") as Label;
		buttonNode = GetNode("button") as Button;

		buttonDialog.Connect("onNewKey", this, "onConfirm");
		userSettings = GetTree().Root.GetNode("UserSettings") as UserSettings;

		updateLabels();
	}

	public void updateLabels()
	{
		Godot.Collections.Array<string> list = new Godot.Collections.Array<string>();

		foreach (var n in InputMap.GetActionList(key))
		{
			if (n is InputEventKey)
			{
				var t = (InputEventKey)n;
				var readFormat = OS.GetScancodeString(t.GetScancodeWithModifiers());

				list.Add(readFormat);
			}
			else if (n is InputEventMouseButton)
			{
				var t = (InputEventMouseButton)n;
				var buttonName = (ButtonList)t.ButtonIndex;

				list.Add(buttonName.ToString());
			}
		}

		var labelText = string.Join(" ,", list);
		labelNode.Text = label;
		keysNode.Text = labelText;
	}

	private void onConfirm(string inputKey, InputEvent _event)
	{
		if (inputKey == key)
		{
			foreach (var ev in InputMap.GetActionList(key))
			{
				InputMap.ActionEraseEvent(inputKey, ev as InputEvent);
			}

			InputMap.ActionAddEvent(inputKey, _event);

			if (_event is InputEventKey)
			{
				var t = (InputEventKey) _event;
				userSettings.setConfigKey("keys", key, "key:"+ t.Scancode);
			}
			else if (_event is InputEventMouseButton)
			{
				var t = (InputEventMouseButton) _event;
				userSettings.setConfigKey("keys", key, "button:"+ t.ButtonIndex);
			}

			updateLabels();
		}
	}
	private void _on_button_pressed()
	{
		buttonDialog.key = key;
		buttonDialog.PopupCentered();
	}
}


