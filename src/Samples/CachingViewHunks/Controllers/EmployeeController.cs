using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CachingViewHunks.Models;
using Spark;

namespace CachingViewHunks.Controllers
{
    public class EmployeeController : Controller
    {
        public ActionResult Index(int? page)
        {
            var data = new NorthwindDataContext();

            const int pageSize = 3;

            // This example passes lambda functions that are called
            // when the view passes into the cache-miss part of the template.
            // They take advantage to closure scope to use local variables and 
            // action arguments.

            ViewData["page"] = page ?? 1;
            ViewData["pageCount"] = ValueHolder.For(() => (data.Employees.Count() + pageSize - 1) / pageSize);
            ViewData["employees"] = ValueHolder.For(() =>
            {
                ++Application.FetchEmployeeListCalls;

                var employeesInOrder = data.Employees
                    .OrderBy(x => x.LastName);

                return employeesInOrder
                    .Skip(((page ?? 1) - 1) * pageSize)
                    .Take(pageSize);
            });

            return View();
        }

        public ActionResult Details(int id)
        {
            // This example uses a method reference instead of a lambda 
            // with closure variables. The key property is passed along to make this work

            ViewData["employee"] = ValueHolder.For<int, Employee>(id, FetchEmployee);

            return View();
        }

        private static Employee FetchEmployee(int employeeID)
        {
            ++Application.FetchEmployeeDetailCalls;

            var data = new NorthwindDataContext();
            return data.Employees.Single(x => x.EmployeeID == employeeID);
        }
    }

}
