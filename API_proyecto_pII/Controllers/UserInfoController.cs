using API_proyecto_pII.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace API_proyecto_pII.Controllers
{
    public class UserInfoController : ApiController
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

        public static bool ContainsTrailingCommas(string json)
        {
            var template = Regex.Replace(json, @"\t|\n|\r|\s+|\"".*?\""", string.Empty);
            return template.Contains(",}");
        }

        public static bool IsValidJson(string input)
        {
            input = input.Trim();

            if (ContainsTrailingCommas(input))
            {
                return false;
            }

            if ((input.StartsWith("{") && input.EndsWith("}")) || //For object
                (input.StartsWith("[") && input.EndsWith("]"))) //For array
            {
                try
                {
                    //parse the input into a JObject
                    var jObject = JObject.Parse(input);

                    foreach (var jo in jObject)
                    {
                        string name = jo.Key;
                        JToken value = jo.Value;

                        //if the element has a missing value, it will be Undefined - this is invalid
                        if (value.Type == JTokenType.Undefined || value.Type == JTokenType.Null)
                        {
                            return false;
                        }
                    }
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        // Comparar si 2 listas tienen los mismos elementos
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        // Comparar si una lista contiene a otra
        public static bool ListCheck(List<string> list1, List<string> list2)
        {
            if (list1.Count >= list2.Count)
            {
                List<string> list3 = list1.Except(list2).ToList();
                if (list1.Count == list2.Count)
                {
                    return list3.Count <= 0;
                }
                else
                {
                    if (list2.Count == 3)
                    {
                        return list3.Count <= 1;
                    }
                    if (list2.Count == 2)
                    {
                        return list3.Count <= 2;
                    }
                    if (list2.Count == 1)
                    {
                        return list3.Count <= 3;
                    }
                }
            }
            return false;
        }

        private string loginAndCheckPermission(string user, string pass)
        {
            string user_res = FireBase.getBody("usuarios/", user);
            string code = "500"; // Usuario no reconocido

            if (!user_res.Equals("null"))
            {
                string pass_bd = QuitarComillas(user_res);
                code = "501"; // Password no reconcido

                if (pass_bd.Equals(GetMD5(pass)))
                {
                    UserInfo userInfo = FireBase.get("usuarios_info/", user).ResultAs<UserInfo>();
                    code = "504"; // Sin permiso para modificar usuarios

                    if (userInfo.Rol.Equals("rh"))
                    {
                        code = "null";
                    }
                }
            }

            return code;
        }

        // GET: api/UserInfo
        public IHttpActionResult Get()
        {
            string code = "999";
            string status = "Error";

            var response = FireBase.get("usuarios_info", "");
            Dictionary<string, UserInfo> users = response.ResultAs<Dictionary<string, UserInfo>>();

            if (users.Count > 0)
            {
                code = "602";
                status = "Succes";
            }

            RespuestaGetUsersInfo res = new RespuestaGetUsersInfo
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = users,
                Status = status
            };

            return Ok(res);
        }

        // GET: api/UserInfo/pruebas1/12345678a
        public IHttpActionResult Get(string user, string pass)
        {
            UserInfo data = new UserInfo();
            string status = "Error";

            string user_res = FireBase.getBody("usuarios/", user);
            string code = "500"; // Usuario no reconocido

            if (!user_res.Equals("null"))
            {
                string pass_bd = QuitarComillas(user_res);
                code = "501"; // Password no reconcido

                if (pass_bd.Equals(GetMD5(pass)))
                {
                    UserInfo userInfo = FireBase.get("usuarios_info/", user).ResultAs<UserInfo>();
                    code = "509"; // Sin permiso

                    if (userInfo.Rol.Equals("rh"))
                    {
                        code = "602";
                        status = "Succes";
                        data = userInfo;
                    }
                }
            }

            RespuestaLogin res = new RespuestaLogin
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = data,
                Status = status
            };

            return Ok(res);
        }

        // POST: api/UserInfo
        public IHttpActionResult Post(string user, string pass, string searchedUser, string userInfoJSON)
        {
            string code = loginAndCheckPermission(user, pass);
            string data = "";
            string status = "Error";

            if (code.Equals("null"))
            {
                string user_exist = FireBase.getBody("usuarios_info/", searchedUser);
                code = "506";

                if (user_exist.Equals("null"))
                {
                    code = "305";

                    if (IsValidJson(userInfoJSON))
                    {
                        JObject usuarioJson = JsonConvert.DeserializeObject<JObject>(userInfoJSON);
                        UserInfo usuario = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);
                        List<string> keys = usuarioJson.Properties().Select(p => p.Name).ToList();
                        List<string> bd_keys = new List<string>() { "correo", "nombre", "rol", "telefono" };
                        code = "304";

                        if (ScrambledEquals(bd_keys, keys))
                        {
                            FireBase.setUserInfo("usuarios_info/" + searchedUser, usuario);
                            code = "402";
                            data = DateTime.Now.ToString("s");
                            status = "Success";
                        }
                    }
                }
            }

            RespuestaSetUserInfo res = new RespuestaSetUserInfo()
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = data,
                Status = status
            };

            return Ok(res);
        }

        // PUT: api/UserInfo/user
        public IHttpActionResult Put(string user, string pass, string searchedUser, string userInfoJSON)
        {
            string code = loginAndCheckPermission(user, pass);
            string data = "";
            string status = "Error";

            if (code.Equals("null"))
            {
                string user_exist = FireBase.getBody("usuarios_info/", searchedUser);
                code = "507";

                if (!user_exist.Equals("null"))
                {
                    code = "305";

                    if (IsValidJson(userInfoJSON))
                    {
                        JObject usuarioJson = JsonConvert.DeserializeObject<JObject>(userInfoJSON);
                        UserInfo usuario = JsonConvert.DeserializeObject<UserInfo>(userInfoJSON);
                        List<string> keys = usuarioJson.Properties().Select(p => p.Name).ToList();
                        List<string> bd_keys = new List<string>() { "correo", "nombre", "rol", "telefono" };
                        code = "306";

                        if (ScrambledEquals(bd_keys, keys))
                        {
                            FireBase.update("usuarios_info/" + searchedUser, usuario);
                            code = "403";
                            data = DateTime.Now.ToString("s");
                            status = "Success";
                        }
                    }
                }
            }

            RespuestaUpdateUserInfo res = new RespuestaUpdateUserInfo()
            {
                Code = code,
                Message = QuitarComillas(FireBase.getBody("respuestas/", code)),
                Data = data,
                Status = status
            };

            return Ok(res);
        }
    }
}
