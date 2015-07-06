﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Platform.Core.Notification
{
	public interface INotificationManager
	{
		SendNotificationResult SendNotification(Notification notification);
		void SheduleSendNotification(Notification notification);
		Notification GetNotificationById(string id);
		void StopSendingNotifications(string[] ids);
		T GetNewNotification<T>(GetNotificationCriteria criteria) where T : Core.Notification.Notification;
		Core.Notification.Notification GetNewNotification(GetNotificationCriteria criteria);
		void UpdateNotification(Notification notifications);
		void DeleteNotification(string id);
		SearchNotificationsResult SearchNotifications(SearchNotificationCriteria criteria);
		void RegisterNotificationType(Func<Notification> notification);
		Notification[] GetNotifications();
	}
}
