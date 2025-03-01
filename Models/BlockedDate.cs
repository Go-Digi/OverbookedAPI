using System.Text.Json.Serialization;

namespace Overbookedapi.Models;

public class BlockedDate
{
    public int BlockedDateId { get; set; }
    public Guid RoomTypeId { get; set; }
    public string Note { get; set; }
    public DateTime Date { get; set; }
    public int RoomCount { get; set; }
    
    [JsonIgnore]
    public RoomType RoomType { get; set; }
}