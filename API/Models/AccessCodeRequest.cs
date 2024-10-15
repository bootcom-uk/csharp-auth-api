using Models;
using MongoDB.Bson;

namespace API.Models
{
    public class AccessCodeRequest : BaseModel
    {

        public ObjectId Id { get; set; }

        public ObjectId UserId { get; set; }

        public required string EmailAddress { get; set; }

        public required string QuickAccessCode { get; set; }

        public required string Audience { get; set; }

        public Guid LoginCode { get; set; }

        public DateTime AccessCodeExpires { get; set; }

        public required string DeviceId { get; set; }

    }
}
