using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }


    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Orchid> Orchids { get; set; } = new List<Orchid>();
}
