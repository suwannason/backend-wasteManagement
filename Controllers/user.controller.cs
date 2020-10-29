
using backend.Models;
using backend.Services;
using backend.response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController]

    public class userController : ControllerBase
    {
        private readonly UserService _userService;
        private IConfiguration _config;

        UserResponse res = new UserResponse();

        public userController(UserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
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
            var book = _userService.Get(id);

            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpPost]
        public ActionResult<UserResponse> Create(User book)
        {
            try
            {
                if (book.username == "" || book.password == "")
                {
                    res.success = false;
                    res.message = "Usename or password is empty.???";
                    return BadRequest(res);
                }
                User created = _userService.Create(book);
                List<User> data = new List<User>();
                data.Add(created);

                res.success = true;
                res.message = "Insert success";
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
            var book = _userService.Get(id);

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
            var book = _userService.Get(id);

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
            string token = GenerateJSONWebToken(data);
            res.success = true;
            res.message = "Login success";
            res.token = token;
            List<User> formatUser = new List<User>();

            formatUser.Add(data);
            res.data = formatUser.ToArray();

            return Ok(res);
        }
        private string GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);
              
              token.Payload["user"] = userInfo;
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}