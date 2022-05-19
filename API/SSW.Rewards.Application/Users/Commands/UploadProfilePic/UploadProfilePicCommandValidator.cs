using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW.Rewards.Application.Users.Commands.UploadProfilePic
{
    public class UploadProfilePicCommandValidator : AbstractValidator<UploadProfilePicCommand>
    {
        public UploadProfilePicCommandValidator()
        {
            RuleFor(x => x.File).Custom((file, context) => {
                if(!IsImage(file))
                {
                    context.AddFailure("The supplied file is not a valid image");
                }
            });
        }

        private bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
    }
}
