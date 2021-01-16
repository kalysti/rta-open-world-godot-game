using System;
using System.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using Newtonsoft.Json;
using EmbedIO.WebApi;
using System.Collections.Generic;
using SQLite;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using Godot;

namespace Game.Rest
{
    public class RestController : WebApiController
    {
        public const int maxAllowedPlayer = 3;
        public const bool playerCanDeleteChar = true;

        [Route(EmbedIO.HttpVerbs.Post, "/register")]
        public async Task<AuthCreateAccountMessage> DoRegister()
        {
            var data = await HttpContext.GetRequestDataAsync<AuthCredentials>();

            try
            {
                GD.Print("[Rest] Try to create account");

                Regex r = new Regex("^[a-zA-Z0-9_.]*$");

                var username = (data.username != null) ? data.username.Trim() : null;
                var password = (data.password != null) ? data.password.Trim() : null;

                if (String.IsNullOrEmpty(username))
                {
                    throw new Exception("Username cant be empty.");
                }

                if (String.IsNullOrEmpty(password))
                {
                    throw new Exception("Password cant be empty.");
                }

                username = username.Trim();
                password = password.Trim();

                if (!r.IsMatch(username))
                {
                    throw new Exception("Username supports only aplhanumeric characters.");
                }

                var userExist = Server.database.Table<AuthUser>().Where(s => s.Username == username).FirstOrDefault();


                if (userExist != null)
                {
                    throw new Exception("Username is already in usage.");
                }

                if (password.Length < 8)
                {
                    throw new Exception("Password have to be min. 8 characters.");
                }



                var salt = PasswordHelper.GenerateSalt();
                var hash = Convert.ToBase64String(salt);
                var passwordHash = Convert.ToBase64String(PasswordHelper.ComputeHash(password, salt));
                var token = PasswordHelper.RandomString(80);

                var user = new AuthUser();
                user.Password = passwordHash;
                user.Salt = hash;
                user.Username = username;
                user.Token = token;

                Server.database.Insert(user);

                GD.Print("[Rest] Create account succesfull");

                return new AuthCreateAccountMessage { token = user.Token, created = true, errorMessage = null };
            }
            catch (Exception e)
            {
                return new AuthCreateAccountMessage { created = false, errorMessage = e.Message };
            }
        }
        public void checkForDefaulTextInput(string input, string word)
        {
            Regex r = new Regex("^[a-zA-Z ]*$");

            if (String.IsNullOrEmpty(input))
                throw new Exception(word + ": Cant be empty");
            else if (!r.IsMatch(input))
                throw new Exception(word + ": Only alphabetic characters allowed");
            else if (input.Length < 2)
                throw new Exception(word + ": Min 2 characters are required");
            else if (input.Length > 25)
                throw new Exception(word + ": Max 25 characters are possible");

        }

        [Route(EmbedIO.HttpVerbs.Post, "/createCharacter")]
        public async Task<CharCreatedMessage> CreteCharacter()
        {
            var user = GetUser();
            var data = await HttpContext.GetRequestDataAsync<OnlineCharacter>();

            try
            {
                var _chars = Server.database.Table<OnlineCharacter>().Where(s => s.AuthId == user.Id).Count();
                if (_chars >= maxAllowedPlayer)
                {
                    throw new Exception("Maximum " + maxAllowedPlayer + " characters allowed on this server.");
                }

                var firstname = (data.firstname != null) ? data.firstname.Trim() : null;
                var lastname = (data.lastname != null) ? data.lastname.Trim() : null;
                var birthday = (data.birthday != null) ? data.birthday.Trim() : null;

                if (String.IsNullOrEmpty(birthday))
                    throw new Exception("Birthday  Cant be empty");

                DateTime birthdayDateObj = new DateTime(); ;
                CultureInfo provider = CultureInfo.InvariantCulture;

                checkForDefaulTextInput(firstname, "Firstnme");
                checkForDefaulTextInput(lastname, "Lastname");

                var dateStringFormat = "yyyy-MM-dd";



                try
                {
                    birthdayDateObj = DateTime.ParseExact(birthday, dateStringFormat, provider);
                }
                catch (FormatException)
                {
                    throw new Exception(String.Format("Birthday: Wrong format: {0}", dateStringFormat.ToUpper()));
                }

                TimeSpan diff = DateTime.Today - birthdayDateObj;

                if (diff.TotalDays > (365 * 100) || diff.TotalDays < (365 * 16))
                    throw new Exception("Birthday: U are age have be between 16 and 100 years");

                if (birthdayDateObj > DateTime.Today)
                    throw new Exception("Your birthday cant be in the future.");

                var _char = new OnlineCharacter
                {
                    firstname = firstname,
                    lastname = lastname,
                    isMale = data.isMale,
                    hunger = 0,
                    thirst = 0,
                    bladder = 0,
                    hp = 100,
                    birthday = birthdayDateObj.ToString("yyyy-MM-dd"),
                    AuthId = user.Id,
                };

                Server.database.Insert(_char);

                GD.Print("[Rest] Create new character");

                return new CharCreatedMessage { character = _char, created = true, errorMessage = null };
            }
            catch (Exception e)
            {
                return new CharCreatedMessage { created = false, errorMessage = e.Message };
            }
        }

