using System.Text;
using SSW.Rewards.Application.Achievements.Common;
using SSW.Rewards.Application.Common.Helpers;

namespace SSW.Rewards.Application.System.Commands.Common;

public sealed record DemoSeedOptions
{
    public required string DevEmail { get; init; }
    public string? DevName { get; init; }
    public int Years { get; init; } = 3;
    public DateTime Today { get; init; } = DateTime.UtcNow.Date;
}

public sealed record DemoSeedSummary(
    int Users, int StaffMembers, int Rewards, int Quizzes,
    int AwardsAdded, int ClaimsAdded, int CompletionsAdded, int PendingAdded);

/// <summary>
/// Seeds the fictional Northwind demo dataset: people with avatars, an achievement
/// catalog, multi-year scan/event/quiz history with event clusters, reward claims
/// and the developer's own user. Fully idempotent — every decision is a stable hash
/// of (entity key, calendar date), so re-running tops up new days without duplicating
/// old ones. Every persisted claim is charged against the balance and new claims are only
/// added after the latest one, so no balance goes negative (open pending costs included).
/// QR codes use the app's Base64 <c>ach:</c>/<c>rwd:</c>/<c>pnd:</c> payloads so seeded items scan.
/// </summary>
public class DemoDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly IDemoAssetProvider? _assets;
    private readonly Action<string> _log;

    private string _devEmail = string.Empty;

    public DemoDataSeeder(IApplicationDbContext context, IDemoAssetProvider? assets = null, Action<string>? log = null)
    {
        _context = context;
        _assets = assets;
        _log = log ?? (_ => { });
    }

    public async Task<DemoSeedSummary> SeedAsync(DemoSeedOptions options, CancellationToken ct)
    {
        _devEmail = options.DevEmail.Trim().ToLowerInvariant();
        var today = options.Today.Date;
        var horizon = today.AddYears(-options.Years);

        var people = BuildPeople(options);

        // Phase 1 — catalog (achievements, skills, platforms, rewards, quizzes).
        await EnsureSkills(people, ct);
        var achievements = new AchievementLookup(_context);
        await EnsureMilestoneAchievements(achievements, ct);
        await EnsureSocialPlatforms(achievements, ct);
        var rewards = await EnsureRewards(ct);
        var events = DemoDataSet.GetEvents(horizon, today).ToList();
        await EnsureEventAchievements(achievements, events, ct);
        await EnsurePeopleAchievements(achievements, people, ct);
        await EnsureQuizzes(achievements, ct);
        await _context.SaveChangesAsync(ct);

        // Phase 2 — people (staff members + users). Saved so ids exist for history.
        var staffCount = await EnsureStaffMembers(people, achievements, ct);
        var users = await EnsureUsers(people, achievements, ct);
        await _context.SaveChangesAsync(ct);

        // Phase 3 — history (awards, socials, completions), then claims on top of it.
        var history = await GenerateHistory(people, users, achievements, events, horizon, today, ct);
        await _context.SaveChangesAsync(ct);

        var (claims, pending) = await GenerateClaims(people, users, rewards, today, ct);
        await _context.SaveChangesAsync(ct);

        await AssertNoNegativeBalances(users.Values.Select(u => u.Id).ToList(), ct);

        var summary = new DemoSeedSummary(
            users.Count, staffCount, rewards.Count, DemoDataSet.Quizzes.Length,
            history.Awards, claims, history.Completions, pending);
        _log($"Seed complete: {summary}");
        return summary;
    }

    private static List<DemoPerson> BuildPeople(DemoSeedOptions options)
    {
        var people = DemoDataSet.Everyone.ToList();
        var devName = options.DevName ?? GuessNameFromEmail(options.DevEmail);
        people.Add(new DemoPerson("demo-dev", devName, null, IsStaff: false, Activity: 0.9, Skills: []));
        return people;
    }

    private static string GuessNameFromEmail(string email)
    {
        var local = email.Split('@')[0].Replace('.', ' ').Replace('-', ' ').Replace('_', ' ');
        return string.Join(' ', local.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
    }

    private string EmailFor(DemoPerson person) => person.Key == "demo-dev" ? _devEmail : person.Email;

    // ---- phase 1: catalog --------------------------------------------------

    private async Task EnsureSkills(List<DemoPerson> people, CancellationToken ct)
    {
        var existing = (await _context.Skills.AsNoTracking()
                .TagWithContext()
                .Select(s => s.Name)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var name in people.SelectMany(p => p.Skills).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!existing.Contains(name))
                _context.Skills.Add(new Skill { Name = name });
        }
    }

    private static async Task EnsureMilestoneAchievements(AchievementLookup achievements, CancellationToken ct)
    {
        // Same names/values as SampleDataSeeder.SeedV2DataAsync so the two seeders coexist.
        await achievements.EnsureByName(MilestoneAchievements.ClaimPrize, 50, AchievementType.Completed, Icons.Gift, ct);
        await achievements.EnsureByName(MilestoneAchievements.LeaderboardTopUsers, 500, AchievementType.Completed, Icons.Ribbon, ct);
        await achievements.EnsureByName(MilestoneAchievements.MeetSSW, 300, AchievementType.Completed, Icons.Handshake, ct);
        await achievements.EnsureByName(MilestoneAchievements.ProfilePic, 100, AchievementType.Completed, Icons.Camera, ct);
        await achievements.EnsureByName(MilestoneAchievements.AttendUG, 200, AchievementType.Completed, Icons.Puzzle, ct);
        await achievements.EnsureByName(MilestoneAchievements.AttendWorkshop, 300, AchievementType.Completed, Icons.Certificate, ct);
        await achievements.EnsureByName(MilestoneAchievements.AttendHackday, 200, AchievementType.Completed, Icons.Lightbulb, ct);
    }

    private async Task EnsureSocialPlatforms(AchievementLookup achievements, CancellationToken ct)
    {
        var existing = (await _context.SocialMediaPlatforms.AsNoTracking()
                .TagWithContext()
                .Select(p => p.Name)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, achievementName, icon) in new[]
        {
            ("GitHub", "Follow SSW on GitHub", Icons.Github),
            ("LinkedIn", "Follow SSW on LinkedIn", Icons.Linkedin),
            ("Twitter", "Follow SSW TV on Twitter", Icons.Twitter),
            (DemoDataSet.CompanyPlatformName, "Add your company", Icons.People),
        })
        {
            if (existing.Contains(name)) continue;
            var achievement = await achievements.EnsureByName(achievementName,
                name == DemoDataSet.CompanyPlatformName ? 0 : 150, AchievementType.Linked, icon, ct);
            _context.SocialMediaPlatforms.Add(new SocialMediaPlatform { Name = name, Achievement = achievement });
        }
    }

    private async Task<List<Reward>> EnsureRewards(CancellationToken ct)
    {
        var result = new List<Reward>();
        foreach (var demo in DemoDataSet.Rewards)
        {
            var code = AchievementHelpers.GenerateCode($"demo:reward:{demo.Slug}", IsReward: true);
            var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.Code == code, ct);
            if (reward is null)
            {
                reward = new Reward
                {
                    Code = code,
                    Name = demo.Name,
                    Description = demo.Description,
                    Cost = demo.Cost,
                    RewardType = demo.Type,
                    Icon = demo.Icon,
                    IsOnboardingReward = demo.IsOnboarding,
                    IsCarousel = demo.IsCarousel,
                };
                _context.Rewards.Add(reward);
            }
            if (string.IsNullOrEmpty(reward.ImageUri) && _assets is not null)
            {
                reward.ImageUri = await _assets.GetAssetUriAsync($"reward-{demo.Slug}", ct);
                if (demo.IsCarousel) reward.CarouselImageUri = reward.ImageUri;
            }
            result.Add(reward);
        }
        return result;
    }

    private static async Task EnsureEventAchievements(AchievementLookup achievements, List<DemoEvent> events, CancellationToken ct)
    {
        foreach (var e in events)
            await achievements.EnsureByCode($"demo:event:{e.Slug}", e.Name, e.Value, AchievementType.Attended, e.Icon, ct);
    }

    private async Task EnsurePeopleAchievements(AchievementLookup achievements, List<DemoPerson> people, CancellationToken ct)
    {
        foreach (var person in people)
        {
            var (prefix, value) = person.IsStaff
                ? ("demo:staff:", DemoDataSet.StaffScanValue)
                : ("demo:user:", DemoDataSet.UserScanValue);
            await achievements.EnsureByCode(ScanCode(person), person.Name, value, AchievementType.Scanned, Icons.QRCode, ct);
        }
    }

    // The dev's own QR achievement is keyed by email so a different --dev-email gets its
    // own achievement instead of sharing one row across two developer users.
    private string ScanCode(DemoPerson person) => person.Key switch
    {
        "demo-dev" => $"demo:user:dev:{_devEmail}",
        _ when person.IsStaff => $"demo:staff:{person.Key}",
        _ => $"demo:user:{person.Key}",
    };

    private async Task EnsureQuizzes(AchievementLookup achievements, CancellationToken ct)
    {
        var existing = (await _context.Quizzes.AsNoTracking()
                .TagWithContext()
                .Select(q => q.Title)
                .ToListAsync(ct))
            .ToHashSet();
        foreach (var demo in DemoDataSet.Quizzes)
        {
            if (existing.Contains(demo.Title)) continue;
            var achievement = await achievements.EnsureByCode(
                $"demo:quiz:{demo.Slug}", demo.Title, DemoDataSet.QuizValue, AchievementType.Completed, demo.Icon, ct);
            var quiz = new Quiz
            {
                Title = demo.Title,
                Description = demo.Description,
                Icon = demo.Icon,
                IsArchived = demo.IsArchived,
                Achievement = achievement,
            };
            foreach (var q in demo.Questions)
            {
                var question = new QuizQuestion { Text = q.Text };
                for (int i = 0; i < q.Answers.Length; i++)
                    question.Answers.Add(new QuizAnswer { Text = q.Answers[i], IsCorrect = i == q.CorrectIndex });
                quiz.Questions.Add(question);
            }
            _context.Quizzes.Add(quiz);
        }
    }

    // ---- phase 2: people ----------------------------------------------------

    private async Task<int> EnsureStaffMembers(List<DemoPerson> people, AchievementLookup achievements, CancellationToken ct)
    {
        var skillIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var skill in await _context.Skills.AsNoTracking()
                     .TagWithContext()
                     .Where(s => s.Name != null)
                     .Select(s => new { s.Id, Name = s.Name! })
                     .ToListAsync(ct))
            skillIds.TryAdd(skill.Name, skill.Id);

        int count = 0;
        foreach (var person in people.Where(p => p.IsStaff))
        {
            var staff = await _context.StaffMembers
                .IgnoreQueryFilters()
                .Include(s => s.StaffMemberSkills)
                .FirstOrDefaultAsync(s => s.Email == person.Email, ct);
            if (staff is null)
            {
                staff = new StaffMember { Email = person.Email };
                _context.StaffMembers.Add(staff);
            }
            staff.DeletedUtc = null;
            staff.Name = person.Name;
            staff.Title = person.Title;
            staff.Profile = $"{person.Name} is part of the Northwind Traders crew as {person.Title}.";
            staff.TwitterUsername = person.Twitter;
            staff.GitHubUsername = person.GitHub;
            staff.StaffAchievement ??= achievements.ByCode(ScanCode(person));
            if (string.IsNullOrEmpty(staff.ProfilePhoto) && _assets is not null)
                staff.ProfilePhoto = await _assets.GetAssetUriAsync(person.Key, ct);

            var assigned = staff.StaffMemberSkills.Select(s => s.SkillId).ToHashSet();
            foreach (var skillName in person.Skills)
            {
                var skillId = skillIds[skillName];
                if (assigned.Add(skillId))
                    staff.StaffMemberSkills.Add(new StaffMemberSkill { SkillId = skillId, Level = SkillLevel.Advanced });
            }
            count++;
        }
        return count;
    }

    private async Task<Dictionary<string, User>> EnsureUsers(
        List<DemoPerson> people, AchievementLookup achievements, CancellationToken ct)
    {
        // The "User" role is seeded by migration; create defensively for empty test DBs.
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", ct);
        if (userRole is null)
        {
            userRole = new Role { Name = "User" };
            _context.Roles.Add(userRole);
        }

        var result = new Dictionary<string, User>();
        foreach (var person in people)
        {
            var email = EmailFor(person);
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user is null)
            {
                user = new User { Email = email };
                _context.Users.Add(user);
            }
            user.FullName = person.Name;
            user.Activated = true;
            user.Achievement ??= achievements.ByCode(ScanCode(person));
            if (string.IsNullOrEmpty(user.Avatar) && person.Key != "demo-dev" && _assets is not null)
                user.Avatar = await _assets.GetAssetUriAsync(person.Key, ct);
            if (!user.Roles.Any(r => ReferenceEquals(r.Role, userRole) || (userRole.Id != 0 && r.RoleId == userRole.Id)))
                user.Roles.Add(new UserRole { Role = userRole });
            result[person.Key] = user;
        }
        return result;
    }

    // ---- phase 3: history ---------------------------------------------------

    private sealed record HistoryCounts(int Awards, int Completions);

    private async Task<HistoryCounts> GenerateHistory(
        List<DemoPerson> people,
        Dictionary<string, User> users,
        AchievementLookup achievements,
        List<DemoEvent> events,
        DateTime horizon,
        DateTime today,
        CancellationToken ct)
    {
        // Everything from phases 1-2 is saved, so all ids are real here.
        var userIds = users.Values.Select(u => u.Id).ToList();
        var existingPairs = (await _context.UserAchievements.AsNoTracking()
                .TagWithContext()
                .Where(ua => userIds.Contains(ua.UserId))
                .Select(ua => new { ua.UserId, ua.AchievementId })
                .ToListAsync(ct))
            .Select(x => (x.UserId, x.AchievementId)).ToHashSet();
        var existingCompletions = (await _context.CompletedQuizzes.AsNoTracking()
                .TagWithContext()
                .Where(cq => userIds.Contains(cq.UserId))
                .Select(cq => new { cq.UserId, cq.QuizId })
                .ToListAsync(ct))
            .Select(x => (x.UserId, x.QuizId)).ToHashSet();
        var existingSocials = (await _context.UserSocialMediaIds.AsNoTracking()
                .TagWithContext()
                .Where(s => userIds.Contains(s.UserId))
                .Select(s => new { s.UserId, s.SocialMediaPlatformId })
                .ToListAsync(ct))
            .Select(x => (x.UserId, x.SocialMediaPlatformId)).ToHashSet();
        var platforms = (await _context.SocialMediaPlatforms.AsNoTracking()
                .TagWithContext()
                .Select(p => new { p.Name, p.Id, p.AchievementId })
                .ToListAsync(ct))
            .ToDictionary(p => p.Name, p => (p.Id, p.AchievementId), StringComparer.OrdinalIgnoreCase);
        var quizzes = (await _context.Quizzes.AsNoTracking()
                .TagWithContext()
                .Where(q => q.Title != null)
                .Select(q => new { q.Title, q.Id, q.AchievementId })
                .ToListAsync(ct))
            .GroupBy(q => q.Title!)
            .ToDictionary(g => g.Key, g => (g.First().Id, g.First().AchievementId));

        int awards = 0, completions = 0;

        void Award(User user, int achievementId, DateTime at)
        {
            if (!existingPairs.Add((user.Id, achievementId))) return;
            _context.UserAchievements.Add(new UserAchievement { UserId = user.Id, AchievementId = achievementId, AwardedAt = at });
            awards++;
        }

        void AddSocial(User user, int platformId, string handle)
        {
            if (!existingSocials.Add((user.Id, platformId))) return;
            _context.UserSocialMediaIds.Add(new UserSocialMediaId
            {
                UserId = user.Id,
                SocialMediaPlatformId = platformId,
                SocialMediaUserId = handle,
            });
        }

        var scanTargets = people.Where(p => p.Key != "demo-dev")
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => (p.Key, AchievementId: achievements.ByCode(ScanCode(p)).Id, p.IsStaff))
            .ToList();

        foreach (var person in people)
        {
            var user = users[person.Key];
            var email = user.Email!;
            var join = JoinDate(person, email, horizon, today);

            Award(user, achievements.ByName(MilestoneAchievements.ProfilePic).Id, At(email, join.AddDays(1)));
            if (person.Activity >= 0.5)
                Award(user, achievements.ByName(MilestoneAchievements.LeaderboardTopUsers).Id, At(email, Earlier(join.AddDays(30), today)));

            AddSocial(user, platforms[DemoDataSet.CompanyPlatformName].Id, DemoDataSet.CompanyName);
            if (person.Twitter is not null)
            {
                AddSocial(user, platforms["Twitter"].Id, person.Twitter);
                Award(user, platforms["Twitter"].AchievementId, At(email, Earlier(join.AddDays(2), today)));
            }
            if (person.GitHub is not null)
            {
                AddSocial(user, platforms["GitHub"].Id, person.GitHub);
                Award(user, platforms["GitHub"].AchievementId, At(email, Earlier(join.AddDays(3), today)));
            }

            // Daily scans — deterministic per (email, day); boosted for the last week so
            // the Today/ThisWeek leaderboard filters stay alive even after long gaps.
            var scanned = new HashSet<string>();
            DateTime? firstStaffScan = null;
            for (var day = join; day <= today; day = day.AddDays(1))
            {
                double boost = (today - day).TotalDays < 7 ? 6.0 : 1.0;
                if (Frac($"scan:{email}:{day:yyyyMMdd}") >= 0.06 * person.Activity * boost) continue;

                var pool = scanTargets.Where(t => t.Key != person.Key && !scanned.Contains(t.Key)).ToList();
                if (pool.Count == 0) break;
                var pick = pool[(int)(Frac($"target:{email}:{day:yyyyMMdd}") * pool.Count)];
                scanned.Add(pick.Key);
                var at = At(email, day);
                Award(user, pick.AchievementId, at);
                if (firstStaffScan is null && pick.IsStaff) firstStaffScan = at;
            }
            if (firstStaffScan is not null)
                Award(user, achievements.ByName(MilestoneAchievements.MeetSSW).Id, firstStaffScan.Value);

            // Event clusters — cohort bursts on the event date. The flagship founder
            // attends practically everything, which keeps him on top of the leaderboard.
            DateTime? firstUg = null, firstHack = null, firstWorkshop = null;
            var isFlagship = person.Key == DemoDataSet.Flagship.Key;
            foreach (var e in events.Where(e => e.Date >= join))
            {
                var attendProbability = isFlagship ? 0.95 : e.Attendance * (0.5 + person.Activity / 2);
                if (Frac($"event:{email}:{e.Slug}") >= attendProbability) continue;
                var at = e.Date.AddHours(18);
                Award(user, achievements.ByCode($"demo:event:{e.Slug}").Id, at);
                if (e.Slug.StartsWith("ug-")) firstUg ??= at;
                else if (e.Slug.StartsWith("hackday-")) firstHack ??= at;
                else if (e.Slug.StartsWith("workshop-")) firstWorkshop ??= at;
            }
            if (firstUg is not null) Award(user, achievements.ByName(MilestoneAchievements.AttendUG).Id, firstUg.Value);
            if (firstHack is not null) Award(user, achievements.ByName(MilestoneAchievements.AttendHackday).Id, firstHack.Value);
            if (firstWorkshop is not null) Award(user, achievements.ByName(MilestoneAchievements.AttendWorkshop).Id, firstWorkshop.Value);

            // Quiz completions — only for quizzes flagged for it. Any passed completion
            // currently blocks ALL further submissions of that quiz (global-pass check in
            // SubmitUserQuizCommandValidator), so playable quizzes stay completion-free.
            foreach (var demo in DemoDataSet.Quizzes)
            {
                if (!demo.SeedCompletions) continue;
                if (Frac($"quiz:{email}:{demo.Slug}") >= 0.4 * person.Activity) continue;
                var quiz = quizzes[demo.Title];
                if (!existingCompletions.Add((user.Id, quiz.Id))) continue;
                var span = Math.Max((today - join).TotalDays, 1);
                var at = At(email, join.AddDays(span * Frac($"quizdate:{email}:{demo.Slug}")));
                _context.CompletedQuizzes.Add(new CompletedQuiz { UserId = user.Id, QuizId = quiz.Id, Passed = true });
                Award(user, quiz.AchievementId, at);
                completions++;
            }
        }

        return new HistoryCounts(awards, completions);
    }

    // ---- phase 4: claims ----------------------------------------------------

    private async Task<(int claims, int pending)> GenerateClaims(
        List<DemoPerson> people, Dictionary<string, User> users, List<Reward> rewards, DateTime today, CancellationToken ct)
    {
        var userIds = users.Values.Select(u => u.Id).ToList();
        var persistedClaims = (await _context.UserRewards.AsNoTracking()
                .TagWithContext()
                .Where(ur => userIds.Contains(ur.UserId))
                .Select(ur => new { ur.UserId, ur.AwardedAt, ur.Reward.Cost })
                .ToListAsync(ct))
            .ToLookup(x => x.UserId, x => (x.AwardedAt, Delta: -x.Cost));
        var pendingCostByUser = (await _context.PendingRedemptions.AsNoTracking()
                .TagWithContext()
                .Where(pr => userIds.Contains(pr.UserId) && !pr.Completed && !pr.CancelledByUser && !pr.CancelledByAdmin)
                .Select(pr => new { pr.UserId, pr.Reward.Cost })
                .ToListAsync(ct))
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cost));
        var pendingCodes = (await _context.PendingRedemptions.AsNoTracking()
                .TagWithContext()
                .Where(pr => userIds.Contains(pr.UserId) && pr.Code != null)
                .Select(pr => pr.Code!)
                .ToListAsync(ct))
            .ToHashSet();

        var claimAchievement = await _context.Achievements.AsNoTracking()
            .IgnoreQueryFilters()
            .TagWithContext()
            .Where(a => a.Name == MilestoneAchievements.ClaimPrize)
            .Select(a => new { a.Id, a.Value })
            .FirstAsync(ct);
        var usersWithClaimPrize = (await _context.UserAchievements.AsNoTracking()
                .TagWithContext()
                .Where(ua => userIds.Contains(ua.UserId) && ua.AchievementId == claimAchievement.Id)
                .Select(ua => ua.UserId)
                .ToListAsync(ct))
            .ToHashSet();

        // The ClaimPrize milestone is granted below, AFTER the walk — exclude it from the
        // timeline so the deterministic decision stream is identical on every re-run.
        var grants = (await _context.UserAchievements.AsNoTracking()
                .TagWithContext()
                .Where(ua => userIds.Contains(ua.UserId) && ua.AchievementId != claimAchievement.Id)
                .Select(ua => new { ua.UserId, ua.AwardedAt, ua.Achievement.Value })
                .ToListAsync(ct))
            .ToLookup(x => x.UserId, x => (x.AwardedAt, Delta: x.Value));

        int claims = 0, pendingCount = 0;
        var cheapest = rewards.Min(r => r.Cost);

        foreach (var person in people)
        {
            var user = users[person.Key];
            var email = user.Email!;
            var spent = persistedClaims[user.Id].ToList();
            var timeline = grants[user.Id].Select(g => (g.AwardedAt, g.Delta, IsGrant: true))
                .Concat(spent.Select(c => (c.AwardedAt, c.Delta, IsGrant: false)))
                .OrderBy(x => x.AwardedAt).ThenByDescending(x => x.IsGrant)
                .ToList();
            if (timeline.Count == 0) continue;

            // New claims only go after the latest persisted one (seeded or real): the balance there
            // already reflects all spending, and history added later (gap-fill, wider --years) only raises it.
            var lastSpent = spent.Count > 0 ? spent.Max(c => c.AwardedAt) : DateTime.MinValue;
            var reserved = pendingCostByUser.GetValueOrDefault(user.Id);
            int balance = 0, claimed = 0;
            DateTime? firstClaim = null;
            foreach (var (at, delta, isGrant) in timeline)
            {
                balance += delta;
                if (!isGrant)
                {
                    claimed++;
                    firstClaim ??= at;
                    continue;
                }
                if (claimed >= 5 || balance < (int)(cheapest * 1.5)) continue;
                if (Frac($"claim:{email}:{at:yyyyMMddHHmm}") >= 0.12 * person.Activity) continue;

                var affordable = rewards.Where(r => r.Cost <= balance).ToList();
                var reward = affordable[(int)(Frac($"claimpick:{email}:{at:yyyyMMddHHmm}") * affordable.Count)];
                var claimDate = at.AddHours(1);
                if (claimDate <= lastSpent || balance - reward.Cost < reserved) continue;

                _context.UserRewards.Add(new UserReward { UserId = user.Id, RewardId = reward.Id, AwardedAt = claimDate });
                balance -= reward.Cost;
                claimed++;
                firstClaim ??= claimDate;
                claims++;
            }

            if (firstClaim is not null && usersWithClaimPrize.Add(user.Id))
            {
                _context.UserAchievements.Add(new UserAchievement
                {
                    UserId = user.Id,
                    AchievementId = claimAchievement.Id,
                    AwardedAt = firstClaim.Value,
                });
                balance += claimAchievement.Value;
            }

            // One seeded pending redemption per user, ever — completed/cancelled ones keep their outcome.
            var pendingCode = PendingCode(email);
            if (Frac($"pending:{email}") < 0.08 && !pendingCostByUser.ContainsKey(user.Id) && !pendingCodes.Contains(pendingCode))
            {
                var affordable = rewards.Where(r => r.Cost <= balance).ToList();
                if (affordable.Count > 0)
                {
                    var reward = affordable[(int)(Frac($"pendingpick:{email}") * affordable.Count)];
                    _context.PendingRedemptions.Add(new PendingRedemption
                    {
                        UserId = user.Id,
                        RewardId = reward.Id,
                        Code = pendingCode,
                        ClaimedAt = today.AddDays(-(int)(Frac($"pendingday:{email}") * 10)),
                    });
                    pendingCount++;
                }
            }
        }

        return (claims, pendingCount);
    }

    private static string PendingCode(string email) =>
        Convert.ToBase64String(Encoding.ASCII.GetBytes($"pnd:demo:{email}"));

    private async Task AssertNoNegativeBalances(List<int> userIds, CancellationToken ct)
    {
        var offenders = await _context.Users.AsNoTracking()
            .TagWithContext()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new
            {
                u.Email,
                Earned = u.UserAchievements.Sum(ua => (int?)ua.Achievement.Value) ?? 0,
                Claimed = u.UserRewards.Sum(ur => (int?)ur.Reward.Cost) ?? 0,
                Pending = u.PendingRedemptions
                    .Where(pr => !pr.Completed && !pr.CancelledByUser && !pr.CancelledByAdmin)
                    .Sum(pr => (int?)pr.Reward.Cost) ?? 0,
            })
            .Where(x => x.Earned - x.Claimed - x.Pending < 0)
            .ToListAsync(ct);

        if (offenders.Count > 0)
            throw new InvalidOperationException(
                "Demo seed would leave negative balances for: " + string.Join(", ", offenders.Select(o => o.Email)) +
                ". No seed rows were committed; `rewards-dev db reset` rebuilds a clean dataset.");
    }

    // ---- deterministic helpers ----------------------------------------------

    private static DateTime Earlier(DateTime a, DateTime b) => a <= b ? a : b;

    private static DateTime JoinDate(DemoPerson person, string email, DateTime horizon, DateTime today)
    {
        // Flagship + staff have been around since the horizon; community joined at a
        // hash-picked point in the first 80% of the window.
        if (person.IsStaff) return horizon;
        var window = (today - horizon).TotalDays * 0.8;
        return horizon.AddDays(Frac($"join:{email}") * window);
    }

    private static DateTime At(string email, DateTime day) =>
        day.Date.AddHours(8 + Frac($"time:{email}:{day:yyyyMMdd}") * 10);

    /// <summary>
    /// Stable FNV-1a hash of a string to [0, 1) — decisions survive re-runs and process
    /// restarts (string.GetHashCode is randomised per process, so it can't be used here).
    /// </summary>
    internal static double Frac(string input)
    {
        ulong hash = 14695981039346656037UL;
        foreach (var c in input)
        {
            hash ^= c;
            hash *= 1099511628211UL;
        }
        return (hash >> 11) / (double)(1UL << 53);
    }

    /// <summary>Idempotent achievement upsert — looks past soft-delete filters and caches by key.</summary>
    private sealed class AchievementLookup(IApplicationDbContext context)
    {
        private readonly Dictionary<string, Achievement> _byCode = [];
        private readonly Dictionary<string, Achievement> _byName = [];

        public Achievement ByCode(string key) => _byCode[key];
        public Achievement ByName(string name) => _byName[name];

        public async Task<Achievement> EnsureByCode(string key, string name, int value, AchievementType type, Icons icon, CancellationToken ct)
        {
            if (_byCode.TryGetValue(key, out var cached)) return cached;
            var code = AchievementHelpers.GenerateCode(key, IsReward: false);
            var achievement = await context.Achievements.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Code == code, ct);
            if (achievement is null)
            {
                achievement = new Achievement { Code = code, Name = name, Value = value, Type = type, Icon = icon };
                context.Achievements.Add(achievement);
            }
            achievement.DeletedUtc = null;
            return _byCode[key] = achievement;
        }

        public async Task<Achievement> EnsureByName(string name, int value, AchievementType type, Icons icon, CancellationToken ct)
        {
            if (_byName.TryGetValue(name, out var cached)) return cached;
            var achievement = await context.Achievements.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Name == name, ct);
            if (achievement is null)
            {
                achievement = new Achievement { Code = AchievementHelper.GenerateCode(), Name = name, Value = value, Type = type, Icon = icon };
                context.Achievements.Add(achievement);
            }
            achievement.DeletedUtc = null;
            return _byName[name] = achievement;
        }
    }
}
