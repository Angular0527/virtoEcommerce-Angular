﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.MerchandisingModule.Web.Model
{
	public class Asset
	{
		public string Url { get; set; }
		public string Group { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public string MimeType { get; set; }
	}
}