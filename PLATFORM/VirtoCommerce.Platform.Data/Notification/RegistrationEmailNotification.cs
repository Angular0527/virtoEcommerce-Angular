﻿using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Notification;

namespace VirtoCommerce.Platform.Data.Notification
{
	[CLSCompliant(false)]
	[LiquidType("Login", "FirstName", "LastName")]
	public class RegistrationEmailNotification : EmailNotification
	{
		public RegistrationEmailNotification(IEmailNotificationSendingGateway emailNotificationSendingGateway) : base(emailNotificationSendingGateway)
		{

		}

		public string Login { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
