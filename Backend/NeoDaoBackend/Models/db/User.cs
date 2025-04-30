using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NeoDaoBackend.Models.db;

public class User
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public DateTimeOffset LastLoginAt { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool IsOnline { get; set; } = false;

    public virtual Inventory Inventory { get; set; } = null!;
    public virtual UserBalance UserBalance { get; set; } = null!;
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<UserAvatar> UserAvatars { get; set; } = new List<UserAvatar>();
    public virtual ICollection<UserMission> UserMissions { get; set; } = new List<UserMission>();
    public virtual ICollection<UserStorePurchase> UserStorePurchases { get; set; } = new List<UserStorePurchase>();
    public virtual ICollection<UserEmotion> UserEmotions { get; set; } = new List<UserEmotion>();
    public virtual ICollection<AuctionLot> AuctionLots { get; set; } = new List<AuctionLot>();
    public virtual ICollection<UserActiveCustomization> UserActiveCustomizations { get; set; } = new List<UserActiveCustomization>();
    public virtual ICollection<UserAvailableCustomization> UserAvailableCustomizations { get; set; } = new List<UserAvailableCustomization>();
}