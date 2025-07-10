using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects;

public partial class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? AccountId { get; set; }

    public DateOnly? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public decimal? TotalAmount { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Account? Account { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
