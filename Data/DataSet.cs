using Overbookedapi.Models;
using Microsoft.EntityFrameworkCore;

namespace Overbookedapi.Data;

public class DataSet : DbContext
{
    public DataSet(DbContextOptions<DataSet> options) : base(options)
    {
    }
    
    public virtual DbSet<Hotel> Hotels { get; set; }
    public virtual DbSet<RoomType> RoomTypes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // hotel model
        modelBuilder.Entity<Hotel>()
            .HasMany(g => g.RoomTypes)
            .WithOne(g => g.Hotel)
            .HasForeignKey(g => g.HotelId)
            .IsRequired();
        
        // room type model
        modelBuilder.Entity<RoomType>()
            .HasMany(g => g.RoomRates)
            .WithOne(g => g.RoomType)
            .HasForeignKey(g => g.RoomTypeId)
            .IsRequired();
    }
}