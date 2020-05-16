using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Test.Helpers
{  
    [DataContract]
    public class Person
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Age { get; set; }

        [DataMember(IsRequired = false)]
        public Address Address { get; set; }
        
        [DataMember(IsRequired = false)]
        public List<Person> Friends { get; set; } = new List<Person>();

        public override bool Equals(object obj)
        {
            if(obj is Person person)
            {
                return person.Name.Equals(this.Name) && person.Age.Equals(this.Age) && person.Address.Equals(this.Address);
            }
            return false;
        }
    }
}
