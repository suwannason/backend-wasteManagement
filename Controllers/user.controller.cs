
using backend.Models;
using backend.Services;
using backend.response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController]

    public class userController : ControllerBase
    {
        private readonly UserService _userService;

        UserResponse res = new UserResponse();

        public userController(UserService userService)
        {
            _userService = userService;
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
            res.success = true;
            res.message = "Login success";
            List<User> formatUser = new List<User>();

            formatUser.Add(data);
            res.data = formatUser.ToArray();

            // Console.Write(data.email);
            // res.data = data;
            return Ok(res);
        }
    }
}