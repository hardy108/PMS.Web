using System;

namespace PMS.EFCore.Helper
{
    [Obsolete]
    public class ParamExtra
    {
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }
}
