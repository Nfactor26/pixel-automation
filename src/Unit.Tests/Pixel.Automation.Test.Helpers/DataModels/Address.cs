using System.Runtime.Serialization;

namespace Pixel.Automation.Test.Helpers
{
    [DataContract]
    public class Address
    {
        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string Country { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is Address address)
            {
                return address.City.Equals(this.City) && address.Country.Equals(this.Country);
            }
            return false;
        }
    }
}
