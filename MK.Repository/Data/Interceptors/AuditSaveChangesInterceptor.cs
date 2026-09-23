using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.HelperModels;
using MK.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace MK.Repository.Data.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly HashSet<string> SensitiveProperties =
        [
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "RefreshToken",
            "RefreshTokenExpiryTime"
        ];
        
        private static readonly HashSet<string> AuditedUserProperties =
        [
            "FullName",
            "ProfileImageUrl",
            "Status",
            "PhoneNumber"
        ];
        
        private readonly ICurrentUserService _currentUserService;
        private readonly IRequestContext _requestContext;
        private readonly List<(AuditLog AuditLog, ModelBase Entity, Dictionary<string, object?> NewValues)> _pendingCreateAudits = [];

        public AuditSaveChangesInterceptor(ICurrentUserService currentUserService, IRequestContext requestContext)
        {
            _currentUserService = currentUserService;
            _requestContext = requestContext;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            if (context == null)
                return result;

            var entries = context.ChangeTracker
                .Entries()
                .Where(x =>
                    x.Entity is not AuditLog &&
                    (x.State == EntityState.Added ||
                     x.State == EntityState.Modified ||
                     x.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                if (entry.Entity is ApplicationUser user)
                {
                    if (entry.State == EntityState.Added)
                        continue;

                    foreach (var property in entry.Properties)
                    {
                        var propertyName = property.Metadata.Name;

                        if (entry.State == EntityState.Modified)
                        {

                            if (!AuditedUserProperties.Contains(propertyName) &&
                                propertyName != "PasswordHash")
                            {
                                continue;
                            }

                            if (Equals(property.OriginalValue, property.CurrentValue))
                                continue;

                            if (propertyName == "PasswordHash")
                            {
                                newValues["PasswordChanged"] = true;
                                continue;
                            }

                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;

                            continue;
                        }

                        // -------------------------
                        // Delete
                        // -------------------------
                        if (entry.State == EntityState.Deleted)
                        {
                            // Never store sensitive properties
                            if (SensitiveProperties.Contains(propertyName))
                                continue;

                            oldValues[propertyName] = property.OriginalValue;
                        }
                    }

                    if (oldValues.Count == 0 && newValues.Count == 0)
                        continue;

                    var action = entry.State switch
                    {
                        EntityState.Modified => AuditAction.Update,
                        EntityState.Deleted => AuditAction.Delete,
                        _ => throw new InvalidOperationException()
                    };
                                        
                    var auditLog = new AuditLog
                    {
                        UserId = _currentUserService.UserId,
                        Action = action,
                        ActorType = GetActorType(),
                        EntityName = nameof(ApplicationUser),
                        EntityId = user.Id,
                        OldValues = oldValues.Count > 0
                            ? JsonSerializer.Serialize(oldValues, AuditJsonOptions)
                            : null,
                        NewValues = newValues.Count > 0
                            ? JsonSerializer.Serialize(newValues, AuditJsonOptions)
                            : null,
                        IpAddress = _requestContext.IpAddress,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Set<AuditLog>().Add(auditLog);
                }

                else
                {
                    var entity = entry.Entity as ModelBase;
                    var entityId = entity?.Id.ToString();

                    var entityName = entry.Metadata.ClrType.Name;

                    AuditAction action = entry.State switch
                    {
                        EntityState.Added => AuditAction.Create,
                        EntityState.Modified => AuditAction.Update,
                        EntityState.Deleted => AuditAction.Delete,
                        _ => throw new InvalidOperationException()
                    };

                    foreach (var property in entry.Properties)
                    {
                        if (SensitiveProperties.Contains(property.Metadata.Name))
                            continue;

                        if (entry.State == EntityState.Modified &&
                            Equals(property.OriginalValue, property.CurrentValue))
                        {
                            continue;
                        }

                        switch (entry.State)
                        {
                            case EntityState.Added:
                                newValues[property.Metadata.Name] = property.CurrentValue;
                                break;

                            case EntityState.Modified:
                                oldValues[property.Metadata.Name] = property.OriginalValue;
                                newValues[property.Metadata.Name] = property.CurrentValue;
                                break;

                            case EntityState.Deleted:
                                oldValues[property.Metadata.Name] = property.OriginalValue;
                                break;
                        }
                    }

                    var oldValuesJson = oldValues.Count > 0
                        ? JsonSerializer.Serialize(oldValues, AuditJsonOptions)
                        : null;

                    var newValuesJson = newValues.Count > 0
                        ? JsonSerializer.Serialize(newValues, AuditJsonOptions)
                        : null;

                    var auditLog = new AuditLog
                    {
                        UserId = _currentUserService.UserId,
                        Action = action,
                        ActorType = GetActorType(),
                        EntityName = entityName,
                        EntityId = entityId,
                        OldValues = oldValuesJson,
                        NewValues = newValuesJson,
                        IpAddress = _requestContext.IpAddress,
                        CreatedAt = DateTime.UtcNow
                    };

                    if (entry.State == EntityState.Added)
                    {
                        if (entity != null)
                        {
                            auditLog.EntityId = null;

                            _pendingCreateAudits.Add(
                                (auditLog, entity, new Dictionary<string, object?>(newValues))
                            );
                        }
                    }
                    else
                    {
                        context.Set<AuditLog>().Add(auditLog);
                    }
                }
            }

            return result;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            if (context == null || _pendingCreateAudits.Count == 0)
                return result;

            var pendingAudits = _pendingCreateAudits.ToList();

            _pendingCreateAudits.Clear();

            foreach (var (auditLog, entity, newValues) in pendingAudits)
            {
                var entityId = entity.Id;

                auditLog.EntityId = entityId.ToString();

                newValues["Id"] = entityId;

                auditLog.NewValues = JsonSerializer.Serialize(
                    newValues,
                    AuditJsonOptions);

                context.Set<AuditLog>().Add(auditLog);
            }

            await context.SaveChangesAsync(cancellationToken);

            return result;
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            _pendingCreateAudits.Clear();

            return Task.CompletedTask;
        }

        private AuditActorType GetActorType()
        {
            if (_currentUserService.UserId is null)
                return AuditActorType.System;

            if (_currentUserService.Role == "Customer")
                return AuditActorType.User;

            return AuditActorType.Management;
        }

        private static readonly JsonSerializerOptions AuditJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
                {
                    new JsonStringEnumConverter()
                }
        };
    
    
    }
}
