using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class AccessCodeRequest : BaseModel
    {

        public ObjectId Id { get; set; }

        public required string EmailAddress { get; set; }

        public required string QuickAccessCode { get; set; }

        public Guid LoginCode { get; set; }

        public DateTime AccessCodeExpires { get; set; }

        public Guid DeviceId { get; set; }

    }
}
