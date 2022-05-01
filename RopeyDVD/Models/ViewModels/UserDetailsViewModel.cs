namespace RopeyDVD.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
