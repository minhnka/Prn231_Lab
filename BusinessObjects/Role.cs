using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
