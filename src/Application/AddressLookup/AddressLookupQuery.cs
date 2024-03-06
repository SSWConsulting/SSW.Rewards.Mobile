using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.Application.AddressLookup;

public class AddressLookupQuery : IRequest<IEnumerable<Address>>
{
    public string QueryString { get; set; } = string.Empty;
}

public class AddressLookupQueryHandler : IRequestHandler<AddressLookupQuery, IEnumerable<Address>>
{
    private readonly IAddressLookupService _addressLookupService;

    public AddressLookupQueryHandler(IAddressLookupService addressLookupService)
    {
        _addressLookupService = addressLookupService;
    }

    public async Task<IEnumerable<Address>> Handle(AddressLookupQuery request, CancellationToken cancellationToken)
    {
        return await _addressLookupService.Search(request.QueryString);
    }
}
