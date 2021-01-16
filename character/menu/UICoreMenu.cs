using Godot;
using System;
using System.Collections.Generic;

namespace UI.Menu
{
    public class UICoreMenu
    {
        public Dictionary<string, UIMenuButton> menuElements = new Dictionary<string, UIMenuButton>();


        public void Add(string name, string title, Type classType = null, string icon = null, bool visible = false)
        {
            var record = new UIMenuButton();
            record.title = title;
            record.visible = visible;
            record.classType = classType;
            menuElements.Add(name, record);
        }

        public void Open(WindowDialog root, Godot.Object self, Node target = null)
        {

            //clear buttons
            foreach (Node child in root.GetNode("grid").GetChildren())
            {
                if (child is Button)
                    root.GetNode("grid").RemoveChild(child);
            }


            //add buttons
            foreach (var x in menuElements)
            {
                if (x.Value.visible == false)
                {
                    continue;
                }
                if (target == null && x.Value.classType != null)
                {
                    continue;
                }
                if (x.Value.classType != null && target.GetType().IsAssignableFrom(x.Value.classType))
                {
                    continue;
                }

                Button btn = new Button();
                btn.Text = x.Value.title;
                btn.Name = x.Key;

                var collect = new Godot.Collections.Array();
                collect.Add(target);

                btn.Connect("pressed", self, "on_" + x.Key + "_pressed", collect);
                root.GetNode("grid").AddChild(btn);
            }

            Input.SetMouseMode(Input.MouseMode.Visible);
            root.Popup_();
        }
    }
}