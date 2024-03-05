using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderBookApiHW.Logger.Entity;

public class OrderBookLogsEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column(TypeName = "float")]
    public double Price { get; set; }
    
    public int Count { get; set; }
    
    [Column(TypeName = "float")]
    public double Amount { get; set; }
    
    [Column(TypeName = "float")]
    public double Rate { get; set; }
    
    [Column(TypeName = "float")]
    public double Period { get; set; }
    
    [MaxLength(255)]
    public string Pair { get; set; }
    
    [MaxLength(255)]
    public string Symbol { get; set; }

    public DateTime OrderTime { get; set; }
}
