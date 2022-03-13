using WebAdvert.Api.Domain.Enums;

namespace WebAdvert.Api.Domain.Models
{
    public class ConfirmAdvertModel
    {
        public int Id { get; set; }
        public AdvertStatusEnum Status { get; set; }
    }
}
