using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Common;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Reward, RewardDto>();
        CreateMap<Reward, RewardEditDto>()
            .ForMember(dst => dst.ImageBytesInBase64, opt => opt.Ignore())
            .ForMember(dst => dst.ImageFileName, opt => opt.Ignore())
            .ForMember(dst => dst.CarouselImageBytesInBase64, opt => opt.Ignore())
            .ForMember(dst => dst.CarouselImageFileName, opt => opt.Ignore())
            .ForMember(dst => dst.DeleteThumbnailImage, opt => opt.Ignore())
            .ForMember(dst => dst.DeleteCarouselImage, opt => opt.Ignore());
    }
}
