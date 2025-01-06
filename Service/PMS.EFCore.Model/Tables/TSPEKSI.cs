using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.EFCore.Model
{
    public partial class TSPEKSI
    {
        public string ID { get; set; }
        public string UNITID { get; set; }
        public string DIVID { get; set; }
        public string BLOCKID { get; set; }
        public DateTime TGLSPEKSI { get; set; }
        public string EMPLOYEEID { get; set; }
        public string MANDORID { get; set; }
        public decimal LUASSPEKSI { get; set; }
        public DateTime TGLPANEN { get; set; }
        public decimal TBSTDKPANEN { get; set; }
        public decimal TBSTDKBAWA { get; set; }
        public decimal BRDTDKKUTIP { get; set; }
        public decimal BRDTDKBERSIH { get; set; }
        public decimal BUNGAMTH { get; set; }
        public decimal PELEPAHTDKSUSUN { get; set; }
        public decimal PELEPAHTDKMEPET { get; set; }
        public decimal PELEPAHTDKSESUAI { get; set; }
        public bool HANCATERTINGGAL { get; set; }
        public string STATUS { get; set; }
        public string UPDATEBY { get; set; }
        public DateTime UPDATED { get; set; }

        public virtual MUNIT UNIT { get; set; }
        public virtual MDIVISI DIV { get; set; }
        public virtual MBLOCK BLOCK { get; set; }
        public virtual MEMPLOYEE EMPLOYEE { get; set; }
        public virtual MEMPLOYEE MANDOR { get; set; }
        public virtual MDOCSTATUS STATUSNavigation { get; set; }
    }
}
