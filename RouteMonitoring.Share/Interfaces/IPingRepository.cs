using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RouteMonitoring.Domain.Interfaces
{
    public interface IPingRepository
    {
        Task<bool> SavePingAsync(ResponseFormat ping);

        Task<ResponseFormat?> GetAsync(Guid id);
    }
}
