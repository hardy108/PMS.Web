using System;

namespace PMS.Shared.EFCoreUtilities
{
    struct Prop
    {
        public int ColumnOrdinal { get; set; }
        public Action<object, object> Setter { get; set; }
    }
}
