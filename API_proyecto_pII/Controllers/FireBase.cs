using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;
using Newtonsoft.Json;
using API_proyecto_pII.Models;

namespace API_proyecto_pII.Controllers
{
    public class FireBase
    {
        public static IFirebaseClient init()
        {
            IFirebaseConfig config = new FirebaseConfig();

            config.AuthSecret = "GSUsDlaMzMXc4NEyK9QsVVnDlMbe0Ttgd8QcB2bC";
            config.BasePath = "https://web-service-productos-default-rtdb.firebaseio.com/";

            return new FireSharp.FirebaseClient(config);
        }

        public static FirebaseResponse get(string url, string data)
        {
            IFirebaseClient cliente = init();
            return cliente.Get(url + data);
        }

        public static string getBody(string url, string data)
        {
            IFirebaseClient cliente = init();
            return cliente.Get(url + data).Body;
        }

        public static SetResponse set(string url, string data)
        {
            IFirebaseClient cliente = init();
            return cliente.Set(url, data);
        }

        public static SetResponse setUserInfo(string url, UserInfo user)
        {
            IFirebaseClient cliente = init();
            return cliente.Set(url, user);
        }

        public static FirebaseResponse update(string url, UserInfo user)
        {
            IFirebaseClient cliente = init();
            return cliente.Update(url, user);
        }

        public static FirebaseResponse delete(string url)
        {
            IFirebaseClient cliente = init();
            return cliente.Delete(url);
        }

    }
}