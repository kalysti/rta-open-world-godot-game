using Godot;
using System;
using System.Text;
using System.Collections.Generic;
using RestClient.Net;
using RestClient.Net.Abstractions;
using Game;
using System.Linq;

public class CharacterSelector : Control
{
    [Export]
    public string hostname = "localhost";

    [Export]
    public int port = 27021;

    [Export]
    public NodePath holderPath;

    public GridContainer charHolder;

    [Export]
    public NodePath charEditorPath;

    public CharacterEditor charEditor;

    protected string accessToken = "";

    [Signal]
    public delegate void onSelect(int id);

    protected List<OnlineCharacter> charList = new List<OnlineCharacter>();


    // Called when the node enters the scene tree for the first time.
    private CreateDialogWindow createDialog = null;
    public override void _Ready()
    {
        createDialog = GetNode("CreateDialog") as CreateDialogWindow;
        createDialog.Connect("OnPlayerCreated", this, "OnPlayerCreated");

        charHolder = GetNode(holderPath) as GridContainer;
        charEditor = GetNode(charEditorPath) as CharacterEditor;

        charEditor.Connect("onCharacterSave", this, "storeCharacter");
    }

    public void setWelcomeText(string message)
    {
        (FindNode("welcome_text") as Label).Text = message;
    }
    private void UpdateCharList()
    {

        foreach (CharacterMenuItem _child in FindNode("char_list").GetChildren())
        {
            FindNode("char_holder").RemoveChild(_child);
            FindNode("char_list").RemoveChild(_child);
        }

        foreach (var item in charList)
        {
            var menuItemScene = (PackedScene)ResourceLoader.Load("res://utils/character/selector/CharacterMenuItem.tscn");
            CharacterMenuItem character = (CharacterMenuItem)menuItemScene.Instance();
            character.Name = item.Id.ToString();
            FindNode("char_list").AddChild(character);
            character.setCharacter(item);
            character.Connect("characterSelected", this, "onSelectChar");
        }
    }

    public void onSelectChar(int charId)
    {
        GD.Print("selected " + charId);

        var character = charList.FirstOrDefault((tf) => tf.Id == charId);
        if (character != null)
        {
            foreach (CharacterSelectView _child in FindNode("char_holder").GetChildren())
            {
                if (_child.character != null && _child.character.Id == charId)
                    return;

                FindNode("char_holder").RemoveChild(_child);
            }

            var networkCharScene = (PackedScene)ResourceLoader.Load("res://utils/character/selector/CharacterSelectView.tscn");
            CharacterSelectView charScene = (CharacterSelectView)networkCharScene.Instance();
            charScene.Name = character.Id.ToString();
            FindNode("char_holder").AddChild(charScene);
            charScene.SetCharacter(character);

            charScene.Connect("onDeleteCharacter", this, "CharacterDeleteEvent");
            charScene.Connect("onEditCharacter", this, "CharacterEditEvent");
            charScene.Connect("onLaunchCharacter", this, "CharacterLaunch");
        }
    }

    public async void CharacterDeleteEvent(int id)
    {
        var _charNode = charHolder.GetNodeOrNull(id.ToString());
        if (_charNode != null)
        {
            var _char = (_charNode as CharacterSelectView).character;

            string text = "Are you realy want to delete " + _char.getFullname() + "? Attention: You cant undo this!!";

            bool result = await UIHelper.Helper.AcceptDialog("Realy want to delete character?", text);
            if (result)
                deleteCharacterRequest(_charNode as CharacterSelectView, _char.Id);

        }
    }

    private void CharacterLaunch(int id)
    {
        EmitSignal(nameof(onSelect), id);
    }

    private void storeCharacter(int id, string body)
    {
        //todo store character
    }

    private async void deleteCharacterRequest(CharacterSelectView node, int id)
    {
        try
        {
            var restClient = new RestClient.Net.Client(new RestClient.Net.NewtonsoftSerializationAdapter(), new Uri("http://" + hostname + ":" + port + "/api/deleteCharacter/" + id));
            restClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            CharDeleteMessage response = await restClient.GetAsync<CharDeleteMessage>();

            if (response.success == true)
            {
                var _charNode = charHolder.GetNodeOrNull(id.ToString());
                if (_charNode != null)
                {
                    charHolder.RemoveChild(node);
                }
                
                charList.RemoveAll((tf) => tf.Id == id);
            }
            else
            {
                throw new Exception(response.errorMessage);
            }
        }
        catch (Exception e)
        {
            await UIHelper.Helper.AcceptDialog("Something went wrong.", e.Message);
        }
    }

    public void CharacterEditEvent(int id)
    {
        var _charNode = charHolder.GetNodeOrNull(id.ToString());
        if (_charNode != null)
        {
            var _char = (_charNode as CharacterSelectView).character;

            charEditor.Visible = true;
            charEditor.characterId = _char.Id;
            charEditor.loadJson(_char.body);
        }
    }

    public async void GetCharList()
    {
        try
        {
            var restClient = new RestClient.Net.Client(new RestClient.Net.NewtonsoftSerializationAdapter(), new Uri("http://" + hostname + ":" + port + "/api/characters"));
            restClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            CharReponseList response = await restClient.GetAsync<CharReponseList>();

            if (response.characters != null)
            {
                charList = response.characters;
                if (charList.Count > 0)
                    onSelectChar(charList[0].Id);

                UpdateCharList();
            }

        }
        catch (Exception e)
        {
            GD.Print("client error: " + e.Message);
            Hide();
        }
    }

    public void SetToken(string token)
    {
        accessToken = token;
        createDialog.SetToken(accessToken);

    }

    public void onCreateButtonPressed()
    {
        createDialog.PopupCentered();
    }

    public void OnPlayerCreated(int id, string fristname, string lastname, string birthday, bool isMale)
    {
        charList.Add(new OnlineCharacter { Id = id, firstname = fristname, lastname = lastname, birthday = birthday, isMale = isMale });
        createDialog.hostname = hostname;
        createDialog.port = port;

        UpdateCharList();
        onSelectChar(id);
    }
}
