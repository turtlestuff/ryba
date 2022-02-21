#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ryba.Data;

public class RybaContext : DbContext
{
    public RybaContext(DbContextOptions<RybaContext> options) : base(options)
    {
    }

    public DbSet<RybaUser> Users { get; set; }
}

public class RybaUser
{
    public string Id { get; set; }
    public List<PortablePin> PortablePins { get; set; }
}

public class PortablePin
{
    public int Id { get; set;  }

    public string Channel { get; set; }
    public string Message { get; set; }
}
