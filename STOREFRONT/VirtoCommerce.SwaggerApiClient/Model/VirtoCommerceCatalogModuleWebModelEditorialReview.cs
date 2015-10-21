using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;



namespace VirtoCommerce.SwaggerApiClient.Model {

  /// <summary>
  /// Editorial review for an item.
  /// </summary>
  [DataContract]
  public class VirtoCommerceCatalogModuleWebModelEditorialReview {
    
    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    [DataMember(Name="id", EmitDefaultValue=false)]
    public string Id { get; set; }

    
    /// <summary>
    /// Gets or sets the review content.
    /// </summary>
    /// <value>Gets or sets the review content.</value>
    [DataMember(Name="content", EmitDefaultValue=false)]
    public string Content { get; set; }

    
    /// <summary>
    /// Gets or sets the type of the review.
    /// </summary>
    /// <value>Gets or sets the type of the review.</value>
    [DataMember(Name="reviewType", EmitDefaultValue=false)]
    public string ReviewType { get; set; }

    
    /// <summary>
    /// Gets or sets the review language.
    /// </summary>
    /// <value>Gets or sets the review language.</value>
    [DataMember(Name="languageCode", EmitDefaultValue=false)]
    public string LanguageCode { get; set; }

    

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class VirtoCommerceCatalogModuleWebModelEditorialReview {\n");
      
      sb.Append("  Id: ").Append(Id).Append("\n");
      
      sb.Append("  Content: ").Append(Content).Append("\n");
      
      sb.Append("  ReviewType: ").Append(ReviewType).Append("\n");
      
      sb.Append("  LanguageCode: ").Append(LanguageCode).Append("\n");
      
      sb.Append("}\n");
      return sb.ToString();
    }

    /// <summary>
    /// Get the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson() {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}


}
