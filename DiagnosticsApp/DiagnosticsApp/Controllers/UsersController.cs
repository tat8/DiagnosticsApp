using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.Models;
using DiagnosticsApp.Services.Errors;
using DiagnosticsApp.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private IUserService userService;
        private IErrorService errorService;

        public UsersController(IUserService userService, IErrorService errorService)
        {
            this.userService = userService;
            this.errorService = errorService;
        }
        
        // GET api/users
        [HttpGet]
        public IActionResult Get()
        {
            JsonResult result;
            try
            {
                var userModels = userService.GetUsers();
                result = Json(Ok(userModels));
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        // GET api/users/filters
        [HttpPost("filters")]
        public IActionResult Get(UserModel userModel)
        {
            JsonResult result;
            try
            {
                var userModels = new List<UserModel>();
                if (userModel.UserId != null)
                {
                    userModels.Add(userService.GetUsersById(userModel));
                }
                else if (userModel.Inn != null)
                {
                    userModels.Add(userService.GetUsersByInn(userModel));
                }
                else if (userModel.RoleId != null || (userModel.RoleName != null) || (userModel.FirstName != null) || (userModel.LastName != null) || (userModel.FatherName != null))
                {
                    userModels = (List<UserModel>)userService.GetUsersByRoleFIO(userModel);
                }

                result = Json(Ok(userModels));
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        // POST api/users/add
        [HttpPost("add")]
        public IActionResult Add(UserModel userModel)
        {
            JsonResult result;
            try
            {
                if(ModelState.IsValid)
                {
                    userModel = userService.AddUser(userModel);
                }
                result = Json(Ok(userModel));
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        // POST api/users/edit
        [HttpPost("edit")]
        public IActionResult Edit(UserModel userModel)
        {
            //TODO: test with wrong inn without id / with wrong id
            JsonResult result;
            try
            {
                if(ModelState.IsValid)
                {
                    userService.EditUser(userModel);
                }
                result = Json(Ok());
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

    }
}