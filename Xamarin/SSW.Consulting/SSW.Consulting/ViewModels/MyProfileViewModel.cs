using System;
using System.Collections.ObjectModel;
using System.Linq;
using SSW.Consulting.Models;
using SSW.Consulting.Services;
using Xamarin.Forms;

namespace SSW.Consulting.ViewModels
{
    public class MyProfileViewModel : BaseViewModel
    {
        private IUserService _userService;
        private IChallengeService _challengeService;

        public string ProfilePic { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Points { get; set; }

        public ObservableCollection<MyChallenge> CompletedChallenges { get; set; }
        public ObservableCollection<MyChallenge> OutstandingChallenges { get; set; }
        public ObservableCollection<ChallengeListViewModel> ChallengeList { get; set; }

        public MyProfileViewModel(IUserService userService, IChallengeService challengeService)
        {
            _userService = userService;
            _challengeService = challengeService;
            CompletedChallenges = new ObservableCollection<MyChallenge>();
            OutstandingChallenges = new ObservableCollection<MyChallenge>();
            ChallengeList = new ObservableCollection<ChallengeListViewModel>();
            MessagingCenter.Subscribe<object>(this, "NewAchievement", (obj) => { Initialise(); });
            Initialise();
        }

        public MyProfileViewModel(LeaderSummaryViewModel vm)
        {
            _challengeService = Resolver.Resolve<IChallengeService>();
            _userService = Resolver.Resolve<IUserService>();
            ProfilePic = vm.ProfilePic;
            Name = vm.Name;
            Email = vm.Title;
            Points = vm.BaseScore.ToString();
            CompletedChallenges = new ObservableCollection<MyChallenge>();
            OutstandingChallenges = new ObservableCollection<MyChallenge>();
            ChallengeList = new ObservableCollection<ChallengeListViewModel>();
            InitialiseOther(vm.Id);
        }

        public async void InitialiseOther(int userId)
        {
            var userChallenges = await _userService.GetOThersAchievementsAsync(userId);

            userChallenges = userChallenges.OrderByDescending(c => c.awardedAt);
            
            ChallengeList.Add(new ChallengeListViewModel { IsHeader = true, HeaderTitle = "Prizes", Challenge = new MyChallenge { IsBonus = false }, IsRow = false });

            foreach (MyChallenge challenge in userChallenges)
            {
                if (challenge.IsBonus)
                {
                    ChallengeListViewModel vm = new ChallengeListViewModel();
                    vm.IsHeader = false;
                    vm.IsRow = false;

                    if(challenge.Completed)
                    {
                        challenge.Title = "🏆 WON: " + challenge.Title;
                    }

                    vm.Challenge = challenge;


                    ChallengeList.Add(vm);//new ChallengeListViewModel { IsHeader = false, Challenge = challenge, IsRow = true });
                }
            }

            ChallengeList.Add(new ChallengeListViewModel { IsHeader = true, HeaderTitle = "Completed", Challenge = new MyChallenge { IsBonus = false }, IsRow = false });

            foreach (MyChallenge challenge in userChallenges)
            {
                if (challenge.Completed && !challenge.IsBonus)
                    ChallengeList.Add(new ChallengeListViewModel { IsHeader = false, Challenge = challenge, IsRow = true });
            }

            ChallengeList.Add(new ChallengeListViewModel { IsHeader = true, HeaderTitle = "Outstanding", Challenge = new MyChallenge { IsBonus = false }, IsRow = false });

            userChallenges = userChallenges.OrderBy(c => c.Points);

            foreach (MyChallenge challenge in userChallenges)
            {
                if (!challenge.Completed && !challenge.IsBonus)
                    ChallengeList.Add(new ChallengeListViewModel { IsHeader = false, Challenge = challenge, IsRow = true });
            }

            RaisePropertyChanged("Points", "ChallengeList");
        }

        private async void Initialise()
        {
            ProfilePic = await _userService.GetMyProfilePicAsync();
            Name = await _userService.GetMyNameAsync();
            Email = await _userService.GetMyEmailAsync();
            int points = await _userService.GetMyPointsAsync();
            Points = points.ToString();

            var challenges = await _challengeService.GetMyChallengesAsync();

            //TODO: Get rid of this nasty hack and group/display the data properly
            ChallengeList.Add(new ChallengeListViewModel { IsHeader = true, HeaderTitle = "Prizes", Challenge = new MyChallenge { IsBonus = false }, IsRow = false });

            foreach (MyChallenge challenge in challenges)
            {
                if (challenge.IsBonus)
                {
                    ChallengeListViewModel vm = new ChallengeListViewModel();
                    vm.IsHeader = false;
                    vm.IsRow = false;

                    if (challenge.Completed)
                    {
                        challenge.Title = "🏆 WON: " + challenge.Title;
                    }

                    vm.Challenge = challenge;


                    ChallengeList.Add(vm);
                }
            }


            ChallengeList.Add(new ChallengeListViewModel { IsHeader = true, HeaderTitle = "Completed", Challenge = new MyChallenge { IsBonus = false }, IsRow = false });

            challenges = challenges.OrderBy(c => c.awardedAt);

            foreach (MyChallenge challenge in challenges)
            {
                if (challenge.Completed && !challenge.IsBonus)
                    ChallengeList.Add(new ChallengeListViewModel { IsHeader = false, Challenge = challenge, IsRow = true });
            }

            challenges = challenges.OrderBy(c => c.Points);

            ChallengeList.Add(new ChallengeListViewModel { IsHeader = true, HeaderTitle = "Outstanding", Challenge = new MyChallenge { IsBonus = false }, IsRow = false });
            foreach (MyChallenge challenge in challenges)
            {
                if (!challenge.Completed && !challenge.IsBonus)
                    ChallengeList.Add(new ChallengeListViewModel { IsHeader = false, Challenge = challenge, IsRow = true });
            }

            RaisePropertyChanged("Points", "ChallengeList");
        }
    }
}
