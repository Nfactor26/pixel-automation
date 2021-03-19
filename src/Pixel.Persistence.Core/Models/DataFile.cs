using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Persistence.Core.Models
{
    public class DataFile
    {
        public string FileName { get; set; }

        public string Type { get; set; }

        public byte[] Bytes { get; set; }
    }

    public class ImageDataFile : DataFile
    {
        public string Resolution { get; set; }
    }
}
