using Group5.Data;
using Group5.Models;
using Microsoft.EntityFrameworkCore;

namespace Group5.Service
{
    public class EmployeeService : IEmployeeService 
    {
        ApplicationDbContext ctx;

        public EmployeeService(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task UpdateAmount(StationeryRequest? req)
        {
            var user = await ctx.Users.SingleOrDefaultAsync(a=>a.Id == req!.RequestBy!.Id);
            user!.AmountRequestPerMonth += req!.Total;
            ctx.Users.Update(user);
            await ctx.SaveChangesAsync();    
        }

        public async Task NewUpdateAmount(NewStationeryRequest? req)
        {
            var user = await ctx.Users.SingleOrDefaultAsync(a => a.Id == req!.RequestBy!.Id);
            user!.AmountRequestPerMonth += req!.Total;
            ctx.Users.Update(user);
            await ctx.SaveChangesAsync();
        }
    }
}
