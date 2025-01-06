using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Shared.Models
{
    public enum XFieldType
    {
        FText = 0,
        FInteger,
        FFloat,
        FBool,
        FDate
    }

    public enum XFieldAlignment
    {
        FLeft = 0,
        FCenter,
        FRight
    }
    public enum XFieldSort
    {
        FNoSort = 0,
        FSortAsc,
        FSorcDesc
    }
    public enum XListType
    {
        Data = 0,
        SingleLoV,
        MultipleLoV
    }
}
