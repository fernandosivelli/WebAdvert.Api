using System.Threading.Tasks;
using WebAdvert.Api.Domain.Models;

namespace WebAdvert.Api.Services
{
    public interface IAdvertStorageService
    {
        Task<string> Add(AdvertModel model);
        Task Confirm(ConfirmAdvertModel model);
        Task<bool> CheckHealth();
    }
}
