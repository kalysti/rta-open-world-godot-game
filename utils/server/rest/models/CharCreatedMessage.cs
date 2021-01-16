using System;
[Serializable]
public class CharCreatedMessage
{
    public OnlineCharacter character {get;set;}
    public string errorMessage {get;set;}

    public bool created {get;set;}
}