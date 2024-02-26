namespace SSW.Rewards.Application.Common.Interfaces;

public interface ISkillPicStorageProvider
{
    Task<Uri> UploadSkillPic(byte[] imageArray, string filename);
    Task<Uri> GetSkillPicUri(string picId);
}