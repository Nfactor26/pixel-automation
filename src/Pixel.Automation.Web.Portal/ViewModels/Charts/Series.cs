using System.Collections.Generic;

namespace Pixel.Automation.Web.Portal.Charts
{
    public class Series<T>
    {
        public string Name { get; set; }

        public List<T> Data { get; set; } = new List<T>();

        public Series(string name, IEnumerable<T> data)
        {
            this.Name = name;
            if(data != null)
            {
                this.Data.AddRange(data);
            }
        }
    }
}
