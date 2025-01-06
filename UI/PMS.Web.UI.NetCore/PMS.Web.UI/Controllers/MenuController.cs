using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PMS.Web.UI.Code;
using Microsoft.Extensions.Options;
using System.IO;
using PMS.Shared.Models;
using PMS.Web.UI.Services;
using Newtonsoft.Json;
using PMS.Shared.Utilities;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using PMS.Shared.Services;
using System.Threading.Tasks;

namespace PMS.Web.UI.Controllers
{
    public class MenuController : Controller
    {
        readonly IOptions<AppSetting> _config;

        public MenuController(IOptions<AppSetting> config)
        {
            _config = config;
        }

        

        private async System.Threading.Tasks.Task<string> GetMenuInfoStringAsync(string menuId)
        {
            string menuInfoApi = _config.Value.WebApiUrl + "api/login/useraccess?MenuID=" + menuId;
            RestClient restClient = new RestClient(menuInfoApi, Request.Cookies["BearerToken"]);
            return (await restClient.GetStringAsync());            
        }

    
        private async System.Threading.Tasks.Task<string> GetRecordStringAsync(string apiHost, string apiUrl, string key)
        {
            if (string.IsNullOrWhiteSpace(apiHost))
                apiHost = _config.Value.WebApiUrl;
            apiUrl = $"{apiHost}{apiUrl.Replace("{Id}",key)}";
            RestClient restClient = new RestClient(apiUrl, Request.Cookies["BearerToken"]);
            return (await restClient.GetStringAsync());
        }

        private async System.Threading.Tasks.Task<string> GetNewRecordStringAsync(string apiHost,string apiUrlMain)
        {
            if (string.IsNullOrWhiteSpace(apiHost))
                apiHost = _config.Value.WebApiUrl;
            string apiUrl = $"{apiHost}{apiUrlMain}/initnew";
            RestClient restClient = new RestClient(apiUrl, Request.Cookies["BearerToken"]);
            return (await restClient.GetStringAsync());
        }

        

        

        public ActionResult Index(string menuId)
        {
            return RedirectToRoute("Default");
        }


