using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Models
{
    public interface IEmployeeRepository
    {
        Employee GetEmployeeById(int id);
        IEnumerable<Employee> GetEmployees();
        Employee Create(Employee employee);
        Employee Delete(int id);
        Employee Update(Employee employee);

    }
}
