DECLARE @cupReward NVARCHAR(50)
DECLARE @bandReward NVARCHAR(50)
DECLARE @angularSuperpowersReward NVARCHAR(50)
DECLARE @azureSuperpowersReward NVARCHAR(50)
DECLARE @netCoreSuperpowersReward NVARCHAR(50)

DECLARE @srcAchievementId INT
DECLARE @dstRewardId INT

SET @cupRewardDst = 'SSW Smart Keepcup'
SET @cupRewardSrc = 'SSW Water Bottle'
SET @bandReward = 'Xiaomi Mi Band 4'
SET @angularSuperpowersReward = 'Free Ticket - Angular Superpowers'
SET @azureSuperpowersReward = 'Free Ticket - Azure Superpowers'
SET @netCoreSuperpowersReward = 'Free Ticket - .NET Core Superpowers'

-- Cup
SELECT @srcAchievementId = Id FROM Achievements WHERE Name = @cupRewardSrc
SELECT @dstRewardId = Id FROM Rewards WHERE Name = @cupRewardDst

INSERT INTO UserRewards (UserId, RewardId, AwardedAt)
SELECT @dstRewardId, UserId, CURRENT_TIMESTAMP FROM UserAchievements
WHERE AchievementId = @srcAchievementId

-- Mi Band
SELECT @srcAchievementId = Id FROM Achievements WHERE Name = @bandReward
SELECT @dstRewardId = Id FROM Rewards WHERE Name = @bandReward

INSERT INTO UserRewards (UserId, RewardId, AwardedAt)
SELECT @dstRewardId, UserId, CURRENT_TIMESTAMP FROM UserAchievements
WHERE AchievementId = @srcAchievementId

-- Angular Superpowers
SELECT @srcAchievementId = Id FROM Achievements WHERE Name = @angularSuperpowersReward
SELECT @dstRewardId = Id FROM Rewards WHERE Name = @angularSuperpowersReward

INSERT INTO UserRewards (UserId, RewardId, AwardedAt)
SELECT @dstRewardId, UserId, CURRENT_TIMESTAMP FROM UserAchievements
WHERE AchievementId = @srcAchievementId


-- Azure Superpowers
SELECT @srcAchievementId = Id FROM Achievements WHERE Name = @azureSuperpowersReward
SELECT @dstRewardId = Id FROM Rewards WHERE Name = @azureSuperpowersReward

INSERT INTO UserRewards (UserId, RewardId, AwardedAt)
SELECT @dstRewardId, UserId, CURRENT_TIMESTAMP FROM UserAchievements
WHERE AchievementId = @srcAchievementId

-- .NET Core Superpowers
SELECT @srcAchievementId = Id FROM Achievements WHERE Name = @netCoreSuperpowersReward
SELECT @dstRewardId = Id FROM Rewards WHERE Name = @netCoreSuperpowersReward

INSERT INTO UserRewards (UserId, RewardId, AwardedAt)
SELECT @dstRewardId, UserId, CURRENT_TIMESTAMP FROM UserAchievements
WHERE AchievementId = @srcAchievementId