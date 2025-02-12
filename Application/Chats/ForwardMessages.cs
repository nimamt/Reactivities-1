﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain.Direct;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Chats
{
    public class ForwardMessages
    {
        public class Command : IRequest<Result<bool>>
        {
            public List<Guid> ChatIds { get; set; }
            public List<int> MessageIds { get; set; }
            public Guid SrcChatId { get; set; }
            public string Body { get; set; }
            public bool ShowSender { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<bool>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _accessor;

            public Handler(DataContext context, IUserAccessor accessor)
            {
                _context = context;
                _accessor = accessor;
            }
            
            public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userChats = await _context.UserChats
                    .Include(x => x.AppUser)
                    .Include(x => x.Chat)
                    .ThenInclude(x => x.Messages)
                    .ThenInclude(x => x.Sender)
                    .Include(x => x.Chat)
                    .AsSplitQuery()
                    .Where(x => x.AppUser.UserName == _accessor.GetUsername()
                                && (x.ChatId == request.SrcChatId || request.ChatIds.Contains(x.ChatId)))
                    .ToListAsync(cancellationToken);

                var user = userChats.FirstOrDefault()?.AppUser;
                var srcChat = userChats.Select(x => x.Chat).SingleOrDefault(x => x.Id == request.SrcChatId);
                var others = userChats.Select(x => x.Chat)
                    .Where(x => request.ChatIds.Contains(x.Id)).ToList();

                if (srcChat == null)
                {
                    return Result<bool>.Failure("The source chat cannot be accessed.");
                }

                if (others.Count != request.ChatIds.Count)
                {
                    return Result<bool>.Failure("Some of the target chats can be accessed");
                }
                
                ICollection<Message> messages = srcChat.Messages.Where(x => request.MessageIds.Contains(x.Id))
                    .ToList();;

                foreach (var chat in others)
                {
                    ICollection<Message> targetMessages = chat.Messages;

                    if (request.Body.Trim() != "")
                    {
                        var copiedMsg = new Message
                        {
                            Body = request.Body,
                            Sender = user,
                            Type = MessageType.Text
                        };
                        targetMessages!.Add(copiedMsg);
                    }


                    foreach (var msg in messages!)
                    {
                        var copiedMsg = new Message
                        {
                            Body = msg.Body,
                            Sender = user,
                            Type = msg.Type
                        };

                        if (request.ShowSender)
                        {
                            copiedMsg.ForwardedFrom = msg.Sender;
                        }

                        targetMessages!.Add(copiedMsg);
                    }
                }

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Result<bool>.Success(true);
                }
                return Result<bool>.Failure("Failed to save the forwarded messages to database.");
            }
        }
    }
}