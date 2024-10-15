using System.Text.Json;

namespace API.Models
{
    public abstract class BaseModel
    {

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
