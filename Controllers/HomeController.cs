using Microsoft.AspNetCore.Mvc;
using ASp.netCore_empty_tutorial.Models;
using System;
using System.Collections.Generic;
using ASp.netCore_empty_tutorial.ViewModels;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using ASp.netCore_empty_tutorial.Security;
using System.Linq;

namespace ASp.netCore_empty_tutorial.Controllers
{

    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger logger;
        private readonly IDataProtector _protector;
        public HomeController(IEmployeeRepository employeeRepository, IHostEnvironment hostEnvironment, ILogger<HomeController> logger,
            IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
            _employeeRepository = employeeRepository;
            _hostEnvironment = hostEnvironment;
            this.logger = logger;
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetEmployees().Select(e =>
            {
                e.EncryptedId = _protector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
        }

        [AllowAnonymous]
        public ViewResult Details(string id)
        {
            //Logging information
            logger.LogTrace("Log Trace");
            logger.LogDebug("Log Debug");
            logger.LogInformation("Log Information");
            logger.LogWarning("Log Warning");
            logger.LogError("Log Error");
            logger.LogCritical("Log Critical");
            int decryptedEmployeeId = Convert.ToInt32(_protector.Unprotect(id));

            Employee detailsById = _employeeRepository.GetEmployeeById(decryptedEmployeeId);

            if (detailsById == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", decryptedEmployeeId);
            }
            return View(detailsById);
        }
        [HttpGet]

        public ViewResult Create()
        {
            return View();
        }
        [HttpPost]

        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UpladFileMethod(model);

                var newEmployee = new Employee()
                {
                    Name = model.Name,
                    Department = model.Department,
                    Email = model.Email,
                    ImagePath = uniqueFileName
                };
                _employeeRepository.Create(newEmployee);
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]

        public ViewResult Edit(string id)
        {
            int decryptedId = Convert.ToInt32(_protector.Unprotect(id));
            Employee employee = _employeeRepository.GetEmployeeById(decryptedId);

            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Department = employee.Department,
                Email = employee.Email,
                ExistingPhotoPath = employee.ImagePath
            };

            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployeeById(model.Id);
                employee.Name = model.Name;
                employee.Department = model.Department;
                employee.Email = model.Email;
                if (model.Photos != null)
                {
                    if (model.ExistingPhotoPath != null)
                    {
                        string fileFullPath = Path.Combine(_hostEnvironment.ContentRootPath, @"wwwroot\images", model.ExistingPhotoPath);
                        System.IO.File.Delete(fileFullPath);
                    }
                    employee.ImagePath = UpladFileMethod(model);
                }

                _employeeRepository.Update(employee);
                return RedirectToAction("Index");
            }
            return View();
        }



        private string UpladFileMethod(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photos != null && model.Photos.Count > 0)
            {
                foreach (IFormFile photo in model.Photos)
                {
                    string pathFoulder = Path.Combine(_hostEnvironment.ContentRootPath, @"wwwroot\images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(pathFoulder, uniqueFileName);
                    using (var filestream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(filestream);
                    }

                }
            }

            return uniqueFileName;
        }

        //delete
        [AllowAnonymous]
        public IActionResult Delete(string id)
        {
            int decryptedId = Convert.ToInt32(_protector.Unprotect(id));

            var result = _employeeRepository.Delete(decryptedId);
            if (result != null)
            {
                ViewBag.Message = result.Name;
                return View("SuccesAction");
            }
            return RedirectToAction("Index");
        }
    }
}
