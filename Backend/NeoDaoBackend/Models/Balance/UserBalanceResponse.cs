using NeoDaoBackend.Models.WsMessage;

namespace NeoDaoBackend.Models.Balance;

public class UserBalanceResponse: IOutputMessageData
{
    public decimal SoftAmount { get; set; }
    public decimal HardAmount {  get; set; }
    public decimal BitForceAmount {  get; set; }
}