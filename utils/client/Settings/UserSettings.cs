using Godot;
using System;
using Game;

public class UserSettings : Node
{
    public static float MOUSE_SENSITIVITY = 0.05f;
    public static float INVERSION = -1f;
    protected ConfigFile cfg = null;

    public Godot.Collections.Dictionary<string, string> possibleKeys = new Godot.Collections.Dictionary<string, string>();
    public Godot.Collections.Array<Vector2> possibleResolutions = new Godot.Collections.Array<Vector2>();
    static int GCD(int a, int b)
    {
        return b == 0 ? Math.Abs(a) : GCD(b, a % b);
    }
    public static double AspectRatio(double height, double width)
    {
        return Math.Round((height != 0) ? (width / (double)height) : double.NaN, 2);
    }

    public UserSettings()
    {

        //4:3
        possibleResolutions.Add(new Vector2(2048, 1536));
        possibleResolutions.Add(new Vector2(1920, 1440));
        possibleResolutions.Add(new Vector2(1856, 1392));
        possibleResolutions.Add(new Vector2(1600, 1200));
        possibleResolutions.Add(new Vector2(1440, 1080));
        possibleResolutions.Add(new Vector2(1400, 1050));
        possibleResolutions.Add(new Vector2(1280, 960));
        possibleResolutions.Add(new Vector2(1024, 768));
        possibleResolutions.Add(new Vector2(960, 720));
        possibleResolutions.Add(new Vector2(800, 600));
        possibleResolutions.Add(new Vector2(640, 480));

        //16:10
        possibleResolutions.Add(new Vector2(2560, 1600));
        possibleResolutions.Add(new Vector2(1920, 1200));
        possibleResolutions.Add(new Vector2(1680, 1050));
        possibleResolutions.Add(new Vector2(1440, 900));
        possibleResolutions.Add(new Vector2(1280, 800));

        //16:9
        possibleResolutions.Add(new Vector2(7680, 4320));
        possibleResolutions.Add(new Vector2(2560, 1440));
        possibleResolutions.Add(new Vector2(1920, 1080));
        possibleResolutions.Add(new Vector2(1600, 900));
        possibleResolutions.Add(new Vector2(1366, 768));
        possibleResolutions.Add(new Vector2(1280, 720));
        possibleResolutions.Add(new Vector2(1152, 648));
        possibleResolutions.Add(new Vector2(1024, 576));

        //21:9
        possibleResolutions.Add(new Vector2(3840, 2160));
        possibleResolutions.Add(new Vector2(2560, 1080));
        possibleResolutions.Add(new Vector2(1680, 720));

        possibleKeys.Add("move_forward", "Forward");
        possibleKeys.Add("move_backward", "Backward");
        possibleKeys.Add("move_left", "Left");
        possibleKeys.Add("move_right", "Right");
        possibleKeys.Add("move_jump", "Jump");

        possibleKeys.Add("lmb", "Primary Fire");
        possibleKeys.Add("rmb", "Secondary Fire");

        //possibleKeys.Add("previous_weapon", "Previous Weapon");
        //possibleKeys.Add("next_weapon", "Next Weapon");

        possibleKeys.Add("move_sprint", "Sprint");
        possibleKeys.Add("map", "Map");
    }

    public override void _Ready()
    {
        cfg = new ConfigFile();
        cfg.Load("user://settings.cfg");

        mapKeys();
    }

