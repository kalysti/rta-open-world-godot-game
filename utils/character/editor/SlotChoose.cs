using Godot;
using System;
using System.Collections.Generic;

public class SlotChoose : GridContainer
{
    [Signal]
    public delegate void Activate(string overlay, string slotName, Color changedColor, string material);

    public string overlayName = "";
    public string slotName = "";

    public void buttonPressed()
    {
        EmitSignal(nameof(Activate), overlayName, slotName, getCurrentColor(), getCurrentMaterial());
    }

    public void colorSelected(Color selected)
    {
        EmitSignal(nameof(Activate), overlayName, slotName, selected, getCurrentMaterial());
    }

    public void materialSelected(int selected)
    {
        var node = GetNode<OptionButton>("container/details/material");

        if (node == null)
            return;

        var mat = node.GetItemText(selected);
        EmitSignal(nameof(Activate), overlayName, slotName, getCurrentColor(), mat);

    }

    private Color getCurrentColor()
    {
        var node = GetNode<ColorPickerButton>("container/details/color");
        return node.Color;
    }

    private string getCurrentMaterial()
    {
        var node = GetNode<OptionButton>("container/details/material");

        if (node.Selected == -1)
            return null;

        return node.GetItemText(node.Selected);
    }

    public void Init(string value, string slot)
    {
        overlayName = value;
        slotName = slot;
        (GetNode("Button") as Button).Text = value;
    }

    public void SetActive(bool isActive)
    {
        (GetNode("Button") as Button).Pressed = isActive;
        GetNode<PanelContainer>("container").Visible = isActive;
    }

    public void SetMaterials(List<string> materials, string currentMaterial)
    {
        var node = GetNode<OptionButton>("container/details/material");
        var label = GetNode<Label>("container/details/material_label");

        node.Clear();

        if (materials == null || materials.Count <= 0)
        {
            label.Visible = false;
            node.Visible = false;

            return;
        }
        else
        {
            node.Visible = true;
            label.Visible = true;
        }

        int i = 0;
        foreach (var mat in materials)
        {
            node.AddItem(mat, i);

            if (mat == currentMaterial)
            {
                node.Selected = i;
            }

            i++;
        }
    }
    public void SetSkinColor(Color skinColor, bool active)
    {
        var node = GetNode<ColorPickerButton>("container/details/color");
        var label = GetNode<Label>("container/details/color_label");

        if (skinColor == null || !active)
        {
            label.Visible = false;
            node.Visible = false;
            return;
        }
        else
        {
            node.Visible = true;
            label.Visible = true;
        }

        node.Color = skinColor;
    }
}
