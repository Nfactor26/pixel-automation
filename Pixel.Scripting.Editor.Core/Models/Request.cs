﻿using System.Collections.Generic;
using System.IO;

namespace Pixel.Scripting.Editor.Core.Models
{
    public interface IRequest
    {
    }

    public class SimpleFileRequest : IRequest
    {
        private string _fileName;

        public string FileName
        {
            get => _fileName?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            set => _fileName = value;
        }
    }

    public class Request : SimpleFileRequest
    {       
        public int Line { get; set; }
     
        public int Column { get; set; }

        public string Buffer { get; set; }

        public IEnumerable<LinePositionSpanTextChange> Changes { get; set; }
    }
}
