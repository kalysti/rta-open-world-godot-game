using Godot;
using System;
public class SettingsMenu : WindowDialog
{
	protected OptionButton windowMode = null;
	protected OptionButton resolution = null;
	protected OptionButton vsync = null;
	protected OptionButton antialising = null;
	protected OptionButton ssao = null;
	protected OptionButton shadow_quality = null;

	protected VBoxContainer controlList = null;
	protected UserSettings userSettings = null;

	protected Godot.Collections.Array<Vector2> usableResolutions = new Godot.Collections.Array<Vector2>();
	public Vector2 highestPossibleResoultion = Vector2.Zero;
	
	SettingsMenu()
	{
	}


	public override void _Ready()
	{
		highestPossibleResoultion = OS.GetScreenSize(OS.CurrentScreen);

		
		userSettings = GetTree().Root.GetNode("UserSettings") as UserSettings;


		windowMode = FindNode("window_mode", true, false) as OptionButton;
		resolution = FindNode("resolution", true, false) as OptionButton;
		vsync = FindNode("vsync", true, false) as OptionButton;
		antialising = FindNode("anti_alising", true, false) as OptionButton;
		ssao = FindNode("ssao", true, false) as OptionButton;
		shadow_quality = FindNode("shadow_quality", true, false) as OptionButton;
		controlList = FindNode("control_list", true, false) as VBoxContainer;


		vsync.AddItem("Disabled", 0);
		vsync.AddItem("Enabled", 1);

		ssao.AddItem("Disabled", 0);
		ssao.AddItem("Enabled", 1);

		windowMode.AddItem("Window mode", 0);
		windowMode.AddItem("Borderless mode", 1);
		windowMode.AddItem("Fullscreen mode", 2);

		antialising.AddItem("Disabled", 0);
		antialising.AddItem("2x", 1);
		antialising.AddItem("4x", 2);
		antialising.AddItem("8x", 3);
		antialising.AddItem("16x", 4);

		shadow_quality.AddItem("Low", 0);
		shadow_quality.AddItem("Normal", 1);
		shadow_quality.AddItem("High", 1);
		shadow_quality.AddItem("Super High", 2);

		checkResolutions();

		loadSettings();
		addKeyControl();
	}

	private void loadSettings()
	{
		

		var antialiasingSetting = userSettings.readConfigKey("video", "antialiasing");
		var vsyncSetting = userSettings.readConfigKey("video", "vsync");
		var ssaoSetting = userSettings.readConfigKey("video", "enable_ssao");

		var mode = userSettings.readConfigKey("video", "mode");
		var shadoqQualitySetting = userSettings.readConfigKey("video", "shadow_quality");

		if(antialiasingSetting != null)
		{
			antialising.Selected = (int) antialiasingSetting;
			GD.Print("set anti aliasing");
			GD.Print(antialiasingSetting);
		}

		if(vsyncSetting != null)
		{
			vsync.Selected = ((bool) vsyncSetting) ? 1 : 0;
		}

		if(ssaoSetting != null)
		{
			ssao.Selected = ((bool) ssaoSetting) ? 1 : 0;
		}

		if(shadoqQualitySetting != null)
		{
			int ShadowQualityInt = (int) shadoqQualitySetting;
			int shadow_quality_nr = 0;
			if (ShadowQualityInt == 8192)
				shadow_quality_nr = 3;
			if (ShadowQualityInt == 4096)
				shadow_quality_nr = 2;
			if (ShadowQualityInt == 2048)
				shadow_quality_nr = 1; 
			if (ShadowQualityInt == 1024) 
				shadow_quality_nr = 0;

			shadow_quality.Selected = shadow_quality_nr;
		}

		if(mode != null)
		{
			windowMode.Selected = (int) mode;
		}
	}

	private void addKeyControl()
	{
		var scene = ResourceLoader.Load("res://utils/client/Settings/KeyButton.tscn") as PackedScene;

		if (scene == null)
		{
			return;
		}

		foreach (var x in userSettings.possibleKeys)
		{
			var button = scene.Instance();
			button.Name = x.Key;

			KeyButton buttonNode = button as KeyButton;

			buttonNode.key = x.Key;
			buttonNode.label = x.Value;
			buttonNode.buttonDialog = GetNode("button_dialog") as ButtonDialog;

			controlList.AddChild(buttonNode);
		}
	}

	private void checkResolutions()
	{
		foreach (var x in userSettings.possibleResolutions)
		{
			if (highestPossibleResoultion.y >= x.y && highestPossibleResoultion.x >= x.x)
			{
			
				usableResolutions.Add(x);
			}
		}

		addResolutions();
	}

	private void addResolutions()
	{
		resolution.Clear();
		
		var res_x = userSettings.readConfigKey("video", "res_x");
		var res_y =   userSettings.readConfigKey("video", "res_y");
		int i = 0;
		int selectedI = 0;
		foreach (var x in usableResolutions)
		{
			
			if(res_x != null && res_y != null && x.x == (float) res_x && x.y == (float) res_y)
			{
				selectedI = i;
			}

			resolution.AddItem(x.x.ToString() + " x " + x.y.ToString(), i);
			i++;
		}
		resolution.Selected = selectedI;
	}

	private void _on_apply_button_pressed()
	{
		if (windowMode.Selected == 2)
		{
			userSettings.setFullscreenMode(windowMode.Selected, highestPossibleResoultion);
		}
		else
		{
			var reso = usableResolutions[resolution.Selected];
			userSettings.setFullscreenMode(windowMode.Selected, reso);
		}

		userSettings.enableVsync((vsync.Selected == 1) ? true : false);
		userSettings.setSSAO((ssao.Selected == 1) ? true : false);

		Viewport.MSAA msaa = (Viewport.MSAA)antialising.Selected;
		userSettings.setAntiAlising(msaa);

		int shadow_quality_nr = 0;

		if (shadow_quality.Selected == 3)
			shadow_quality_nr = 8192;
		if (shadow_quality.Selected == 2)
			shadow_quality_nr = 4096;
		if (shadow_quality.Selected == 1)
			shadow_quality_nr = 2048;
		if (shadow_quality.Selected == 0)
			shadow_quality_nr = 1024;

		userSettings.setShadowQuality(shadow_quality_nr);
	}

	public void isIngameMenu(bool ingame){
		if(!ingame)
		{
			(GetNode("background") as Control).Visible = false;
			(GetNode("control") as Control).AnchorLeft = 0;
			(GetNode("control") as Control).AnchorRight = 1;
			(GetNode("control") as Control).AnchorBottom = 1;
			(GetNode("control") as Control).AnchorTop = 0;
		}
	}

}










