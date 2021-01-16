using Godot;
using System;
namespace Game
{

    public class CharacterMenuItem : HBoxContainer
    {
        [Signal]
        public delegate void characterSelected(int charId);


        public OnlineCharacter character = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            (FindNode("character_name") as Button).Connect("pressed", this, "onSelectCharacter");
        }

        public void setCharacter(OnlineCharacter _char)
        {
            character = _char;
            (FindNode("character_name") as Button).Text = character.getFullname();
        }


        public void onSelectCharacter()
        {
            EmitSignal(nameof(characterSelected), character.Id);
        }
    }

}