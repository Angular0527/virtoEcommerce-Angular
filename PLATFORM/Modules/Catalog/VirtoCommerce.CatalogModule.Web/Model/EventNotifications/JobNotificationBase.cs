﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Notification;

namespace VirtoCommerce.CatalogModule.Web.Model.EventNotifications
{
	public class JobNotificationBase : NotifyEvent
	{
		public JobNotificationBase(string creator)
			: base(creator)
		{
			Errors = new List<string>();
		}
		[JsonProperty("finished")]
		public DateTime? Finished { get; set; }
		[JsonProperty("totalCount")]
		public long TotalCount { get; set; }
		[JsonProperty("processedCount")]
		public long ProcessedCount { get; set; }
		[JsonProperty("errorCount")]
		public long ErrorCount { get; set; }
		[JsonProperty("errors")]
		public ICollection<string> Errors { get; set; }
	}
}