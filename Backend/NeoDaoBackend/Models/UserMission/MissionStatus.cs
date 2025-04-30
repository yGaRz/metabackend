using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.Mission;

public enum MissionStatus
{
    [Display(Name = "Active")]
    Active = 0,
    [Display(Name = "Completed")]
    Completed = 1,
    [Display(Name = "Failed")]
    Failed = 2
}