        public async System.Threading.Tasks.Task<ActionResult> Open(string menuId, string mode, string recordId,[FromServices]IHttpMenuServices menuServices)
        {
            if (string.IsNullOrWhiteSpace(mode))
                mode = string.Empty;

            Menu menuInfo = new Menu();
            menuInfo.CopyFrom(menuServices.GetMenu(menuId));
            if (menuInfo == null)
            {
                ViewBag.Error = "You are not authorized";
                return Error();
            }

            if (menuInfo.NeedAUthentication)
            {
                string menuString = string.Empty;
                try
                {
                    menuString = await GetMenuInfoStringAsync(menuInfo.AMPermission);
                }
                catch (ExceptionInvalidToken ex)
                {

                    return RedirectToLogin();
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return Error();
                }

                if (string.IsNullOrWhiteSpace(menuString))
                {
                    ViewBag.Error = "Anda tidak memiliki hak akses";
                    return Error();
                }
                List<UserAccess> userAccesses = menuString.Deserialize<List<UserAccess>>();
                if (userAccesses == null || !userAccesses.Any() )
                {
                    ViewBag.Error = "Anda tidak memiliki hak akses";
                    return Error();
                }
                if (!userAccesses[0].ACTIVE)
                {
                    ViewBag.Error = "Menu tidak aktif";
                    return Error();
                }

                menuInfo.ShowAdd = menuInfo.ShowAdd && userAccesses[0].FADD;
                menuInfo.ShowEdit = menuInfo.ShowEdit && userAccesses[0].FEDIT;
                menuInfo.ShowDelete = menuInfo.ShowDelete && userAccesses[0].FDEL;
                menuInfo.ShowApproval = menuInfo.ShowApproval && userAccesses[0].FAPPR;
                menuInfo.NeedApproval = menuInfo.NeedApproval && userAccesses[0].FAPPR;
                menuInfo.ShowPost = menuInfo.ShowPost && userAccesses[0].FAPPR;
                menuInfo.ShowUnpost = menuInfo.ShowUnpost && userAccesses[0].FCANCEL;

                switch (mode.ToLower())                
                { 
                    case "new":
                        if (!menuInfo.ShowAdd)
                        {
                            ViewBag.Error = "Anda tidak memiliki hak akses insert";
                            return Error();
                        }
                        break;
                    case "edit":
                        if (!menuInfo.ShowEdit)
                        {
                            ViewBag.Error = "Anda tidak memiliki hak akses edit";
                            return Error();
                        }
                        break;
                    case "delete":
                        if (!menuInfo.ShowDelete)
                        {
                            ViewBag.Error = "Anda tidak memiliki hak akses delete";
                            return Error();
                        }
                        break;
                    case "post":
                        if (!menuInfo.ShowPost)
                        {
                            ViewBag.Error = "Anda tidak memiliki hak akses approve";
                            return Error();
                        }
                        break;
                    case "unpost":
                        if (!menuInfo.ShowUnpost)
                        {
                            ViewBag.Error = "Anda tidak memiliki hak akses cancel";
                            return Error();
                        }
                        break;
                    case "approval":
                        if (!userAccesses[0].FAPPR)
                        {
                            ViewBag.Error = "Anda tidak memiliki hak akses approval";
                            return Error();
                        }
                        break;


                }
            }

            //ViewBag.MenuAccess = menuString;

            ViewBag.ApiUrl = menuInfo.ApiUrlMain;
            ViewBag.ApiHost = string.IsNullOrWhiteSpace(menuInfo.ApiHost) ? string.Empty : menuInfo.ApiHost;
            string dataString = string.Empty;
            if (!string.IsNullOrWhiteSpace(mode))
            {
                ViewBag.Layout = menuInfo.Layout;//Temporary
                ViewBag.MainFormID = $"Form_{menuId}";
                ViewBag.Prefix = $"Form_{menuId}";
                string keyFields = string.Empty;
                if (menuInfo.KeyFields != null && menuInfo.KeyFields.Any())
                {
                    menuInfo.KeyFields.ForEach(d =>
                    {
                        keyFields += "'" + d + "',";
                    });
                    keyFields = "[" + keyFields.Substring(0, keyFields.Length - 1) + "]";
                }
                else keyFields = "[]";

                ViewBag.KeyFields = keyFields;
                

                if (mode.Equals("List"))
                {
                    ViewBag.ShowAdd = menuInfo.ShowAdd;
                    ViewBag.FilterID = "Filter_" + menuInfo.MenuID;

                    ViewBag.HasFilter = (!string.IsNullOrWhiteSpace(menuInfo.ViewFilterName) || (menuInfo.FilterRows != null && menuInfo.FilterRows.Any()));
                    string mandatoryFilterElementIds = string.Empty;
                    ViewBag.Prefix = "List_" + menuInfo.MenuID;
                    if (menuInfo.MandatoryFilterItems != null && menuInfo.MandatoryFilterItems.Any())
                    {
                        menuInfo.MandatoryFilterItems.ForEach(d =>
                        {
                            mandatoryFilterElementIds += "'" + d + "',";
                        });
                        mandatoryFilterElementIds = "[" + mandatoryFilterElementIds.Substring(0, mandatoryFilterElementIds.Length - 1) + "]";
                    }
                    else
                        mandatoryFilterElementIds = "[]";
                    ViewBag.MandatoryFilterItems = mandatoryFilterElementIds;

                    ViewBag.ApiUrl = menuInfo.ApiUrlList;
                    ViewBag.ApiUrlCount = menuInfo.ApiUrlListCount;
                    
                    ViewBag.FilterValidation = menuInfo.FilterValidation;

                    if (menuInfo.FilterRows != null && menuInfo.FilterRows.Any())
                    {
                        menuInfo.FilterRows.ForEach(d =>
                        {
                            d.ForEach(c =>
                            {
                                if (c.Ascendants != null && c.Ascendants.Any())
                                {
                                    c.Ascendants.ForEach(f => { f.ElementID = ViewBag.Prefix + f.ElementID; });
                                }
                            });
                        });
                    }

                    ViewBag.FilterRows = menuInfo.FilterRows;
                    ViewBag.ListColumns = menuServices.GetFTColumns(menuId).SerializeList<FTColumn>();


                    bool showView = false;

                    var rowActionName = string.Empty;
                    if (menuInfo.ShowEdit)
                    {
                        rowActionName += "<span style='margin-left:5px;'><a href='#' onclick='doEdit($(this))'><i class='fa fa-pencil'></i></a></span>";
                        showView = true;
                    }
                    if (menuInfo.ShowDelete)
                    {
                        rowActionName += "<span style='margin-left:5px;'><a href='#' onclick='doDelete($(this))'><i class='fa fa-trash'></i></a></span>";
                        showView = true;
                    }
                    if (menuInfo.ShowPost)
                    {
                        rowActionName += "<span style='margin-left:5px;'><a href='#' onclick='doPost($(this))'><i class='fa fa-lock'></i></a></span>";
                        showView = true;
                    }
                    if (menuInfo.ShowUnpost)
                    {
                        rowActionName += "<span style='margin-left:5px;'><a href='#' onclick='doUnpost($(this))'><i class='fa fa-unlock'></i></a></span>";
                        showView = true;
                    }
                    if (menuInfo.ShowApproval)
                        rowActionName += "<span style='margin-left:5px;'><a href='#' onclick='doApproval($(this))'><i class='fa fa-check-square'></i></a></span>";
                    else
                        rowActionName = "<span style='margin-left:5px;'><a href='#' onclick='doDisplay($(this))'><i class='fa fa-eye'></i></a></span>" + rowActionName;
                    ViewBag.RowAction = rowActionName;
                }
                else if (mode.Equals("New"))
                {
                    if (menuInfo.ShowAdd)
                    {
                        try
                        {
                            dataString = await GetNewRecordStringAsync(menuInfo.ApiHost, menuInfo.ApiUrlMain);
                            ViewBag.Key = string.Empty;
                        }
                        catch (ExceptionInvalidToken ex)
                        {

                            return RedirectToLogin();
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Error = ex.Message;
                            return Error();
                        }

                    }
                    else
                    {
                        ViewBag.Error = "Anda tidak memiliki hak akses untuk insert record baru";
                        return Error();
                    }
                }
                else if (mode.Equals("Approval"))
                {
                    if (string.IsNullOrWhiteSpace(recordId))
                    {
                        ViewBag.Error = "Invalid record Id";
                        return Error();
                    }
                    try
                    {
                        dataString = await GetRecordStringAsync(menuInfo.ApiHost, menuInfo.GetActionUrl("getforapproval"), recordId);

                        ViewBag.Key = recordId;
                    }
                    catch (ExceptionInvalidToken ex)
                    {

                        return RedirectToLogin();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return Error();
                    }

                }
                else
                {

                    if (string.IsNullOrWhiteSpace(recordId))
                    {
                        ViewBag.Error = "Invalid record Id";
                        return Error();
                    }
                    try
                    {
                        dataString = await GetRecordStringAsync(menuInfo.ApiHost, menuInfo.GetActionUrl("get"), recordId);
                        ViewBag.Key = recordId;
                    }
                    catch (ExceptionInvalidToken ex)
                    {

                        return RedirectToLogin();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return Error();
                    }

                }

                ViewBag.Mode2 = Request.Query["Mode2"];
                ViewBag.ReadOnly = (!mode.Equals("New") && !mode.Equals("Edit"));
                ViewBag.ReadOnlyWhenNotInsert = !mode.Equals("New");

                ViewBag.JsonId = menuInfo.ControllerName;

                if (string.IsNullOrWhiteSpace(dataString))
                    dataString = string.Empty;
                else
                {
                    dataString = dataString.Replace("'", "\\'")// Escape special chars
                        .Replace("\"", "\\\"");// Escape single quotation mark with backslash
                        //.Replace("\n", "\\n")
                        //.Replace("\r", "\\r")
                        //.Replace("\t", "\\t");
                }

                ViewBag.DataString = dataString;
            }
            else mode = string.Empty;
            ViewBag.Menu = menuInfo;
            ViewBag.Mode = mode;                        
            

            string viewFolderPath = string.Empty;
            if (!string.IsNullOrWhiteSpace(menuInfo.ViewFolderPath))
                viewFolderPath = $"{menuInfo.ViewFolderPath}/";
            ViewBag.ViewFolderPath = viewFolderPath;
            ViewBag.Reference = Request.Query["Ref"];

            ViewBag.ViewSource = $"{viewFolderPath}{menuInfo.ControllerName}.cshtml";
            if (string.IsNullOrWhiteSpace(mode))
                return View("Form");
            if (mode.Equals("List"))
                return View("List");
            if (menuInfo.Layout=="V1")
                return View("FormDisplayV1");
            return View("FormDisplayV2");
        }

        
       
        

       

