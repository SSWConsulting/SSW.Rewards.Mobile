using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace SSW.Rewards.Application.User.Queries.GetUserRewards
{
    public class UploadPictureQuery : IRequest<string>
    {
        public IFormFile File { get; set; }

        public class UpdateProfilePictureQueryHandler : IRequestHandler<UploadPictureQuery, string>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public UpdateProfilePictureQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<string> Handle(UploadPictureQuery request, CancellationToken cancellationToken)
            {
                // TODO: Get connection strign from config
                var connectionString = _context.GetBlobStorageConnectionString();


                if (CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount storage))
                {
                    CloudBlobClient blobClient = storage.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("profile");
                    await container.CreateIfNotExistsAsync();
                    Guid id = Guid.NewGuid();
                    var picBlob = container.GetBlockBlobReference(id.ToString());
                    await picBlob.UploadFromStreamAsync(request.File.OpenReadStream());

                    return id.ToString();

                }

                return "Something Went Wrong";
            }
        
        }
    }
}
