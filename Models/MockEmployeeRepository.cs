using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;
        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee(){Id=1,Name="Alex",Email="SomeEmail@.com",Department=DeptEnum.HR,ImagePath="img1.jpg"},
                new Employee(){Id=2,Name="John",Email="SomeEmail@.com",Department=DeptEnum.IT,ImagePath="img2.jpg"},

            };
        }

        //create
        public Employee Create(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);

            return employee;
        }

        //read
        public Employee GetEmployeeById(int id)
        {
            return _employeeList.FirstOrDefault(emp => emp.Id == id);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _employeeList;
        }
        //update
        public Employee Update(Employee employee)
        {
            var updated_item = _employeeList.FirstOrDefault(e => e.Id == employee.Id);
            if (updated_item != null)
            {
                updated_item.Name = employee.Name;
                updated_item.Email = employee.Email;
                updated_item.Department = employee.Department;
            }
            return updated_item;
        }
        //delete
        public Employee Delete(int id)
        {
            var employee_item = _employeeList.FirstOrDefault(e => e.Id == id);
            if (employee_item != null)
            {
                _employeeList.Remove(employee_item);
            }

            return employee_item;
        }


    }
}
