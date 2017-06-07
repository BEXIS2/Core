using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.IO.Transform.Input;

namespace BExIS.IO.Transform.Input
{
    public class EasyUploadFileReaderInfo : FileReaderInfo
    {
        public int VariablesStartRow { get; set; }

        public int VariablesEndRow { get; set; }

        public int VariablesStartColumn { get; set; }

        public int VariablesEndColumn { get; set; }

        public int DataStartRow { get; set; }

        public int DataEndRow { get; set; }

        public int DataStartColumn { get; set; }

        public int DataEndColumn { get; set; }
    }
}