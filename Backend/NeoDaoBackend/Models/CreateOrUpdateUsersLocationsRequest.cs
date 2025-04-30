using FluentValidation;

namespace NeoDaoBackend.Models.UserLocation;

public class CreateOrUpdateUsersLocationsRequest
{
    public List<CreatePlayerLocationRequest> PlayerLocations { get; set; } = null!;

    public class CreatePlayerLocationRequestCollectionValidator : AbstractValidator<CreateOrUpdateUsersLocationsRequest>
    {
        public CreatePlayerLocationRequestCollectionValidator()
        {
            RuleFor(x => x.PlayerLocations.Select(x => x.UserId).ToList())
                .NotEmpty()
                .WithMessage("Player locations shall be specified").WithName("EmptyPayload")
                .Must(list => list.Count == list.Distinct().Count())
                .WithMessage("User ids shall be distinct").WithName("InvalidUserIds");
        }
    }
}
