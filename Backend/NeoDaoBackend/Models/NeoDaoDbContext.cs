using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models.db;
using static NeoDaoBackend.Models.Constants;
using Stream = NeoDaoBackend.Models.db.Stream;

namespace NeoDaoBackend.Models;

public partial class NeoDaoDbContext : DbContext
{
    public NeoDaoDbContext()
    {
    }

    public NeoDaoDbContext(DbContextOptions<NeoDaoDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Session> UserSessions { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserAvatar> UserAvatar { get; set; }
    public virtual DbSet<db.Group> Groups { get; set; }
    public virtual DbSet<UserGroup> UserGroups { get; set; }
    public virtual DbSet<Inventory> Inventories { get; set; }
    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<Equipment> Equipments { get; set; }
    public virtual DbSet<PlayerLocation> PlayerLocations { get; set; }
    public virtual DbSet<UserBalance> UserBalances { get; set; }
    public virtual DbSet<UserBalanceTransaction> UserBalanceTransactions { get; set; }
    public virtual DbSet<UserRelation> UserRelations { get; set; }
    public virtual DbSet<UserMission> UserMissions { get; set; }
    public virtual DbSet<MissionObjectives> MissionObjectives { get; set; }
    public virtual DbSet<db.UserTransport> UserTransports { get; set; }
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
    public virtual DbSet<Channel> Channels { get; set; }
    public virtual DbSet<StoreItem> StoreItems { get; set; }
    public virtual DbSet<UserStorePurchase> UserStorePurchases { get; set; }
    public virtual DbSet<FreeTradeItem> FreeTradeItems { get; set; }
    public virtual DbSet<Stream> Streams { get; set; }
    public virtual DbSet<AuctionLot> AuctionLots { get; set; }
    public virtual DbSet<UserEmotion> UserEmotions { get; set; }
    public virtual DbSet<UserActiveCustomization> UserActiveCustomizations { get; set; }
    public virtual DbSet<UserAvailableCustomization> UserAvailableCustomizations { get; set; }
    public virtual DbSet<Auction> Auctions { get; set; }
    public virtual DbSet<Promocode> Promocodes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string pgConnectionEnv = Environment.GetEnvironmentVariable("PG_CONNECTION") ??
                                     throw new ApplicationException("Environment variable PG_CONNECTION is not set!");
            optionsBuilder.UseNpgsql(pgConnectionEnv!);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NeoDaoDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}