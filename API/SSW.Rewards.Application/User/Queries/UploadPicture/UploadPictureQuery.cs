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
                var storageConnectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

                if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storage))
                {
                    CloudBlobClient blobClient = storage.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference("profile");
                    await container.CreateIfNotExistsAsync();

                    var picBlob = container.GetBlockBlobReference(request.File.FileName);
                    await picBlob.UploadFromStreamAsync(request.File.OpenReadStream());

                    return picBlob.ToString();

                }

                return "You Cooked the Book bruh";
            }
        
        }
    }
}
