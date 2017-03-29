using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace TsocIdServer.Controllers
{
 
    public class CoreController : ApiController
    {
        public string Get()
        {
            return "this is working";
        }
        // POST api/<controller>
        public void Post([FromBody] string value)
        {
            HttpContext.Current.Response.Write(this.Request.Content.ToString());
        }
    }
}