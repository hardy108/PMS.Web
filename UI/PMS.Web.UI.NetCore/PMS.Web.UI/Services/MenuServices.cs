using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMS.Shared.Services;
using PMS.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace PMS.Web.UI.Services
{

    public interface IHttpMenuServices
    {

        List<Menu> GetMenus(string parentId);
        List<Menu> GetRootMenus();
        Menu GetMenu(string menuId);
        List<ListField> GetListFields(string menuId);
        List<FTColumn> GetFTColumns(string menuId);
        List<DTColumn> GetDTColumns(string menuId);
        
        Menu GetMenuAccess(string menuId, UserAccess userAccess);
        List<Menu> GetAllMenus();
        string GetMenuHtmlLinks();
        string GetMenuHtmlLinks(List<string> authorizedMenuIds);
    }
    public class HttpMenuServices: IHttpMenuServices
    {

        MenuServices _service;
        public HttpMenuServices(IHostingEnvironment hostingEnvirontment, IOptions<AppSetting> appSettings)
        {
            string menuFile = hostingEnvirontment.ContentRootPath + "/" + appSettings.Value.MenuJsonFile,
                   dataListFolder = hostingEnvirontment.ContentRootPath + appSettings.Value.ListFieldsFolder,
                   filterFolder = hostingEnvirontment.ContentRootPath + appSettings.Value.FilterFolder;

            _service = new MenuServices(menuFile, dataListFolder, filterFolder);
        }

        public List<Menu> GetMenus(string parentId)
        {
            return _service.GetMenus(parentId);
        }
        public List<Menu> GetRootMenus()
        {
            return _service.GetRootMenus();
        }
        public Menu GetMenu(string menuId)
        {
            return _service.GetMenu(menuId);
        }
        public List<ListField> GetListFields(string menuId)
        {
            return _service.GetListFields(menuId);
        }

        public List<FTColumn> GetFTColumns(string menuId)
        {
            return _service.GetFTColumns(menuId);
        }
        public List<DTColumn> GetDTColumns(string menuId)
        {
            return _service.GetDTColumns(menuId);
        }


        public Menu GetMenuAccess(string menuId, UserAccess userAccess)
        {
            return _service.GetMenuAccess(menuId, userAccess);
        }


        public List<Menu> GetAllMenus()
        {
            return _service.GetAllMenus();
        }
        public string GetMenuHtmlLinks()
        {
            return GetMenuHtmlLinks(new List<string>());
        }
        public string GetMenuHtmlLinks(List<string> authorizedMenuIds)
        {

            var _menus = _service.GetAllMenus();

            string html = string.Empty;
            List<Menu> webRootMenus = _menus
                    .Where(d => string.IsNullOrWhiteSpace(d.ParentID))
                    .ToList();
            foreach (Menu webRootMenu in webRootMenus)
            {
                html += GetMenuHtmlLinks(webRootMenu, authorizedMenuIds);
            }

            html = "<li class='treeview menu-open active'>" +
                     "<a href='#'>" +
                     "<i class='fa fa-tasks'></i><span>Authorized Access</span>" +
                     "<span class='pull-right-container'>" +
                     "<i class='fa fa-angle-left pull-right'></i>" +
                     "</span>" +
                     "</a>" +
                     "<ul class='treeview-menu'>" +
                     html +
                     "</ul>" +
                     "</li>";
            return html;
        }




        private string GetMenuHtmlLinks(Menu root, List<string> authorizedMenuIds)
        {
            string html = string.Empty;
            if (root.IsHidden)
                return string.Empty;

            var _menus = _service.GetAllMenus();
            List<Menu> chidren = _menus
                .Where(d => !string.IsNullOrWhiteSpace(d.ParentID) && d.ParentID.Equals(root.MenuID))
                .ToList();
            if (chidren.Any())
            {
                //Menu Parent
                string htmlChildenMenu = string.Empty;
                foreach (Menu child in chidren)
                {
                    htmlChildenMenu += GetMenuHtmlLinks(child, authorizedMenuIds);
                }

                if (string.IsNullOrWhiteSpace(htmlChildenMenu))
                    return string.Empty;

                html = "<li class='treeview'>" +
                    "<a href='#'>" +
                    "<i class='fa fa-link'></i><span>" + root.MenuName + "</span>" +
                    "<span class='pull-right-container'>" +
                    "<i class='fa fa-angle-left pull-right'></i>" +
                    "</span>" +
                    "</a>" +
                    "<ul class='treeview-menu'>" +
                    htmlChildenMenu +
                    "</ul>" +
                    "</li>";

            }
            else
            {
                //Lowest Level Menu
                if ((authorizedMenuIds.Any() && authorizedMenuIds.Contains(root.AMPermission)) || !root.NeedAUthentication)
                {
                    string url = root.Url;
                    string icon = "link";
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        url = "/Menu/Open/" + root.MenuID;
                        if (!string.IsNullOrWhiteSpace(root.ListID))
                            url = "/Menu/Open/" + root.MenuID + "/List";
                    }
                    if (!string.IsNullOrWhiteSpace(root.Icon))
                        icon = root.Icon;

                    html = "<li id='" + root.MenuID + "'>" +
                                "<a href='" + url + "'>" +
                                "<i class='fa fa-" + icon + "'></i><span>" + root.MenuName + "</span>" +
                                "</a>" +
                           "</li>";
                }
            }
            return html;

        }
    }
}
