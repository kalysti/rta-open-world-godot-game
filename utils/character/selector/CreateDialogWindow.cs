using Godot;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using RestClient.Net;
using RestClient.Net.Abstractions;

public class CreateDialogWindow : WindowDialog
{
    protected string accessToken = null;
    public string hostname = "localhost";
    public int port = 27021;

    [Signal]
    public delegate void OnPlayerCreated(int charId, string fristname, string lastname, string birthday, bool isMale);


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        (FindNode("gender_list") as OptionButton).AddItem("Male", 0);
        (FindNode("gender_list") as OptionButton).AddItem("Female", 1);
        Connect("about_to_show", this, "onShow");

        FindNode("cancel_confirm_button").Connect("pressed", this, "onPlayerCanceled");
        FindNode("create_confirm_button").Connect("pressed", this, "onPlayerCreationConfirmed");
        
    }

    public void onShow()
    {
        (FindNode("error_message") as Label).Visible = false;
        (FindNode("create_confirm_button") as Button).Disabled = false;
    }

    public void onPlayerCanceled()
    {
        (FindNode("create_confirm_button") as Button).Disabled = true;
        Hide();
    }
    public void SetToken(string token)
    {
        accessToken = token;

    }
    private async void createCharacter()
    {
        try
        {
            var firstname = (FindNode("firstname") as LineEdit).Text.Trim();
            var lastname = (FindNode("lastname") as LineEdit).Text.Trim();
            var birthday = (FindNode("birthday") as LineEdit).Text.Trim();
            var gender = ((FindNode("gender_list") as OptionButton).Selected > 0) ? false : true;

            var onlineCharacter = new OnlineCharacter { firstname = firstname, lastname = lastname, birthday = birthday, isMale = gender };

            var restClient = new RestClient.Net.Client(new RestClient.Net.NewtonsoftSerializationAdapter(), new Uri("http://" + hostname + ":" + port + "/api/createCharacter"));
            restClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            CharCreatedMessage response = await restClient.PostAsync<CharCreatedMessage, OnlineCharacter>(onlineCharacter);


            if (response.created)
            {
                EmitSignal(nameof(OnPlayerCreated), response.character.Id, response.character.firstname, response.character.lastname, response.character.birthday, response.character.isMale);
                Hide();
            }
            else
                throw new Exception(response.errorMessage);

        }
        catch (Exception e)
        {
            (FindNode("error_message") as Label).Visible = true;
            (FindNode("error_message") as Label).Text = e.Message;

        }


      (FindNode("create_confirm_button") as Button).Disabled = false;
    }



    public void onPlayerCreationConfirmed()
    {


        (FindNode("create_confirm_button") as Button).Disabled = true;
        createCharacter();

    }

}
