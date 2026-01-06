using Microsoft.EntityFrameworkCore;
namespace MVCDHProject.Models
{
    public class CustomerSqlDAL : ICustomerDAL
    {
        private readonly MVCCoreDbContext context;

        //Passing MVCCoreDbContext class object as dependency object in constructor
        public CustomerSqlDAL(MVCCoreDbContext context)
        {
            this.context = context;
        }
        public List<Customer> Customers_Select()
        {
            var customer=context.Customers.Where(C => C.Status == true).ToList();
            return customer;
        }
        public Customer Customer_Select(int Custid)
        {
            //var customer = context.Customers.Where(C => C.Custid == Custid).Single();
            var customer = context.Customers.Find(Custid);
            return customer;
        }
        public void Customer_Insert(Customer customer)
        {
            context.Customers.Add(customer);
            context.SaveChanges();
        }
        public void Customer_Update(Customer customer)
        {
            customer.Status= true;
            //context.Entry(customer).State = EntityState.Modified;
            context.Update(customer);
            context.SaveChanges();
        }
        public void Customer_Delete(int Custid)
        {
            Customer customer = context.Customers.Find(Custid);
            customer.Status = false;
            context.SaveChanges();
        }
    }
}
