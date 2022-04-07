namespace RopeyDVD.Models;
using System.ComponentModel.DataAnnotations;


public class User
{
    [Key]
    public int UserNumber { get; set; }
    public string UserName { get; set; }
    public int UserType { get; set; }
    public string UserPassword { get; set; }
}
