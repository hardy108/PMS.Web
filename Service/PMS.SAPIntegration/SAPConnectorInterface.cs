using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PMS.EFCore.Model;
using Dbosoft.YaNco;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using PMS.EFCore.Helper;
using PMS.Shared.Models;
using PMS.Shared.Utilities;
using Microsoft.Extensions.Options;

namespace PMS.SAPIntegration
{
    public class SAPConnectorInterface
    {
        private Exception ex;
        //public IConfiguration Configuration { get; }
        AppSetting _appSetting;

        public SAPConnectorInterface(IOptions<AppSetting> appSettings)
        {
            _appSetting = appSettings.Value;
        }

        private IRfcContext SAPContext()
        {
            var configurationBuilder =
                new ConfigurationBuilder();

            var config = configurationBuilder.Build();

            var settings = new Dictionary<string, string>
            {
                {"ashost", _appSetting.SapAppServerHost},//10.99.17.240
                {"sysnr", _appSetting.SapSystemNumber},//00
                {"client", _appSetting.SapClient},//310
                {"user", PMSEncryption.Decrypt(_appSetting.SapUser,"pa5")}, //helpdesk5
                {"passwd", PMSEncryption.Decrypt(_appSetting.SapPassword,"pa5")}, //Ehpjuve2016#
                {"lang", _appSetting.SapLanguage}//EN

            };
            
            var runtime = new RfcRuntime(new SimpleConsoleLogger());
            Task<Either<RfcErrorInfo, IConnection>> ConnFunc() => Connection.Create(settings, runtime);
            var context_ = new RfcContext(ConnFunc);

            return context_;
        }

        public async Task<string> Tesrfc()
        {
            //var configurationBuilder =
            //    new ConfigurationBuilder();

            ////configurationBuilder.AddInMemoryCollection(new[]
            ////{
            ////    new KeyValuePair<string, string>("tests:repeats", "10"),
            ////    new KeyValuePair<string, string>("tests:rows", "10")
            ////});
            ////configurationBuilder.AddEnvironmentVariables("saprfc");
            ////configurationBuilder.AddCommandLine(args);
            ////configurationBuilder.AddUserSecrets<Program>();

            //var config = configurationBuilder.Build();

            //var settings = new Dictionary<string, string>
            //{
            //    {"ashost", "10.99.17.240"},
            //    {"sysnr", "00"},
            //    {"client", "310"},
            //    {"user", "helpdesk5"},
            //    {"passwd", "Ehpjuve2016#"},
            //    {"lang", "EN"}

            //};

            ////var rows = Convert.ToInt32(config["tests:rows"]);
            ////var repeats = Convert.ToInt32(config["tests:repeats"]);

            ////Console.WriteLine($"Test rows: {rows}");
            ////Console.WriteLine($"Test repeats: {repeats}");


            ////StartProgramDelegate callback = command =>
            ////{
            ////    var programParts = command.Split(' ');
            ////    var arguments = command.Replace(programParts[0], "");
            ////    var p = Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\" + programParts[0] + ".exe",
            ////        arguments.TrimStart());

            ////    return RfcErrorInfo.Ok();
            ////};

            //var runtime = new RfcRuntime(new SimpleConsoleLogger());


            //Task<Either<RfcErrorInfo, IConnection>> ConnFunc() => Connection.Create(settings, runtime);

            string error = "";
            string res = "";

            using (var context = SAPContext())
            {
                var rfc = await context.CallFunction("DDIF_FIELDLABEL_GET",
                        Input: f => f
                            .SetField("TABNAME", "USR01")
                            .SetField("FIELDNAME", "BNAME"),
                        Output: f => f
                            .GetField<string>("LABEL")) //;

                        // this is from language.ext to extract the value from a either
                        .ToAsync().Match(r => res = r, l => error = l.Message);

                //return res.ToString();

                // this is from language.ext to extract the value from a either
                //.ToAsync().Match(r => result = r ), // should return: User Name
                //                  l => error = l ));
            }

            if (string.IsNullOrEmpty(error))
                return res;
            else
                return error;
        }

        public async Task<string> CompName(string code)
        {
            string error = "";
            string res = "";

            using (var context = SAPContext())
            {
                var rfc = await context.CallFunction("BAPI_COMPANYCODE_GETDETAIL",
                            Input: f => f
                                .SetField("COMPANYCODEID", code),
                            Output: func => func.MapStructure("COMPANYCODE_DETAIL", s =>
                                from name in s.GetField<string>("COMP_NAME")
                                select name
                            ))

                        //await context.CallFunction("DDIF_FIELDLABEL_GET",
                        //    Input: f => f
                        //        .SetField("TABNAME", "USR01")
                        //        .SetField("FIELDNAME", "BNAME"),
                        //    Output: f => f
                        //        .GetField<string>("LABEL")) //;

                        // this is from language.ext to extract the value from a either
                        .ToAsync().Match(r => res = r, l => error = l.Message);

            }

            if (string.IsNullOrEmpty(error))
                return res;
            else
                return error;
        }

