using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PMS.EFCore.Helper;

namespace PMS.EFCore.Model.Filter
{

    public class GeneralFilter:GeneralPagingFilter
    {
        public GeneralFilter()
        {
            Ids = new List<string>();
            _unitIds = new List<string>();
            _divisionIds = new List<string>();
            Date = DateTime.Today;
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(1 - EndDate.Day);
        }

        public long WFTransNo { get; set; }
        public string Active { get; set; }
        public bool? IsActive
        {
            get
            {

                try
                {
                    if (!string.IsNullOrWhiteSpace(Active))
                    {
                        if (Active.Equals("true"))
                            return true;
                        if (Active.Equals("false"))
                            return false;
                    }
                    return null;
                }
                catch { return null; }
            }
        }

        
        public string RegionID { get; set; }
        public string AreaID { get; set; }

        public string UnitID { get; set; }
        private List<string> _unitIds;
        public List<string> UnitIDs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UnitID))
                    return _unitIds;
                return _unitIds.Union(new List<string> { UnitID }).ToList();
            }
            set { _unitIds = value; }
        }

        public string DivisionID { get; set; }
        private List<string> _divisionIds;
        public List<string> DivisionIDs
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DivisionID))
                    return _divisionIds;
                return _divisionIds.Union(new List<string> { DivisionID }).ToList();
            }
            set
            {
                _divisionIds = value;
            }
        }
        
        public DateTime Date { get; set; }



        public DateTime StartDate { get; set; }


        public DateTime EndDate { get; set; }
        public string Id { get; set; }
        public List<string> Ids { get; set; }
        public string RecordStatus { get; set; }
        public string UserName { get; set; }
        

        public string MenuID { get; set; }

        

        
    }
}
