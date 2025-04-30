using FluentValidation;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Validation;
using System.ComponentModel.DataAnnotations;
using static NeoDaoBackend.Validation.ErrorCode;

namespace NeoDaoBackend.Util;

public class ValidationUtils
{
    public static bool BasicWsValidation(IInputMessageData? inputMessageData, IValidationStorage validationStorage)
    {
        if (inputMessageData == null)
        {
            AddEmptyDataError(validationStorage);
            return false;
        }
        var context = new ValidationContext(inputMessageData, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(inputMessageData, context, validationResults, true))
        {
            validationStorage.AddError(InvalidData, "Request data is invalid");
        }
        return validationStorage.IsValid;
    }

    public static string? GetValidationError<T>(T value, AbstractValidator<T> validator)
    {
        var validationResult = validator.Validate(value);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                return ($"Property {error.PropertyName} failed validation. Error was: {error.ErrorMessage}");
            }
        }
        return null;
    }

    public static bool ValidatePaginationModel(PaginationModel? request, IValidationStorage validationStorage)
    {
        if (request == null)
        {
            AddEmptyDataError(validationStorage);
            return false;
        }
        var context = new ValidationContext(request, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, validationResults, true))
        {
            validationStorage.AddError(InvalidData, "Request data is invalid");
        }
        return validationStorage.IsValid;
    }

    public static void AddUnknownUserError(IValidationStorage validationStorage, Guid userId)
    {
        validationStorage.AddError(UnknownUser, $"User with Id {userId} does not exist");
    }

    public static void AddUnknownInventoryItemError(IValidationStorage validationStorage, Guid itemId)
    {
        validationStorage.AddError(UnknownInventoryItem, $"Inventory item {itemId} does not exist");
    }

    public static void AddEmptyDataError(IValidationStorage validationStorage)
    {
        validationStorage.AddError(ErrorCode.EmptyData, "Request data is empty");
    }
    
    public static void AddUnknownStreamError(IValidationStorage validationStorage, Guid streamId)
    {
        validationStorage.AddError(ErrorCode.UnknownStream, $"Stream with Id {streamId} does not exist");
    }
}
