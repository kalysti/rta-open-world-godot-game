using System;
[Serializable]
public class AuthLoginAccountMessage
{
    public string token {get;set;}
    public string errorMessage {get;set;}

    public bool success {get;set;}
}