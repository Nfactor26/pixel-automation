using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class AutomationProject : Document
    {      
        /// <summary>
        /// Unique Identifier of the automation project
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display Name of the automation project
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// NameSpace for generated models. NameSpace must be unique
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public string Namespace { get; set; }

        [DataMember(IsRequired = true, Order = 50)]
        public List<VersionInfo> AvailableVersions { get; set; } = new();       
    }
}
