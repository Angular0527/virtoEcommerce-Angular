﻿using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Platform.Data.Model
{
    public class DynamicPropertyNameEntity : AuditableEntity
    {
        [StringLength(64)]
        public string Locale { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        public string PropertyId { get; set; }
        public virtual DynamicPropertyEntity Property { get; set; }
    }
}
