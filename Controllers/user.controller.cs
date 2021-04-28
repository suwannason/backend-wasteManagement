
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

        [HttpPost("login/ldap"), AllowAnonymous]
        public async Task<ActionResult> loginAD(Login body)
        {

            AD_API res = null;
            User user = _userService.Get(body.username);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Please contact FAE assign this username" });
            }

            if (body.username != "admin")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    string request = "{ \"username\": \"" + body.username + "\"," + "\"password\":" + "\"" + body.password + "\"}";
                    StringContent content = new StringContent(request, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(LDAP_AUTH + "/authentication/ldap", content);

                    res = JsonConvert.DeserializeObject<AD_API>(response.Content.ReadAsStringAsync().Result);

                     return Ok();
                }
            }
            string token = GenerateJSONWebToken(user);

            return Ok(new { success = true, token, data = user });
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