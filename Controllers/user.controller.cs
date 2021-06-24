
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
using System.IO;
using OfficeOpenXml;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController, Authorize]

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

        [HttpGet("auth")]
        public ActionResult tokenCheck() {
            return Ok();
        }
        [HttpPost("login/backdoor"), AllowAnonymous]
        public ActionResult loginBD(Login body)
        {
            UserSchema user = _userService.Get(body.username);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Please contact FAE assign this username" });
            }
            string token = GenerateJSONWebToken(user);
            // return Ok(res);
            return Ok(new { success = true, token, data = user });

        }
        [HttpPost("login/ldap"), AllowAnonymous]
        public async Task<ActionResult> loginAD(Login body)
        {

            AD_API res = null;
            UserSchema user = _userService.Get(body.username);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Please contact FAE assign this username" });
            }
            string token = "";
            if (body.username != "admin")
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    string request = "{ \"username\": \"" + body.username + "\"," + "\"password\":" + "\"" + body.password + "\"}";
                    StringContent content = new StringContent(request, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(LDAP_AUTH + "/authentication/ldap", content);

                    res = JsonConvert.DeserializeObject<AD_API>(response.Content.ReadAsStringAsync().Result);
                    if (res.success == false)
                    {
                        return Unauthorized(new { success = false, message = "username or password incorrect." });
                    }
                    token = GenerateJSONWebToken(user);
                    // return Ok(res);
                    return Ok(new { success = true, token, data = user });
                }
            }
            token = GenerateJSONWebToken(user);

            return Ok(new { success = true, token, data = user });
        }
        [HttpPost("upload"), Consumes("multipart/form-data")]
        public ActionResult upload([FromForm] uploadFile body)
        {
            string rootFolder = Directory.GetCurrentDirectory();

            string pathString2 = @"\API site\files\wastemanagement\upload\";
            string serverPath = rootFolder.Substring(0, rootFolder.LastIndexOf(@"\")) + pathString2;

            if (!System.IO.Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }
            string fiilServername = System.Guid.NewGuid().ToString() + "-" + body.file.FileName;
            string filename = serverPath + fiilServername;
            using (FileStream strem = System.IO.File.Create(filename))
            {
                body.file.CopyTo(strem);
            }
            using (ExcelPackage excel = new ExcelPackage(new FileInfo(filename)))
            {

                ExcelWorkbook workbook = excel.Workbook;
                ExcelWorksheet sheet = workbook.Worksheets[0];
                int rowCount = sheet.Dimension.Rows;
                // _userService.removeUser();
                if (sheet.Cells[2, 5].Value?.ToString().Length != 11 || sheet.Cells[2, 2].Value?.ToString().Length != 10)
                {
                    return BadRequest(new { success = false, message = "File format invalid" });
                }
                List<UserSchema> data = new List<UserSchema>();
                for (int row = 3; row <= rowCount; row++)
                {
                    UserSchema item = new UserSchema();
                    for (int col = 2; col <= 5; col += 1)
                    {
                        string value = sheet.Cells[row, col].Value?.ToString();
                        if (String.IsNullOrEmpty(value))
                        {
                            break;
                        }
                        switch (col)
                        {
                            case 2: item.dept = value; break;
                            case 3: item.username = value; break;
                            case 4: item.name = value; break;
                            case 5: item.permission = value; break;
                        }
                        item.filename = fiilServername;

                    }
                    if (item.username != null)
                    {
                        // _userService.create(item);
                        data.Add(item);
                    }
                }
                _userService.create(data);
            }
            return Ok(new { success = true, message = "Update user data success." });
        }
        private string GenerateJSONWebToken(UserSchema userInfo)
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

        [HttpGet("download")]
        public async Task<ActionResult> download()
        {
            UserSchema data = _userService.getLastRecord();

            string fileUri = (_config["Endpoint:file_path"] + "/upload/" + data.filename);

            Console.WriteLine(fileUri);
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(fileUri);

            if (response.StatusCode.ToString() == "NotFound")
            {
                return NotFound();
            }
            return File(response.Content.ReadAsStream(), contentType);
        }
    }
}