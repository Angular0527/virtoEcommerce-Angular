using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;



namespace VirtoCommerce.Client.Model {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class VirtoCommercePlatformCoreNotificationsNotificationParameter {
    
    /// <summary>
    /// Gets or Sets ParameterName
    /// </summary>
    [DataMember(Name="parameterName", EmitDefaultValue=false)]
    public string ParameterName { get; set; }

    
    /// <summary>
    /// Gets or Sets ParameterDescription
    /// </summary>
    [DataMember(Name="parameterDescription", EmitDefaultValue=false)]
    public string ParameterDescription { get; set; }

    
    /// <summary>
    /// Gets or Sets ParameterCodeInView
    /// </summary>
    [DataMember(Name="parameterCodeInView", EmitDefaultValue=false)]
    public string ParameterCodeInView { get; set; }

    
    /// <summary>
    /// Gets or Sets IsDictionary
    /// </summary>
    [DataMember(Name="isDictionary", EmitDefaultValue=false)]
    public bool? IsDictionary { get; set; }

    
    /// <summary>
    /// Gets or Sets IsArray
    /// </summary>
    [DataMember(Name="isArray", EmitDefaultValue=false)]
    public bool? IsArray { get; set; }

    
    /// <summary>
    /// Gets or Sets Type
    /// </summary>
    [DataMember(Name="type", EmitDefaultValue=false)]
    public string Type { get; set; }

    
    /// <summary>
    /// Gets or Sets Value
    /// </summary>
    [DataMember(Name="value", EmitDefaultValue=false)]
    public Object Value { get; set; }

    

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class VirtoCommercePlatformCoreNotificationsNotificationParameter {\n");
      
      sb.Append("  ParameterName: ").Append(ParameterName).Append("\n");
      
      sb.Append("  ParameterDescription: ").Append(ParameterDescription).Append("\n");
      
      sb.Append("  ParameterCodeInView: ").Append(ParameterCodeInView).Append("\n");
      
      sb.Append("  IsDictionary: ").Append(IsDictionary).Append("\n");
      
      sb.Append("  IsArray: ").Append(IsArray).Append("\n");
      
      sb.Append("  Type: ").Append(Type).Append("\n");
      
      sb.Append("  Value: ").Append(Value).Append("\n");
      
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