        public async Task<string> TesRFC(string uname)
        {
            //string error = "";
            //string res = "";
            //bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            var list = new List<SAPPOSTING>();

            var a = new SAPPOSTING();
            a.CANCEL = "X";
            list.Add(a);

            var b = new SAPPOSTING();
            b.CANCEL = "Y";
            list.Add(b);
            
            //if (!string.IsNullOrWhiteSpace(type) && type == "BKM_GI")
            //{
                using (var context = SAPContext())
                {
                //var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                //            Input: I => I
                //                    .SetField("FI_INTERFACE_FLAG", "X")
                //                    .BindAsync(dc => dc.GetTable("FT_BKMGI")
                //                        .Use(used => used.Map(table => table.AppendRow()
                //                            .Apply(row => row.Map(s =>
                //                                from TRANS_DATE in s.SetField("TRANS_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                //                                from cancel in s.SetField("CANCEL", string.Empty)
                //                                from pms_doc_no in s.SetField("HEADER_TXT", bkm.PMS_DOC_NO)
                //                                from block in s.SetField("BLOCK", bkm.BLOCK)
                //                                from act in s.SetField("ACTTYPE", bkm.ACT)
                //                                from unit in s.SetField("PLANT", bkm.PLANT)
                //                                from DIV in s.SetField("DIVISION", bkm.DIV)
                //                                from MATID in s.SetField("MATERIAL", bkm.MATID)
                //                                from QTY in s.SetField("ENTRY_QNT", bkm.QTY)
                //                                from UOM in s.SetField("ENTRY_UOM", bkm.UOM)
                //                                from BATCH in s.SetField("BATCH", bkm.BATCH)
                //                                from CE in s.SetField("GL_ACCOUNT", bkm.CE)
                //                                select pms_doc_no))
                //                            ).Traverse(l => l).Map(u => dc))),
                //            //.Use(used => used.Map(table => (dc, table))
                //            //    .Bind(t => t.Map(input => t.table.AppendRow()
                //            //       .Apply(row => row.Map(s => input
                //            //             .Add(s.SetField("HEADER_TXT", bkm.PMS_DOC_NO)
                //            //         )))))))
                //            //,
                //            Output:
                //                func => func.MapTable("FT_RETURN", s =>
                //                    from stat in s.GetField<string>("PSTG_STAT")
                //                    from docno in s.GetField<string>("DOC_NUMBER")
                //                    from msg in s.GetField<string>("PSTG_MSG")
                //                    select (stat, docno, msg)))

                //            .ToAsync().Match(r =>
                //            {
                //                    //res = r.ToString() ;
                //                    foreach (var (stat, docno, msg) in r)
                //                {
                //                    var ftr = new FTRETURN();
                //                    ftr.STAT = stat;
                //                    ftr.DOCNO = docno;
                //                    ftr.MSG = msg;
                //                    FT.Add(ftr);
                //                }
                //            }, l => { Ex(l.Message); });
                var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMGI", list, (structure, table) => structure
                                            .SetField("CANCEL", table.CANCEL))
                                ,
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                                .ToAsync().Match(r =>
                                                {
                                                    foreach (var (stat, docno, msg) in r)
                                                    {
                                                        var ftr = new FTRETURN();
                                                        ftr.STAT = stat;
                                                        ftr.DOCNO = docno;
                                                        ftr.MSG = msg;
                                                        FT.Add(ftr);
                                                    }
                                                }, l => { Ex(l.Message); });
                }
            //}

            return "OKAY";
        }

