﻿namespace SSW.Rewards.Mobile.Services;

public interface IDevService
{
    Task<IEnumerable<DevProfile>> GetProfilesAsync();
}