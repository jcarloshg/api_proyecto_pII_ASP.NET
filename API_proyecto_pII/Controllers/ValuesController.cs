using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using API_proyecto_pII.Models;

namespace API_proyecto_pII.Controllers
{
    public class ValuesController : ApiController
    {
        /* GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        */

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        public string Get()
        {
            return FireBase.get("usuarios_info/", "pruebas1").ResultAs<UserInfo>().ToString();
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {

        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
