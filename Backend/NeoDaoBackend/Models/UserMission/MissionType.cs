using System.ComponentModel.DataAnnotations;

namespace NeoDaoBackend.Models.Mission;

public enum MissionType
{
    [Display(Name = "Story")]
    Story = 0,
    [Display(Name = "Daily")]
    Daily = 1,
    [Display(Name = "Weekly")]
    Weekly = 2,
    [Display(Name = "Monthly")]
    Monthly = 3,
    [Display(Name = "Events")]
    Events = 4
}
