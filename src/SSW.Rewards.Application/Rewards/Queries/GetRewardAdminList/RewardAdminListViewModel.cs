using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;
public class RewardAdminListViewModel
{
    public List<RewardAdminDto> Rewards { get; set; } = new List<RewardAdminDto>();

    // TODO: Check if this Mapping function is needed
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Reward, RewardAdminDto>();
    }
}

public class RewardAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string Code { get; set; } = string.Empty;
}
