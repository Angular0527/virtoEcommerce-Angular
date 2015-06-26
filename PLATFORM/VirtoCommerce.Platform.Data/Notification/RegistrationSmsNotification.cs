﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Notification;

namespace VirtoCommerce.Platform.Data.Notification
{
	public class RegistrationSmsNotification : SmsNotification
	{
		public RegistrationSmsNotification(Func<ISmsNotificationSendingGateway> smsNotificationSendingGateway) : base(smsNotificationSendingGateway)
		{

		}

		public string Login { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Password { get; set; }
	}
}
