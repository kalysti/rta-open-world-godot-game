using Godot;
using System;
using Newtonsoft.Json;

namespace Game
{
    public class CharacterSelectView : Panel
    {
        public OnlineCharacter character = null;

        [Signal]
        public delegate void onDeleteCharacter(int id);


        [Signal]
        public delegate void onEditCharacter(int id, string body);


        [Signal]
        public delegate void onLaunchCharacter(int id);

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

            if (String.IsNullOrEmpty(_tempChar.body))
            {
                GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").initCharacter(_tempChar.isMale);
                GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").doCatwalk();
            }
            else
            {
                var reciepe = JsonConvert.DeserializeObject<UMAReciepe>(_tempChar.body);
                GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").initCharacter(reciepe.isMale, reciepe);
                GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").doCatwalk();
            }

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
            EmitSignal(nameof(onEditCharacter), character.Id, character.body);
        }

        public void loadSkin(UMAReciepe reciepe)
        {
            GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").skeleton.loadReciepe(reciepe);
            GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").skeleton.generateDNA(reciepe.dna);
            GetNode<NetworkPlayerChar>("viewport_container/char_viewport/char").doCatwalk();
        }
    }
}