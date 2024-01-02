using Azure.Identity;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Users.Item.SendMail;
using Attachment = FluentEmail.Core.Models.Attachment;

namespace FluentEmail.Graph;

/// <summary>
/// Implementation of <c>ISender</c> for the Microsoft Graph API.
/// See <see cref="FluentEmailServicesBuilderExtensions"/>.
/// </summary>
public class GraphSender : ISender
{
    private const int ThreeMbLimit = 3145728;

    private readonly GraphServiceClient graphClient;

    public GraphSender(GraphSenderOptions options)
    {
        ClientSecretCredential spn = new(
            options.TenantId,
            options.ClientId,
            options.Secret);
        this.graphClient = new(spn);
    }

    public GraphSender(GraphServiceClient graphClient)
    {
        this.graphClient = graphClient;
    }

    public SendResponse Send(IFluentEmail email, CancellationToken? token = default)
    {
        return this.SendAsync(email, token)
            .GetAwaiter()
            .GetResult();
    }

    public async Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = default)
    {
        try
        {
            token ??= default;
            if (email.Data.Attachments?.Any() == true)
            {
                var draftMessage = await this.SendWithAttachments(email, token.Value);

                return new SendResponse { MessageId = draftMessage.Id };
            }

            var message = await this.SendWithoutAttachments(email, token.Value);

            // message.Id is empty
            return new SendResponse { MessageId = message.Id };
        }
        catch (ODataError odataError)
        {
            return new SendResponse
            {
                ErrorMessages = new List<string> { $"[{odataError.Error.Code}] {odataError.Error.Message}" },
            };
        }
        catch (Exception ex)
        {
            return new SendResponse { ErrorMessages = new List<string> { ex.Message } };
        }
    }

    private static byte[] GetAttachmentBytes(Stream stream)
    {
        using var m = new MemoryStream();
        stream.CopyTo(m);

        return m.ToArray();
    }

    private async Task<Message> SendWithoutAttachments(
        IFluentEmail email,
        CancellationToken cancellationToken = default)
    {
        // https://docs.microsoft.com/en-us/graph/api/user-sendmail?view=graph-rest-1.0&tabs=http
        var message = MessageCreation.CreateGraphMessageFromFluentEmail(email);
        await this.graphClient.Users[email.Data.FromAddress.EmailAddress]
            .SendMail.PostAsync(
                new SendMailPostRequestBody { Message = message },
                requestConfiguration: null,
                cancellationToken: cancellationToken);

        return message;
    }

    private async Task<Message> SendWithAttachments(IFluentEmail email, CancellationToken cancellationToken)
    {
        // https://docs.microsoft.com/en-us/graph/api/user-post-messages?view=graph-rest-1.0&tabs=csharp
        var message = MessageCreation.CreateGraphMessageFromFluentEmail(email);

        var draftMessage = await this.graphClient.Users[email.Data.FromAddress.EmailAddress]
            .Messages.PostAsync(message, cancellationToken: cancellationToken);

        // upload attachments in the draft message
        foreach (var attachment in email.Data.Attachments)
        {
            if (attachment.Data.Length < ThreeMbLimit)
            {
                await this.UploadAttachmentUnder3Mb(
                    email,
                    draftMessage,
                    attachment,
                    cancellationToken);

                continue;
            }

            await this.UploadAttachment3MbOrOver(
                email,
                draftMessage,
                attachment,
                cancellationToken);
        }

        await this.graphClient.Users[email.Data.FromAddress.EmailAddress]
            .Messages[draftMessage.Id]
            .Send.PostAsync(cancellationToken: cancellationToken);

        return draftMessage;
    }

    private async Task UploadAttachmentUnder3Mb(
        IFluentEmail email,
        Message draft,
        Attachment attachment,
        CancellationToken cancellationToken)
    {
        var fileAttachment = new FileAttachment
        {
            Name = attachment.Filename,
            ContentType = attachment.ContentType,
            ContentBytes = GetAttachmentBytes(attachment.Data),
            ContentId = attachment.ContentId,
            IsInline = attachment.IsInline,

            // can never be bigger than 3MB, so it is safe to cast to int
            Size = (int)attachment.Data.Length,
        };

        await this.graphClient.Users[email.Data.FromAddress.EmailAddress]
            .Messages[draft.Id]
            .Attachments.PostAsync(fileAttachment, cancellationToken: cancellationToken);
    }

    private async Task UploadAttachment3MbOrOver(
        IFluentEmail email,
        Message draft,
        Attachment attachment,
        CancellationToken cancellationToken)
    {
        var attachmentItem = new AttachmentItem
        {
            AttachmentType = AttachmentType.File,
            Name = attachment.Filename,
            Size = attachment.Data.Length,
            ContentType = attachment.ContentType,
            ContentId = attachment.ContentId,
            IsInline = attachment.IsInline,
        };

        var uploadSession = await this.graphClient.Users[email.Data.FromAddress.EmailAddress]
            .Messages[draft.Id]
            .Attachments.CreateUploadSession.PostAsync(
                new Microsoft.Graph.Users.Item.Messages.Item.Attachments.CreateUploadSession.CreateUploadSessionPostRequestBody { AttachmentItem = attachmentItem },
                requestConfiguration: null,
                cancellationToken: cancellationToken);

        var fileUploadTask = new LargeFileUploadTask<FileAttachment>(uploadSession, attachment.Data);

        await fileUploadTask.UploadAsync(cancellationToken: cancellationToken);
    }
}