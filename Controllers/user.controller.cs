
using backend.Models;
using backend.Services;
using backend.response;
using backend.request;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController]

    public class userController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly string GLOBAL_API_ENDPOINT;
        private readonly string LDAP_AUTH;
        private IConfiguration _config;

        UserResponse res = new UserResponse();

        public userController(UserService userService, IConfiguration config, IEndpoint setting)
        {
            _userService = userService;
            _config = config;
            GLOBAL_API_ENDPOINT = setting.global_api;
            LDAP_AUTH = setting.ldap_auth;
        }

        [HttpGet]
        public ActionResult<UserResponse> Get()
        {
            List<User> data = _userService.Get();

            res.success = true;
            res.data = data.ToArray();
            if (data.Count == 0)
            {
                res.message = "Notfound Data.";
                return NotFound(res);
            }
            res.message = "Get user success";
            return Ok(res);
        }

        [HttpGet("{id}")]
        public ActionResult<User> Get(string id)
        {
            User book = _userService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }
        [HttpPost]
        public ActionResult<UserResponse> Create(User body)
        {
            try
            {
                if (body.username == "" || body.password == "")
                {
                    res.success = false;
                    res.message = "Usename or password is empty.???";
                    return BadRequest(res);
                }

                User created = _userService.Create(body);

                res.success = true;
                res.message = "Insert success";

                List<User> data = new List<User>();
                data.Add(body);
                res.data = data.ToArray();

                return Ok(res);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, User body)
        {
            User book = _userService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _userService.Update(id, body);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            User book = _userService.Get(id);

            if (book == null)
            {
                return NotFound();
            }

            _userService.Remove(book._id);

            res.success = true;
            res.message = "Delete success";

            return Ok(res);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<UserResponse> login(User body)
        {
            User data = _userService.Login(body.username, body.password);

            if (data == null)
            {
                res.success = false;
                res.message = "User name or password incorrect.";
                return NotFound(res);
            }
            if (data.canLogin == false) {
                res.success = false;
                res.message = "Can't login please update profile !!";

                return BadRequest(res);
            }
            string token = GenerateJSONWebToken(data);
            res.success = true;
            res.message = "Login success";
            res.token = token;
            List<User> formatUser = new List<User>();

            formatUser.Add(data);
            res.data = formatUser.ToArray();

            return Ok(res);
        }
        [HttpPost("login/ldap"), AllowAnonymous]
        public async Task<ActionResult> loginAD(User body) {

            AD_API res = null;
            using(HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                string request = "{ \"username\": \"" + body.username + "\"," + "\"password\":" + "\""+ body.password + "\"}";
                StringContent content = new StringContent(request, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(LDAP_AUTH + "/authentication/ldap", content);

                res = JsonConvert.DeserializeObject<AD_API>(response.Content.ReadAsStringAsync().Result);

            }
            return Ok(res);
        }
        [AllowAnonymous]
        [HttpPatch("changepw")]
        public async Task<IActionResult> changePassword(RequestChangePw body)
        {
            User data = _userService.Login(body.username, body.password);
            if (data == null)
            {
                res.success = false;
                res.message = "User name or password incorrect.";
                return NotFound(res);
            }
            User setData = new User();
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                string req = "{\"command\": \"SELECT GNAME_ENG,FNAME_ENG, BAND, DEPT_ABB_NAME, DIV_ABB_NAME FROM ADMIN.V_EMP_DATA_ALL_H where emp_no='" + body.username + "'\"}";
                StringContent content = new StringContent(req, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(GLOBAL_API_ENDPOINT + "/middleware/oracle/hrms", content);

                Console.WriteLine(response.Content.ReadAsStringAsync());
                var rec = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

                setData.username = body.username;
                setData.name = rec["data"][0]["GNAME_ENG"] + " " + rec["data"][0]["FNAME_ENG"];
                setData.band = rec["data"][0]["BAND"];
                setData.dept = rec["data"][0]["DEPT_ABB_NAME"];
                setData.div = rec["data"][0]["DIV_ABB_NAME"];
                setData.tel = body.tel;
                setData.email = body.email;
                setData.password = body.newPassword;

                _userService.changePassword(setData);
                return Ok(setData);
            }
        }
        private string GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddHours(8),
              signingCredentials: credentials);

            token.Payload["user"] = userInfo;
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}