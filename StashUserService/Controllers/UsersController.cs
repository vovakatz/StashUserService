using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StashUserService.Business;
using StashUserService.Model;

namespace StashUserService.Controllers
{
    [Route("v1/[controller]")]
    public class UsersController : Controller
    {
        IUserManager _userManager;

        public UsersController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                return Json(_userManager.GetUsers());
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new Error(ex.Message));
            }
        }

        [HttpPost]
        public JsonResult Post([FromBody]User user)
        {
            List<string> errors;
            if (_userManager.ValidateUser(user, out errors))
            {
                try
                {
                    _userManager.SaveUser(user);
                    User createdUser = _userManager.GetUser(user.Email);
                    Response.StatusCode = 201;
                    return Json(user);
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500;
                    return Json(new Error(ex.Message));
                }
            }
            else
            {
                Response.StatusCode = 422;
                return Json(errors);
            }
        }
    }
}
