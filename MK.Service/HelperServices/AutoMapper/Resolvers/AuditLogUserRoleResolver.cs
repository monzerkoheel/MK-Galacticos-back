using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MK.Core.DTOs;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Service.HelperServices.AutoMapper.Resolvers
{
    public class AuditLogUserRoleResolver : IValueResolver<AuditLog, AuditLogDto, string?>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogUserRoleResolver(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string? Resolve(AuditLog source, AuditLogDto destination, string? destMember, ResolutionContext context)
        {
            if (source.User is null)
                return null;

            var roles = _userManager.GetRolesAsync(source.User).GetAwaiter().GetResult();

            return roles.FirstOrDefault();
        }
    }
}
