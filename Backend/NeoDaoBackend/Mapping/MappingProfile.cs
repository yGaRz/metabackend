using AutoMapper;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserEquipment;
using NeoDaoBackend.Models.InventoryItems;
using NeoDaoBackend.Models.NFTAuction;
using NeoDaoBackend.Models.StoreItems;
using NeoDaoBackend.Models.Streams;
using Stream = NeoDaoBackend.Models.db.Stream;

namespace NeoDaoBackend.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, GetItemsUserInventoryDTO>().ReverseMap();
        
        CreateMap<Equipment, GetUserEquipmentsData>().ReverseMap();
        CreateMap<AuctionLotWithDetails, AuctionLotDTO>().ReverseMap();

        CreateMap<AuctionLotWithDetails, GetItemsUserInventoryDTO>()
            .ForMember(dest => dest.ItemId, opt => 
                opt.MapFrom(src => src.InventoryItemId))
            .ReverseMap();
        
        CreateMap<Stream, StreamResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.StreamId))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
            .ReverseMap();
        
        CreateMap<StoreItem, UserStoreItemsDTO>()
            .ForMember(dest => dest.IsBought, opt => opt.Ignore());
    }
}