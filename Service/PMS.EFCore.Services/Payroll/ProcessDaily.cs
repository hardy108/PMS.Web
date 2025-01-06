using System;
using System.Collections;
using System.Collections.Generic;

using PMS.Shared.Utilities;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using PMS.EFCore.Model.Filter;
using PMS.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PMS.EFCore.Services.General;
using PMS.EFCore.Services.Location;
using Microsoft.AspNetCore.Http;
using PMS.EFCore.Helper;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;
using Newtonsoft.Json;
using PMS.EFCore.Services.Utilities;
using PMS.EFCore.Services.Payroll;
using PMS.EFCore.Services.Attendances;
using PMS.EFCore.Services.Organization;
using PMS.EFCore.Services.Logistic;
using PMS.Shared.Utilities;
using AM.EFCore.Services;

namespace PMS.EFCore.Services.Payroll
{
    public class ProcessDaily : EntityFactory<TPAYMENT,TPAYMENT,GeneralFilter,PMSContextBase>
    {
        private Divisi _serviceDivisi;
        private PremiPanen _servicePremiPanen;
        private Employee _serviceEmployee;
        private TransaksiAbsensi _serviceAbsensi;
        private Block _serviceBlock;
        private Period _servicePeriod;
        private Activity _serviceActivity;
        private Attendance _serviceAttendance;
        private Harvesting _serviceHarvesting;
        private Loading _serviceLoading;

        private AuthenticationServiceBase _authenticationService;
        public ProcessDaily(PMSContextBase context, AuthenticationServiceBase authenticationService, AuditContext auditContext) : base(context, auditContext)
        {
            _serviceName = "ProcessDaily";
            _authenticationService = authenticationService;
            _serviceDivisi = new Divisi(_context,_authenticationService, auditContext);
            _servicePremiPanen = new PremiPanen(_context, _authenticationService, auditContext);
            _serviceEmployee = new Employee(_context, _authenticationService,auditContext);
            _serviceAbsensi = new TransaksiAbsensi(_context, _authenticationService, auditContext);
            _serviceBlock = new Block(_context, _authenticationService, auditContext);
            _serviceActivity = new Activity(_context, _authenticationService,auditContext);
            _serviceAttendance = new Attendance(_context, _authenticationService, auditContext);
            _servicePeriod = new Period(_context, _authenticationService, auditContext);
            _serviceHarvesting = new Harvesting(_context, _authenticationService,auditContext);
            _serviceLoading = new Loading(_context, _authenticationService, auditContext);
        }

        public bool Process(string divId, DateTime date, string userName)
        {

            var result = string.Empty;

            var div = _context.MDIVISI.Where(d=> d.DIVID.Equals(divId)).FirstOrDefault();
            if (div == null) throw new Exception("Divisi Tidak Ada");
            var scheme = _context.MPAYMENTSCHEME.Where(d => d.UNITCODE.Equals(div.UNITCODE)).FirstOrDefault();

            _servicePeriod.CheckValidPeriod(div.UNITCODE, date);

            TPAYMENT payment = new TPAYMENT();
            payment.InitEditDaily();


            //List<sp_HarvestingResult_GetDaily_Result> harvestResultDaily = _context.sp_HarvestingResult_GetDaily_Result(divId, date.Date).ToList();
            //VTHARVESTRESULT1 harvestResult;
            //foreach (var item in harvestResultDaily)
            //{
            //    harvestResult = new VTHARVESTRESULT1();
            //    harvestResult.CopyFrom(item);
            //    payment.VTHARVESTRESULT1.Add(harvestResult);
            //}

            //List<sp_HarvestingResult_GetGerdanDaily_Result> gerdanResultDaily = _context.sp_HarvestingResult_GetGerdanDaily(divId, date.Date).ToList();
            //VTHARVESTRESULT1 gerdanResult;
            //foreach (var item in gerdanResultDaily)
            //{
            //    gerdanResult = new VTHARVESTRESULT1();
            //    gerdanResult.CopyFrom(item);
            //    payment.VTHARVESTRESULT1GERDAN.Add(gerdanResult);
            //}

            //List<sp_LoadingResult_GetDaily_Result> loadingResultDaily = _context.sp_LoadingResult_GetDaily_Result(divId, date.Date).ToList();
            //TLOADINGRESULT loadingResult1;
            //foreach (var item in loadingResultDaily)
            //{
            //    loadingResult1 = new TLOADINGRESULT();
            //    loadingResult1.CopyFrom(item);
            //    payment.TLOADINGRESULT.Add(loadingResult1);
            //}

            //List<sp_OperatingResult_GetDaily_Result> operatingResultDaily = _context.sp_OperatingResult_GetDaily_Result(divId, date.Date).ToList();
            //TOPERATINGRESULT operatingResult1;
            //foreach (var item in operatingResultDaily)
            //{
            //    operatingResult1 = new TOPERATINGRESULT();
            //    operatingResult1.CopyFrom(item);
            //    payment.TOPERATINGRESULT.Add(operatingResult1);
            //}

            List<VTHARVESTRESULT1> harvestResultDaily = _context.sp_VTHARVESTRESULT1_GetDaily(divId, date.Date).ToList();
            payment.VTHARVESTRESULT1.AddRange(harvestResultDaily);

            List<VTHARVESTRESULT1> gerdanResultDaily = _context.sp_VTHARVESTRESULT1GetGerdanDaily(divId, date.Date).ToList();
            payment.VTHARVESTRESULT1GERDAN.AddRange(gerdanResultDaily);

            List<TLOADINGRESULT> loadingResultDaily = _context.sp_TLOADINGRESULT_GetDaily_Result(divId, date.Date).ToList();
            payment.TLOADINGRESULT.AddRange(loadingResultDaily);

            List<TOPERATINGRESULT> operatingResultDaily = _context.sp_TOPERATINGRESULT_GetDaily_Result(divId, date.Date).ToList();
            payment.TOPERATINGRESULT.AddRange(operatingResultDaily);

            //Union Harvest & Gerdan
            var unionHvt = new List<VTHARVESTRESULT1>();
            unionHvt.AddRange(payment.VTHARVESTRESULT1);
            unionHvt.AddRange(payment.VTHARVESTRESULT1GERDAN);


            /*--Harvesting */
            var hvtCheck = (from i in unionHvt
                            group i by new { i.HARVESTCODE }
                           into g
                            select new { g.Key.HARVESTCODE }).ToList();
            foreach (var hvt in hvtCheck)
            {
                var curHvt = _context.THARVEST.Where(d => d.HARVESTCODE.Equals(hvt.HARVESTCODE)).FirstOrDefault();
                if (curHvt != null && curHvt.STATUS == "P") result += "Buku Panen No " + curHvt.HARVESTCODE + " belum di approve." + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(result)) throw new Exception(result);


            var hvtNoBasis = (from i in unionHvt
                              where string.IsNullOrEmpty(i.BASISGROUP)
                              group i by new { i.BLOCKID, i.ACTIVITYID }
               into g
                              select new { g.Key.BLOCKID, g.Key.ACTIVITYID }).ToList();
            foreach (var hvt in hvtNoBasis)
            {
                result += "Blok " + hvt.BLOCKID + " activity " + hvt.ACTIVITYID + " belum mempunyai basis panen." + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(result)) throw new Exception(result);



            /*--Loading--*/
            var Load = new List<TLOADINGRESULT>();
            Load.AddRange(payment.TLOADINGRESULT);

            var loadCheck = (from i in Load
                             group i by new { i.LOADINGCODE }
                             into g
                             select new { g.Key.LOADINGCODE }).ToList();

            foreach (var load in loadCheck)
            {
                var curLoad = _context.TLOADING.Where(d => d.LOADINGCODE.Equals(load.LOADINGCODE)).FirstOrDefault();
                if (curLoad != null && curLoad.STATUS == "P") result += "Buku Kegiatan Muat No " + curLoad.LOADINGCODE + " belum di approve." + Environment.NewLine;
            }

            var loadNoBasis = (from i in Load
                               where string.IsNullOrEmpty(i.BASISGROUP)
                               group i by new { i.ACTIVITYID, i.VEHICLETYPEID}
                   into g
                               select new { g.Key.ACTIVITYID, g.Key.VEHICLETYPEID }).ToList();
            foreach (var load in loadNoBasis)
            {
                result += "Basis Muat Activity " + load.ACTIVITYID + " dan Vehicle Type " + load.VEHICLETYPEID + " belum mempunyai basis muat." + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(result)) throw new Exception(result);



            /*--Operator--*/
            var operatingRslt = new List<TOPERATINGRESULT>();
            operatingRslt.AddRange(payment.TOPERATINGRESULT);

            var operatorCheck = (from i in operatingRslt
                                 group i by new { i.LOADINGCODE }
                             into g
                                 select new { g.Key.LOADINGCODE }).ToList();

            foreach (var operating in operatorCheck)
            {
                var curLoad = _context.TLOADING.Where(d => d.LOADINGCODE.Equals(operating.LOADINGCODE)).FirstOrDefault();
                if (curLoad != null && curLoad.STATUS == "P") result += "Buku Kegiatan Muat No " + curLoad.LOADINGCODE + " belum di approve." + Environment.NewLine;
            }

            var operatingNoBasis = (from i in operatingRslt
                                    where string.IsNullOrEmpty(i.BASISGROUP)
                                    group i by new { i.ACTIVITYID, i.VEHICLETYPEID }
                                    into g
                                    select new { g.Key.ACTIVITYID, g.Key.VEHICLETYPEID }).ToList();

            foreach (var operating in operatingNoBasis)
            {
                result += "Basis Operator activity " + operating.ACTIVITYID + " dan Vehicle Type " + operating.VEHICLETYPEID + " belum mempunyai basis operator." + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(result)) throw new Exception(result);


            //Process Calculate Basis
            var harvestEmployees = new List<THARVESTEMPLOYEE>();
            var harvestEmployeesGerdan = new List<THARVESTEMPLOYEE>();

            var harvestBlocks = new List<THARVESTBLOCK>();
            var harvestBlocksGerdan = new List<THARVESTBLOCK>();

            var loadingEmployees = new List<TLOADINGEMPLOYEE>();
            var loadingDrivers = new List<TLOADINGDRIVER>();
            var loadingBlocks = new List<TLOADINGBLOCK>();

            HarvestingDailyProcess(payment.VTHARVESTRESULT1.ToList(), harvestEmployees, harvestBlocks, scheme);
            HarvestingGerdanDailyProcess(payment.VTHARVESTRESULT1GERDAN.ToList(), harvestEmployeesGerdan, harvestBlocksGerdan, scheme);

            LoadingDailyProcess(payment.TLOADINGRESULT.ToList(), loadingEmployees, loadingBlocks, scheme);
            OperatingDailyProcess(payment.TOPERATINGRESULT.ToList(), loadingDrivers, loadingBlocks, scheme);


            //Process Save All Items
            // 1. Delete ; Update status = D           

            //_context.THARVESTRESULT1.Where(d => d.DIVID.Equals(divId) && d.HARVESTDATE.Date == date.Date).ToList()
            //    .ForEach(e => { e.STATUS = "D"; e.UPDATED = GetServerTime(); });
            List<String> existingHarvestCode = new List<string>();
            existingHarvestCode = payment.VTHARVESTRESULT1.Select(d => d.HARVESTCODE).Distinct().ToList();
            List<THARVESTRESULT1> harvestingResults =
            (
            from p in _context.THARVESTRESULT1
            .Where(d => d.DIVID.Equals(divId) && d.HARVESTDATE == date && !(existingHarvestCode.Contains(d.HARVESTCODE)))
            select p
            ).ToList();
            foreach (THARVESTRESULT1 p in harvestingResults)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();
                _context.Entry<THARVESTRESULT1>(p).State = EntityState.Modified;
            }

