﻿using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Scrapper
{
    [Serializable]
    [DataContract]
    public class ScrapedData
    {
        [DataMember]
        public string ControlLocation { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public int Left { get; set; }

        [DataMember]
        public int Top { get; set; }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string[] FrameHierarchy { get; set; }

        [DataMember]
        public int ScreenLeft { get; set; }

        [DataMember]
        public int ScreenTop { get; set; }


    }

  
}
