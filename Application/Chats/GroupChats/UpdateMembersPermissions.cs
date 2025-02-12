﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Direct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Chats.GroupChats
{
    public class UpdateMembersPermissions
    {
        public class Command : IRequest<Result<MemberPermissionsDto>>
        {
            public Guid ChatId { get; set; }
            public bool SendMessages { get; set; }
            public bool SendMedia { get; set; }
            public bool AddUsers { get; set; }
            public bool PinMessages { get; set; }
            public bool ChangeChatInfo { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<MemberPermissionsDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _accessor;

            public Handler(DataContext context, IUserAccessor accessor)
            {
                _context = context;
                _accessor = accessor;
            }

            public async Task<Result<MemberPermissionsDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                
                var userChat = await _context.UserChats
                    .Include(x => x.AppUser)
                    .Include(x => x.Chat)
                    .SingleOrDefaultAsync(x => x.AppUser.UserName == _accessor.GetUsername()
                                              && x.ChatId == request.ChatId, cancellationToken);

                if (userChat == null)
                {
                    return Result<MemberPermissionsDto>.Failure("Chat does not exist");
                }
                if (userChat.Chat.Type != ChatType.Group)
                {
                    return Result<MemberPermissionsDto>.Failure("Chat is not a group");
                }
                if (userChat.MembershipType != MemberType.Owner && userChat.MembershipType != MemberType.Admin)
                {
                    return Result<MemberPermissionsDto>.Failure("User is not the owner or an admin of the group.");
                }

                var chat = userChat.Chat;

                chat.SendMessages = request.SendMessages;
                chat.SendMedia = request.SendMedia;
                chat.AddUsers = request.AddUsers;
                chat.PinMessages = request.PinMessages;
                chat.ChangeChatInfo = request.ChangeChatInfo;
                                
                var result = await _context.SaveChangesAsync(cancellationToken);

                if (result > 0)
                {
                    var dto = new MemberPermissionsDto
                    {
                        SendMessages = chat.SendMessages,
                        SendMedia = chat.SendMedia,
                        AddUsers = chat.AddUsers,
                        PinMessages = chat.PinMessages,
                        ChangeChatInfo = chat.ChangeChatInfo
                    };
                    return Result<MemberPermissionsDto>.Success(dto);
                }

                return Result<MemberPermissionsDto>.Failure("Failed saving the new permissions to database");
            }
        }
    }
}