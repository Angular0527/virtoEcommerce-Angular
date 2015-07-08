﻿using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Model;

namespace VirtoCommerce.Platform.Data.Repositories
{
    public interface IPlatformRepository : IRepository
    {
        IQueryable<SettingEntity> Settings { get; }

        IQueryable<DynamicPropertyEntity> DynamicProperties { get; }
        IQueryable<DynamicPropertyObjectValueEntity> DynamicPropertyValues { get; }

        IQueryable<AccountEntity> Accounts { get; }
        IQueryable<ApiAccountEntity> ApiAccounts { get; }
        IQueryable<RoleEntity> Roles { get; }
        IQueryable<PermissionEntity> Permissions { get; }
        IQueryable<RoleAssignmentEntity> RoleAssignments { get; }
        IQueryable<RolePermissionEntity> RolePermissions { get; }
        IQueryable<OperationLogEntity> OperationLogs { get; }

        IQueryable<NotificationEntity> Notifications { get; }
        IQueryable<NotificationTemplateEntity> NotificationTemplates { get; }

        AccountEntity GetAccountByName(string userName, UserDetails detailsLevel);
        NotificationTemplateEntity GetNotificationTemplateByNotification(string notificationTypeId, string objectId, string objectTypeId, string language);
    }
}
