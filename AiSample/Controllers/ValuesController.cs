using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AiSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // POST api/values
        [HttpPost]
        public ActionResult<AddNumbersRespone> Post([FromBody] AddNumbersRequest bodyBag)
        {
            Response.Headers.Add("XX-ServerRespondTime", DateTime.Now.ToShortTimeString());
            return (new AddNumbersRespone() { Result = bodyBag.Num1 + bodyBag.Num2 });
        }
    }
    public class AddNumbersRespone
    {
        public int Result { get; set; }
    }

    public class AddNumbersRequest
    {
        public int Num1 { get; set; }
        public int Num2 { get; set; }
    }
}
