using MongoDB.Bson;

namespace API.Models
{
    public class RefreshToken
    {
        public ObjectId Id { get; set; }

        public ObjectId UserId { get; set; }

        public required string Token { get; set; }

        public required string Audience { get; set; }    

        public DateTime Expiry { get; set; }

        public required String DeviceId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
