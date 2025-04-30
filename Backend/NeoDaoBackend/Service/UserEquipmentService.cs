using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserEquipment;
using NeoDaoBackend.Repository;
using NeoDaoBackend.Util;
using NeoDaoBackend.Validation;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace NeoDaoBackend.Service;

public class UserEquipmentService
{
    private readonly ILogger<UserEquipmentService> _logger;
    private readonly IValidationStorage _validationStorage;
    private readonly IUserEquipmentRepository _userEquipmentRepository;
    private readonly IUserInventoryRepository _userInventoryRepository;
    private readonly IMapper _mapper;
    private readonly UserInventoryService _userInventoryService;
    private readonly IUserDataRepository _userDataRepository;
    private readonly UserDataService _userDataService;
    
    public UserEquipmentService(ILogger<UserEquipmentService> logger, IValidationStorage validationStorage,
        IUserEquipmentRepository userEquipmentRepository, IUserInventoryRepository userInventoryRepository,
        IMapper mapper, UserInventoryService userInventoryService, IUserDataRepository userDataRepository,
        UserDataService userDataService)
    {
        _logger = logger;
        _validationStorage = validationStorage;
        _userEquipmentRepository = userEquipmentRepository;
        _userInventoryRepository = userInventoryRepository;
        _mapper = mapper;
        _userInventoryService = userInventoryService;
        _userDataRepository = userDataRepository;
        _userDataService = userDataService;
    }

    #region Actions

    public async Task<IEnumerable<GetUserEquipmentsData>> GetEquipments(Guid userId, CancellationToken ct)
    {
        bool isValid = await _userInventoryService.ValidateGetItems(userId, ct);
        if (!isValid)
        {
            return null!;
        }

        Inventory inventory = await _userEquipmentRepository.GetInventory(userId, ct);
        IEnumerable<Equipment> userItemsEquipments = await _userEquipmentRepository.GetEquipments(inventory.InventoryId, ct);
        return _mapper.Map<IEnumerable<GetUserEquipmentsData>>(userItemsEquipments);
    }
    
    public async Task<bool> CreateEquipment(CreateUserEquipmentData request, CancellationToken ct)
    {
        bool isValid = await ValidateCreateEquipment(request, request.UserId, ct);
        if (!isValid)
        {
            return false;
        }

        Inventory inventory = await _userEquipmentRepository.GetInventory(request.UserId, ct);
        var equipment = new Equipment()
        {
            ItemId = request!.ItemId,
            InventoryId = inventory.InventoryId,
            SlotType = request.SlotType
        };
        _logger.LogInformation($"Creating Equipment with inventory: {inventory.InventoryId}");
        
        await _userEquipmentRepository.CreateEquipment(equipment, ct);
        return true;
    }
    
    public async Task<bool> UpdateEquipment(CreateUserEquipmentData request, CancellationToken ct)
    {
        bool isValid = await ValidateUpdateEquipment(request, request.UserId, ct);
        if (!isValid)
        {
            return false;
        }
        Inventory inventory = await _userEquipmentRepository.GetInventory(request.UserId, ct);
        Equipment equipment = (await _userEquipmentRepository.GetEquipmentBySlotType(inventory.InventoryId, request.SlotType, ct))!;
        equipment.ItemId = request.ItemId;
        await _userEquipmentRepository.UpdateEquipment(equipment, ct);
        return true;
    }
    
    public async Task<bool> DeleteEquipment(DeleteEquipmentData request, CancellationToken ct)
    {
        bool isValid = await ValidateDeleteEquipment(request, ct);
        if (!isValid)
        {
            return false;
        }
        Inventory inventory = await _userEquipmentRepository.GetInventory(request.UserId, ct);
        await _userEquipmentRepository.DeleteEquipment(inventory.InventoryId, request.SlotType, ct);
        return true;
    }

    #endregion

    #region Validation

    private async Task<bool> ValidateCreateEquipment(CreateUserEquipmentData request, Guid userId, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }
        
        Item? item = await _userInventoryRepository.GetItemByIdOptional(request.ItemId, ct);
        if (item == null)
        {
            ValidationUtils.AddUnknownInventoryItemError(_validationStorage, request.ItemId);
        }
        
        Inventory inventory = await _userEquipmentRepository.GetInventory(userId, ct);
        Equipment? existingEquipment = await _userEquipmentRepository.GetEquipmentBySlotType(inventory.InventoryId, request.SlotType, ct);
        if (existingEquipment != null)
        {
            _validationStorage.AddError(ErrorCode.EquipmentSlotIsAlreadyUsed, $"Slot type {request.SlotType} is already in use.");
            return false;
        }
        
        return _validationStorage.IsValid;
    }
    
    private async Task<bool> ValidateUpdateEquipment(CreateUserEquipmentData request, Guid userId, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(userId, ct))
        {
            return false;
        }

        Item? item = await _userInventoryRepository.GetItemByIdOptional(request.ItemId, ct);
        if (item == null)
        {
            ValidationUtils.AddUnknownInventoryItemError(_validationStorage, request.ItemId);
        }

        Inventory inventory = await _userEquipmentRepository.GetInventory(userId, ct);
        Equipment? existingEquipment = await _userEquipmentRepository.GetEquipmentBySlotType(inventory.InventoryId, request.SlotType, ct);
        if (existingEquipment == null)
        {
            _validationStorage.AddError(ErrorCode.EquipmentSlotIsEmpty, $"No equipment found in slot type {request.SlotType}.");
            return false;
        }

        return _validationStorage.IsValid;
    }

    private async Task<bool> ValidateDeleteEquipment(DeleteEquipmentData request, CancellationToken ct)
    {
        if (!await _userDataService.ValidateUser(request.UserId, ct))
        {
            return false;
        }
        
        Inventory inventory = await _userEquipmentRepository.GetInventory(request.UserId, ct);
        Equipment? equipment = await _userEquipmentRepository.GetEquipmentBySlotType(inventory.InventoryId, request.SlotType, ct);
        if (equipment == null)
        {
            _validationStorage.AddError(ErrorCode.EquipmentSlotIsEmpty, $"No equipment found in slot type {request.SlotType}.");
            return false;
        }
        return _validationStorage.IsValid;
    }

    #endregion
}