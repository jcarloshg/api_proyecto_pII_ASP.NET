using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

using API_proyecto_pII.Models;
using Newtonsoft.Json;

namespace API_proyecto_pII.Controllers
{
    public class UserController : ApiController
    {
        
        private static string GetMD5(string pass)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(pass));
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }

            return hash.ToString();
        }

        public static string QuitarComillas(string str)
        {
            string str_inicio = str.Substring(1);
            string str_fin = str_inicio.Remove(str_inicio.Length - 1);

            return str_fin;
        }

        private string loginAndCheckPermission(string user, string pass, string[] roles, string auxCode)
        {
            string passUser = QuitarComillas( FireBase.getBody("usuarios/", user) );
            string code = auxCode;

            if ( !passUser.Equals("ul") )
            {
                if ( passUser.Equals(GetMD5(pass)) )
                {
                    UserInfo userInfo = FireBase.get("usuarios_info/", user).ResultAs<UserInfo>();
                    code = (Array.Exists(roles, item => item == userInfo.Rol)) ? auxCode : "504";
                }
                else 
                { 
                    code = "501"; 
                }
            }
            else
            {
                code = "500";
            }

            return code;
        }

        public IHttpActionResult Post(string user, string pass, string newUser, string newPass)
        {
            string code = loginAndCheckPermission(user, pass, new string[] {"ventas", "almacen"}, "404");
            string data = "";
            string status = "Error";

            if (code == "404")
            {
                string newUserPass = QuitarComillas(FireBase.getBody("usuarios/", newUser));

                // check existence of new user
                if (newUserPass.Equals("ul"))
                {
                    FireBase.set("usuarios/" + newUser, newPass);
                    data = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
                    status = "Successfully";
                }
                else
                {
                    code = "508";
                }
            }

            RespuestaSetUser res = new RespuestaSetUser
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = data,
                Status = status
            };
            
            return Ok(res);

        }

        public IHttpActionResult Put(string user, string pass, string oldUser, string newUser, string newPass)
        {
            string code = loginAndCheckPermission(user, pass, new string[] { "rh" }, "401");
            string data = "";
            string status = "Error";

            if (code == "401")
            {
                // check newpass that contains
                if ( newPass.Length < 8 || newPass != newPass.Replace(" ", "") || !newPass.Any(char.IsDigit) )
                { code = "502"; }

                // checo string newUser
                if (code == "401")
                    if ( !newUser.All(char.IsLetterOrDigit) || newPass != newPass.Replace(" ", "") )
                        code = "503";

                // check the existence oldUser
                if (code == "401")
                {
                    string searchUserResponse = QuitarComillas( FireBase.getBody("usuarios/", oldUser) );

                    code = searchUserResponse.Equals("ul") ? "505" : "401";

                    // if exits then delete user and insert newUser
                    if (code == "401")
                    {
                        FireBase.delete("usuarios/" + oldUser);
                        FireBase.set("usuarios/" + newUser, newPass);
                        data = DateTime.Now.ToString("yyy/MM/dd HH:mm:ss");
                        status = "Successfully";
                    }
                }
            }

            RespuestaUpdateUser res = new RespuestaUpdateUser
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = data,
                Status = status
            };

            return Ok(res);
        }
    
        public IHttpActionResult Get()
        {
            string code = "600";
            string[] data = { };
            string status = "Error";

            string users = QuitarComillas(FireBase.getBody("usuarios/", ""));

            if ( !users.Equals("ul"))
            {
                users = users.Replace("\"", string.Empty);

                string[] authorsList = users.Split(','); // gets [ "AAA:aaa", "BBB:bbb", "CCC:ccc",... ]

                // gets [ "AAA", "BBB", "CCC",... ]
                for (int i = 0; i < authorsList.Length; i++)
                {
                    string[] aux = authorsList[i].Split(':');
                    authorsList[i] = aux[0];
                } 

                data = authorsList;
                status = "Successfully";
            }
            else
            {
                code = "999";
            }


            RespuestaGetUsers res = new RespuestaGetUsers
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = data,
                Status = status
            };

            return Ok(res);
        }

        public IHttpActionResult Get(string user)
        {
            string code = "601";
            string data = "";
            string status = "Error";

            UserInfo userInfo = FireBase.get("usuarios_info/", user).ResultAs<UserInfo>();

            return Ok(userInfo);
        }
    }
}