        [Route(EmbedIO.HttpVerbs.Get, "/deleteCharacter/{id?}")]
        public async Task<CharDeleteMessage> DeleteCharacter(int id)
        {
            var user = GetUser();
            try
            {
                var _char = Server.database.Table<OnlineCharacter>().Where(s => s.AuthId == user.Id && s.Id == id).FirstOrDefault();
                if (_char == null)
                    throw new Exception("Character not found");

                if (playerCanDeleteChar == false)
                    throw new Exception("Only a server administrator can remove your character.");

                Server.database.Delete(_char);
                GD.Print("[Rest] Delete character");

                return new CharDeleteMessage { success = true, errorMessage = null };
            }
            catch (Exception e)
            {
                return new CharDeleteMessage { success = false, errorMessage = e.Message };
            }
        }

        [Route(EmbedIO.HttpVerbs.Get, "/hello")]
        public ServerVersion Hello()
        {
            var tf = new ServerVersion();
            tf.version = "0.0.0.1";
            tf.system = "economics";
            tf.name = "Greiz City";

            return tf;
        }
        [Route(EmbedIO.HttpVerbs.Get, "/characters")]
        public async Task<CharReponseList> Characters()
        {
            var user = GetUser();
            var chars = Server.database.Table<OnlineCharacter>().Where(s => s.AuthId == user.Id).ToList();

            return new CharReponseList { characters = chars };
        }

        private AuthUser GetUser()
        {
            var authToken = HttpContext.Request.Headers.Get("Authorization");
            if (String.IsNullOrEmpty(authToken))
            {
                throw HttpException.Forbidden("Wrong crendetials.");
            }

            authToken = authToken.Replace("Bearer", "").Replace(" ", "").Trim();
            var user = Server.database.Table<AuthUser>().Where(s => s.Token == authToken).FirstOrDefault();

            if (user == null)
            {
                throw HttpException.Forbidden("Wrong crendetials.");
            }

            return user;
        }
        [Route(EmbedIO.HttpVerbs.Post, "/login")]
        public async Task<AuthLoginAccountMessage> DoLogin()
        {

            try
            {
                var data = await HttpContext.GetRequestDataAsync<AuthCredentials>();
                var username = data.username;
                var password = data.password;

                if (String.IsNullOrEmpty(username))
                {
                    throw new Exception("Username cant be empty.");
                }

                if (String.IsNullOrEmpty(password))
                {
                    throw new Exception("Password cant be empty.");
                }

                username = username.Trim();
                password = password.Trim();

                var user = Server.database.Table<AuthUser>().Where(s => s.Username == username).FirstOrDefault();

                if (user == null)
                    throw new Exception("Wrong crendetials.");

                byte[] passwordSalt = Convert.FromBase64String(user.Salt);

                var hashedPassword = Convert.ToBase64String(PasswordHelper.ComputeHash(password, passwordSalt));

                if (hashedPassword != user.Password)
                    throw new Exception("Wrong crendetials.");

                return new AuthLoginAccountMessage { token = user.Token, success = true, errorMessage = null };
            }
            catch (Exception e)
            {
                return new AuthLoginAccountMessage { token = null, success = false, errorMessage = e.Message };
            }
        }
    }
}