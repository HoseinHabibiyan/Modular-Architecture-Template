﻿using CleanArc.Application.Models.Common;
using CleanArc.Application.Models.Jwt;
using CleanArc.Identity.Infrastructure.Jwt;
using CleanArc.SharedKernel.Contracts.Identity;
using Mapster;
using Mediator;

namespace CleanArc.Identity.Application.Queries.Admin;

public class AdminGetTokenQueryHandler : IRequestHandler<AdminGetTokenQuery, OperationResult<AccessToken>>
{
    private readonly IAppUserManager _userManager;
    private readonly IJwtService _jwtService;
    public AdminGetTokenQueryHandler(IAppUserManager userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async ValueTask<OperationResult<AccessToken>> Handle(AdminGetTokenQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetByUserName(request.UserName);

        if (user is null)
            return OperationResult<AccessToken>.FailureResult("User not found");

        var isUserLockedOut = await _userManager.IsUserLockedOutAsync(user);

        if (isUserLockedOut)
            if (user.LockoutEnd != null)
                return OperationResult<AccessToken>.FailureResult(
                    $"User is locked out. Try in {(user.LockoutEnd - DateTimeOffset.Now).Value.Minutes} Minutes");

        var passwordValidator = await _userManager.AdminLogin(user, request.Password);


        if (!passwordValidator.Succeeded)
        {
            var lockoutIncrementResult = await _userManager.IncrementAccessFailedCountAsync(user);

            return OperationResult<AccessToken>.FailureResult("Password is not correct");
        }

        var token = await _jwtService.GenerateAsync(user.Adapt<Domain.User>());


        return OperationResult<AccessToken>.SuccessResult(token);
    }
}