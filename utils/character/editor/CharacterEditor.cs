using Godot;
using System;
using Game;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;


public class CharacterEditor : Control
{
    [Signal]
    public delegate void onCharacterSave(int characterId, string body);

    [Export]
    public NodePath characterPath;

    [Export]
    public NodePath menuGridPath;

    [Export]
    public NodePath menuTitlePath;

    [Export]
    public NodePath morphsButtonPath;

    [Export]
    public NodePath skinsButtonPath;

    [Export]
    public NodePath morphHolderPath;

    [Export]
    public Godot.Collections.Array<Color> skinColorList = new Godot.Collections.Array<Color>();

    [Export]
    public NodePath skinsHolderPath;

    [Export]
    public NodePath colorsHolderPath;

    [Export]
    public NodePath colorsButtonPath;

    [Export]
    public NodePath positionHolderPath;

    [Export]
    public NodePath cameraHolderPath;

    NetworkPlayerChar character = null;
    Node menuGrid = null;
    Node slotTab = null;
    GridContainer morphHolder = null;
    GridContainer colorsHolder = null;
    GridContainer skinsHolder = null;
    Camera cameraHolder = null;
    Spatial positionHolder = null;

    Button colorsButton;
    Button morphsButton;
    Button skinsButton;

    Label menuTitle = null;

    public int characterId = 0;
    private float cartoonish = 1.0f;

    private Vector3 origCameraPosition = Vector3.Zero;

    private bool isMale = false;

    public override void _Ready()
    {
        character = GetNode<NetworkPlayerChar>(characterPath);
        menuGrid = GetNode<Node>(menuGridPath);
        character.Connect("CharacterInitalized", this, "initCharacter");

        cameraHolder = GetNode<Camera>(cameraHolderPath);
        positionHolder = GetNode<Spatial>(positionHolderPath);

        morphHolder = GetNode<GridContainer>(morphHolderPath);
        colorsHolder = GetNode<GridContainer>(colorsHolderPath);
        skinsHolder = GetNode<GridContainer>(skinsHolderPath);

        menuTitle = GetNode<Label>(menuTitlePath);

        morphsButton = GetNode<Button>(morphsButtonPath);
        skinsButton = GetNode<Button>(skinsButtonPath);
        colorsButton = GetNode<Button>(colorsButtonPath);

        origCameraPosition = cameraHolder.GlobalTransform.origin;

        var mo = new Godot.Collections.Array();
        mo.Add("morphs");
        var mc = new Godot.Collections.Array();
        mc.Add("colors");
        var ms = new Godot.Collections.Array();
        ms.Add("skins");

        morphsButton.Connect("pressed", this, "switchToSideMenu", mo);
        skinsButton.Connect("pressed", this, "switchToSideMenu", ms);
        colorsButton.Connect("pressed", this, "switchToSideMenu", mc);

        int i = 0;
        foreach (var name in Enum.GetValues(typeof(UMASlotCategory)))
        {
            var scene = GD.Load<PackedScene>("res://utils/character/editor/UMACategory.tscn");
            var button = (Button)scene.Instance();

            button.Icon = GD.Load<StreamTexture>("res://utils/character/editor/icons/" + name.ToString().ToLower() + ".svg");
            button.Name = name.ToString();
            menuGrid.AddChild(button);

            var bind = new Godot.Collections.Array();
            bind.Add(name.ToString());
            button.Connect("pressed", this, "switchToMenu", bind);

            if (i == 0)
                switchToMenu(name.ToString());

            i++;
        }


        var node = FindNode("ColorItemList") as ItemList;
        node.Clear();

        int colorId = 0;
        foreach (var skinColor in skinColorList)
        {
            node.AddItem("", createImageTexture(skinColor));
            node.SetItemMetadata(colorId, skinColor);
            colorId++;
        }
    }

    public void onMaleFemaleToggle(bool toggle)
    {
        character.initCharacter(toggle);
        switchToMenu(currentMenu);
    }

    public UMAReciepe reciepe = null;
    public void initCharacter(string _reciepeJson)
    {
        var _reciepe = JsonConvert.DeserializeObject<UMAReciepe>(_reciepeJson);

        reciepe = _reciepe;

        var node = FindNode("IsMaleOrFemale") as Button;
        node.Pressed = reciepe.isMale;

        GD.Print("Character intialized");


        UMASlotCategory type = (UMASlotCategory)Enum.Parse(typeof(UMASlotCategory), currentMenu);
        loadValues(type);
        switchToSideMenu("morphs");

        character.doCatwalk();
    }

    public void isColorSelected(int idx)
    {
        var node = FindNode("ColorItemList") as ItemList;

        //  character.skeleton.SkinColor = (Color)node.GetItemMetadata(idx);
        //      character.skeleton.ColorizeMesh();
    }

    private ImageTexture createImageTexture(Color c)
    {
        var imageTexture = new ImageTexture();
        var dynImage = new Image();

        dynImage.Create(64, 64, true, Image.Format.Rgb8);
        dynImage.Fill(c);

        imageTexture.CreateFromImage(dynImage);

        return imageTexture;
    }

    public void switchToSideMenu(string menu)
    {
        currentSkinMenu = menu;

        var mc = morphHolder.GetParent() as ScrollContainer;
        var sc = skinsHolder.GetParent() as ScrollContainer;
        var cc = colorsHolder.GetParent() as ScrollContainer;

        if (menu == "colors")
        {
            mc.Visible = false;
            sc.Visible = false;
            cc.Visible = (currentMenu.ToLower() == "body");
        }
        else if (menu == "morphs")
        {
            mc.Visible = true;
            sc.Visible = false;
            cc.Visible = false;
        }
        else if (menu == "skins")
        {
            mc.Visible = false;
            sc.Visible = true;
            cc.Visible = false;
        }
    }

