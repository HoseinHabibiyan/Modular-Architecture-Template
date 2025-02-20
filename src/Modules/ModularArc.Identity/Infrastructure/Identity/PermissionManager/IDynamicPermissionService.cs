﻿using System.Security.Claims;

namespace ModularArc.Identity.Infrastructure.Identity.PermissionManager;

public interface IDynamicPermissionService
{
    bool CanAccess(ClaimsPrincipal user, string area, string controller, string action);
}