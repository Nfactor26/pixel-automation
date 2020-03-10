using Pixel.Automation.Core.Interfaces;
using System;
using System.Drawing;

namespace Pixel.Automation.Core.Models
{
    public class ScrapedControl : IDisposable
    {
        public Bitmap ControlImage { get; set; }

        public IControlIdentity ControlData { get; set; }

        ~ScrapedControl()
        {
            Dispose(false);
        }

        public void Dispose()
        {         
            Dispose(true);          
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ControlImage?.Dispose();
            }           
        }

    }

    //public class ScrapedControlCollection : ICollection<ScrapedControl>
    //{
    //    List<ScrapedControl> controls = new List<ScrapedControl>();

    //    public int Count => controls.Count;

    //    public bool IsReadOnly => false;

    //    public void Add(ScrapedControl item)
    //    {
    //        if(item != null)
    //            controls.Add(item);
    //    }

    //    public void Clear()
    //    {
    //        controls.Clear();
    //    }

    //    public bool Contains(ScrapedControl item)
    //    {
    //        return controls.Contains(item);
    //    }

    //    public void CopyTo(ScrapedControl[] array, int arrayIndex)
    //    {
    //        controls.CopyTo(array, arrayIndex);
    //    }

    //    public IEnumerator<ScrapedControl> GetEnumerator()
    //    {
    //        return controls.GetEnumerator();
    //    }

    //    public bool Remove(ScrapedControl item)
    //    {
    //       return controls.Remove(item);
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return controls.GetEnumerator();
    //    }
    //}
}
