using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMS.Shared.Models
{
    public class Menu
    {
        public string MenuID { get; set; }
        public string MenuName { get; set; }

        public string Description { get; set; }
        public string ParentID { get; set; }
        public string ControllerName { get; set; }
        public string ViewFilterName { get; set; }
        public string ViewFolderPath { get; set; }
        public string FilterJson { get; set; }
        public List<string> MandatoryFilterItems { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool NeedAUthentication { get { return !string.IsNullOrWhiteSpace(AMPermission); } }
        public string AMPermission { get; set; }
        public string ListID { get; set; }
        public bool ShowPaging { get; set; }
        public bool ShowAdd { get; set; }
        public bool ShowEdit { get; set; }
        public bool ShowDelete { get; set; }
        public bool ShowPost { get; set; }
        public bool ShowUnpost { get; set; }
        public bool ShowApproval { get; set; }
        public bool NeedApproval { get; set; }
        public bool FromWebAPI { get; set; }

        public string Layout { get; set; }
        public string ApiUrlMain { get; set; }

        private string _apiUrlList = string.Empty;
        public string ApiUrlList
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_apiUrlList))
                    return _apiUrlList;
                if (string.IsNullOrWhiteSpace(ApiUrlMain))
                    return string.Empty;
                return ApiUrlMain + "/list";
            }
            set { _apiUrlList = value; }
        }

        private string _apiUrlListCount = string.Empty;
        public string ApiUrlListCount
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_apiUrlListCount))
                    return _apiUrlListCount;
                if (string.IsNullOrWhiteSpace(ApiUrlMain))
                    return string.Empty;
                return ApiUrlMain + "/listcount";
            }
            set { _apiUrlListCount = value; }
        }

        public string ConnectionName { get; set; }
        public string SourceTableName { get; set; }
        public FilterJson FilterRows { get; set; }
        public List<string> KeyFields { get; set; }
        public string FilterValidation { get; set; }

        public List<MenuUrl> ActionUrls { get; set; }

        public Menu()
        {
            ActionUrls = new List<MenuUrl>();
        }

        public string GetActionUrl(string action)
        {
            var menuUrl = ActionUrls.SingleOrDefault(d => d.Action.ToLower().Trim() == action.ToLower().Trim());
            if (menuUrl == null)
                return ApiUrlMain + "/" + action + "/{Id}";
            if (menuUrl.Url.ToLower().StartsWith("http://") || menuUrl.Url.ToLower().StartsWith("https://"))
                return menuUrl.Url;
            return ApiUrlMain + "/" + menuUrl.Url;
        }
        
        public int DisplayOrder { get; set; }

        public string ApiHost { get; set; }

        public bool IsHidden { get; set; }
    }
}

