using Overbookedapi.Models;
using Microsoft.EntityFrameworkCore;

namespace Overbookedapi.Data;

public class DataSet : DbContext
{
    public DataSet(DbContextOptions<DataSet> options) : base(options)
    {
    }
    
    public virtual DbSet<Hotel> Hotels { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public virtual DbSet<RoomType> RoomTypes { get; set; }
    public virtual DbSet<RoomRate> RoomRates { get; set; }
    public virtual DbSet<BlockedDate> BlockedDates { get; set; }
    public virtual DbSet<Amenity> Amenities { get; set; }
    public virtual DbSet<RoomAmenity> RoomAmenities { get; set; }
    public virtual DbSet<Reservation> Reservations { get; set; }
    public virtual DbSet<ReservationRoom> ReservationRooms { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // hotel model
        modelBuilder.Entity<Hotel>()
            .HasMany(g => g.RoomTypes)
            .WithOne(g => g.Hotel)
            .HasForeignKey(g => g.HotelId)
            .IsRequired();
        modelBuilder.Entity<Hotel>()
            .HasMany(g => g.Users)
            .WithOne(g => g.Hotel)
            .HasForeignKey(g => g.HotelId)
            .IsRequired();
        modelBuilder.Entity<Hotel>()
            .HasMany(g => g.Amenities)
            .WithOne(g => g.Hotel)
            .HasForeignKey(g => g.HotelId)
            .IsRequired();
        
        // room type model
        modelBuilder.Entity<RoomType>()
            .HasMany(g => g.RoomRates)
            .WithOne(g => g.RoomType)
            .HasForeignKey(g => g.RoomTypeId)
            .IsRequired();
        modelBuilder.Entity<RoomType>()
            .HasMany(g => g.BlockedDates)
            .WithOne(g => g.RoomType)
            .HasForeignKey(g => g.RoomTypeId)
            .IsRequired();
        modelBuilder.Entity<RoomType>()
            .HasMany(g => g.RoomAmenities)
            .WithOne(g => g.RoomType)
            .HasForeignKey(g => g.RoomTypeId)
            .IsRequired();
        
        // user model
        modelBuilder.Entity<User>()
            .HasMany(g => g.UserActivityLogs)
            .WithOne(g => g.User)
            .HasForeignKey(g => g.UserId)
            .IsRequired();
        
        
        // amenity
        modelBuilder.Entity<Amenity>()
            .HasMany(g => g.RoomAmenities)
            .WithOne(g => g.Amenity)
            .HasForeignKey(g => g.AmenityId)
            .IsRequired();
        
        modelBuilder.Entity<ReservationRoom>()
            .HasOne(rr => rr.Reservation)
            .WithMany(r => r.ReservationRooms)
            .HasForeignKey(rr => rr.ReservationId);

        modelBuilder.Entity<ReservationRoom>()
            .HasOne(rr => rr.RoomType)
            .WithMany()
            .HasForeignKey(rr => rr.RoomTypeId);

        modelBuilder.Entity<ReservationRoom>()
            .HasOne(rr => rr.RoomRate)
            .WithMany()
            .HasForeignKey(rr => rr.RoomRateId);
    }
}