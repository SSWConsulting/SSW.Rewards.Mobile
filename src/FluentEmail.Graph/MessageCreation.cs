namespace FluentEmail.Graph;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Graph.Models;

internal static class MessageCreation
{
    internal static Message CreateGraphMessageFromFluentEmail(IFluentEmail email)
    {
        var itemBody = new ItemBody
        {
            Content = email.Data.Body,
            ContentType = email.Data.IsHtml ? BodyType.Html : BodyType.Text,
        };

        var message = new Message
        {
            Subject = email.Data.Subject,
            Body = itemBody,
            From = ConvertToRecipient(email.Data.FromAddress),
            ReplyTo = CreateRecipientList(email.Data.ReplyToAddresses),
            ToRecipients = CreateRecipientList(email.Data.ToAddresses),
            CcRecipients = CreateRecipientList(email.Data.CcAddresses),
            BccRecipients = CreateRecipientList(email.Data.BccAddresses),
        };

        SetPriority(email, message);
        AddHeaders(email, message);

        return message;
    }

    private static List<Recipient> CreateRecipientList(IEnumerable<Address> addressList)
    {
        if (addressList == null)
        {
            return new List<Recipient>();
        }

        return addressList.Select(ConvertToRecipient)
            .ToList();
    }

    private static Recipient ConvertToRecipient([NotNull] Address address)
    {
        if (address is null)
        {
            throw new ArgumentNullException(nameof(address));
        }

        return new Recipient
        {
            EmailAddress = new EmailAddress { Address = address.EmailAddress, Name = address.Name },
        };
    }

    private static void SetPriority(IFluentEmail email, Message draftMessage)
    {
        switch (email.Data.Priority)
        {
            case Priority.High:
                draftMessage.Importance = Importance.High;

                break;

            case Priority.Normal:
                draftMessage.Importance = Importance.Normal;

                break;

            case Priority.Low:
                draftMessage.Importance = Importance.Low;

                break;

            default:
                draftMessage.Importance = Importance.Normal;

                break;
        }
    }

    private static void AddHeaders(IFluentEmail email, Message message)
    {
        if (!email.Data.Headers.Any())
        {
            return;
        }

        var headers = email.Data.Headers
            .Select(header => new InternetMessageHeader { Name = header.Key, Value = header.Value })
            .ToList();
        message.InternetMessageHeaders = headers;
    }
}