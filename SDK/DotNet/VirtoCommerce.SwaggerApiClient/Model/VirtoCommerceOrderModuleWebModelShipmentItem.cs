using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;



namespace VirtoCommerce.SwaggerApiClient.Model {

  /// <summary>
  /// Represent information about quantity and line item belongs to shipment
  /// </summary>
  [DataContract]
  public class VirtoCommerceOrderModuleWebModelShipmentItem {
    
    /// <summary>
    /// Gets or Sets LineItemId
    /// </summary>
    [DataMember(Name="lineItemId", EmitDefaultValue=false)]
    public string LineItemId { get; set; }

    
    /// <summary>
    /// Gets or Sets LineItem
    /// </summary>
    [DataMember(Name="lineItem", EmitDefaultValue=false)]
    public VirtoCommerceOrderModuleWebModelLineItem LineItem { get; set; }

    
    /// <summary>
    /// Gets or Sets BarCode
    /// </summary>
    [DataMember(Name="barCode", EmitDefaultValue=false)]
    public string BarCode { get; set; }

    
    /// <summary>
    /// Gets or Sets Quantity
    /// </summary>
    [DataMember(Name="quantity", EmitDefaultValue=false)]
    public int? Quantity { get; set; }

    
    /// <summary>
    /// Gets or Sets CreatedDate
    /// </summary>
    [DataMember(Name="createdDate", EmitDefaultValue=false)]
    public DateTime? CreatedDate { get; set; }

    
    /// <summary>
    /// Gets or Sets ModifiedDate
    /// </summary>
    [DataMember(Name="modifiedDate", EmitDefaultValue=false)]
    public DateTime? ModifiedDate { get; set; }

    
    /// <summary>
    /// Gets or Sets CreatedBy
    /// </summary>
    [DataMember(Name="createdBy", EmitDefaultValue=false)]
    public string CreatedBy { get; set; }

    
    /// <summary>
    /// Gets or Sets ModifiedBy
    /// </summary>
    [DataMember(Name="modifiedBy", EmitDefaultValue=false)]
    public string ModifiedBy { get; set; }

    
    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    [DataMember(Name="id", EmitDefaultValue=false)]
    public string Id { get; set; }

    

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class VirtoCommerceOrderModuleWebModelShipmentItem {\n");
      
      sb.Append("  LineItemId: ").Append(LineItemId).Append("\n");
      
      sb.Append("  LineItem: ").Append(LineItem).Append("\n");
      
      sb.Append("  BarCode: ").Append(BarCode).Append("\n");
      
      sb.Append("  Quantity: ").Append(Quantity).Append("\n");
      
      sb.Append("  CreatedDate: ").Append(CreatedDate).Append("\n");
      
      sb.Append("  ModifiedDate: ").Append(ModifiedDate).Append("\n");
      
      sb.Append("  CreatedBy: ").Append(CreatedBy).Append("\n");
      
      sb.Append("  ModifiedBy: ").Append(ModifiedBy).Append("\n");
      
      sb.Append("  Id: ").Append(Id).Append("\n");
      
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
