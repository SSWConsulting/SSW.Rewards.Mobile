using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.System.Commands.Common
{
    public class SampleDataSeeder
    {
        private readonly ISSWRewardsDbContext _context;

        private Dictionary<string, int> _skills;

        public SampleDataSeeder(ISSWRewardsDbContext context)
        {
            _context = context;
        }


        public async Task SeedV2DataAsync(CancellationToken cancellationToken)
        {
            var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);
            var existingRewards = await _context.Rewards.ToListAsync(cancellationToken);

            MigratePrefixes(existingAchievements, existingRewards);

            AddExistingAchievementIcons(existingAchievements);

            SetupAchievement(existingAchievements, "Claim a prize", 500, AchievementType.Completed, Icons.Gift);
            SetupAchievement(existingAchievements, "Get into the top 100", 500, AchievementType.Completed, Icons.Trophy);
            SetupAchievement(existingAchievements, "Follow SSW TV on Twitter", 500, AchievementType.Linked, Icons.Twitter, true);
            SetupAchievement(existingAchievements, "Follow SSW TV on YouTube", 500, AchievementType.Linked, Icons.Youtube, true);
            SetupAchievement(existingAchievements, "Follow SSW on LinkedIn", 500, AchievementType.Linked, Icons.Linkedin, true);
            SetupAchievement(existingAchievements, "Follow SSW on GitHub", 500, AchievementType.Linked, Icons.Github, true);
            SetupAchievement(existingAchievements, "Scan an SSWer", 500, AchievementType.Completed, Icons.Handshake);
            SetupAchievement(existingAchievements, "Upload a profile picture", 500, AchievementType.Completed, Icons.Camera);
            SetupAchievement(existingAchievements, "Attend an SSW Superpowers", 500, AchievementType.Completed, Icons.Lightning);
            SetupAchievement(existingAchievements, "Attend a NetUG", 500, AchievementType.Completed, Icons.Puzzle);
            SetupAchievement(existingAchievements, "Attend an SSW Workshop", 500, AchievementType.Completed, Icons.Certificate);
            SetupAchievement(existingAchievements, "Attend an SSW Hackday", 500, AchievementType.Completed, Icons.Lightbulb);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private void AddExistingAchievementIcons(List<Achievement> achievements)
        {
            foreach (var achievement in achievements)
            {
                if (achievement.Type == AchievementType.Scanned)
                {
                    achievement.Icon = Icons.QRCode;
                }
                else if (achievement.Type == AchievementType.Attended)
                {
                    var name = achievement.Name.ToLower();

                    if (name.Contains("workshop"))
                    {
                        achievement.Icon = Icons.Certificate;

                    }
                    else if (name.Contains("superpowers"))
                    {
                        achievement.Icon = Icons.Lightning;
                    }
                    else if (name.Contains("hackday") || name.Contains("hack day"))
                    {
                        achievement.Icon = Icons.Lightbulb;
                    }
                    else
                    {
                        achievement.Icon = Icons.Puzzle;
                    }
                }

                achievement.IconIsBranded = false;
            }
        }

        private void MigratePrefixes(List<Achievement> achievements, List<Reward> rewards)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var achievement in achievements)
            {
                var codeData = Encoding.ASCII.GetBytes($"ach:{achievement.Name}");
                achievement.Code = Convert.ToBase64String(codeData);

                sb.AppendLine(achievement.Code);
            }

            foreach (var reward in rewards)
            {
                var codeData = Encoding.ASCII.GetBytes($"rwd:{reward.Name}");
                reward.Code = Convert.ToBase64String(codeData);

                sb.AppendLine(reward.Code);
            }

            Console.WriteLine("Migrated achievement and rewards codes:");
            Console.WriteLine(sb.ToString());
        }

        public async Task SeedAllAsync(byte[] profileData, CancellationToken cancellationToken)
        {
            var profiles = GetProfiles(profileData)
                .Where(p => !string.IsNullOrWhiteSpace(p.Profile))
                .ToList();

            await SeedSkillsAsync(profiles.SelectMany(p => p.Skills), cancellationToken);
            await SeedStaffMembers(profiles, cancellationToken);
            await SeedAchievementsAsync(cancellationToken);
        }

        private async Task SeedSkillsAsync(IEnumerable<string> newSkills, CancellationToken cancellationToken)
        {
            //nuke all skills (will be re-added later)
            _context.StaffMemberSkills.RemoveRange(await _context.StaffMemberSkills.ToArrayAsync(cancellationToken));

            _context.Skills.RemoveRange(await _context.Skills.ToArrayAsync(cancellationToken));
            await _context.SaveChangesAsync(cancellationToken);

            // group by lowered name to ensure no dupes
            var skillsToInsert = newSkills
                .Select(s => new
                {
                    Key = s.ToLower(),
                    Value = s
                })
                .GroupBy(p => p.Key)
                .Select(g => new Skill { Name = g.First().Value })
                .ToArray();
            await _context.Skills.AddRangeAsync(skillsToInsert, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _skills = await _context.Skills.ToDictionaryAsync(s => s.Name.ToLower(), s => s.Id);
        }

        private async Task SeedStaffMembers(IEnumerable<UserProfile> profiles, CancellationToken cancellationToken)
        {
            //remove removed profiles
            var profileNames = profiles.Select(p => p.Name).ToArray();
            var profilesToRemove = await _context
                .StaffMembers
                .Where(sm => !profileNames.Contains(sm.Name))
                .ToArrayAsync(cancellationToken);
            _context.StaffMembers.RemoveRange(profilesToRemove);
            await _context.SaveChangesAsync(cancellationToken);

            //add/update profiles
            var existingStaffMembers = await _context.StaffMembers.ToListAsync(cancellationToken);
            var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);
            profiles.ForEach(p =>
            {
                var staffMember = existingStaffMembers.FirstOrDefault(sm => sm.Name == p.Name) ?? new StaffMember();
                staffMember.Name = p.Name;
                staffMember.Title = p.Title;
                staffMember.StaffMemberSkills = p.Skills.Select(s => new StaffMemberSkill { SkillId = _skills[s.ToLower()] }).ToArray();
                staffMember.Profile = p.Profile;
                staffMember.TwitterUsername = p.TwitterUsername;
                staffMember.IsExternal = p.IsExternal;
                staffMember.StaffAchievement = new Achievement { Name = p.Name, Value = p.Value, Code = GenerateCode(p.Name) };

                if (staffMember.Id == 0)
                {
                    _context.StaffMembers.Add(staffMember);
                }

                //SetupAchievement(existingAchievements, p.Name, p.Value);
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedAchievementsAsync(CancellationToken cancellationToken)
        {
            var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);
            var existingRewards = await _context.Rewards.ToListAsync(cancellationToken);

            // tech quiz
            SetupAchievement(existingAchievements, "SSW Tech Quiz", 500);

            // prizes
            SetupReward(existingRewards, "SSW Smart Keepcup", 4000);
            SetupReward(existingRewards, "Xiaomi Mi Band 4", 5000);
            SetupReward(existingRewards, "Free Ticket - Angular Superpowers", 2000);
            SetupReward(existingRewards, "Free Ticket - Azure Superpowers", 2000);
            SetupReward(existingRewards, "Free Ticket - .NET Core Superpowers", 2000);
            SetupReward(existingRewards, "Free Ticket - Clean Architecture Superpowers", 2000);

            // prizes
            SetupAchievement(existingAchievements, "SSW Smart Keepcup", 0);
            SetupAchievement(existingAchievements, "Xiaomi Mi Band 4", 0);
            SetupAchievement(existingAchievements, "Free Ticket - Angular Superpowers", 0);
            SetupAchievement(existingAchievements, "Free Ticket - Azure Superpowers", 0);
            SetupAchievement(existingAchievements, "Free Ticket - .NET Core Superpowers", 0);
            SetupAchievement(existingAchievements, "Free Ticket - Clean Architecture Superpowers", 0);

            // talks
            SetupAchievement(existingAchievements, "Chinafy your apps + Lessons you can steal from China", 500);
            SetupAchievement(existingAchievements, "How to put a Penguin in a Cloud: Linux on Azure", 500);
            SetupAchievement(existingAchievements, "Clean Architecture with ASP.NET Core 3.0", 500);
            SetupAchievement(existingAchievements, "Real-time Face Recognition With Microsoft Cognitive Services", 500);
            SetupAchievement(existingAchievements, "Azure SpendOps – The Art of Effectively Managing Azure Costs", 500);
            SetupAchievement(existingAchievements, "NETUG October 2019 - 7 Deadly Presentation Sins", 500);
            SetupAchievement(existingAchievements, "NETUG November 2019 - gRPC in .NET Core 3", 500);
            SetupAchievement(existingAchievements, "NETUG November 2019 - CSS Grid: The end of Flex and Bootstrap?", 500);
            SetupAchievement(existingAchievements, "NETUG December 2019 - A Merry Geek-mas Party & Fishbowl Presentations!", 500);
            SetupAchievement(existingAchievements, "NETUG January 2020 - PWAs: You may not need to go native", 500);
            SetupAchievement(existingAchievements, "NETUG February 2020 - Access Granted: Demystifying the identity options", 500);
            SetupAchievement(existingAchievements, "NETUG April 2020 - From Paper to Power using Azure Form Recognition", 500);
            SetupAchievement(existingAchievements, "NETUG June 2020 - Build your first deep learning solution using Azure Automated ML", 500);
            SetupAchievement(existingAchievements, "NETUG July 2020 - Power Apps - The Tesla of Software Development", 500);
            SetupAchievement(existingAchievements, "NETUG August 2020 - Blazor WebApps - Goodbye JavaScript! Im in love with C#", 500);

            // Hack days
            SetupAchievement(existingAchievements, "AI Hackday Feb 2020", 500);
            SetupAchievement(existingAchievements, "AI Hack Day Online - June 2020", 500);
            SetupAchievement(existingAchievements, "Angular Hack Day Online - June 2020", 500);
            SetupAchievement(existingAchievements, "Xamarin Hack Day Online - July 2020", 500);

            // superpowers
            SetupAchievement(existingAchievements, "Angular Superpowers", 500);
            SetupAchievement(existingAchievements, "Azure Superpowers", 500);
            SetupAchievement(existingAchievements, ".NET Core Superpowers", 500);
            SetupAchievement(existingAchievements, "Clean Architecture Superpowers", 500);
            SetupAchievement(existingAchievements, "Azure Superpowers Online April 2020", 500);
            SetupAchievement(existingAchievements, ".NET Core Superpowers Online April 2020", 500);
            SetupAchievement(existingAchievements, "Clean Architecture Superpowers Online May 2020", 500);
            SetupAchievement(existingAchievements, "Angular Superpowers Online May 2020", 500);

            // workshops
            SetupAchievement(existingAchievements, "2019 2 Day Angular Workshop", 500);
            SetupAchievement(existingAchievements, "Clean Architecture 2-day Workshop July 2020", 500);

            // social media
            SetupAchievement(existingAchievements, "SSW TV", 100);
            SetupAchievement(existingAchievements, "SSW/SSW TV Twitter", 100);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private void SetupAchievement(IEnumerable<Achievement> existingAchievements, string name, int value, AchievementType type = AchievementType.Completed, Icons icon = Icons.Trophy, bool branded = false)
        {
            var achievement = existingAchievements
                .FirstOrDefault(a => a.Name.Equals(name, StringComparison.InvariantCulture))
                ?? new Achievement();

            achievement.Name = name;
            achievement.Code = GenerateCode(name);
            achievement.Value = value;
            achievement.Type = type;
            achievement.Icon = icon;
            achievement.IconIsBranded = branded;

            if (achievement.Id == 0)
            {
                _context.Achievements.Add(achievement);
            }
        }

        private string GenerateCode(string inputValue)
        {
            var codeData = Encoding.ASCII.GetBytes(inputValue);
            return Convert.ToBase64String(codeData);
        }

        private void SetupReward(IEnumerable<Reward> existingRewards, string name, int cost)
        {
            var reward = existingRewards
                .FirstOrDefault(r => r.Name.Equals(name, StringComparison.InvariantCulture))
                ?? new Reward();

            reward.Name = name;
            var codeData = Encoding.ASCII.GetBytes(name);
            reward.Code = Convert.ToBase64String(codeData);
            reward.Cost = cost;

            if (reward.Id == 0)
            {
                _context.Rewards.Add(reward);
            }
        }

        private IEnumerable<UserProfile> GetProfiles(byte[] profileData)
        {
            using (var stream = new MemoryStream(profileData))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                reader.Read();
                while (reader.Read())
                {
                    yield return new UserProfile
                    {
                        Name = reader.GetString(0)?.Trim(),
                        Title = reader.GetString(1)?.Trim(),
                        Skills = Enumerable.Range(2, 4)
                            .Select(i => reader.GetString(i)?.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct()
                            .ToArray(),
                        Profile = reader.GetString(6)?.Trim(),
                        Value = (int)reader.GetDouble(7),
                        TwitterUsername = reader.GetString(8)?.Trim()
                        //IsExternal = reader.GetBoolean(9)
                    };
                }
            }
        }

        private class UserProfile
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string[] Skills { get; set; }
            public string Profile { get; set; }
            public string TwitterUsername { get; set; }
            public int Value { get; set; }
            public bool IsExternal { get; set; }
        }
    }
}
