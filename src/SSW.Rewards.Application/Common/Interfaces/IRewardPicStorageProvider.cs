namespace SSW.Rewards.Application.Common.Interfaces;
public interface IRewardPicStorageProvider
{
    Task<Uri> UploadRewardPic(byte[] imageArray, string filename);
    Task<Uri> GetRewardPicUri(string picId);

}
