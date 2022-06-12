using SSW.Rewards.Api;

namespace SSW.Rewards.Admin.Services;

public class SkillService
{
    private readonly SkillClient _client;
    public SkillService(IHttpClientFactory clientFactory)
    {
        _client = new SkillClient(clientFactory.CreateClient(Constants.RewardsApiClient));
    }

    public async Task<SkillListViewModel> GetAsync()
    {
        return await _client.GetAsync();
    }

    public async Task UpsertSkill(UpsertSkillCommand command)
    {
        await _client.UpsertSkillAsync(command);
    }
    
    public async Task DeleteSkill(DeleteSkillCommand command)
    {
        await _client.DeleteSkillAsync(command);
    }
}