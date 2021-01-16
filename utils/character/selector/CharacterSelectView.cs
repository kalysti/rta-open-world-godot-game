using Godot;
using System;
namespace Game
{
    public class CharacterSelectView : Panel
    {
        public OnlineCharacter character = null;

        [Signal]
        public delegate void onDeleteCharacter(int id);


        [Signal]
        public delegate void onEditCharacter(int id);


        [Signal]
        public delegate void onLaunchCharacter(int id);

        public override void _Process(float delta)
        {
            var node = GetNodeOrNull("viewport_container/char_viewport/char");
            if (node != null)
                (node as NetworkPlayerChar).doCatwalk();
        }

        public void SetCharacter(OnlineCharacter _tempChar)
        {
            character = _tempChar;

            //doCatwalk

            (FindNode("char_name") as Label).Text = _tempChar.getFullname();
            (FindNode("char_birthday") as Label).Text = "Age: " + _tempChar.Age().ToString();
            (FindNode("gender") as Label).Text = _tempChar.isMale ? "Male" : "Female";

            (FindNode("edit_button") as Button).Connect("pressed", this, "onCharacterEdit");
            (FindNode("launch_button") as Button).Connect("pressed", this, "onCharacterLaunch");
            (FindNode("delete_button") as Button).Connect("pressed", this, "onCharacterDelete");
        }

        public void onCharacterDelete()
        {
            EmitSignal(nameof(onDeleteCharacter), character.Id);

        }

        public void onCharacterLaunch()
        {
            EmitSignal(nameof(onLaunchCharacter), character.Id);

        }

        public void onCharacterEdit()
        {
            EmitSignal(nameof(onEditCharacter), character.Id);
        }
    }
}