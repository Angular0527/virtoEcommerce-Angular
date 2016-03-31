using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



namespace VirtoCommerce.Client.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class VirtoCommerceStoreModuleWebModelSearchResult :  IEquatable<VirtoCommerceStoreModuleWebModelSearchResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtoCommerceStoreModuleWebModelSearchResult" /> class.
        /// Initializes a new instance of the <see cref="VirtoCommerceStoreModuleWebModelSearchResult" />class.
        /// </summary>
        /// <param name="TotalCount">TotalCount.</param>
        /// <param name="Stores">Stores.</param>

        public VirtoCommerceStoreModuleWebModelSearchResult(int? TotalCount = null, List<VirtoCommerceStoreModuleWebModelStore> Stores = null)
        {
            this.TotalCount = TotalCount;
            this.Stores = Stores;
            
        }

        /// <summary>
        /// Gets or Sets TotalCount
        /// </summary>
        [DataMember(Name="totalCount", EmitDefaultValue=false)]
        public int? TotalCount { get; set; }

        /// <summary>
        /// Gets or Sets Stores
        /// </summary>
        [DataMember(Name="stores", EmitDefaultValue=false)]
        public List<VirtoCommerceStoreModuleWebModelStore> Stores { get; set; }


        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class VirtoCommerceStoreModuleWebModelSearchResult {\n");
            sb.Append("  TotalCount: ").Append(TotalCount).Append("\n");
            sb.Append("  Stores: ").Append(Stores).Append("\n");
            
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as VirtoCommerceStoreModuleWebModelSearchResult);
        }

        /// <summary>
        /// Returns true if VirtoCommerceStoreModuleWebModelSearchResult instances are equal
        /// </summary>
        /// <param name="other">Instance of VirtoCommerceStoreModuleWebModelSearchResult to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(VirtoCommerceStoreModuleWebModelSearchResult other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.TotalCount == other.TotalCount ||
                    this.TotalCount != null &&
                    this.TotalCount.Equals(other.TotalCount)
                ) && 
                (
                    this.Stores == other.Stores ||
                    this.Stores != null &&
                    this.Stores.SequenceEqual(other.Stores)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)
                
                if (this.TotalCount != null)
                    hash = hash * 59 + this.TotalCount.GetHashCode();
                
                if (this.Stores != null)
                    hash = hash * 59 + this.Stores.GetHashCode();
                
                return hash;
            }
        }

    }


}
