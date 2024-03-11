namespace SSW.Rewards.Shared.DTOs.AddressTypes;

public class Address
{
    public string streetNumber { get; set; }
    public string streetName { get; set; }
    public string municipality { get; set; }
    public string countrySecondarySubdivision { get; set; }
    public string countrySubdivision { get; set; }
    public string countrySubdivisionName { get; set; }
    public string countrySubdivisionCode { get; set; }
    public string postalCode { get; set; }
    public string countryCode { get; set; }
    public string country { get; set; }
    public string countryCodeISO3 { get; set; }
    public string freeformAddress { get; set; }
    public string localName { get; set; }
    public string municipalitySubdivision { get; set; }
}