    private string currentMenu = null;
    private string currentSkinMenu = null;
    public void switchToMenu(string menu)
    {
        currentMenu = menu;

        GD.Print("switch:" + menu);
        menuTitle.Text = menu;

        UMASlotCategory type = (UMASlotCategory)Enum.Parse(typeof(UMASlotCategory), menu);

        var gt = cameraHolder.GlobalTransform;
        var pos = positionHolder.GetNodeOrNull<Position3D>(type.ToString().ToLower());
        if (pos != null)
        {
            gt.origin = pos.GlobalTransform.origin;
        }
        else
        {
            gt.origin = origCameraPosition;
        }

        cameraHolder.GlobalTransform = gt;
        var cc = colorsHolder.GetParent() as ScrollContainer;

        if (currentSkinMenu == "colors" && menu.ToLower() == "body")
        {
            cc.Visible = true;
        }
        else
            cc.Visible = false;

        loadValues(type);
    }

    private void loadValues(UMASlotCategory type)
    {
        if (reciepe == null)
            return;

        var sliderScene = GD.Load<PackedScene>("res://utils/character/editor/morph_slider.tscn");
        var slotScene = GD.Load<PackedScene>("res://utils/character/editor/SlotChoose.tscn");

        foreach (var child in morphHolder.GetChildren())
        {
            if (child is CharSlider)
                morphHolder.RemoveChild(child as CharSlider);
        }

        foreach (var child in skinsHolder.GetChildren())
        {
            if (child is SlotChoose)
                skinsHolder.RemoveChild(child as SlotChoose);
        }

        var properties = typeof(UMA.UMADnaHumanoid).GetProperties()
               .Where(o => o.PropertyType == typeof(float) && o.IsDefined(typeof(UMACategoryAttribute), false));

        foreach (var prop in properties)
        {
            UMACategoryAttribute umaAttribute = (UMACategoryAttribute)prop.GetCustomAttribute(typeof(UMACategoryAttribute));
            if (umaAttribute.category != type)
                continue;

            var slider = (CharSlider)sliderScene.Instance();
            slider.Connect("change_morph", this, "changeMorph");
            slider.SetText(umaAttribute.Name);
            slider.sliderName = prop.Name;

            slider.SetSlider((float)prop.GetValue(reciepe.dna));
            morphHolder.AddChild(slider);
        }

        foreach (var slot in reciepe.slots)
        {
            var origSlot = character.skeleton.slotList[slot.Key];

            if (origSlot.category != type)
                continue;

            foreach (var element in origSlot.overlayList.OrderBy(df => df.Key))
            {
                var slotItem = (SlotChoose)slotScene.Instance();

                slotItem.Init(element.Key, slot.Key);
                slotItem.Name = element.Key;

                slotItem.SetActive(slot.Value.ContainsKey(element.Key));

                slotItem.Connect("Activate", this, "activateOverlay");
                var color = element.Value.skinColor;
                var currentMaterial = element.Value.currentMaterial;

                if (slot.Value.ContainsKey(element.Key))
                {
                    color = slot.Value[element.Key].overlayAlbedoColor;
                    currentMaterial = slot.Value[element.Key].usedMaterial;
                }

                if (element.Value.isSkinable)
                {
                    var materialList = element.Value.allowedMaterials.Where(df => df.Value == true).Select(df => df.Key).ToList();
                    slotItem.SetMaterials(materialList, currentMaterial);
                }
                else
                    slotItem.SetMaterials(null, null);


                if (slot.Value.ContainsKey(element.Key))
                {
                    color = slot.Value[element.Key].overlayAlbedoColor;
                }

                slotItem.SetSkinColor(color, element.Value.useSkinColor);
                skinsHolder.AddChild(slotItem);
            }
        }
    }

    private void activateOverlay(string overlayName, string slotName, Color changedColor, string material)
    {
        var newReciepe = reciepe;
        if (!reciepe.slots.ContainsKey(slotName))
            return;

        bool needMeshRegenrade = false;
        if (!reciepe.slots[slotName].ContainsKey(overlayName))
            needMeshRegenrade = true;

        newReciepe.slots[slotName].Clear();

        var reciepeOverlay = new UMAReciepeOverlay();
        reciepeOverlay.usedMaterial = material;
        reciepeOverlay.overlayAlbedoColor = changedColor;

        newReciepe.slots[slotName].Add(overlayName, reciepeOverlay);

        foreach (var child in skinsHolder.GetChildren())
        {
            if (child is SlotChoose)
            {
                var c = (child as SlotChoose);
                c.SetActive(c.Name == overlayName);
            }
        }

        if (needMeshRegenrade)
            character.skeleton.loadReciepe(newReciepe);
        else
            character.skeleton.setMainSkinColor(newReciepe);

        reciepe = newReciepe;
    }

    private void changeMorph(string text, float value)
    {
        var newReciepe = reciepe;

        Type myType = reciepe.dna.GetType();
        PropertyInfo myPropInfo = myType.GetProperty(text);
        myPropInfo.SetValue(newReciepe.dna, value);

        character.skeleton.generateDNA(newReciepe.dna);

        reciepe = newReciepe;
    }

    public void onCloseButtonPressed()
    {
        Visible = false;
    }

    public void onSaveButtonPressed()
    {
        EmitSignal(nameof(onCharacterSave), characterId, JsonConvert.SerializeObject(reciepe));
        Visible = false;
    }

    public void loadJson(string json)
    {
        if (!String.IsNullOrEmpty(json))
        {
            var res = JsonConvert.DeserializeObject<UMAReciepe>(json);
            reciepe = res;
            character.initCharacter(res.isMale, res);
        }
    }

}
