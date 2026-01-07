using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCDHProject.Models;
namespace MVCDHProject.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerDAL obj;

        //CustomerXmlDAL obj=new CustomerXmlDAL();
        public CustomerController(ICustomerDAL obj)     //Constructor Injection
        {
            this.obj = obj;
        }

        [AllowAnonymous]
        public ViewResult DisplayCustomers()
        {
            //Manual Injection
            //var services = HttpContext.RequestServices;
            //var obj = (ICustomerDAL)services.GetService(typeof(ICustomerDAL));

            return View(obj.Customers_Select());
        }

        [AllowAnonymous]
        public ViewResult DisplayCustomer(int Custid)
        {
            var customer = obj.Customer_Select(Custid);
            return View(customer);
        }

        [HttpGet,Authorize]
        public ViewResult AddCustomer()
        {
            return View();
        }

        [HttpPost,Authorize]
        public RedirectToActionResult AddCustomer(Customer customer)
        {
            obj.Customer_Insert(customer);
            return RedirectToAction("DisplayCustomers");
        }

        [Authorize]
        public ViewResult EditCustomer(int Custid)
        {
            return View(obj.Customer_Select(Custid));
        }

        [Authorize]
        public RedirectToActionResult UpdateCustomer(Customer customer)
        {
            obj.Customer_Update(customer);
            return RedirectToAction("DisplayCustomers");
        }

        [Authorize]
        public RedirectToActionResult DeleteCustomer(int Custid)
        {
            obj.Customer_Delete(Custid);
            return RedirectToAction("DisplayCustomers");
        }   
    }
}
