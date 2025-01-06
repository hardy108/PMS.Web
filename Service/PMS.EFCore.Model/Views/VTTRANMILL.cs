using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    public class VTTRANMILL
    {
        public string WBUNITNAME { get; set; }
        public string ESTATENAME { get; set; }
        public string PRODUCTNAME { get; set; }
        public decimal? TONASE { get; set; }
        public string ID { get; set; }
        public string WBUNITID { get; set; }
        public string NO { get; set; }
        public string TICKETNO { get; set; }
        public DateTime? SENDDATE { get; set; }
        public DateTime DATEIN { get; set; }
        public DateTime DATETRXIN { get; set; }
        public DateTime? DATEOUT { get; set; }
        public DateTime? DATETRXOUT { get; set; }
        public int SQIN { get; set; }
        public int SIN { get; set; }
        public int SOUT { get; set; }
        public string UNITID { get; set; }
        public string UNITGRP { get; set; }
        public string PRODID { get; set; }
        public string SENDID { get; set; }
        public string DIVID { get; set; }
        public string VEHID { get; set; }
        public string DRIVER { get; set; }
        public string SIM { get; set; }
        public string DONO { get; set; }
        public string CONTID { get; set; }
        public string SOID { get; set; }
        public string TRANID { get; set; }
        public string BUYID { get; set; }
        public bool MULTILOC { get; set; }
        public string LOCID { get; set; }
        public decimal DELWEIGHT { get; set; }
        public decimal WEIGHT1 { get; set; }
        public decimal WEIGHT2 { get; set; }
        public bool DIRECT { get; set; }
        public string PC1 { get; set; }
        public string PC2 { get; set; }
        public bool AUTO1 { get; set; }
        public bool AUTO2 { get; set; }
        public string OPR1 { get; set; }
        public string OPR2 { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATED { get; set; }
    }

}
