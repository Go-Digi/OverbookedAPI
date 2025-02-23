namespace Overbookedapi.Models;

public class Hotel
{
    public int HotelId { get; set; }
    public string HotelName { get; set; }
    // public string HotelLogoUrl {get;set;}
    
    public List<RoomType> RoomTypes { get; set; }
    public List<User> Users { get; set; }


}