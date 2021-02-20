using System;
public enum UMASlotCategory
{
    Head,
    Body,
    Torso,
    Legs,
    Feets
}
public enum UMASkinGroup
{
    Hair,
    Body,
    Eyelash,
    Eyes,
    None
}

public class UMACategoryAttribute : Attribute
{
    public string Name { get; set; }
    public UMASlotCategory category { get; set; }
    public UMACategoryAttribute(UMASlotCategory _category, string str)
    {
        category = _category;
        Name = str;
    }
}