using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.db;

public enum StoreType
{
    [Display(Name = "Clothes")]
    Clothes = 0,

    [Display(Name = "Emotion")]
    Emotion = 1,

    [Display(Name = "Customization")]
    Customization = 2,

    [Display(Name = "Transport")]
    Transport = 3,
    
    [Display(Name = "Pets")]
    Pets = 4,

    [Display(Name = "FreeStore")]
    FreeStore = 5,

    [Display(Name = "NftStore")]
    NftStore = 6
}