using Pixel.Automation.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Designer.Views
{
    public class TemplateMapper
    {
        private readonly static string mappingFile = @"Resources\TemplateMapping.xml";
        private readonly static List<ComponentTemplate> templateMappings;
        static TemplateMapper()
        {
            if(!File.Exists(mappingFile))
            {
                throw new FileNotFoundException("TemplateMapping.xml doesn't exist in the Resources folder");
            }
            XmlSerializer serializer = new XmlSerializer();
            templateMappings = serializer.Deserialize<List<ComponentTemplate>>(mappingFile);

        }

        public static string GetDataTemplateKey(string componentName)
        {
            return templateMappings.Where(c => c.Name.Equals(componentName)).FirstOrDefault()?.DataTemplateKey;
        }

        public static string GetStyleKey(string componentName)
        {
            return templateMappings.Where(c => c.Name.Equals(componentName)).FirstOrDefault()?.StyleKey;
        }
    }

    [DataContract]
    [Serializable]
    public class ComponentTemplate
    {
        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public string DataTemplateKey { get; set; }

        [DataMember(Order = 2)]
        public string StyleKey { get; set; }

        public ComponentTemplate()
        {

        }

        public ComponentTemplate(string name,string dataTemplateKey,string styleKey)
        {
            this.Name = name;
            this.DataTemplateKey = dataTemplateKey;
            this.StyleKey = styleKey;
        }
    }
}
