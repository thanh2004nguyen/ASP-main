using Group5.Data;
using Group5.Models;

namespace Group5.Service
{
    public interface IRequestService
    {
        Task<StationeryRequest?> FindRequestById(int id);
        Task<NewStationeryRequest?> FindNewRequestById(int id);
        Task<List<int>> ListRequest(Employee emp);
        Task ApplyStockLv(int? quantity, int id);
        Task ApplyUselessLv(int? days, int id);
        Task<List<RequestItem>> ListRequestItems(int id);
    }
}