            //_context.TGERDANRESULT.Where(d => d.DIVID.Equals(divId) && d.HARVESTDATE.Date == date.Date).ToList()
            //.ForEach(e => { e.STATUS = "D"; e.UPDATED = GetServerTime(); });
            List<String> existingHarvestCodeGerdan = new List<string>();
            existingHarvestCodeGerdan = payment.VTHARVESTRESULT1GERDAN.Select(d => d.HARVESTCODE).Distinct().ToList();
            List<TGERDANRESULT> gerdanResults =
            (
            from p in _context.TGERDANRESULT
            .Where(d => d.DIVID.Equals(divId) && d.HARVESTDATE == date && !(existingHarvestCodeGerdan.Contains(d.HARVESTCODE)))
            select p
            ).ToList();
            foreach (TGERDANRESULT p in gerdanResults)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();
                _context.Entry<TGERDANRESULT>(p).State = EntityState.Modified;
            }

            //_context.TLOADINGRESULT.Where(d => d.DIVID.Equals(divId)).ToList()
            //.ForEach(e => { e.STATUS = "D"; e.UPDATED = GetServerTime(); });
            List<String> existingLoadingCode = new List<string>();
            existingLoadingCode = payment.TLOADINGRESULT.Select(d => d.LOADINGCODE).Distinct().ToList();
            List<TLOADINGRESULT> loadingResults =
            (
            from p in _context.TLOADINGRESULT
            .Where(d => d.DIVID.Equals(divId) && d.LOADINGDATE == date && !(existingLoadingCode.Contains(d.LOADINGCODE)))
            select p
            ).ToList();
            foreach (TLOADINGRESULT p in loadingResults)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();
                _context.Entry<TLOADINGRESULT>(p).State = EntityState.Modified;
            }

            //_context.TOPERATINGRESULT.Where(d => d.DIVID.Equals(divId) && d.LOADINGDATE.Date == date.Date).ToList()
            //.ForEach(e => { e.STATUS = "D"; e.UPDATED = GetServerTime(); });
            List<String> existingOperatingCode = new List<string>();
            existingOperatingCode = payment.TOPERATINGRESULT.Select(d => d.LOADINGCODE).Distinct().ToList();
            List<TOPERATINGRESULT> operatingResults =
            (
            from p in _context.TOPERATINGRESULT
            .Where(d => d.DIVID.Equals(divId) && d.LOADINGDATE == date && !(existingOperatingCode.Contains(d.LOADINGCODE)))
            select p
            ).ToList();
            foreach (TOPERATINGRESULT p in operatingResults)
            {
                p.STATUS = "D";
                p.UPDATED = GetServerTime();
                _context.Entry<TOPERATINGRESULT>(p).State = EntityState.Modified;
            }

            _context.SaveChanges();

            // 2. Delete Data 
            _context.THARVESTRESULT1.RemoveRange(_context.THARVESTRESULT1.Where(d => existingHarvestCode.Contains(d.HARVESTCODE)));

            _context.TGERDANRESULT.RemoveRange(_context.TGERDANRESULT.Where(d => existingHarvestCodeGerdan.Contains(d.HARVESTCODE)));

            _context.TLOADINGRESULT.RemoveRange(_context.TLOADINGRESULT.Where(d => existingLoadingCode.Contains(d.LOADINGCODE)));

            _context.TOPERATINGRESULT.RemoveRange(_context.TOPERATINGRESULT.Where(d => existingOperatingCode.Contains(d.LOADINGCODE)));

            _context.SaveChanges();

            // 3. Insert Data         

            //Proses Save THARVESTRESULT1
            THARVESTRESULT1 _harvestResult1;
            foreach (var item in payment.VTHARVESTRESULT1)
            {
                _harvestResult1 = new THARVESTRESULT1();
                item.UPDATED = GetServerTime();
                _harvestResult1.CopyFrom(item);               
                payment.THARVESTRESULT1.Add(_harvestResult1);
            }
            _context.THARVESTRESULT1.AddRange(payment.THARVESTRESULT1);

            //Proses Save TGERDANRESULT
            TGERDANRESULT _gerdanResult;
            foreach (var item in payment.VTHARVESTRESULT1GERDAN)
            {
                _gerdanResult = new TGERDANRESULT();
                item.UPDATED = GetServerTime();
                _gerdanResult.CopyFrom(item);
                payment.TGERDANRESULT.Add(_gerdanResult);
            }
            _context.TGERDANRESULT.AddRange(payment.TGERDANRESULT);

            //Proses Save TLOADINGRESULT
            payment.TLOADINGRESULT.ToList().ForEach(e => { e.UPDATED = GetServerTime(); });
            _context.TLOADINGRESULT.AddRange(payment.TLOADINGRESULT);

            //Proses Save TOPERATINGRESULT
            payment.TOPERATINGRESULT.ToList().ForEach(e => { e.UPDATED = GetServerTime(); });
            _context.TOPERATINGRESULT.AddRange(payment.TOPERATINGRESULT);

            _context.SaveChanges();

            //----------------------------------------------------------------

            //Process Save HarvestEmployees
            THARVESTEMPLOYEE _HarvestEmployees;
            foreach (var item in harvestEmployees)
            {
                _HarvestEmployees = new THARVESTEMPLOYEE();
                _HarvestEmployees = _context.THARVESTEMPLOYEE.Where(d => d.HARVESTCODE.Equals(item.HARVESTCODE)
                                       && d.EMPLOYEEID.Equals(item.EMPLOYEEID)).FirstOrDefault();
                if (_HarvestEmployees != null)
                {
                    _HarvestEmployees.VALUE = item.VALUE;
                    _HarvestEmployees.VALUECALC = item.VALUECALC;
                    _context.Entry(_HarvestEmployees).State = EntityState.Modified;
                }
            }

            //Process Save HarvestEmployeesGerdan
            THARVESTEMPLOYEE _HarvestEmployeesGerdan;
            foreach (var item in harvestEmployeesGerdan)
            {
                _HarvestEmployeesGerdan = new THARVESTEMPLOYEE();
                _HarvestEmployeesGerdan = _context.THARVESTEMPLOYEE.Where(d => d.HARVESTCODE.Equals(item.HARVESTCODE)
                                       && d.EMPLOYEEID.Equals(item.EMPLOYEEID)).FirstOrDefault();
                if (_HarvestEmployeesGerdan != null)
                {
                    _HarvestEmployeesGerdan.VALUE = item.VALUE;
                    _HarvestEmployeesGerdan.VALUECALC = item.VALUECALC;
                    _context.Entry(_HarvestEmployeesGerdan).State = EntityState.Modified;
                }
            }

            //Process Save HarvestBlock
            THARVESTBLOCK _HarvestBlock;
            foreach (var item in harvestEmployeesGerdan)
            {
                _HarvestBlock = new THARVESTBLOCK();
                _HarvestBlock = _context.THARVESTBLOCK.Where(d => d.HARVESTCODE.Equals(item.HARVESTCODE)
                                       && d.EMPLOYEEID.Equals(item.EMPLOYEEID)).FirstOrDefault();
                if (_HarvestBlock != null)
                {
                    _HarvestBlock.VALUE = item.VALUE;
                    _context.Entry(_HarvestBlock).State = EntityState.Modified;
                }
            }

            //Process Save HarvestBlockGerdan
            THARVESTBLOCK _HarvestBlockGerdan;
            foreach (var item in harvestEmployeesGerdan)
            {
                _HarvestBlockGerdan = new THARVESTBLOCK();
                _HarvestBlockGerdan = _context.THARVESTBLOCK.Where(d => d.HARVESTCODE.Equals(item.HARVESTCODE)
                                       && d.EMPLOYEEID.Equals(item.EMPLOYEEID)).FirstOrDefault();
                if (_HarvestBlockGerdan != null)
                {
                    _HarvestBlockGerdan.VALUE = item.VALUE;
                    _context.Entry(_HarvestBlockGerdan).State = EntityState.Modified;
                }
            }

            _context.SaveChanges();

            //Union Employees Harvest & Employees Gerdan
            var unionHvtEmp = new List<THARVESTEMPLOYEE>();
            unionHvtEmp.AddRange(harvestEmployees);
            unionHvtEmp.AddRange(harvestEmployeesGerdan);

            //Delete Pemanen & Gerdan Attendance by Reference
            var harvestmuatcode = ((from r in unionHvtEmp
                                    group r by new { r.HARVESTCODE, r.EMPLOYEEID }
                               into grp
                                    select new { grp.Key.HARVESTCODE, grp.Key.EMPLOYEEID })
                               ).ToList();

            foreach (var lhvtcode in harvestmuatcode)
            {
                _context.TATTENDANCE.RemoveRange(_context.TATTENDANCE.Where(d => d.REF.Equals(lhvtcode.HARVESTCODE) && d.EMPLOYEEID.Equals(lhvtcode.EMPLOYEEID)));
            }
            _context.SaveChanges();

            //Save Attendance Pemanen & Gerdan
            foreach (var employee in unionHvtEmp)
            {
                if (employee.VALUE > 0)
                {
                    var empdetail = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(employee.EMPLOYEEID)).FirstOrDefault();
                    var attendance = new TATTENDANCE
                    {
                        DIV = _context.MDIVISI.Where(d => d.DIVID.Equals(empdetail.DIVID)).FirstOrDefault(),
                        DIVID = empdetail.DIVID,
                        EMPLOYEE = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(employee.EMPLOYEEID)).FirstOrDefault(),
                        EMPLOYEEID = employee.EMPLOYEEID,
                        DATE = date,
                        REMARK = string.Empty,
                        HK = employee.VALUE,
                        PRESENT = true,
                        //AbsentId = "K",
                        //-*Constant
                        STATUS = "A",
                        REF = employee.HARVESTCODE,
                        AUTO = true,
                        CREATEBY = userName,
                        CREATEDDATE = date,
                        UPDATEBY = userName,
                        UPDATEDDATE = date,
                    };
                    _serviceAttendance.SaveInsert(attendance, userName);
                }
            }


            //Delete Pemuat Attendance by Reference
            var loadingmuatcode = ((from r in loadingEmployees
                                    group r by new { r.LOADINGCODE, r.EMPLOYEEID }
                               into grp
                                    select new { grp.Key.LOADINGCODE, grp.Key.EMPLOYEEID })
                               ).ToList();

            foreach (var lmuatcode in loadingmuatcode)
            {
                _context.TATTENDANCE.RemoveRange(_context.TATTENDANCE.Where(d => d.REF.Equals(lmuatcode.LOADINGCODE) && d.EMPLOYEEID.Equals(lmuatcode.EMPLOYEEID)));
            }
            _context.SaveChanges();

            //Save Attendance Pemuat
            foreach (var employee in loadingEmployees)
            {
                if (employee.VALUE > 0)
                {
                    var empdetail = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(employee.EMPLOYEEID)).FirstOrDefault();
                    var attendance = new TATTENDANCE
                    {
                        DIV = _context.MDIVISI.Where(d => d.DIVID.Equals(empdetail.DIVID)).FirstOrDefault(),
                        DIVID = empdetail.DIVID,
                        EMPLOYEE = _context.MEMPLOYEE.Where(d=> d.EMPID.Equals(employee.EMPLOYEEID)).FirstOrDefault(),
                        EMPLOYEEID = employee.EMPLOYEEID,
                        DATE = date,
                        REMARK = string.Empty,
                        HK = employee.VALUE,
                        PRESENT = true,
                        //AbsentId = "K",
                        //-*Constant
                        STATUS = "A",
                        REF = employee.LOADINGCODE,
                        AUTO = true,
                        CREATEBY = userName,
                        CREATEDDATE = date,
                        UPDATEBY = userName,
                        UPDATEDDATE = date,
                    };
                    _serviceAttendance.SaveInsert(attendance, userName);
                }
            }

            //Delete Driver Attendance by Reference
            var loadingoperatorcode = ((from r in loadingDrivers
                                    group r by new { r.LOADINGCODE, r.DRIVERID }
                               into grp
                                    select new { grp.Key.LOADINGCODE, grp.Key.DRIVERID })
                               ).ToList();

            foreach (var loperatorcode in loadingoperatorcode)
            {
                _context.TATTENDANCE.RemoveRange(_context.TATTENDANCE.Where(d => d.REF.Equals(loperatorcode.LOADINGCODE) && d.EMPLOYEEID.Equals(loperatorcode.DRIVERID)));
            }
            _context.SaveChanges();

            //Save Attendance Driver
            foreach (var employee in loadingDrivers)
            {
                if (employee.VALUE > 0)
                {
                    var empdetail = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(employee.DRIVERID)).FirstOrDefault();
                    var attendance = new TATTENDANCE
                    {
                        DIV = _context.MDIVISI.Where(d => d.DIVID.Equals(empdetail.DIVID)).FirstOrDefault(),
                        DIVID = empdetail.DIVID,
                        EMPLOYEE = _context.MEMPLOYEE.Where(d => d.EMPID.Equals(employee.DRIVERID)).FirstOrDefault(),
                        EMPLOYEEID = employee.DRIVERID,
                        DATE = date,
                        REMARK = string.Empty,
                        HK =  Convert.ToDecimal(employee.VALUE),
                        PRESENT = true,
                        //AbsentId = "K",
                        //-*Constant
                        STATUS = "A",
                        REF = employee.LOADINGCODE,
                        AUTO = true,
                        CREATEBY = userName,
                        CREATEDDATE = date,
                        UPDATEBY = userName,
                        UPDATEDDATE = date,
                    };
                    _serviceAttendance.SaveInsert(attendance, userName);
                }
            }


            //Update Header BKP Status UPLOAD = 0
            var loadingcode = ((from r in loadingEmployees
                                group r by new { r.LOADINGCODE }
                               into grp
                                select new { grp.Key.LOADINGCODE })
                               .Union
                               (from p in loadingDrivers
                                group p by new { p.LOADINGCODE }
                               into grp
                                select new { grp.Key.LOADINGCODE })
                               ).ToList();

            TLOADING loadingRecord;
            foreach (var item in loadingcode)
            {
                loadingRecord = new TLOADING();
                loadingRecord = _context.TLOADING.Where(d => d.LOADINGCODE.Equals(item.LOADINGCODE)).FirstOrDefault();
                loadingRecord.UPLOAD = 0;
                loadingRecord.UPLOADED = GetServerTime();
                _context.Entry(loadingRecord).State = EntityState.Modified;
            }
            _context.SaveChanges();

            return true;

        }

        private void HarvestingDailyProcess(List<VTHARVESTRESULT1> harvestingResults, List<THARVESTEMPLOYEE> harvestEmployees, List<THARVESTBLOCK> harvestBlocks,
    MPAYMENTSCHEME scheme)
        {

            var stringbase1TolerancePercent = HelperService.GetConfigValue(PMSConstants.CfgHarvestingResultBase1TolerancePercent + scheme.UNITCODE,_context);
            decimal base1TolerancePercent = 0;
            if (!string.IsNullOrEmpty(stringbase1TolerancePercent))
                base1TolerancePercent = Convert.ToDecimal(stringbase1TolerancePercent);

            if (scheme.PREMIBASEDCALCULATION == "Proporsional")
            {
                foreach (var r in harvestingResults)
                {
                    if (r.HARVESTDATE.DayOfWeek == DayOfWeek.Friday && r.FRIDAY1 != 0)
                    {
                        r.ORIBASE1 = r.ORIBASE1 * r.FRIDAY1 / 100;
                        r.BASE1 = r.BASE1 * r.FRIDAY1 / 100;
                        r.BASE2 = r.BASE2 * r.FRIDAY1 / 100;
                        r.BASE3 = r.BASE3 * r.FRIDAY1 / 100;
                    }

                    r.PCTORIBASIS1 = r.ORIBASE1 == 0 ? 0 : r.HASILPANEN / r.ORIBASE1 * 100;
                    r.PCTBASIS1 = r.BASE1 == 0 ? 0 : r.HASILPANEN / r.BASE1 * 100;
                    r.PCTBASIS2 = r.BASE2 == 0 ? 0 : r.HASILPANEN / r.BASE2 * 100;
                    r.PCTBASIS3 = r.BASE3 == 0 ? 0 : r.HASILPANEN / r.BASE3 * 100;

                    r.HAPCT = r.HABASE == 0 ? 0 : r.HAHASIL / r.HABASE * 100;
                }
                
                foreach (var r in harvestingResults)
                {
                    var totPctOriBas1 =
                        harvestingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTORIBASIS1);
                    var totPctBas1 =
                        harvestingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS1);
                    var totPctBas2 =
                        harvestingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS2);
                    var totPctBas3 =
                        harvestingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS3);
                    var totPctHa =
                        harvestingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.HAPCT);

                    r.TOTALPCTORIBASIS1 = totPctOriBas1.Sum();
                    r.TOTALPCTBASIS1 = totPctBas1.Sum();
                    r.TOTALPCTBASIS2 = totPctBas2.Sum();
                    r.TOTALPCTBASIS3 = totPctBas3.Sum();
                    r.HATOTPCT = totPctHa.Sum();
                }

                foreach (var r in harvestingResults)
                {
                    r.NEWORIBASISPCT1 = r.TOTALPCTORIBASIS1 > 0 ? (r.PCTORIBASIS1 / r.TOTALPCTORIBASIS1) * 100 : 0;
                    r.NEWBASISPCT1 = r.TOTALPCTBASIS1 > 0 ? (r.PCTBASIS1 / r.TOTALPCTBASIS1) * 100 : 0;
                    r.NEWBASISPCT2 = r.TOTALPCTBASIS2 > 0 ? (r.PCTBASIS2 / r.TOTALPCTBASIS2) * 100 : 0;
                    r.NEWBASISPCT3 = r.TOTALPCTBASIS3 > 0 ? (r.PCTBASIS3 / r.TOTALPCTBASIS3) * 100 : 0;
                    r.NEWHAPCT = r.HATOTPCT > 0 ? (r.HAPCT / r.HATOTPCT) * 100 : 0;

                    r.NEWORIBASIS1 = Math.Round(r.NEWORIBASISPCT1 * r.ORIBASE1 / 100, 1);
                    r.NEWBASIS1 = Math.Round(r.NEWBASISPCT1 * r.BASE1 / 100, 1);
                    r.NEWBASIS2 = Math.Round(r.NEWBASISPCT2 * r.BASE2 / 100, 1);
                    r.NEWBASIS3 = Math.Round(r.NEWBASISPCT3 * r.BASE3 / 100, 1);
                    r.NEWHABASE = r.NEWHAPCT * r.HABASE / 100;

                    if (base1TolerancePercent > 0)
                    {
                        decimal base1Tolerance = r.NEWBASIS1 - Math.Round(r.NEWBASIS1 * base1TolerancePercent / 100, 0);
                        if (r.HASILPANEN < r.NEWBASIS1 && r.HASILPANEN >= base1Tolerance)
                            r.NEWBASIS1 = r.HASILPANEN;
                    }

                    r.NEWPREMISIAP1 = r.NEWBASISPCT1 * r.PREMI1 / 100;
                    r.NEWPREMISIAP2 = r.NEWBASISPCT2 * r.PREMI2 / 100;
                    r.NEWPREMISIAP3 = r.NEWBASISPCT3 * r.PREMI3 / 100;
                    r.NEWHAPREMI = r.NEWHAPCT * r.HAPREMI / 100;
                    r.NEWATTPREMI = r.NEWBASISPCT1 * r.ATTPREMI / 100;

                    if (r.HASILPANEN <= r.NEWBASIS1)
                        r.NEWHASIL1 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS2)
                        if (r.NEWBASIS2 == 0)
                            r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;
                        else
                            r.NEWHASIL1 = r.NEWBASIS2 - r.NEWBASIS1;
                    else
                        r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;

                    if (r.NEWBASIS2 == 0 || r.HASILPANEN <= r.NEWBASIS2)
                        r.NEWHASIL2 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS3 == 0)
                            r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;
                        else
                            r.NEWHASIL2 = r.NEWBASIS3 - r.NEWBASIS2;
                    else
                        r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;

                    if (r.NEWBASIS3 == 0 || r.HASILPANEN <= r.NEWBASIS3)
                        r.NEWHASIL3 = 0;
                    else
                        r.NEWHASIL3 = r.HASILPANEN - r.NEWBASIS3;

                    r.NEWPREMILEBIH1 = r.NEWHASIL1 * r.EXCEED1;
                    r.NEWPREMILEBIH2 = r.NEWHASIL2 * r.EXCEED2;
                    r.NEWPREMILEBIH3 = r.NEWHASIL3 * r.EXCEED3;

                    r.NEWINCENTIVE1 = (r.HASILPANEN >= r.NEWBASIS1 ? r.NEWPREMISIAP1 : 0) + r.NEWPREMILEBIH1;
                    r.NEWINCENTIVE2 = (r.HASILPANEN >= r.NEWBASIS2 ? r.NEWPREMISIAP2 : 0) + r.NEWPREMILEBIH2;
                    r.NEWINCENTIVE3 = (r.HASILPANEN >= r.NEWBASIS3 ? r.NEWPREMISIAP3 : 0) + r.NEWPREMILEBIH3;

                    if (r.HASILPANEN >= r.NEWBASIS1)
                        r.INCENTIVEPKKTGI = (r.NEWBASISPCT1 / 100) * r.PREMIPKKTGI;

                    if (r.HASILPANEN < r.NEWBASIS1)
                        if (r.HAHASIL >= r.NEWHABASE) r.HAINCENTIVE = r.NEWHAPREMI;
                    if (r.HASILPANEN >= r.NEWBASIS1) r.ATTINCENTIVE = r.NEWATTPREMI;
                }
            }

            //Hitung HK

            // HarvestingPaymentType.Harian
            var empList = (from i in harvestingResults
                           where i.HARVESTPAYMENTTYPE == 0
                            && i.NEWBASIS1 > 0
                           group i by new { i.HARVESTDATE, i.EMPLOYEEID }
                           into g
                           select new
                           {
                               g.Key.HARVESTDATE,
                               g.Key.EMPLOYEEID,
                               Ratio = g.Average(r => Decimal.Round(r.HASILPANEN / r.NEWBASIS1, 2)),
                               TotalKg = g.Sum(r => r.HASILPANEN)
                           }).ToList();

            foreach (var emp in empList)
            {
                //Cek jam kerja & luasan

                var totalHk = Decimal.Round(emp.Ratio, 2);
                if (totalHk > 1) totalHk = 1;

                var empResult = from i in harvestingResults
                                where i.HARVESTDATE == emp.HARVESTDATE && i.EMPLOYEEID == emp.EMPLOYEEID
                                && i.HARVESTPAYMENTTYPE == 0 && i.NEWBASIS1 > 0
                                select i;

                decimal hkUsed = 0;
                int j = 1;
                foreach (var res in empResult)
                {
                    decimal hk;
                    if (j != empResult.Count())
                    {
                        hk = Decimal.Round(totalHk * (res.HASILPANEN / emp.TotalKg), 2);
                        hkUsed += hk;
                    }
                    else
                        hk = totalHk - hkUsed;

                    res.HK = hk;
                    j++;
                }
            }

            //Simpan HK
            var empHK = (from i in harvestingResults
                         where i.EFLAG == true
                         group i by new { i.HARVESTCODE, i.EMPLOYEEID }
                        into g
                         select new { g.Key.HARVESTCODE, g.Key.EMPLOYEEID, HK = g.Sum(r => r.HK) }).ToList();

            var blockHk = (from i in harvestingResults
                           where i.EFLAG == true
                           group i by new { i.HARVESTCODE, i.EMPLOYEEID, i.BLOCKID }
                        into g
                           select new { g.Key.HARVESTCODE, g.Key.EMPLOYEEID, g.Key.BLOCKID, HK = g.Sum(r => r.HK) }).ToList();


            harvestEmployees.Clear();
            foreach (var hvtEmp in empHK)
            {
                var newHvt = new THARVESTEMPLOYEE
                {
                    HARVESTCODE = hvtEmp.HARVESTCODE,
                    EMPLOYEEID = hvtEmp.EMPLOYEEID,
                    VALUE = hvtEmp.HK,
                    VALUECALC = hvtEmp.HK,
                };
                harvestEmployees.Add(newHvt);
            }

            harvestBlocks.Clear();
            foreach (var hvtBlk in blockHk)
            {
                var newHvt = new THARVESTBLOCK
                {
                    HARVESTCODE = hvtBlk.HARVESTCODE,
                    EMPLOYEEID = hvtBlk.EMPLOYEEID,
                    BLOCKID = hvtBlk.BLOCKID,
                    VALUE = hvtBlk.HK,
                };
                harvestBlocks.Add(newHvt);
            }
        }

        private void HarvestingGerdanDailyProcess(List<VTHARVESTRESULT1> harvestingResults, List<THARVESTEMPLOYEE> harvestEmployees, List<THARVESTBLOCK> harvestBlocks,
            MPAYMENTSCHEME scheme)
        {

            var stringbase1TolerancePercent = HelperService.GetConfigValue(PMSConstants.CfgHarvestingResultBase1TolerancePercent + scheme.UNITCODE, _context);
            decimal base1TolerancePercent = 0;
            if (!string.IsNullOrEmpty(stringbase1TolerancePercent))
                base1TolerancePercent = Convert.ToDecimal(stringbase1TolerancePercent);

            if (scheme.PREMIBASEDCALCULATION == "Proporsional")
            {
                foreach (var r in harvestingResults)
                {
                    if (r.HARVESTDATE.DayOfWeek == DayOfWeek.Friday && r.FRIDAY1 != 0)
                    {
                        r.ORIBASE1 = r.ORIBASE1 * r.FRIDAY1 / 100;
                        r.BASE1 = r.BASE1 * r.FRIDAY1 / 100;
                        r.BASE2 = r.BASE2 * r.FRIDAY1 / 100;
                        r.BASE3 = r.BASE3 * r.FRIDAY1 / 100;
                    }

                    r.PCTORIBASIS1 = r.ORIBASE1 == 0 ? 0 : r.HASILPANEN / r.ORIBASE1 * 100;
                    r.PCTBASIS1 = r.BASE1 == 0 ? 0 : r.HASILPANEN / r.BASE1 * 100;
                    r.PCTBASIS2 = r.BASE2 == 0 ? 0 : r.HASILPANEN / r.BASE2 * 100;
                    r.PCTBASIS3 = r.BASE3 == 0 ? 0 : r.HASILPANEN / r.BASE3 * 100;
                }

                foreach (var r in harvestingResults)
                {
                    var totPctOriBas1 =
                        harvestingResults.Where(
                            itm =>
                            itm.GEMPID == r.GEMPID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTORIBASIS1);
                    var totPctBas1 =
                        harvestingResults.Where(
                            itm =>
                            itm.GEMPID == r.GEMPID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS1);
                    var totPctBas2 =
                        harvestingResults.Where(
                            itm =>
                            itm.GEMPID == r.GEMPID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS2);
                    var totPctBas3 =
                        harvestingResults.Where(
                            itm =>
                            itm.GEMPID == r.GEMPID && itm.HARVESTDATE == r.HARVESTDATE &&
                            itm.HARVESTTYPE == r.HARVESTTYPE && itm.HARVESTPAYMENTTYPE == r.HARVESTPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS3);

                    r.TOTALPCTORIBASIS1 = totPctOriBas1.Sum();
                    r.TOTALPCTBASIS1 = totPctBas1.Sum();
                    r.TOTALPCTBASIS2 = totPctBas2.Sum();
                    r.TOTALPCTBASIS3 = totPctBas3.Sum();
                }

                foreach (var r in harvestingResults)
                {
                    r.NEWORIBASISPCT1 = r.TOTALPCTORIBASIS1 > 0 ? (r.PCTORIBASIS1 / r.TOTALPCTORIBASIS1) * 100 : 0;
                    r.NEWBASISPCT1 = r.TOTALPCTBASIS1 > 0 ? (r.PCTBASIS1 / r.TOTALPCTBASIS1) * 100 : 0;
                    r.NEWBASISPCT2 = r.TOTALPCTBASIS2 > 0 ? (r.PCTBASIS2 / r.TOTALPCTBASIS2) * 100 : 0;
                    r.NEWBASISPCT3 = r.TOTALPCTBASIS3 > 0 ? (r.PCTBASIS3 / r.TOTALPCTBASIS3) * 100 : 0;

                    r.NEWORIBASIS1 = Math.Round(r.NEWORIBASISPCT1 * r.ORIBASE1 / 100, 1);
                    r.NEWBASIS1 = Math.Round(r.NEWBASISPCT1 * r.BASE1 / 100, 1);
                    r.NEWBASIS2 = Math.Round(r.NEWBASISPCT2 * r.BASE2 / 100, 1);
                    r.NEWBASIS3 = Math.Round(r.NEWBASISPCT3 * r.BASE3 / 100, 1);

                    if (base1TolerancePercent > 0)
                    {
                        decimal base1Tolerance = r.NEWBASIS1 - Math.Round(r.NEWBASIS1 * base1TolerancePercent / 100, 0);
                        if (r.HASILPANEN < r.NEWBASIS1 && r.HASILPANEN >= base1Tolerance)
                            r.NEWBASIS1 = r.HASILPANEN;
                    }

                    r.NEWPREMISIAP1 = r.NEWBASISPCT1 * r.PREMI1 / 100;
                    r.NEWPREMISIAP2 = r.NEWBASISPCT2 * r.PREMI2 / 100;
                    r.NEWPREMISIAP3 = r.NEWBASISPCT3 * r.PREMI3 / 100;

                    if (r.HASILPANEN <= r.NEWBASIS1)
                        r.NEWHASIL1 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS2)
                        if (r.NEWBASIS2 == 0)
                            r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;
                        else
                            r.NEWHASIL1 = r.NEWBASIS2 - r.NEWBASIS1;
                    else
                        r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;

                    if (r.NEWBASIS2 == 0 || r.HASILPANEN <= r.NEWBASIS2)
                        r.NEWHASIL2 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS3 == 0)
                            r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;
                        else
                            r.NEWHASIL2 = r.NEWBASIS3 - r.NEWBASIS2;
                    else
                        r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;

                    if (r.NEWBASIS3 == 0 || r.HASILPANEN <= r.NEWBASIS3)
                        r.NEWHASIL3 = 0;
                    else
                        r.NEWHASIL3 = r.HASILPANEN - r.NEWBASIS3;

                    r.NEWPREMILEBIH1 = r.NEWHASIL1 * r.EXCEED1;
                    r.NEWPREMILEBIH2 = r.NEWHASIL2 * r.EXCEED2;
                    r.NEWPREMILEBIH3 = r.NEWHASIL3 * r.EXCEED3;

                    r.NEWINCENTIVE1 = (r.HASILPANEN >= r.NEWBASIS1 ? r.NEWPREMISIAP1 : 0) + r.NEWPREMILEBIH1;
                    r.NEWINCENTIVE2 = (r.HASILPANEN >= r.NEWBASIS2 ? r.NEWPREMISIAP2 : 0) + r.NEWPREMILEBIH2;
                    r.NEWINCENTIVE3 = (r.HASILPANEN >= r.NEWBASIS3 ? r.NEWPREMISIAP3 : 0) + r.NEWPREMILEBIH3;
                }
            }

            //Hitung HK
            //HARVESTPAYMENTTYPE.Harian
            var empList = (from i in harvestingResults
                           where i.HARVESTPAYMENTTYPE == 0
                            && i.NEWBASIS1 > 0
                           group i by new { i.HARVESTDATE, i.GEMPID }
                           into g
                           select new
                           {
                               g.Key.HARVESTDATE,
                               g.Key.GEMPID,
                               Ratio = g.Average(r => Decimal.Round(r.HASILPANEN / r.NEWBASIS1, 2)),
                               TotalKg = g.Sum(r => r.HASILPANEN)
                           }).ToList();

            foreach (var emp in empList)
            {
                var totalHk = Decimal.Round(emp.Ratio, 2);
                if (totalHk > 1) totalHk = 1;

                var empResult = from i in harvestingResults
                                where i.HARVESTDATE == emp.HARVESTDATE && i.GEMPID == emp.GEMPID
                                && i.HARVESTPAYMENTTYPE == 0 && i.NEWBASIS1 > 0
                                select i;

                decimal hkUsed = 0;
                int j = 1;
                foreach (var res in empResult)
                {
                    decimal hk;
                    if (j != empResult.Count())
                    {
                        hk = Decimal.Round(totalHk * (res.HASILPANEN / emp.TotalKg), 2);
                        hkUsed += hk;
                    }
                    else
                        hk = totalHk - hkUsed;

                    res.HK = hk;
                    j++;
                }
            }

            //Simpan HK
            var empHK = (from i in harvestingResults
                         group i by new { i.HARVESTCODE, i.EMPLOYEEID }
                        into g
                         select new { g.Key.HARVESTCODE, g.Key.EMPLOYEEID, HK = g.Sum(r => r.HK) }).ToList();

            var blockHk = (from i in harvestingResults
                           group i by new { i.HARVESTCODE, i.EMPLOYEEID, i.BLOCKID }
                        into g
                           select new { g.Key.HARVESTCODE, g.Key.EMPLOYEEID, g.Key.BLOCKID, HK = g.Sum(r => r.HK) }).ToList();


            harvestEmployees.Clear();
            foreach (var hvtEmp in empHK)
            {
                var newHvt = new THARVESTEMPLOYEE
                {
                    HARVESTCODE = hvtEmp.HARVESTCODE,
                    EMPLOYEEID = hvtEmp.EMPLOYEEID,
                    VALUE = hvtEmp.HK,
                    VALUECALC = hvtEmp.HK,
                };
                harvestEmployees.Add(newHvt);
            }

            harvestBlocks.Clear();
            foreach (var hvtBlk in blockHk)
            {
                var newHvt = new THARVESTBLOCK
                {
                    HARVESTCODE = hvtBlk.HARVESTCODE,
                    EMPLOYEEID = hvtBlk.EMPLOYEEID,
                    BLOCKID = hvtBlk.BLOCKID,
                    VALUE = hvtBlk.HK,
                };
                harvestBlocks.Add(newHvt);
            }
        }

        private void LoadingDailyProcess(List<TLOADINGRESULT> loadingResults, List<TLOADINGEMPLOYEE> loadingEmployees, List<TLOADINGBLOCK> loadingBlocks,
            MPAYMENTSCHEME scheme)
        {
            var stringbase1TolerancePercent = HelperService.GetConfigValue(PMSConstants.CfgLoadingResultBase1TolerancePercent + scheme.UNITCODE,_context);
            decimal base1TolerancePercent = 0;
            if (!string.IsNullOrEmpty(stringbase1TolerancePercent))
                base1TolerancePercent = Convert.ToDecimal(stringbase1TolerancePercent);

            if (scheme.PREMIBASEDCALCULATION == "Proporsional")
            {
                foreach (var r in loadingResults)
                {
                    if (r.LOADINGDATE.DayOfWeek == DayOfWeek.Friday && r.FRIDAY1 != 0)
                    {
                        r.ORIBASE1 = r.ORIBASE1 * r.FRIDAY1 / 100;
                        r.BASE1 = r.BASE1 * r.FRIDAY1 / 100;
                        r.BASE2 = r.BASE2 * r.FRIDAY1 / 100;
                        r.BASE3 = r.BASE3 * r.FRIDAY1 / 100;
                        r.BASE4 = r.BASE4 * r.FRIDAY1 / 100;
                        r.BASE5 = r.BASE5 * r.FRIDAY1 / 100;
                        r.BASE6 = r.BASE6 * r.FRIDAY1 / 100;
                    }

                    r.PCTORIBASIS1 = r.ORIBASE1 == 0 ? 0 : r.HASILPANEN / r.ORIBASE1 * 100;
                    r.PCTBASIS1 = r.BASE1 == 0 ? 0 : r.HASILPANEN / r.BASE1 * 100;
                    r.PCTBASIS2 = r.BASE2 == 0 ? 0 : r.HASILPANEN / r.BASE2 * 100;
                    r.PCTBASIS3 = r.BASE3 == 0 ? 0 : r.HASILPANEN / r.BASE3 * 100;
                    r.PCTBASIS4 = r.BASE4 == 0 ? 0 : r.HASILPANEN / r.BASE4 * 100;
                    r.PCTBASIS5 = r.BASE5 == 0 ? 0 : r.HASILPANEN / r.BASE5 * 100;
                    r.PCTBASIS6 = r.BASE6 == 0 ? 0 : r.HASILPANEN / r.BASE6 * 100;

                }

                foreach (var r in loadingResults)
                {
                    var totPctOriBas1 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTORIBASIS1);
                    var totPctBas1 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS1);
                    var totPctBas2 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS2);
                    var totPctBas3 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS3);
                    var totPctBas4 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS4);
                    var totPctBas5 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS5);
                    var totPctBas6 =
                        loadingResults.Where(
                            itm =>
                            itm.EMPLOYEEID == r.EMPLOYEEID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS6);

                    r.TOTALPCTORIBASIS1 = totPctOriBas1.Sum();
                    r.TOTALPCTBASIS1 = totPctBas1.Sum();
                    r.TOTALPCTBASIS2 = totPctBas2.Sum();
                    r.TOTALPCTBASIS3 = totPctBas3.Sum();
                    r.TOTALPCTBASIS4 = totPctBas4.Sum();
                    r.TOTALPCTBASIS5 = totPctBas5.Sum();
                    r.TOTALPCTBASIS6 = totPctBas6.Sum();
                }

                foreach (var r in loadingResults)
                {
                    r.NEWORIBASISPCT1 = r.TOTALPCTORIBASIS1 > 0 ? (r.PCTORIBASIS1 / r.TOTALPCTORIBASIS1) * 100 : 0;
                    r.NEWBASISPCT1 = r.TOTALPCTBASIS1 > 0 ? (r.PCTBASIS1 / r.TOTALPCTBASIS1) * 100 : 0;
                    r.NEWBASISPCT2 = r.TOTALPCTBASIS2 > 0 ? (r.PCTBASIS2 / r.TOTALPCTBASIS2) * 100 : 0;
                    r.NEWBASISPCT3 = r.TOTALPCTBASIS3 > 0 ? (r.PCTBASIS3 / r.TOTALPCTBASIS3) * 100 : 0;
                    r.NEWBASISPCT4 = r.TOTALPCTBASIS4 > 0 ? (r.PCTBASIS4 / r.TOTALPCTBASIS4) * 100 : 0;
                    r.NEWBASISPCT5 = r.TOTALPCTBASIS5 > 0 ? (r.PCTBASIS5 / r.TOTALPCTBASIS5) * 100 : 0;
                    r.NEWBASISPCT6 = r.TOTALPCTBASIS6 > 0 ? (r.PCTBASIS6 / r.TOTALPCTBASIS6) * 100 : 0;

                    r.NEWORIBASIS1 = Math.Round(r.NEWORIBASISPCT1 * r.ORIBASE1 / 100, 1);
                    r.NEWBASIS1 = Math.Round(r.NEWBASISPCT1 * r.BASE1 / 100, 1);
                    r.NEWBASIS2 = Math.Round(r.NEWBASISPCT2 * r.BASE2 / 100, 1);
                    r.NEWBASIS3 = Math.Round(r.NEWBASISPCT3 * r.BASE3 / 100, 1);
                    r.NEWBASIS4 = Math.Round(r.NEWBASISPCT4 * r.BASE4 / 100, 1);
                    r.NEWBASIS5 = Math.Round(r.NEWBASISPCT5 * r.BASE5 / 100, 1);
                    r.NEWBASIS6 = Math.Round(r.NEWBASISPCT6 * r.BASE6 / 100, 1);

                    if (base1TolerancePercent > 0)
                    {
                        decimal base1Tolerance = r.NEWBASIS1 - Math.Round(r.NEWBASIS1 * base1TolerancePercent / 100, 0);
                        if (r.HASILPANEN < r.NEWBASIS1 && r.HASILPANEN >= base1Tolerance)
                            r.NEWBASIS1 = r.HASILPANEN;
                    }

                    r.NEWPREMISIAP1 = r.NEWBASISPCT1 * r.PREMI1 / 100;
                    r.NEWPREMISIAP2 = r.NEWBASISPCT2 * r.PREMI2 / 100;
                    r.NEWPREMISIAP3 = r.NEWBASISPCT3 * r.PREMI3 / 100;
                    r.NEWPREMISIAP4 = r.NEWBASISPCT4 * r.PREMI4 / 100;
                    r.NEWPREMISIAP5 = r.NEWBASISPCT5 * r.PREMI5 / 100;
                    r.NEWPREMISIAP6 = r.NEWBASISPCT6 * r.PREMI6 / 100;

                    if (r.HASILPANEN <= r.NEWBASIS1)
                        r.NEWHASIL1 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS2)
                        if (r.NEWBASIS2 == 0)
                            r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;
                        else
                            r.NEWHASIL1 = r.NEWBASIS2 - r.NEWBASIS1;
                    else
                        r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;

                    if (r.NEWBASIS2 == 0 || r.HASILPANEN <= r.NEWBASIS2)
                        r.NEWHASIL2 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS3 == 0)
                            r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;
                        else
                            r.NEWHASIL2 = r.NEWBASIS3 - r.NEWBASIS2;
                    else
                        r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;

                    if (r.NEWBASIS3 == 0 || r.HASILPANEN <= r.NEWBASIS3)
                        r.NEWHASIL3 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS4 == 0)
                            r.NEWHASIL3 = r.HASILPANEN - r.NEWBASIS3;
                        else
                            r.NEWHASIL3 = r.NEWBASIS4 - r.NEWBASIS3;
                    else
                        r.NEWHASIL3 = r.HASILPANEN - r.NEWBASIS3;

                    if (r.NEWBASIS4 == 0 || r.HASILPANEN <= r.NEWBASIS4)
                        r.NEWHASIL4 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS5 == 0)
                            r.NEWHASIL4 = r.HASILPANEN - r.NEWBASIS4;
                        else
                            r.NEWHASIL4 = r.NEWBASIS5 - r.NEWBASIS4;
                    else
                        r.NEWHASIL4 = r.HASILPANEN - r.NEWBASIS4;

                    if (r.NEWBASIS5 == 0 || r.HASILPANEN <= r.NEWBASIS5)
                        r.NEWHASIL5 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS6 == 0)
                            r.NEWHASIL5 = r.HASILPANEN - r.NEWBASIS5;
                        else
                            r.NEWHASIL5 = r.NEWBASIS6 - r.NEWBASIS5;
                    else
                        r.NEWHASIL5 = r.HASILPANEN - r.NEWBASIS5;


                    if (r.NEWBASIS6 == 0 || r.HASILPANEN <= r.NEWBASIS6)
                        r.NEWHASIL6 = 0;
                    else
                        r.NEWHASIL6 = r.HASILPANEN - r.NEWBASIS6;


                    r.NEWPREMILEBIH1 = r.NEWHASIL1 * r.EXCEED1;
                    r.NEWPREMILEBIH2 = r.NEWHASIL2 * r.EXCEED2;
                    r.NEWPREMILEBIH3 = r.NEWHASIL3 * r.EXCEED3;
                    r.NEWPREMILEBIH4 = r.NEWHASIL4 * r.EXCEED4;
                    r.NEWPREMILEBIH5 = r.NEWHASIL5 * r.EXCEED5;
                    r.NEWPREMILEBIH6 = r.NEWHASIL6 * r.EXCEED6;

                    r.NEWINCENTIVE1 = (r.HASILPANEN >= r.NEWBASIS1 ? r.NEWPREMISIAP1 : 0) + r.NEWPREMILEBIH1;
                    r.NEWINCENTIVE2 = (r.HASILPANEN >= r.NEWBASIS2 ? r.NEWPREMISIAP2 : 0) + r.NEWPREMILEBIH2;
                    r.NEWINCENTIVE3 = (r.HASILPANEN >= r.NEWBASIS3 ? r.NEWPREMISIAP3 : 0) + r.NEWPREMILEBIH3;
                    r.NEWINCENTIVE4 = (r.HASILPANEN >= r.NEWBASIS4 ? r.NEWPREMISIAP4 : 0) + r.NEWPREMILEBIH4;
                    r.NEWINCENTIVE5 = (r.HASILPANEN >= r.NEWBASIS5 ? r.NEWPREMISIAP5 : 0) + r.NEWPREMILEBIH5;
                    r.NEWINCENTIVE6 = (r.HASILPANEN >= r.NEWBASIS6 ? r.NEWPREMISIAP6 : 0) + r.NEWPREMILEBIH6;

                }
            }

            //Hitung HK
            //HARVESTPAYMENTTYPE.Harian
            var empList = (from i in loadingResults
                           where i.LOADINGPAYMENTTYPE == 0
                            && i.NEWBASIS1 > 0
                           group i by new { i.LOADINGDATE, i.EMPLOYEEID }
                           into g
                           select new
                           {
                               g.Key.LOADINGDATE,
                               g.Key.EMPLOYEEID,
                               Ratio = g.Average(r => Decimal.Round(r.HASILPANEN / r.NEWBASIS1, 2)),
                               TotalKg = g.Sum(r => r.HASILPANEN)
                           }).ToList();

            foreach (var emp in empList)
            {
                var totalHk = Decimal.Round(emp.Ratio, 2);
                if (totalHk > 1) totalHk = 1;

                var empResult = from i in loadingResults
                                where i.LOADINGDATE == emp.LOADINGDATE && i.EMPLOYEEID == emp.EMPLOYEEID
                                && i.LOADINGPAYMENTTYPE == 0 && i.NEWBASIS1 > 0
                                select i;

                decimal hkUsed = 0;
                int j = 1;
                foreach (var res in empResult)
                {
                    decimal hk;
                    if (j != empResult.Count())
                    {
                        hk = Decimal.Round(totalHk * (res.HASILPANEN / emp.TotalKg), 2);
                        hkUsed += hk;

                        //Round Edit By Hardi 03/05/2020
                        if (hkUsed > totalHk && hk > 0)
                        {
                            decimal hk_round = 0;
                            hk_round = hk + (totalHk - hkUsed);
                            hkUsed -= hk;
                            hk = hk_round;
                            hkUsed += hk;
                        }

                    }
                    else
                        hk = totalHk - hkUsed;

                    res.HK = hk;
                    j++;
                }
            }

            //Simpan HK
            var empHK = (from i in loadingResults
                         where i.EFLAG == true
                         group i by new { i.LOADINGCODE, i.EMPLOYEEID }
                        into g
                         select new { g.Key.LOADINGCODE, g.Key.EMPLOYEEID, HK = g.Sum(r => r.HK) }).ToList();

            var blockHk = (from i in loadingResults
                           where i.EFLAG == true
                           group i by new { i.LOADINGCODE, i.EMPLOYEEID, i.BLOCKID }
                        into g
                           select new { g.Key.LOADINGCODE, g.Key.EMPLOYEEID, g.Key.BLOCKID, HK = g.Sum(r => r.HK) }).ToList();


            loadingEmployees.Clear();
            foreach (var hvtEmp in empHK)
            {
                var newHvt = new TLOADINGEMPLOYEE
                {
                    LOADINGCODE = hvtEmp.LOADINGCODE,
                    EMPLOYEEID = hvtEmp.EMPLOYEEID,
                    VALUE = hvtEmp.HK,
                    VALUECALC= hvtEmp.HK,
                };
                loadingEmployees.Add(newHvt);
            }

            loadingBlocks.Clear();
            foreach (var hvtBlk in blockHk)
            {
                var newHvt = new TLOADINGBLOCK
                {
                    LOADINGCODE = hvtBlk.LOADINGCODE,
                    EMPLOYEEID = hvtBlk.EMPLOYEEID,
                    BLOCKID = hvtBlk.BLOCKID,
                    VALUE = hvtBlk.HK,
                };
                loadingBlocks.Add(newHvt);
            }


        }

        private void OperatingDailyProcess(List<TOPERATINGRESULT> operatingResults, List<TLOADINGDRIVER> loadingDrivers, List<TLOADINGBLOCK> loadingBlock,
            MPAYMENTSCHEME scheme)
        {
            var stringbase1TolerancePercent = HelperService.GetConfigValue(PMSConstants.CfgHarvestingResultBase1TolerancePercent + scheme.UNITCODE,_context);
            decimal base1TolerancePercent = 0;
            if (!string.IsNullOrEmpty(stringbase1TolerancePercent))
                base1TolerancePercent = Convert.ToDecimal(stringbase1TolerancePercent);

            if (scheme.PREMIBASEDCALCULATION == "Proporsional")
            {
                foreach (var r in operatingResults)
                {
                    if (r.LOADINGDATE.DayOfWeek == DayOfWeek.Friday && r.FRIDAY1 != 0)
                    {
                        r.ORIBASE1 = r.ORIBASE1 * r.FRIDAY1 / 100;
                        r.BASE1 = r.BASE1 * r.FRIDAY1 / 100;
                        r.BASE2 = r.BASE2 * r.FRIDAY1 / 100;
                        r.BASE3 = r.BASE3 * r.FRIDAY1 / 100;
                        r.BASE4 = r.BASE4 * r.FRIDAY1 / 100;
                        r.BASE5 = r.BASE5 * r.FRIDAY1 / 100;
                        r.BASE6 = r.BASE6 * r.FRIDAY1 / 100;
                    }

                    r.PCTORIBASIS1 = r.ORIBASE1 == 0 ? 0 : r.HASILPANEN / r.ORIBASE1 * 100;
                    r.PCTBASIS1 = r.BASE1 == 0 ? 0 : r.HASILPANEN / r.BASE1 * 100;
                    r.PCTBASIS2 = r.BASE2 == 0 ? 0 : r.HASILPANEN / r.BASE2 * 100;
                    r.PCTBASIS3 = r.BASE3 == 0 ? 0 : r.HASILPANEN / r.BASE3 * 100;
                    r.PCTBASIS4 = r.BASE4 == 0 ? 0 : r.HASILPANEN / r.BASE4 * 100;
                    r.PCTBASIS5 = r.BASE5 == 0 ? 0 : r.HASILPANEN / r.BASE5 * 100;
                    r.PCTBASIS6 = r.BASE6 == 0 ? 0 : r.HASILPANEN / r.BASE6 * 100;
                }

                foreach (var r in operatingResults)
                {
                    var totPctOriBas1 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTORIBASIS1);

                    var totPctBas1 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS1);
                    var totPctBas2 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS2);
                    var totPctBas3 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS3);
                    var totPctBas4 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS4);
                    var totPctBas5 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS5);
                    var totPctBas6 =
                        operatingResults.Where(
                            itm =>
                            itm.DRIVERID == r.DRIVERID && itm.LOADINGDATE == r.LOADINGDATE &&
                            itm.LOADINGTYPE == r.LOADINGTYPE && itm.LOADINGPAYMENTTYPE == r.LOADINGPAYMENTTYPE
                            ).Select(itm => itm.PCTBASIS6);


                    r.TOTALPCTORIBASIS1 = totPctOriBas1.Sum();
                    r.TOTALPCTBASIS1 = totPctBas1.Sum();
                    r.TOTALPCTBASIS2 = totPctBas2.Sum();
                    r.TOTALPCTBASIS3 = totPctBas3.Sum();
                    r.TOTALPCTBASIS4 = totPctBas4.Sum();
                    r.TOTALPCTBASIS5 = totPctBas5.Sum();
                    r.TOTALPCTBASIS6 = totPctBas6.Sum();
                }

                foreach (var r in operatingResults)
                {
                    r.NEWORIBASISPCT1 = r.TOTALPCTORIBASIS1 > 0 ? (r.PCTORIBASIS1 / r.TOTALPCTORIBASIS1) * 100 : 0;

                    r.NEWBASISPCT1 = r.TOTALPCTBASIS1 > 0 ? (r.PCTBASIS1 / r.TOTALPCTBASIS1) * 100 : 0;
                    r.NEWBASISPCT2 = r.TOTALPCTBASIS2 > 0 ? (r.PCTBASIS2 / r.TOTALPCTBASIS2) * 100 : 0;
                    r.NEWBASISPCT3 = r.TOTALPCTBASIS3 > 0 ? (r.PCTBASIS3 / r.TOTALPCTBASIS3) * 100 : 0;
                    r.NEWBASISPCT4 = r.TOTALPCTBASIS4 > 0 ? (r.PCTBASIS4 / r.TOTALPCTBASIS4) * 100 : 0;
                    r.NEWBASISPCT5 = r.TOTALPCTBASIS5 > 0 ? (r.PCTBASIS5 / r.TOTALPCTBASIS5) * 100 : 0;
                    r.NEWBASISPCT6 = r.TOTALPCTBASIS6 > 0 ? (r.PCTBASIS6 / r.TOTALPCTBASIS6) * 100 : 0;

                    r.NEWORIBASIS1 = Math.Round(r.NEWORIBASISPCT1 * r.ORIBASE1 / 100, 1);

                    r.NEWBASIS1 = Math.Round(r.NEWBASISPCT1 * r.BASE1 / 100, 1);
                    r.NEWBASIS2 = Math.Round(r.NEWBASISPCT2 * r.BASE2 / 100, 1);
                    r.NEWBASIS3 = Math.Round(r.NEWBASISPCT3 * r.BASE3 / 100, 1);
                    r.NEWBASIS4 = Math.Round(r.NEWBASISPCT4 * r.BASE4 / 100, 1);
                    r.NEWBASIS5 = Math.Round(r.NEWBASISPCT5 * r.BASE5 / 100, 1);
                    r.NEWBASIS6 = Math.Round(r.NEWBASISPCT6 * r.BASE6 / 100, 1);

                    if (base1TolerancePercent > 0)
                    {
                        decimal base1Tolerance = r.NEWBASIS1 - Math.Round(r.NEWBASIS1 * base1TolerancePercent / 100, 0);
                        if (r.HASILPANEN < r.NEWBASIS1 && r.HASILPANEN >= base1Tolerance)
                            r.NEWBASIS1 = r.HASILPANEN;
                    }

                    r.NEWPREMISIAP1 = r.NEWBASISPCT1 * r.PREMI1 / 100;
                    r.NEWPREMISIAP2 = r.NEWBASISPCT2 * r.PREMI2 / 100;
                    r.NEWPREMISIAP3 = r.NEWBASISPCT3 * r.PREMI3 / 100;
                    r.NEWPREMISIAP4 = r.NEWBASISPCT4 * r.PREMI4 / 100;
                    r.NEWPREMISIAP5 = r.NEWBASISPCT5 * r.PREMI5 / 100;
                    r.NEWPREMISIAP6 = r.NEWBASISPCT6 * r.PREMI6 / 100;


                    if (r.HASILPANEN <= r.NEWBASIS1)
                        r.NEWHASIL1 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS2)
                        if (r.NEWBASIS2 == 0)
                            r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;
                        else
                            r.NEWHASIL1 = r.NEWBASIS2 - r.NEWBASIS1;
                    else
                        r.NEWHASIL1 = r.HASILPANEN - r.NEWBASIS1;


                    if (r.NEWBASIS2 == 0 || r.HASILPANEN <= r.NEWBASIS2)
                        r.NEWHASIL2 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS3)
                        if (r.NEWBASIS3 == 0)
                            r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;
                        else
                            r.NEWHASIL2 = r.NEWBASIS3 - r.NEWBASIS2;
                    else
                        r.NEWHASIL2 = r.HASILPANEN - r.NEWBASIS2;

                    if (r.NEWBASIS3 == 0 || r.HASILPANEN <= r.NEWBASIS3)
                        r.NEWHASIL3 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS4)
                        if (r.NEWBASIS4 == 0)
                            r.NEWHASIL3 = r.HASILPANEN - r.NEWBASIS3;
                        else
                            r.NEWHASIL3 = r.NEWBASIS4 - r.NEWBASIS3;
                    else
                        r.NEWHASIL3 = r.HASILPANEN - r.NEWBASIS3;

                    if (r.NEWBASIS4 == 0 || r.HASILPANEN <= r.NEWBASIS4)
                        r.NEWHASIL4 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS5)
                        if (r.NEWBASIS5 == 0)
                            r.NEWHASIL4 = r.HASILPANEN - r.NEWBASIS4;
                        else
                            r.NEWHASIL4 = r.NEWBASIS5 - r.NEWBASIS4;
                    else
                        r.NEWHASIL4 = r.HASILPANEN - r.NEWBASIS4;

                    if (r.NEWBASIS5 == 0 || r.HASILPANEN <= r.NEWBASIS5)
                        r.NEWHASIL5 = 0;
                    else if (r.HASILPANEN >= r.NEWBASIS6)
                        if (r.NEWBASIS6 == 0)
                            r.NEWHASIL5 = r.HASILPANEN - r.NEWBASIS5;
                        else
                            r.NEWHASIL5 = r.NEWBASIS6 - r.NEWBASIS5;
                    else
                        r.NEWHASIL5 = r.HASILPANEN - r.NEWBASIS5;


                    if (r.NEWBASIS6 == 0 || r.HASILPANEN <= r.NEWBASIS6)
                        r.NEWHASIL6 = 0;
                    else
                        r.NEWHASIL6 = r.HASILPANEN - r.NEWBASIS6;


                    r.NEWPREMILEBIH1 = r.NEWHASIL1 * r.EXCEED1;
                    r.NEWPREMILEBIH2 = r.NEWHASIL2 * r.EXCEED2;
                    r.NEWPREMILEBIH3 = r.NEWHASIL3 * r.EXCEED3;
                    r.NEWPREMILEBIH4 = r.NEWHASIL4 * r.EXCEED4;
                    r.NEWPREMILEBIH5 = r.NEWHASIL5 * r.EXCEED5;
                    r.NEWPREMILEBIH6 = r.NEWHASIL6 * r.EXCEED6;

                    r.NEWINCENTIVE1 = (r.HASILPANEN >= r.NEWBASIS1 ? r.NEWPREMISIAP1 : 0) + r.NEWPREMILEBIH1;
                    r.NEWINCENTIVE2 = (r.HASILPANEN >= r.NEWBASIS2 ? r.NEWPREMISIAP2 : 0) + r.NEWPREMILEBIH2;
                    r.NEWINCENTIVE3 = (r.HASILPANEN >= r.NEWBASIS3 ? r.NEWPREMISIAP3 : 0) + r.NEWPREMILEBIH3;
                    r.NEWINCENTIVE4 = (r.HASILPANEN >= r.NEWBASIS4 ? r.NEWPREMISIAP4 : 0) + r.NEWPREMILEBIH4;
                    r.NEWINCENTIVE5 = (r.HASILPANEN >= r.NEWBASIS5 ? r.NEWPREMISIAP5 : 0) + r.NEWPREMILEBIH5;
                    r.NEWINCENTIVE6 = (r.HASILPANEN >= r.NEWBASIS6 ? r.NEWPREMISIAP6 : 0) + r.NEWPREMILEBIH6;

                }
            }

            //Hitung HK
            //HARVESTPAYMENTTYPE.Harian
            var empList = (from i in operatingResults
                           where i.LOADINGPAYMENTTYPE == 0
                            && i.NEWBASIS1 > 0
                           group i by new { i.LOADINGDATE, i.DRIVERID }
                           into g
                           select new
                           {
                               g.Key.LOADINGDATE,
                               g.Key.DRIVERID,
                               Ratio = g.Average(r => Decimal.Round(r.HASILPANEN / r.NEWBASIS1, 2)),
                               TotalKg = g.Sum(r => r.HASILPANEN)
                           }).ToList();

            foreach (var emp in empList)
            {
                var totalHk = Decimal.Round(emp.Ratio, 2);
                if (totalHk > 1) totalHk = 1;

                var empResult = from i in operatingResults
                                where i.LOADINGDATE == emp.LOADINGDATE && i.DRIVERID == emp.DRIVERID
                                && i.LOADINGPAYMENTTYPE == 0 && i.NEWBASIS1 > 0
                                select i;

                decimal hkUsed = 0;
                int j = 1;
                foreach (var res in empResult)
                {
                    decimal hk;
                    if (j != empResult.Count())
                    {
                        hk = Decimal.Round(totalHk * (res.HASILPANEN / emp.TotalKg), 2);
                        hkUsed += hk;
                    }
                    else
                        hk = totalHk - hkUsed;

                    res.HK = hk;
                    j++;
                }
            }

            //Simpan HK
            var driverHK = (from i in operatingResults
                                //where i.EFLAG == true
                            group i by new { i.LOADINGCODE, i.DRIVERID }
                            into g
                            select new { g.Key.LOADINGCODE, g.Key.DRIVERID, HK = g.Sum(r => r.HK) }).ToList();


            loadingDrivers.Clear();
            foreach (var hvtEmp in driverHK)
            {
                var newHvt = new TLOADINGDRIVER
                {
                    LOADINGCODE = hvtEmp.LOADINGCODE,
                    DRIVERID = hvtEmp.DRIVERID,
                    VALUE = hvtEmp.HK,
                    VALUECALC = hvtEmp.HK,
                };
                loadingDrivers.Add(newHvt);
            }

        }

    }
}