        public async Task<List<FTRETURN>> UploadBkm(List<SAPPOSTING> bkm, string type, string uname)
        {
            //string error = "";
            //string res = "";
            //bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BKM_GI")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMGI", bkm, (structure, table) => structure
                                            .SetField("TRANS_DATE", table.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                            .SetField("CANCEL", string.Empty)
                                            .SetField("HEADER_TXT", table.PMS_DOC_NO)
                                            .SetField("BLOCK", table.BLOCK)
                                            .SetField("ACTTYPE", table.ACT)
                                            .SetField("PLANT", table.PLANT)
                                            .SetField("DIVISION", table.DIV)
                                            .SetField("MATERIAL", table.MATID)
                                            .SetField("ENTRY_QNT", table.QTY)
                                            .SetField("ENTRY_UOM", table.UOM)
                                            .SetField("BATCH", table.BATCH)
                                            .SetField("GL_ACCOUNT", table.CE))
                                ,
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))
                                .ToAsync().Match(r =>
                                {
                                    //res = r.ToString() ;
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKM_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMPS", bkm, (structure, table) => structure
                                            .SetField("TRANS_DATE", table.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                            .SetField("BKM_NO", table.PMS_DOC_NO)
                                            .SetField("CANCEL", string.Empty)
                                            .SetField("PLANT", table.PLANT.ToString())
                                            .SetField("DIVISION", table.DIV)
                                            .SetField("EMPL_TYPE", table.EMPTYPE)
                                            .SetField("ACTTYPE", table.ACT)
                                            .SetField("ENTRY_QNT", table.QTY)
                                            .SetField("BLOCK", table.BLOCK)
                                            .SetField("TEXT", string.Empty)
                                            .SetField("BORONGAN", table.BOR)
                                            .SetField("OUTPUT_QTY", table.AREA)
                                            .SetField("OUTPUT_UOM", table.UOM))
                                ,
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKM_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMCO", bkm, (structure, table) => structure
                                            .SetField("ESTATE", table.PLANT.ToString())
                                            .SetField("DIVISION", table.DIV)
                                            .SetField("BLOCK", table.BLOCK)
                                            .SetField("PERIO", table.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                            .SetField("BKM_NO", table.PMS_DOC_NO)
                                            .SetField("ACVTYP", table.ACT)
                                            .SetField("IS_CANCEL", string.Empty)
                                            .SetField("QTY_UPKEEP", table.AREA))
                                ,
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;
            
        }

        public async Task<List<FTRETURN>> CancelUploadBkm(List<SAPPOSTING> bkm, string type, string uname)
        {
            //bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BKM_GI")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMGI", bkm, (structure, table) => structure
                                            .SetField("TRANS_DATE", table.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                            .SetField("CANCEL", "X")
                                            .SetField("HEADER_TXT", table.PMS_DOC_NO)
                                            .SetField("PLANT", table.PLANT)
                                            ),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    //res = r.ToString() ;
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                    
                }
            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKM_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMPS", bkm, (structure, table) => structure
                                            .SetField("BKM_NO", table.PMS_DOC_NO)
                                            .SetField("CANCEL", "X")
                                            .SetField("PLANT", table.PLANT.ToString())
                                            ),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKM_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_BKM_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .SetTable("FT_BKMCO", bkm, (structure, table) => structure
                                            .SetField("ESTATE", table.PLANT.ToString())
                                            .SetField("PERIO", table.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                            .SetField("BKM_NO", table.PMS_DOC_NO)
                                            .SetField("IS_CANCEL", "X")
                                            ),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;

        }

        public async Task<List<FTRETURN>> UploadBkmPlasma(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BKMP_GI")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_BKMGI")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from cancel in s.SetField("CANCEL", string.Empty)
                                                    from pms_doc_no in s.SetField("ZCONCD", bkm.PMS_DOC_NO)
                                                    from block in s.SetField("BLOCK", bkm.BLOCK)
                                                    from act in s.SetField("ACTTYPE", bkm.ACT)
                                                    from unit in s.SetField("PLANT", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("DIVISION", bkm.DIV)
                                                    from MATID in s.SetField("MATERIAL", bkm.MATID)
                                                    from QTY in s.SetField("ACT_CONSUMED", bkm.QTY)
                                                    from UOM in s.SetField("ACT_CONSUMED_UOM", bkm.UOM)
                                                    from BATCH in s.SetField("BATCH", bkm.BATCH)
                                                    from CE in s.SetField("COST_ELEMENT", bkm.CE)
                                                    from COST in s.SetField("ZCOST", bkm.COSTCTR)
                                                    from CURR in s.SetField("ZCURRENCY", bkm.CURR)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    //res = r.ToString() ;
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKMP_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_BKMPS")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from pms_doc_no in s.SetField("ZCONCD", bkm.PMS_DOC_NO)
                                                    from cancel in s.SetField("ZBTISC", string.Empty)
                                                    from unit in s.SetField("GSBER", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("ZDIVI", bkm.DIV)
                                                    from emptype in s.SetField("ZEMTYP", bkm.EMPTYPE)
                                                    from act in s.SetField("NOACT", bkm.ACT)
                                                    from QTY in s.SetField("ZQUANTITY", bkm.QTY)
                                                    from block in s.SetField("ZBLOCK", bkm.BLOCK)
                                                    from text in s.SetField("PSTXT", string.Empty)
                                                    from borongan in s.SetField("ZFLGBRG", bkm.BOR)
                                                    from outputqty in s.SetField("ZOUTQTY", bkm.AREA)
                                                    from outputuom in s.SetField("MEINS", bkm.UOM)
                                                    from COST in s.SetField("ZCOST", bkm.COSTCTR)
                                                    from CURR in s.SetField("ZCURRENCY", bkm.CURR)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKMP_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_BKMCO")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from unit in s.SetField("ESTATE", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("DIVISION", bkm.DIV)
                                                    from block in s.SetField("BLOCK", bkm.BLOCK)
                                                    from period in s.SetField("TRANS_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from pms_doc_no in s.SetField("NO_BKM", bkm.PMS_DOC_NO)
                                                    from act in s.SetField("NOACT", bkm.ACT)
                                                    from cancel in s.SetField("IS_CANCEL", string.Empty)
                                                    from qty in s.SetField("UPKQTY", bkm.AREA)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;

        }

        public async Task<List<FTRETURN>> CancelUploadBkmPlasma(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BKMP_GI")
            {
                //using (var context = SAPContext())
                //{
                //    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD",
                //                Input: I => I
                //                        .SetField("FI_INTERFACE_FLAG", "X")
                //                        .BindAsync(dc => dc.GetTable("FT_BKMGI")
                //                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                //                                row.Map(s =>
                //                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                //                                    from cancel in s.SetField("CANCEL", "X")
                //                                    from pms_doc_no in s.SetField("ZCONCD", bkm.PMS_DOC_NO)
                //                                    from unit in s.SetField("PLANT", bkm.PLANT.ToString())
                //                                    select pms_doc_no
                //                                ))).Traverse(l => l).Map(u => dc))),
                //                Output:
                //                    func => func.MapTable("FT_RETURN", s =>
                //                        from stat in s.GetField<string>("PSTG_STAT")
                //                        from docno in s.GetField<string>("DOC_NUMBER")
                //                        from msg in s.GetField<string>("PSTG_MSG")
                //                        select (stat, docno, msg)))

                //                .ToAsync().Match(r =>
                //                {
                //                    //res = r.ToString() ;
                //                    foreach (var (stat, docno, msg) in r)
                //                    {
                //                        var ftr = new FTRETURN();
                //                        ftr.STAT = stat;
                //                        ftr.DOCNO = docno;
                //                        ftr.MSG = msg;
                //                        FT.Add(ftr);
                //                    }
                //                }, l => { Ex(l.Message); });
                //}
            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKMP_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_BKMPS")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("ZCONCD", bkm.PMS_DOC_NO)
                                                    from TRANS_DATE in s.SetField("BUDAT", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from cancel in s.SetField("ZBTISC", "X")
                                                    from unit in s.SetField("GSBER", bkm.PLANT.ToString())
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BKMP_CO")
            {
                //using (var context = SAPContext())
                //{
                //    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD",
                //                Input: I => I
                //                        .SetField("FI_INTERFACE_FLAG", "X")
                //                        .BindAsync(dc => dc.GetTable("FT_BKMCO")
                //                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                //                                row.Map(s =>
                //                                    from unit in s.SetField("ESTATE", bkm.PLANT.ToString())
                //                                    from period in s.SetField("TRANS_DATE", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                //                                    from pms_doc_no in s.SetField("NO_BKM", bkm.PMS_DOC_NO)
                //                                    from cancel in s.SetField("IS_CANCEL", "X")
                //                                    select pms_doc_no
                //                                ))).Traverse(l => l).Map(u => dc))),
                //                Output:
                //                    func => func.MapTable("FT_RETURN", s =>
                //                        from stat in s.GetField<string>("PSTG_STAT")
                //                        from docno in s.GetField<string>("DOC_NUMBER")
                //                        from msg in s.GetField<string>("PSTG_MSG")
                //                        select (stat, docno, msg)))

                //                .ToAsync().Match(r =>
                //                {
                //                    foreach (var (stat, docno, msg) in r)
                //                    {
                //                        var ftr = new FTRETURN();
                //                        ftr.STAT = stat;
                //                        ftr.DOCNO = docno;
                //                        ftr.MSG = msg;
                //                        FT.Add(ftr);
                //                    }
                //                }, l => { Ex(l.Message); });
                //}
            }

            return FT;

        }

        public async Task<List<FTRETURN>> UploadHarvesting(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BP_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFMBUKUPANENUPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFPS")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("BP_NO", bkm.PMS_DOC_NO)
                                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from cancel in s.SetField("CANCEL", "Y")
                                                    from unit in s.SetField("PLANT", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("DIVISION", bkm.DIV)
                                                    from emptype in s.SetField("EMPL_TYPE", bkm.EMPTYPE)
                                                    from act in s.SetField("ACTTYPE", bkm.ACT)
                                                    from QTY in s.SetField("ENTRY_QNT", bkm.QTY)
                                                    from block in s.SetField("BLOCK", bkm.BLOCK)
                                                    from text in s.SetField("TEXT", string.Empty)
                                                    from borongan in s.SetField("BORONGAN", bkm.BOR)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BP_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFMBUKUPANENUPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFCO")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from unit in s.SetField("ESTATE", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("DIVISION", bkm.DIV)
                                                    from block in s.SetField("BLOCKID", bkm.BLOCK)
                                                    from period in s.SetField("PERIO", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from qty in s.SetField("QTYPANEN", bkm.QTY)
                                                    from pms_doc_no in s.SetField("BUKUPANEN", bkm.PMS_DOC_NO)
                                                    from act in s.SetField("ACTIVITY", bkm.ACT)
                                                    from cancel in s.SetField("IS_CANCEL", "Y")
                                                    from area in s.SetField("HVAREA", bkm.AREA)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;

        }

        public async Task<List<FTRETURN>> CancelUploadHarvesting(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BP_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFMBUKUPANENUPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFPS")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("BP_NO", bkm.PMS_DOC_NO)
                                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from cancel in s.SetField("CANCEL", "X")
                                                    from unit in s.SetField("PLANT", bkm.PLANT.ToString())
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BP_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFMBUKUPANENUPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFCO")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from unit in s.SetField("ESTATE", bkm.PLANT.ToString())
                                                    from period in s.SetField("PERIO", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from qty in s.SetField("QTYPANEN", "0")
                                                    from pms_doc_no in s.SetField("BUKUPANEN", bkm.PMS_DOC_NO)
                                                    from cancel in s.SetField("IS_CANCEL", "X")
                                                    from area in s.SetField("HVAREA", "0")
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;

        }

        public async Task<List<FTRETURN>> UploadHarvestingPlasma(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BPP_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFPS")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("BKM_NO", bkm.PMS_DOC_NO)
                                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from cancel in s.SetField("CANCEL", string.Empty)
                                                    from unit in s.SetField("PLANT", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("DIVISION", bkm.DIV)
                                                    from emptype in s.SetField("EMPL_TYPE", bkm.EMPTYPE)
                                                    from act in s.SetField("ACTTYPE", bkm.ACT)
                                                    from QTY in s.SetField("ENTRY_QNT", bkm.QTY)
                                                    from block in s.SetField("BLOCK", bkm.BLOCK)
                                                    from text in s.SetField("TEXT", string.Empty)
                                                    from borongan in s.SetField("BORONGAN", string.Empty)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BPP_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFCO")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from unit in s.SetField("GSBER", bkm.PLANT.ToString())
                                                    from DIV in s.SetField("ZDIVI", bkm.DIV)
                                                    from block in s.SetField("ZBLOCK", bkm.BLOCK)
                                                    from period in s.SetField("BUDAT", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from qty in s.SetField("ZHARVQ", bkm.QTY)
                                                    from pms_doc_no in s.SetField("ZHARVB", bkm.PMS_DOC_NO)
                                                    from act in s.SetField("NOACT", bkm.ACT)
                                                    from cancel in s.SetField("ZBTISC", string.Empty)
                                                    from area in s.SetField("ZHARVAR", bkm.AREA)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;

        }

        public async Task<List<FTRETURN>> CancelUploadHarvestingPlasma(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "BPP_PS")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFPS")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("BKM_NO", bkm.PMS_DOC_NO)
                                                    from TRANS_DATE in s.SetField("TRANS_DATE", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from cancel in s.SetField("CANCEL", "X")
                                                    from unit in s.SetField("PLANT", bkm.PLANT.ToString())
                                                    from qty in s.SetField("ENTRY_QNT", "0")
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            else if (!string.IsNullOrWhiteSpace(type) && type == "BPP_CO")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_PMSP_UPLOAD2",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SKFCO")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from unit in s.SetField("GSBER", bkm.PLANT.ToString())
                                                    from period in s.SetField("BUDAT", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from qty in s.SetField("ZHARVQ", "0")
                                                    from pms_doc_no in s.SetField("ZHARVB", bkm.PMS_DOC_NO)
                                                    from cancel in s.SetField("ZBTISC", "X")
                                                    from area in s.SetField("ZHARVAR", "0")
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }
            }

            return FT;

        }

        public async Task<List<FTRETURN>> UploadSPB(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "SPB")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_SPB_UPLOAD",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SPB")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("SPBNUM", bkm.PMS_DOC_NO)
                                                    from item in s.SetField("ITEM", bkm.ITEMID)
                                                    from tahun in s.SetField("MJAHR", bkm.YEAR)
                                                    from TRANS_DATE in s.SetField("BUDAT", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from time in s.SetField("UZEIT", bkm.TIME.Value.ToString("HHMMSS"))
                                                    from gmind in s.SetField("GMIND", string.Empty)
                                                    from splant in s.SetField("SPLANT", bkm.SPLANT)
                                                    from rplant in s.SetField("RPLANT", bkm.RPLANT)
                                                    from vhcid in s.SetField("VHCID", bkm.VEH)
                                                    from div in s.SetField("DIV", bkm.DIV)
                                                    from driver in s.SetField("DRIVER", bkm.DRIVER)
                                                    from block in s.SetField("BLOCKID", bkm.BLOCK)
                                                    from mat in s.SetField("MATNR", "TBS")
                                                    from jjg in s.SetField("QTYJ", bkm.JJG)
                                                    from jjgkg in s.SetField("ESTJ", bkm.JJGKG)
                                                    from brd in s.SetField("ESTB", bkm.BRD)
                                                    from nokrt in s.SetField("NOKRTTB", string.Empty)
                                                    from qty in s.SetField("QTY", bkm.QTY)
                                                    from flag in s.SetField("FLGSPB", "0")
                                                    from state in s.SetField("STATE", "N")
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            
            return FT;

        }

        public async Task<List<FTRETURN>> CancelUploadSPB(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "SPB")
            {
                using (var context = SAPContext())
                {
                    var rfc = await context.CallFunction("ZFM_SPB_CANCEL",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("FT_SPB")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("SPBNUM", bkm.PMS_DOC_NO)
                                                    from TRANS_DATE in s.SetField("BUDAT", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from splant in s.SetField("SPLANT", bkm.SPLANT.ToString())
                                                    from rplant in s.SetField("RPLANT", bkm.RPLANT.ToString())
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                }

            }
            
            return FT;

        }

        public async Task<List<FTRETURN>> UploadCarLog(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "PM")
            {
                using (var context = SAPContext())
                {
                    var equipLoc = bkm.EQUIPLOC.ToString();
                    var workLoc = bkm.WORKLOC.ToString();
                    //var isOrder = DataHelper.GetBoolean(bkm.ORDER_);

                    bool isPinjam = false;
                    if (equipLoc != workLoc)
                    {
                        isPinjam = true;
                    }

                    var costCtr = bkm.COSTCTR;
                    var wbs = bkm.WBS;
                    string order2 = string.Empty;
                    var act = bkm.ACT;
                    if (!string.IsNullOrEmpty(bkm.ORDER_))
                    {
                        costCtr = string.Empty;
                        wbs = string.Empty;
                        order2 = bkm.COSTCTR.ToString();
                        isPinjam = false;
                    }
                    else if (isPinjam)
                    {
                        costCtr = string.Empty;
                        wbs = string.Empty;
                        order2 = bkm.ORDER2.ToString();
                        act = bkm.ACT2;
                    }

                    if(!isPinjam)
                    {
                        var rfc = await context.CallFunction("ZFMPM_IMPORT_PMS",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("PMS_DOCUMENT")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("PMS_DOC_NO", bkm.PMS_DOC_NO)
                                                    from item in s.SetField("PMS_DOC_ITEM", bkm.ITEMID)
                                                    from TRANS_DATE in s.SetField("POSTING_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from time in s.SetField("EQUIPMENT", bkm.EQUIP)
                                                    from gmind in s.SetField("VRA_MAINTENANCE_ORDER", bkm.ORDER1)
                                                    from splant in s.SetField("OPERATOR_ID", bkm.OPRID)
                                                    from rplant in s.SetField("COST_CENTER", costCtr)
                                                    from vhcid in s.SetField("WBS", wbs)
                                                    from div in s.SetField("ORDER", order2)
                                                    from driver in s.SetField("ACTIVITY_TYPE", act)
                                                    from block in s.SetField("START_TIME", bkm.TIMESTART.Value.ToString("HH:mm:ss"))
                                                    from mat in s.SetField("END_TIME", bkm.TIMEEND.Value.ToString("HH:mm:ss"))
                                                    from jjg in s.SetField("START_ACTIVITY", bkm.KMSTART)
                                                    from jjgkg in s.SetField("END_ACTIVITY", bkm.KMEND)
                                                    from brd in s.SetField("ACTIVITY_QTY", bkm.VOL)
                                                    from nokrt in s.SetField("UNIT_ACTIVITY", bkm.UOM)
                                                    from qty in s.SetField("PMS_FLAG_CANCEL", string.Empty)
                                                    from flag in s.SetField("CANCEL_DATE", string.Empty)
                                                    from state in s.SetField("REMARKS", bkm.NOTE)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                    }
                    else
                    {
                        var rfc = await context.CallFunction("ZFMPM_IMPORT_PMS",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("PMS_DOCUMENT")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("PMS_DOC_NO", bkm.PMS_DOC_NO)
                                                    from item in s.SetField("PMS_DOC_ITEM", bkm.ITEMID + "A")
                                                    from TRANS_DATE in s.SetField("POSTING_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from time in s.SetField("EQUIPMENT", bkm.EQUIP)
                                                    from gmind in s.SetField("VRA_MAINTENANCE_ORDER", bkm.ORDER2)
                                                    from splant in s.SetField("OPERATOR_ID", bkm.OPRID)
                                                    from rplant in s.SetField("COST_CENTER", bkm.COSTCTR)
                                                    from vhcid in s.SetField("WBS", bkm.WBS)
                                                    from div in s.SetField("ORDER", string.Empty)
                                                    from driver in s.SetField("ACTIVITY_TYPE", bkm.ACT)
                                                    from block in s.SetField("START_TIME", bkm.TIMESTART.Value.ToString("HH:mm:ss"))
                                                    from mat in s.SetField("END_TIME", bkm.TIMEEND.Value.ToString("HH:mm:ss"))
                                                    from jjg in s.SetField("START_ACTIVITY", bkm.KMSTART)
                                                    from jjgkg in s.SetField("END_ACTIVITY", bkm.KMEND)
                                                    from brd in s.SetField("ACTIVITY_QTY", bkm.VOL)
                                                    from nokrt in s.SetField("UNIT_ACTIVITY", bkm.UOM)
                                                    from qty in s.SetField("PMS_FLAG_CANCEL", string.Empty)
                                                    from flag in s.SetField("CANCEL_DATE", string.Empty)
                                                    from state in s.SetField("REMARKS", bkm.NOTE)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                    }
                    
                }

            }

            return FT;

        }

        public async Task<List<FTRETURN>> CancelUploadCarLog(SAPPOSTING bkm, string type, string uname)
        {
            bkm = CheckPropertiesString(bkm);
            var FT = new List<FTRETURN>();
            if (!string.IsNullOrWhiteSpace(type) && type == "PM")
            {
                using (var context = SAPContext())
                {
                    var equipLoc = bkm.EQUIPLOC.ToString();
                    var workLoc = bkm.WORKLOC.ToString();

                    bool isPinjam = false;
                    if (equipLoc != workLoc)
                    {
                        isPinjam = true;
                    }

                    var costCtr = bkm.COSTCTR;
                    var wbs = bkm.WBS;
                    string order2 = string.Empty;
                    var act = bkm.ACT;
                    if (isPinjam)
                    {
                        costCtr = string.Empty;
                        wbs = string.Empty;
                        order2 = bkm.ORDER2.ToString();
                        act = bkm.ACT2;
                    }

                    if (!isPinjam)
                    {
                        var rfc = await context.CallFunction("ZFMPM_IMPORT_PMS",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("PMS_DOCUMENT")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("PMS_DOC_NO", bkm.PMS_DOC_NO)
                                                    from item in s.SetField("PMS_DOC_ITEM", bkm.ITEMID)
                                                    from TRANS_DATE in s.SetField("POSTING_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from time in s.SetField("EQUIPMENT", bkm.EQUIP)
                                                    from gmind in s.SetField("VRA_MAINTENANCE_ORDER", bkm.ORDER1)
                                                    from splant in s.SetField("OPERATOR_ID", bkm.OPRID)
                                                    from rplant in s.SetField("COST_CENTER", costCtr)
                                                    from vhcid in s.SetField("WBS", wbs)
                                                    from div in s.SetField("ORDER", order2)
                                                    from driver in s.SetField("ACTIVITY_TYPE", act)
                                                    from block in s.SetField("START_TIME", bkm.TIMESTART.Value.ToString("HH:mm:ss"))
                                                    from mat in s.SetField("END_TIME", bkm.TIMEEND.Value.ToString("HH:mm:ss"))
                                                    from jjg in s.SetField("START_ACTIVITY", bkm.KMSTART)
                                                    from jjgkg in s.SetField("END_ACTIVITY", bkm.KMEND)
                                                    from brd in s.SetField("ACTIVITY_QTY", bkm.VOL)
                                                    from nokrt in s.SetField("UNIT_ACTIVITY", bkm.UOM)
                                                    from qty in s.SetField("PMS_FLAG_CANCEL", "X")
                                                    from flag in s.SetField("CANCEL_DATE", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from state in s.SetField("REMARKS", bkm.NOTE)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                    }
                    else
                    {
                        var rfc = await context.CallFunction("ZFMPM_IMPORT_PMS",
                                Input: I => I
                                        .SetField("FI_INTERFACE_FLAG", "X")
                                        .BindAsync(dc => dc.GetTable("PMS_DOCUMENT")
                                            .Use(used => used.Map(table => table.AppendRow().Apply(row =>
                                                row.Map(s =>
                                                    from pms_doc_no in s.SetField("PMS_DOC_NO", bkm.PMS_DOC_NO)
                                                    from item in s.SetField("PMS_DOC_ITEM", bkm.ITEMID + "A")
                                                    from TRANS_DATE in s.SetField("POSTING_DATE", bkm.TRANS_DATE.Value.ToString("yyyyMMdd"))
                                                    from time in s.SetField("EQUIPMENT", bkm.EQUIP)
                                                    from gmind in s.SetField("VRA_MAINTENANCE_ORDER", bkm.ORDER2)
                                                    from splant in s.SetField("OPERATOR_ID", bkm.OPRID)
                                                    from rplant in s.SetField("COST_CENTER", bkm.COSTCTR)
                                                    from vhcid in s.SetField("WBS", bkm.WBS)
                                                    from div in s.SetField("ORDER", string.Empty)
                                                    from driver in s.SetField("ACTIVITY_TYPE", bkm.ACT)
                                                    from block in s.SetField("START_TIME", bkm.TIMESTART.Value.ToString("HH:mm:ss"))
                                                    from mat in s.SetField("END_TIME", bkm.TIMEEND.Value.ToString("HH:mm:ss"))
                                                    from jjg in s.SetField("START_ACTIVITY", bkm.KMSTART)
                                                    from jjgkg in s.SetField("END_ACTIVITY", bkm.KMEND)
                                                    from brd in s.SetField("ACTIVITY_QTY", bkm.VOL)
                                                    from nokrt in s.SetField("UNIT_ACTIVITY", bkm.UOM)
                                                    from qty in s.SetField("PMS_FLAG_CANCEL", "X")
                                                    from flag in s.SetField("CANCEL_DATE", bkm.CANCEL_DATE.Value.ToString("yyyyMMdd"))
                                                    from state in s.SetField("REMARKS", bkm.NOTE)
                                                    select pms_doc_no
                                                ))).Traverse(l => l).Map(u => dc))),
                                Output:
                                    func => func.MapTable("FT_RETURN", s =>
                                        from stat in s.GetField<string>("PSTG_STAT")
                                        from docno in s.GetField<string>("DOC_NUMBER")
                                        from msg in s.GetField<string>("PSTG_MSG")
                                        select (stat, docno, msg)))

                                .ToAsync().Match(r =>
                                {
                                    foreach (var (stat, docno, msg) in r)
                                    {
                                        var ftr = new FTRETURN();
                                        ftr.STAT = stat;
                                        ftr.DOCNO = docno;
                                        ftr.MSG = msg;
                                        FT.Add(ftr);
                                    }
                                }, l => { Ex(l.Message); });
                    }

                }

            }

            return FT;

        }

        private SAPPOSTING CheckPropertiesString(SAPPOSTING trx)
        {
            //var myObj = new SAPPOSTING();
            foreach(var propertyInfo in trx.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    if (propertyInfo.GetValue(trx, null) == null)
                    {
                        propertyInfo.SetValue(trx, string.Empty, null);
                    }
                }
            }

            return trx;
        }

        private void Ex(string message)
        {
            throw new NotImplementedException();
        }
    }
}

public class FTRETURN
{
    public string STAT { get; set; }
    public string MSG { get; set; }
    public string DOCNO { get; set; }

    public string String { get; set; }

}
