using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.EFCore.Model
{
    //Not Related To DB
    public class VRKH1MANDOR
    {
        public string ACTID { get; set; }
        public string ACTNAME { get; set; }
        public string UOM { get; set; }
        public string MANDORID { get; set; }
        public string MANDORNAME { get; set; }
    }

    public class VRKH1BLOCK
    {
        public string BLOCKID { get; set; }
        public decimal LUASBLOCK { get; set; }
        public short THNTANAM { get; set; }
        public short SPH { get; set; }
        public string TOPOGRAFI { get; set; }
    }

    public class VRKH1EMPLOYEE
    {
        public string DIVID { get; set; }
        public string EMPID { get; set; }
        public string EMPNAME { get; set; }
        public string EMPTYPE { get; set; }
    }

    public class VRKH1MATERIAL
    {
        public string MATID { get; set; }
        public string MATNAME { get; set; }
        public string UOM { get; set; }
    }

    



}