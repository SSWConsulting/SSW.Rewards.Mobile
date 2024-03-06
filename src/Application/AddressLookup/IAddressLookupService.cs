using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.Application.AddressLookup;

public interface IAddressLookupService
{
    Task<IEnumerable<Address>> Search(string query);
}