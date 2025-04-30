using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.InventoryItems;

public enum SlotType
{
    [Display(Name = "FullSet")]
    FullSet = 0,

    [Display(Name = "Headgear")]
    Headgear = 1,

    [Display(Name = "Accessory")]
    Accessory = 2,

    [Display(Name = "Companion")]
    Companion = 3,
        
    [Display(Name = "Hands")]
    Hands = 4,
    
    [Display(Name = "Head")]
    Head = 5,
    
    [Display(Name = "Hair")]
    Hair = 6
}