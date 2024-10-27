namespace API.Models
{
    public class UserApp
    {

        public required string AppName { get; set; }

        public required List<string> Permissions { get; set; }

    }
}