        public ActionResult DoketGalery()
        {
            return View();
        }

        public ActionResult DownloadSPB()
        {
            return View();
        }

        public ActionResult CloseBook()
        {
            return View();
        }

        public ActionResult ProcessDaily()
        {
            return View();
        }

        //Custom Action

        public ActionResult Error()
        {
            return View("/Views/Error/InternalServerError.cshtml");
        }

        public ActionResult RedirectToLogin()
        {
            return Redirect("/Account/Login?returnUrl=" + Request.GetUri().PathAndQuery);
        }

        public async System.Threading.Tasks.Task<ActionResult> Link([FromServices]IHttpMenuServices menuServices,[FromServices]IHttpReportServices reportServices) 
        {
            string authorizedMenuApi = _config.Value.WebApiUrl + "api/login/useraccess";                        
            RestClient restClient = new RestClient(authorizedMenuApi, Request.Cookies["BearerToken"]);
            string response = await restClient.GetStringAsync();

            List<UserAccess> userAccesses = response.Deserialize<List<UserAccess>>();
            var checkMenuAuthIds = menuServices.GetAllMenus().Where(d => d.NeedAUthentication).Select(d => d.AMPermission).Distinct().ToList();
            if (userAccesses != null && checkMenuAuthIds != null)
            {
                var menuAccesses = userAccesses.Where(d => checkMenuAuthIds.Contains(d.MENUCODE)).ToList();
                ViewBag.Link = menuServices.GetMenuHtmlLinks(menuAccesses.Select(d => d.MENUCODE).Distinct().ToList());
            }

            var checkReportAuthIds = reportServices.GetAllReports().Where(d => d.NeedAUthentication).Select(d => d.AMPermission).Distinct().ToList();
            if (checkMenuAuthIds == null)
                checkMenuAuthIds = new List<string>();
            if (checkReportAuthIds == null)
                checkReportAuthIds = new List<string>();

            checkMenuAuthIds = checkMenuAuthIds.Union(checkReportAuthIds).ToList();
            var allUserAccess = userAccesses.Where(d => checkMenuAuthIds.Contains(d.MENUCODE)).ToList();
            if (allUserAccess == null)
                allUserAccess = new List<UserAccess>();
            ViewBag.AllUserAccess = allUserAccess;
            return View();
        }

        public ActionResult TokenProcessor([FromQuery]string token)
        {
            var claimns = JwtTokenRepository.DecodeTokenClaims(token);
            if (claimns == null || !claimns.Any())
                return RedirectToPage(_config.Value.HomePage);

            var urlClaim = claimns.SingleOrDefault(d => d.Type.StartsWith("url"));
            if (urlClaim == null || string.IsNullOrWhiteSpace(urlClaim.Value))
                return RedirectToPage(_config.Value.HomePage);
            var url = urlClaim.Value.Replace("{token}",token);            
            return Redirect(url);

        }
    }
}
