using Godot;
using SQLite;
using System;
using System.Globalization;

[Table("characters")]
[Serializable]
public class OnlineCharacter
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Indexed]
    [Column("auth_id")]
    public int AuthId { get; set; }

    [Column("firstname")]
    public string firstname { get; set; }

    [Column("lastname")]
    public string lastname { get; set; }

    [Column("is_male")]
    public bool isMale { get; set; }

    [Column("birthday")]
    public string birthday { get; set; }

    [Column("body")]
    public string body { get; set; }

    [Column("hp")]
    public float hp { get; set; }

    [Column("hunger")]
    public float hunger { get; set; }

    [Column("thirst")]
    public float thirst { get; set; }

    [Column("bladder")]
    public float bladder { get; set; }

    public string getFullname()
    {
        return firstname + " " + lastname;
    }

    public int Age()
    {
        try
        {
            var dateStringFormat = "yyyy-MM-dd";
            CultureInfo provider = CultureInfo.InvariantCulture;

            var birthdayDateObj = DateTime.ParseExact(birthday, dateStringFormat, provider);


            int age = 0;
            age = DateTime.Now.Year - birthdayDateObj.Year;
            if (DateTime.Now.DayOfYear < birthdayDateObj.DayOfYear)
                age = age - 1;

            return age;
        }
        catch (FormatException)
        {
            return 0;
        }



    }
}