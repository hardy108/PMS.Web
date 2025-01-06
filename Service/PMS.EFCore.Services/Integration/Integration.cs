using System;
using System.Collections.Generic;
using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Model.Filter;
using PMS.EFCore.Services;
using PMS.EFCore.Helper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using PMS.Shared.Utilities;
using AM.EFCore.Services;
using PMS.SAPIntegration;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using Microsoft.Extensions.Options;
using PMS.Shared.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace PMS.EFCore.Services.Integration
{
    public class Integration : EntityFactory<SAPPOSTING, SAPPOSTING, FilterCompany, PMSContextBase>
    {
        private AuthenticationServiceBase _authenticationService;
        private SAPConnectorInterface _serviceRfc;
        public Integration(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext, IOptions<AppSetting> appSetting) : base(context, auditContext)
        {
            _serviceName = "Integration";
            _authenticationService = authenticationService;
            _serviceRfc = new SAPConnectorInterface(appSetting);
        }

        public async Task<string> TesRFC(string userName)
        {
            var compName = await _serviceRfc.TesRFC(userName);

            return compName.ToString();
        }

        public async Task<string> CompName(string code)
        {
            var compName = await _serviceRfc.CompName(code);

            return compName.ToString();
        }

        public async Task<bool> PMSToSapRFC(string userName) //string unitCode
        {
            var return_ = true;
            //var posting = _context.SAPPOSTING.Where(a => string.IsNullOrEmpty(a.POSTING)
            //                                        || a.POSTING.Equals("E")
            //                                        || (a.POSTING.Equals("X") && string.IsNullOrEmpty(a.SAP_DOC_NO))
            //                                        || (a.CANCEL.Equals("X") && string.IsNullOrEmpty(a.SAP_DOC_NO_CANCEL))
            //                                        || (a.CANCEL.Equals("E") && string.IsNullOrEmpty(a.SAP_DOC_NO_CANCEL))).ToList();
            var posting = _context.SAPPOSTINGHEADER.Include(b => b.SAPPOSTING.Where(a => string.IsNullOrEmpty(a.POSTING)
                                                    || a.POSTING.Equals("E")
                                                    || (a.POSTING.Equals("X") && string.IsNullOrEmpty(a.SAP_DOC_NO))
                                                    || (a.CANCEL.Equals("X") && string.IsNullOrEmpty(a.SAP_DOC_NO_CANCEL))
                                                    || (a.CANCEL.Equals("E") && string.IsNullOrEmpty(a.SAP_DOC_NO_CANCEL)))).ToList();

            //DataTable result = new DataTable();
            int index = 0;
            var rfcResult = string.Empty;
            DataSet dt = new DataSet();
            List<FTRETURN> result = new List<FTRETURN>();
            foreach (var header in posting)
            {
                List<SAPPOSTING> detail = new List<SAPPOSTING>();
                detail.Clear();
                if (header.TYPE == "BKM")
                {
                    //=========== B K M ======================
                    detail.CopyFrom(header.SAPPOSTING);
                    //if (rfc.DOCTYPE == "BKM_GI" || rfc.DOCTYPE == "BKM_PS" || rfc.DOCTYPE == "BKM_CO")
                    //{
                    if (header.POSTING != "S" && string.IsNullOrEmpty(header.CANCEL))
                    {
                        var detBkmGI = (from tab in detail
                                       where tab.DOCTYPE == "BKM_GI"
                                       select tab).ToList() ;

                        //var retGI = await _serviceRfc.UploadBkm(detBkmGI, "BKM_GI", userName);
                        //index = 0;
                        //foreach (var x in retGI)
                        //{
                        //    updReturn(detBkmGI, x, index);
                        //    if (x.STAT == "E") return_ = false;
                        //    index++;
                        //}

                        var detBkmPS = (from tab in detail
                                        where tab.DOCTYPE == "BKM_PS"
                                        select tab).ToList();

                        //var retPS = await _serviceRfc.UploadBkm(detBkmPS, "BKM_PS", userName);
                        //index = 0;
                        //foreach (var x in retPS)
                        //{
                        //    updReturn(detBkmPS, x, index);
                        //    if (x.STAT == "E") return_ = false;
                        //    index++;
                        //}

                        var detBkmCO = (from tab in detail
                                        where tab.DOCTYPE == "BKM_CO"
                                        select tab).ToList();

                        //var retCO = await _serviceRfc.UploadBkm(detBkmCO, "BKM_CO", userName);
                        //index = 0;
                        //foreach (var x in retCO)
                        //{
                        //    updReturn(detBkmCO, x, index);
                        //    if (x.STAT == "E") return_ = false;
                        //    index++;
                        //}
                    }

                    if (header.POSTING == "S" && header.CANCEL == "X")
                    {
                        var detBkmGI = (from tab in detail
                                        where tab.DOCTYPE == "BKM_GI"
                                        select tab).ToList();

                        var retGI = await _serviceRfc.CancelUploadBkm(detBkmGI, "BKM_GI", userName);
                        index = 0;
                        foreach (var x in retGI)
                        {
                            updReturn(detBkmGI, x, index);
                            if (x.STAT == "E") return_ = false;
                            index++;
                        }

                        var detBkmPS = (from tab in detail
                                        where tab.DOCTYPE == "BKM_PS"
                                        select tab).ToList();

                        var retPS = await _serviceRfc.CancelUploadBkm(detBkmPS, "BKM_PS", userName);
                        index = 0;
                        foreach (var x in retPS)
                        {
                            updReturn(detBkmPS, x, index);
                            if (x.STAT == "E") return_ = false;
                            index++;
                        }

                        var detBkmCO = (from tab in detail
                                        where tab.DOCTYPE == "BKM_CO"
                                        select tab).ToList();

                        var retCO = await _serviceRfc.CancelUploadBkm(detBkmCO, "BKM_CO", userName);
                        index = 0;
                        foreach (var x in retCO)
                        {
                            updReturn(detBkmCO, x, index);
                            if (x.STAT == "E") return_ = false;
                            index++;
                        }
                    }
                    //}
                    header.POSTING = "X";
                    _context.SAPPOSTINGHEADER.Update(header);
                    _context.SaveChanges();
                }
                //=========== B K M   P L A S M A ======================
                //if (rfc.DOCTYPE == "BKMP_GI" || rfc.DOCTYPE == "BKMP_PS" || rfc.DOCTYPE == "BKMP_CO")
                //{
                //    if (rfc.POSTING != "S" && string.IsNullOrEmpty(rfc.CANCEL))
                //    {
                //        bkm = rfc;
                //        bkm.POSTING = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.UploadBkmPlasma(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }

                //    if (rfc.POSTING == "S" && rfc.CANCEL == "X")
                //    {
                //        bkm = rfc;
                //        bkm.CANCEL = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.CancelUploadBkmPlasma(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }
                //}

                ////=========== B U K U   P A N E N ======================
                //if (rfc.DOCTYPE == "BP_PS" || rfc.DOCTYPE == "BP_CO")
                //{
                //    if (rfc.POSTING != "S" && string.IsNullOrEmpty(rfc.CANCEL))
                //    {
                //        bkm = rfc;
                //        bkm.POSTING = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.UploadHarvesting(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }

                //    if (rfc.POSTING == "S" && rfc.CANCEL == "X")
                //    {
                //        bkm = rfc;
                //        bkm.CANCEL = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.CancelUploadHarvesting(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }
                //}

                ////=========== B U K U   P A N E N   P L A S M A ======================
                //if (rfc.DOCTYPE == "BPP_PS" || rfc.DOCTYPE == "BPP_CO")
                //{
                //    if (rfc.POSTING != "S" && string.IsNullOrEmpty(rfc.CANCEL))
                //    {
                //        bkm = rfc;
                //        bkm.POSTING = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.UploadHarvestingPlasma(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }

                //    if (rfc.POSTING == "S" && rfc.CANCEL == "X")
                //    {
                //        bkm = rfc;
                //        bkm.CANCEL = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.CancelUploadHarvestingPlasma(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }
                //}

                ////=========== S P B ======================
                //if (rfc.DOCTYPE == "SPB" )
                //{
                //    if (rfc.POSTING != "S" && string.IsNullOrEmpty(rfc.CANCEL))
                //    {
                //        bkm = rfc;
                //        bkm.POSTING = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.UploadSPB(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }

                //    if (rfc.POSTING == "S" && rfc.CANCEL == "X")
                //    {
                //        bkm = rfc;
                //        bkm.CANCEL = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.CancelUploadSPB(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }
                //}

                ////=========== C A R  L O G  / P M ======================
                //if (rfc.DOCTYPE == "PM")
                //{
                //    if (rfc.POSTING != "S" && string.IsNullOrEmpty(rfc.CANCEL))
                //    {
                //        bkm = rfc;
                //        bkm.POSTING = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.UploadCarLog(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }

                //    if (rfc.POSTING == "S" && rfc.CANCEL == "X")
                //    {
                //        bkm = rfc;
                //        bkm.CANCEL = "X";
                //        _context.SAPPOSTING.Update(bkm);
                //        _context.SaveChanges();
                //        var ret = await _serviceRfc.CancelUploadCarLog(bkm, rfc.DOCTYPE, userName);
                //        foreach (var x in ret)
                //        {
                //            updReturn(bkm, x);
                //            if (x.STAT == "E") return_ = false;
                //        }
                //    }
                //}
            }

            return return_;
        }

        private void updReturn(List<SAPPOSTING> bkm, FTRETURN ret, int index)
        {
            if (ret.STAT == "S")
            {
                bkm.ForEach(a => { a.POSTING = "S"; a.SAP_DOC_NO = ret.DOCNO; a.MSG_ERROR = ""; });
                //bkm.POSTING = "S";
                //bkm.SAP_DOC_NO = ret.DOCNO;
                //bkm.MSG_ERROR = "";
                _context.SAPPOSTING.UpdateRange(bkm);
                _context.SaveChanges();
            }
            else if (ret.STAT == "E")
            {
                var row = bkm.ElementAt(index);
                row.POSTING = "E";
                row.MSG_ERROR = ret.MSG.ToString();
                _context.SAPPOSTING.Update(row);
                _context.SaveChanges();
            }
        }

        public bool CheckSapPost()
        {
            var posting = _context.SAPPOSTING.Where(a => string.IsNullOrEmpty(a.POSTING));
            if (posting.Count() > 0)
            {
                return true;
            }

            posting = _context.SAPPOSTING.Where(a => a.POSTING.Equals("X") && string.IsNullOrEmpty(a.SAP_DOC_NO));
            if (posting.Count() > 0)
            {
                return true;
            }

            posting = _context.SAPPOSTING.Where(a => a.POSTING.Equals("E"));
            if (posting.Count() > 0)
            {
                return true;
            }

            posting = _context.SAPPOSTING.Where(a => a.CANCEL.Equals("X") && string.IsNullOrEmpty(a.SAP_DOC_NO_CANCEL));
            if (posting.Count() > 0)
            {
                return true;
            }

            posting = _context.SAPPOSTING.Where(a => a.CANCEL.Equals("E") && string.IsNullOrEmpty(a.SAP_DOC_NO_CANCEL));
            if (posting.Count() > 0)
            {
                return true;
            }

            return false;
        }

        //public bool ToCSV()
        //{

        //}
    }
}
