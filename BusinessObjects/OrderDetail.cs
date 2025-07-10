using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects;

public partial class OrderDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? OrchidId { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public int? OrderId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Orchid? Orchid { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Order? Order { get; set; }
}
