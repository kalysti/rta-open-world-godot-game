using System;
[Serializable]
public class AuthCreateAccountMessage
{
    public string token {get;set;}
    public string errorMessage {get;set;}

    public bool created {get;set;}
}