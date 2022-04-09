namespace RopeyDVD.Models;
using System.ComponentModel.DataAnnotations;


public class Producer
{
    [Key]
    public int ProducerNumber { get; set; } 
    public string ProducerName { get; set; }
    public ICollection<DVDTitle> DVDTitle { get; set; }
}
