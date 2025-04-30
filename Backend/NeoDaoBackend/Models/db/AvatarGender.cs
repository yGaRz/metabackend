using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.db;

public enum AvatarGender {
    [Display(Name = "MALE")]
    MALE,
    
    [Display(Name = "FEMALE")]
    FEMALE,
}
