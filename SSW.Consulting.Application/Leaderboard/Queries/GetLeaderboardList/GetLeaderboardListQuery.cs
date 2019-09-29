using MediatR;
using SSW.Consulting.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList
{
	public class GetLeaderboardListQuery : IRequest<LeaderboardListViewModel>
    {
		public sealed class Handler : IRequestHandler<GetLeaderboardListQuery, LeaderboardListViewModel>
		{
			private readonly ISSWConsultingDbContent _dbContext;
			private readonly IStorageProvider _storage;

			public Handler(ISSWConsultingDbContent dbContext, IStorageProvider storage)
			{
				_dbContext = dbContext;
				_storage = storage;
			}

			public async Task<LeaderboardListViewModel> Handle(GetLeaderboardListQuery request, CancellationToken cancellationToken)
			{
				// TODO: Remove - This is just a test to fire up cosmosdb and keyvault
				var a = _dbContext.StaffMembers.Where(x => x.Name == "William");

				// TODO: Write real integration tests!!
				await _storage.UploadBlob("Testing", "a/b/c/imafile.txt", System.Text.Encoding.UTF8.GetBytes("Hello world!"));

				byte[] blobContents = await _storage.DownloadBlob("Testing", "a/b/c/imafile.txt");

				// throws BlobContainerNotFoundException :)
				// byte[] noBlobContents = await _storage.DownloadBlob("NotTesting", "a/b/c/imnotafile.txt");

				// TODO: get from Cosmos
				var user1 = new LeaderboardUserDto() { Position = 1, Name = "Tan Wuhan", ImageUrl = "", Points = 120, Bonus = 35 };
				var user2 = new LeaderboardUserDto() { Position = 2, Name = "Matt Wicks", ImageUrl = "", Points = 120, Bonus = 35 };
				var user3 = new LeaderboardUserDto() { Position = 3, Name = "Vladena Klimkova", ImageUrl = "", Points = 120, Bonus = 35 };
				var user4 = new LeaderboardUserDto() { Position = 4, Name = "Tatiana Gagelman", ImageUrl = "", Points = 120, Bonus = 35 };
				var user5 = new LeaderboardUserDto() { Position = 5, Name = "Tatiana Gagelman", ImageUrl = "", Points = 120, Bonus = 35 };
				var user6 = new LeaderboardUserDto() { Position = 6, Name = "Adam Cogan", ImageUrl = "", Points = 120, Bonus = 35 };
				var user7 = new LeaderboardUserDto() { Position = 7, Name = "Tatiana Gagelman", ImageUrl = "", Points = 120, Bonus = 35 };
				var user8 = new LeaderboardUserDto() { Position = 8, Name = "Greg Harris", ImageUrl = "", Points = 120, Bonus = 35 };

				var users = new List<LeaderboardUserDto> { user1, user2, user3, user4, user5, user6, user7, user8 };

				var model = new LeaderboardListViewModel
				{
					Users = users
				};

				return await Task.FromResult(model);
			}
		}
	}
}
