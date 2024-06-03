using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserManagementApi.Models;
using UserManagementApi.Models.Authentication.SignUp;

namespace UserManagementApi.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
    [ApiController]
    public class AuthenticationController:ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserManager<IdentityUser> userManager,  RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterUser registerUser, string role)
        {
            //Check if user exists
            var userExist=await _userManager.FindByEmailAsync(registerUser.Email);//email is unique
            if(userExist != null)
            {
                return StatusCode(403);//Forbidden action
            }
            //Add the user to the database
            IdentityUser user = new()
            { 
                Email = registerUser.Email, SecurityStamp = Guid.NewGuid().ToString(), UserName = registerUser.UserName
            };
            if(await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ResponseModel { Status = "Error", Message = "User couldn't be created" });//Forbidden action

                }
                //assign a role to user
                await _userManager.AddToRoleAsync(user, role);
                return StatusCode(StatusCodes.Status201Created, new ResponseModel { Status = "Success", Message = "User created successfully." });

            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "This role doesn't exist." });
            }


        }
    }
}
