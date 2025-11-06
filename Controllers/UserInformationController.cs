// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using hospitalwebapp.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;

// namespace hospitalwebapp.Controllers
// {
//     [Route("[controller]")]
//     public class UserInformationController : Controller
//     {
//         private readonly ILogger<UserInformationController> _logger;
//         private readonly UserInformationInterface service;
//         public UserInformationController(ILogger<UserInformationController> logger
//         , UserInformationInterface userInformationService)
//         {
//             _logger = logger;
//             service = userInformationService;

//         }

//         public IActionResult Index()
//         {
//             return View();
//         }

//         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//         public IActionResult Error()
//         {
//             return View("Error!");
//         }
//         [HttpGet]
//         public Staff GetStaffById(int staffid)
//         {
//             return service.GetCurrentUserId(staffid);
//         }
//     }
// }


