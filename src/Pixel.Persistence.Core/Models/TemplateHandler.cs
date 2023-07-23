using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pixel.Persistence.Core.Models;

[DataContract]
[JsonDerivedType(typeof(DockerTemplateHandler), typeDiscriminator: nameof(DockerTemplateHandler))]
[JsonDerivedType(typeof(WindowsTemplateHandler), typeDiscriminator: nameof(WindowsTemplateHandler))]
[JsonDerivedType(typeof(LinuxTemplateHandler), typeDiscriminator: nameof(LinuxTemplateHandler))]
[BsonKnownTypes(typeof(DockerTemplateHandler))]
[BsonKnownTypes(typeof(WindowsTemplateHandler))]
[BsonKnownTypes(typeof(LinuxTemplateHandler))]
public abstract class TemplateHandler : Document
{   
    [DataMember(Order = 10)]
    public string Name { get; set; }   

    [DataMember(Order = 20, IsRequired = true)]
    public Dictionary<string, string> Parameters { get; set; } = new ();

    [DataMember(Order = 30, IsRequired = false)]
    public string Description { get; set; }   
   
}

//Note : We need to duplicate JsonDerivedType on derived types so that $type information is serialized correctly when
//retrieving TemplateHandler from endpoints like get by name or get by id

[DataContract]
[JsonDerivedType(typeof(DockerTemplateHandler), typeDiscriminator: nameof(DockerTemplateHandler))]
public class DockerTemplateHandler : TemplateHandler
{
    [DataMember(Order = 100, IsRequired = true)]
    public string DockerComposeFileName { get; set; }
}

[DataContract]
[JsonDerivedType(typeof(WindowsTemplateHandler), typeDiscriminator: nameof(WindowsTemplateHandler))]
public class WindowsTemplateHandler : TemplateHandler
{
   
}

[DataContract]
[JsonDerivedType(typeof(LinuxTemplateHandler), typeDiscriminator: nameof(LinuxTemplateHandler))]
public class LinuxTemplateHandler : TemplateHandler
{

}

