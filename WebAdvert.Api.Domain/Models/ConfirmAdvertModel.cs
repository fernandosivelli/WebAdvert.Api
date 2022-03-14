using WebAdvert.Api.Domain.Enums;

namespace WebAdvert.Api.Domain.Models
{
    public class ConfirmAdvertModel
    {
        public string Id { get; set; }
        public AdvertStatusEnum Status { get; set; }
    }
}
