using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model.Filter
{
    public class FilterLoading : GeneralFilter
    {
        public string LoadingCode { get; set; }
        public int? ProductId { get; set; }
        public int? LoadingType { get; set; }
        public int? SPBDataType { get; set; }
        public string NoSPB { get; set; }
        public string VehicleId { get; set; }
        public string VehicleTypeId { get; set; }
        public int? LoadingPaymentType { get; set; }
        public string ActivityId { get; set; }
        public String EmpType { get; set; }
        public bool? Eflag { get; set; }
    }
}
