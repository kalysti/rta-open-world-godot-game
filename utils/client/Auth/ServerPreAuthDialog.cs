using Godot;
using System;
using RestClient.Net;

public class ServerPreAuthDialog : Control
{
    public string hostname = "localhost";

    public int port = 27021;

    [Signal]
    public delegate void onLogin(string token);

    public override void _Ready()
    {

        (FindNode("preauth_register_button") as Button).Connect("pressed", this, "onServerCreateAccount");
        (FindNode("preauth_login_button") as Button).Connect("pressed", this, "onServerLoginAccount");

        (FindNode("preauth_cancel_button") as Button).Connect("pressed", this, "onServerCreateAbort");
        (FindNode("preauth_login_cancel_button") as Button).Connect("pressed", this, "onServerCreateAbort");
        
        (FindNode("register_error_message") as Label).Visible = false;
        (FindNode("login_error_message") as Label).Visible = false;
        (FindNode("server_preauth_tabs") as TabContainer).SetTabTitle(0, "Login");
        (FindNode("server_preauth_tabs") as TabContainer).SetTabTitle(1, "Register");
    }

    private async void doLogin()
    {
        try
        {
            var username = (FindNode("preauth_login_username") as LineEdit).Text;
            var password = (FindNode("preauth_login_password") as LineEdit).Text;

            var credentials = new AuthCredentials { username = username, password = password };
            var restClient = new RestClient.Net.Client(new RestClient.Net.NewtonsoftSerializationAdapter(), new Uri("http://" + hostname + ":" + port + "/api/login"));
            AuthLoginAccountMessage response = await restClient.PostAsync<AuthLoginAccountMessage, AuthCredentials>(credentials);

            if (response.success)
            {
                EmitSignal(nameof(onLogin), response.token);
                Hide();
            }
            else
                throw new Exception(response.errorMessage);

        }
        catch (Exception e)
        {
            (FindNode("login_error_message") as Label).Visible = true;
            (FindNode("login_error_message") as Label).Text = e.Message;
        }

        (FindNode("preauth_login_button") as Button).Disabled = false;
    }
    private async void doRegister()
    {
        try
        {
            var username = (FindNode("preauth_register_username") as LineEdit).Text;
            var password = (FindNode("preauth_register_password") as LineEdit).Text;

            var credentials = new AuthCredentials { username = username, password = password };

            var restClient = new RestClient.Net.Client(new RestClient.Net.NewtonsoftSerializationAdapter(), new Uri("http://" + hostname + ":" + port + "/api/register"));
            AuthCreateAccountMessage response = await restClient.PostAsync<AuthCreateAccountMessage, AuthCredentials>(credentials);


            if (response.created)
            {
                EmitSignal(nameof(onLogin), response.token);
                Hide();
            }
            else
                throw new Exception(response.errorMessage);

        }
        catch (Exception e)
        {
            (FindNode("register_error_message") as Label).Visible = true;
            (FindNode("register_error_message") as Label).Text = e.Message;
        }

        (FindNode("preauth_register_button") as Button).Disabled = false;
    }

    public void onServerCreateAbort()
    {
        Hide();
    }
    public void setWelcomeMessage(string message)
    {
        (GetNode("welcome_text") as Label).Text = message;
    }
    public void onServerCreateAccount()
    {
        (FindNode("register_error_message") as Label).Visible = false;
        (FindNode("preauth_register_button") as Button).Disabled = true;

        doRegister();
    }
    public void onServerLoginAccount()
    {
        (FindNode("login_error_message") as Label).Visible = false;
        (FindNode("preauth_login_button") as Button).Disabled = true;

        doLogin();
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
