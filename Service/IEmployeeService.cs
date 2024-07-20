using Group5.Models;

namespace Group5.Service
{
    public interface IEmployeeService
    {
       Task UpdateAmount(StationeryRequest? req);
       Task NewUpdateAmount(NewStationeryRequest? req);

    }
}
