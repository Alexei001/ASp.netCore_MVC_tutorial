using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;


namespace ASp.netCore_empty_tutorial.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        protected readonly AppDbContext _appContext;
        private readonly ILogger<SQLEmployeeRepository> logger;

        public SQLEmployeeRepository(AppDbContext appContext, ILogger<SQLEmployeeRepository> logger)
        {
            _appContext = appContext;
            this.logger = logger;
            if (!_appContext.Employees.Any())
            {
                _appContext.Employees.AddRange(
                                 new Employee() { Name = "Alex", Email = "SomeEmail@.com", Department = DeptEnum.HR, ImagePath = "img1.jpg" },
                                new Employee() { Name = "John", Email = "SomeEmail@.com", Department = DeptEnum.IT, ImagePath = "img2.jpg" },
                                new Employee() { Name = "Jerry", Email = "SomeEmail@.com", Department = DeptEnum.HR, ImagePath = "img3.jpg" }
              );
                _appContext.SaveChanges();
            }
        }
        //Create
        public Employee Create(Employee employee)
        {
            if (employee != null)
            {
                employee.ImagePath ??= "img1.jpg";
                _appContext.Employees.Add(employee);

                _appContext.SaveChanges();
            }

            return employee;
        }
        //Read
        public Employee GetEmployeeById(int id)
        {
            //logging info
            logger.LogTrace("Log Trace");
            logger.LogDebug("Log Debug");
            logger.LogInformation("Log Information");
            logger.LogWarning("Log Warning");
            logger.LogError("Log Error");
            logger.LogCritical("Log Critical");

            return _appContext.Employees.Find(id);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _appContext.Employees.ToList();
        }
        //Update
        public Employee Update(Employee employeeChanges)
        {
            if (employeeChanges != null)
            {
                var employee_item = _appContext.Employees.Attach(employeeChanges);
                employee_item.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _appContext.SaveChanges();
            }

            return employeeChanges;

        }
        //delete
        public Employee Delete(int id)
        {
            var deletedEmploye_item = _appContext.Employees.Find(id);
            if (deletedEmploye_item != null)
            {
                _appContext.Employees.Remove(deletedEmploye_item);
                _appContext.SaveChanges();

            }
            return deletedEmploye_item;
        }




    }
}
