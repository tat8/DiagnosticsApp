using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.Models;
using DiagnosticsApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private IUserService userService;
        public UsersController(IUserService userService)
        {
            this.userService = userService;
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
                var errorModel = BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        // GET api/users
        [HttpGet("filters")]
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
                var errorModel = BuildError(ex);
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
                userModel = userService.AddUser(userModel);
                result = Json(Ok(userModel));
            }
            catch (Exception ex)
            {
                var errorModel = BuildError(ex);
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
                userService.EditUser(userModel);
                result = Json(Ok());
            }
            catch (Exception ex)
            {
                var errorModel = BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        private ErrorModel BuildError(Exception ex)
        {
            var errorModel = new ErrorModel()
            {
                CustomMessage = "Некорректный запрос",
                Message = ex.Message,
                InnerException = ex.InnerException,
                Source = ex.Source
            };

            return errorModel;
        }
    }
}