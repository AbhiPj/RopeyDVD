namespace RopeyDVD.Models;
using System.ComponentModel.DataAnnotations;


public class Studio
{
    [Key]
    public int StudioNumber { get; set; }   
    public string StudioName { get; set; }
}