    public bool getSsaoEnabled()
    {
        var ssao = readConfigKey("video", "enable_ssao");

        if (ssao != null)
        {
            return (bool) ssao;
        }
        else
            return false;
    }
    public void restoreClientSettings()
    {
        var antialiasing = readConfigKey("video", "antialiasing");
        var vsync = readConfigKey("video", "vsync");

        var mode = readConfigKey("video", "mode");
        var res_x = readConfigKey("video", "res_x");
        var res_y = readConfigKey("video", "res_y");
        var shadow_quality = readConfigKey("video", "shadow_quality");

        if (antialiasing != null)
        {
            var alising = (int)antialiasing;
            setAntiAlising((Viewport.MSAA)alising);
        }

        if (vsync != null)
        {
            enableVsync((bool)vsync);
        }

        if (shadow_quality != null)
        {
            setShadowQuality((int)shadow_quality);
        }

        if (mode != null && res_x != null && res_y != null)
        {
            var vec = new Vector2((float)res_x, (float)res_y);
            setFullscreenMode((int)mode, vec);
        }
    }
    private void mapKeys()
    {
        if (cfg.HasSection("keys"))
        {
            foreach (var _key in cfg.GetSectionKeys("keys"))
            {
                var value = (string)readConfigKey("keys", _key);

                if (value != null && possibleKeys.ContainsKey(_key))
                {
                    //clear assigned input maps
                    foreach (var ev in InputMap.GetActionList(_key))
                    {
                        InputMap.ActionEraseEvent(_key, ev as InputEvent);
                    }

                    if (value.Contains("key:"))
                    {
                        var eventKey = new InputEventKey();
                        var usedKey = value.Replace("key:", "");
                        eventKey.Scancode = uint.Parse(usedKey);

                        InputMap.ActionAddEvent(_key, eventKey);
                    }
                    else if (value.Contains("button:"))
                    {

                        var mouseButton = new InputEventMouseButton();
                        var usedKey = value.Replace("button:", "");
                        mouseButton.ButtonIndex = int.Parse(usedKey);

                        InputMap.ActionAddEvent(_key, mouseButton);
                    }
                }
            }
        }
    }

    public void setConfigKey(string section, string field, object store)
    {
        cfg.SetValue(section, field, store);
        cfg.Save("user://settings.cfg");
    }

    public object readConfigKey(string section, string field)
    {
        if (cfg.HasSectionKey(section, field))
        {
            return cfg.GetValue(section, field);
        }
        else
            return null;
    }

    public void setFullscreenMode(int mode, Vector2 targetRes)
    {
        GD.Print("Set window mode to: " + mode.ToString() + " " + targetRes.ToString());

        setConfigKey("video", "mode", mode);
        setConfigKey("video", "res_x", targetRes.x);
        setConfigKey("video", "res_y", targetRes.y);

        if (mode == 0 || mode == 1)
        {
            OS.WindowMaximized = false;
            OS.WindowFullscreen = false;
            OS.WindowSize = targetRes;
            OS.WindowPosition = new Vector2(0, 0);
            OS.WindowBorderless = (mode == 0) ? false : true;

            var client = (Viewport)GetTree().Root.GetNode("entry/vbox/vbox_client/client_viewport") as Viewport;
            //client.Size = targetRes;	
            //(client.GetNode("ui").GetNode("main_menu").GetNode("viewport_container").GetNode("viewport") as Viewport).Size = targetRes;	
        }
        else
        {
            OS.WindowMaximized = true;
            OS.WindowFullscreen = true;
        }
    }

    public void enableVsync(bool isActive)
    {
        GD.Print("Set vsync to: " + isActive.ToString());

        setConfigKey("video", "vsync", isActive);
        OS.VsyncEnabled = isActive;
    }

    public void setAntiAlising(Viewport.MSAA antiAlising)
    {
        GD.Print("Set anti aliasing to:" + antiAlising.ToString());
        setConfigKey("video", "antialiasing", (int)antiAlising);

        GetTree().Root.Msaa = antiAlising;

        var client = (Viewport)GetTree().Root.GetNode("entry/vbox/vbox_client/client_viewport") as Viewport;
        client.Msaa = antiAlising;
    }

    public void setShadowQuality(int quality)
    {
        GD.Print("Set shadow quality to: " + quality.ToString());

        setConfigKey("video", "shadow_quality", quality);

        GetTree().Root.ShadowAtlasSize = quality;

        var client = (Viewport)GetTree().Root.GetNode("entry/vbox/vbox_client/client_viewport") as Viewport;
        client.ShadowAtlasSize = quality;
    }

    public void setSSAO(bool ssao)
    {
        GD.Print("Set ssao to: " + ssao.ToString());

        setConfigKey("video", "enable_ssao", ssao);

        var client = (Client)GetTree().Root.GetNode("entry/vbox/vbox_client/client_viewport/client") as Client;

        if (client != null && client.gameWorld != null)
        {
            client.gameWorld.setSSAO(ssao);
        }
    }

}
