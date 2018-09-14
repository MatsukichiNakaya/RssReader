﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RSSReader.Model
{
    [Serializable]
    public class RssConfigure
    {
        [XmlElement("BrowserOption", DataType = "string", IsNullable = true)]
        public String BrowserOption { get; set; }

        [XmlElement("UpdateSpan", DataType = "int", IsNullable =false)]
        public Int32 UpdateSpan { get; set; }

        [XmlElement("ShowImage", DataType = "boolean", IsNullable = false)]
        public Boolean IsShowImage { get; set; }
     
    }
}