using Group5.Data;
using Group5.Models;
using Group5.Models.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Group5.Service
{
    public class RequestService: IRequestService
    {
        ApplicationDbContext ctx;

        public RequestService(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<List<RequestItem>> ListRequestItems(int id)
        {
            return await ctx.RequestItems
                .Include(a=>a.StationeryItem)
                .Where(a => a.StationeryRequestId == id)
                .ToListAsync();
        }


        public async Task<StationeryRequest?> FindRequestById(int id)
        {
           var request = await ctx.StationeryRequests
             .Include(a => a.RequestStatus!)
             .Include(a => a.RequestBy!)
             .Include(a => a.RequestItems!)
                .ThenInclude(a => a.StationeryItem!)
             .Include(a => a.RequestBy!.Departments!)
             .Include(a => a.RequestBy!.EmployeePositions!)
             .SingleOrDefaultAsync(a => a.Id == id!);

            return request;
        }

        public async Task<NewStationeryRequest?> FindNewRequestById(int id)
        {
            var request = await ctx.NewStationeryRequests
              .Include(a => a.RequestStatus!)
              .Include(a => a.RequestBy!)
              .Include(a => a.RequestBy!.Departments!)
              .Include(a => a.RequestBy!.EmployeePositions!)
              .SingleOrDefaultAsync(a => a.Id == id!);

            return request;
        }

     public async Task<List<int>> ListRequest(Employee emp)
            {
               var requests = await ctx.StationeryRequests
                 .Where(a=>a.RequestBy!.Id == emp.Id)
                 .ToListAsync();

            var newStationeryRequests = await ctx.NewStationeryRequests
                 .Where(a => a.RequestBy!.Id == emp.Id)
                .ToListAsync();

            var combinedRequests = requests.Cast<RequestBase>()
                .Concat(newStationeryRequests.Cast<RequestBase>())
                .ToList();

            return combinedRequests.Select(request => request.Id).ToList();
        }


        public async Task ApplyStockLv(int? quantity,int id)
        {
             var listStocklv = await ctx.StockLevels
                        .Where(a => a.MinQuantity > quantity)
                        .ToListAsync();
         

            if (listStocklv == null || listStocklv.Count == 0)
            {
                var stationeryItem = await ctx.StationeryItems.SingleOrDefaultAsync(a => a.Id == id);
                stationeryItem!.StockLevel = null;
                ctx.StationeryItems.Update(stationeryItem);
                await ctx.SaveChangesAsync();

            }
            else
            {
                var minStockLv = listStocklv.OrderBy(a => a.MinQuantity).First();
   
                var stationeryItem = await ctx.StationeryItems.SingleOrDefaultAsync(a => a.Id == id);
                stationeryItem!.StockLvId = minStockLv.Id;
                ctx.StationeryItems.Update(stationeryItem);
                await ctx.SaveChangesAsync();
            }
        }


        public async Task ApplyUselessLv(int? days, int id)
        {
           
            Console.WriteLine($"dayssssssssssssssssssssssss:{days}");
            var listStocklv = await ctx.UseLessItems
                       .Where(a => a.MaxTime < days)
                       .ToListAsync();

            var level0 = await ctx.UseLessItems
                   .Where(a => a.MaxTime == 0)
                   .SingleOrDefaultAsync();

            if (listStocklv == null || listStocklv.Count == 0)
            {
                Console.WriteLine($"stocklvvvvvvvvvvvvvvv:{listStocklv}");
                var stationeryItem = await ctx.StationeryItems.SingleOrDefaultAsync(a => a.Id == id);
                stationeryItem!.UseLessId = level0!.Id;
                ctx.StationeryItems.Update(stationeryItem);
                await ctx.SaveChangesAsync();
            }
            else
            {
                foreach (var item in listStocklv)
                {
                    Console.WriteLine($"aaaaaaaaaaaaaaaaaaaaaaaaaaaaa:{item.MaxTime}");
                }
              
            
                var MaxStockLv = listStocklv
                    .OrderByDescending(a => a.MaxTime).First();
                Console.WriteLine($"aaaaaaaaaaaaaaaaaaaaaaaaaaaaa:{MaxStockLv.MaxTime}");
                var stationeryItem = await ctx.StationeryItems.SingleOrDefaultAsync(a => a.Id == id);
                stationeryItem!.UseLessId = MaxStockLv.Id;
                ctx.StationeryItems.Update(stationeryItem);
                await ctx.SaveChangesAsync();
            }
        }


    }
}












