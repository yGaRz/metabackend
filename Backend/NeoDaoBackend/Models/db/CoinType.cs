using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.db;

public enum CoinType
{
    [Display(Name = "SoftCoin")]
    SoftCoin = 0,
    
    [Display(Name = "HardCoin")]
    HardCoin = 1,
    
    [Display(Name = "BitForceCoin")]
    BitForceCoin = 2,
}