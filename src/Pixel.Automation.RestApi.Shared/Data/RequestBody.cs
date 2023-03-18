using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Shared;

[DataContract]
[Serializable]
public abstract class BodyContent : NotifyPropertyChanged
{
    protected string contentType;
    [DataMember]
    [DisplayName("Content Type")]
    public virtual string ContentType
    {
        get => contentType;
        set
        {
            contentType = value;
            OnPropertyChanged();
        }
    }
}

/// <summary>
/// BodyContent without any value for an <see cref="HttpRequest"/>
/// </summary>
[DataContract]
[Serializable]
public class EmptyBodyContent : BodyContent
{

}

/// <summary>
/// Body content with raw data e.g. json serialized
/// </summary>
[DataContract]
[Serializable]
public class RawBodyContent : BodyContent
{
    /// <summary>
    /// Content to add to body
    /// </summary>
    [DataMember(Order = 20)]
    public Argument Content { get; set;  } = new InArgument<string>() { AllowedModes = ArgumentMode.DataBound|ArgumentMode.Scripted, Mode = ArgumentMode.DataBound,  CanChangeType = true };
   
    /// <summary>
    /// constructor
    /// </summary>
    public RawBodyContent()
    {
        this.contentType = "application/json";
    }
}

/// <summary>
/// Binary content from file to add to body
/// </summary>
[DataContract]
[Serializable]
public class BinaryBodyContent : BodyContent
{
    /// <summary>
    /// Path of the file
    /// </summary>
    [DataMember(Order = 20)]   
    public Argument Content { get; set; } = new InArgument<string>() { CanChangeType = true  };

}   

[DataContract]
[Serializable]
public class FormField : NotifyPropertyChanged
{
    /// <summary>
    /// Indicates if FormField row is enabled
    /// </summary>
    [DataMember(Order = 10)]
    [DisplayName("Enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Key for the FormField
    /// </summary>
    [DataMember(Order = 20)]
    public string DataKey { get; set; }

    /// <summary>
    /// Value of the FormField
    /// </summary>
    [DataMember(Order = 30)]
    public Argument DataValue { get; set; } = new InArgument<string>() { DefaultValue = string.Empty };

    /// <summary>
    /// Data type for the FormField
    /// </summary>
    [DataMember(Order = 40)]
    public FormDataType DataType { get; set; } = FormDataType.Text;

    /// <summary>
    /// ContentType for the form field
    /// </summary>
    [DataMember(Order = 50)]
    public string ContentType { get; set; }

    /// <summary>
    /// Description of the FormField
    /// </summary>
    [DataMember(Order = 60)]
    public string Description { get; set; }
    
    /// <summary>
    /// constructor
    /// </summary>
    public FormField()
    {

    }

}

/// <summary>
/// Body content with a collection of <see cref="FormField"/> for an <see cref="HttpRequest"/>
/// </summary>
public class FormDataBodyContent : BodyContent
{
    /// <summary>
    /// Collection of FormField to add in the Body of request
    /// </summary>
    [DataMember(Order = 20)]
    public List<FormField> FormFields = new();   
}

