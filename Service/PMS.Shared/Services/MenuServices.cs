using System;
using System.Collections.Generic;
using System.Text;
using PMS.Shared.Models;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Dynamic;
using System.Data;
using System.Data.Common;



namespace PMS.Shared.Services
{
   
    public class MenuServices
    {
        private List<Menu> _menus = new List<Menu>();

        string _dataListFolder = string.Empty;
        string _filterFolder = string.Empty;
        
        


        public void InitServices(string menuFile, string dataListFolder, string filterFolder)
        {
            using (StreamReader stream = System.IO.File.OpenText(menuFile))
            {
                using (JsonTextReader reader = new JsonTextReader(stream))
                {
                    var x = Newtonsoft.Json.JsonSerializer.Create();
                    _menus.AddRange(x.Deserialize<IEnumerable<Menu>>(reader));
                }
            }
            _menus = _menus.OrderBy(d => d.DisplayOrder).ToList();
            _dataListFolder = dataListFolder;
            _filterFolder = filterFolder;
        }
        public MenuServices(string menuFile,string dataListFolder, string filterFolder)
        {
            InitServices(menuFile, dataListFolder, filterFolder);
        }

        public List<Menu> AllMenus
        {
            get { return _menus; }
        }


        public List<Menu> GetAllMenus()
        {
            return AllMenus;
        }



        public List<Menu> GetMenus(string parentId)
        {
            List<Menu> menus = new List<Menu>();
            if (string.IsNullOrWhiteSpace(parentId))
                menus = _menus;
            else
                menus = _menus.Where(d => !string.IsNullOrWhiteSpace(d.ParentID) && d.ParentID.Equals(parentId)).ToList();

           
            return menus;
        }

        public List<Menu> GetRootMenus()
        {
            
            
            List<Menu>  menus = _menus.Where(d => string.IsNullOrWhiteSpace(d.ParentID)).ToList();
            
            return menus;
        }

        

        public Menu GetMenu(string menuId)
        {
            Menu menu = _menus.SingleOrDefault(d => d.MenuID.Equals(menuId));            
            menu.FilterRows = new FilterJson();
            if (!string.IsNullOrWhiteSpace(menu.FilterJson))
            {
                FilterServices filter = new FilterServices(_filterFolder + "/" + menu.FilterJson + ".json");
                menu.FilterRows = filter.AllRows;
            }
            return menu;

        }

        public Menu GetMenuAccess(string menuId, UserAccess userAccess)
        {
            Menu menu = GetMenu(menuId);
            if (menu.NeedAUthentication)
            {
                if (userAccess == null)
                    throw new Exception("Anda tidak memiliki akses ke menu ini");
                if (!userAccess.ACTIVE)
                    throw new Exception("Anda tidak memiliki akses ke menu ini");

                menu.ShowAdd &= userAccess.FADD;
                menu.ShowEdit &= userAccess.FEDIT;
                menu.ShowDelete &= userAccess.FDEL;
                menu.ShowApproval &= userAccess.FAPPR;
                menu.ShowPost &= userAccess.FAPPR;
                menu.ShowUnpost &= userAccess.FCANCEL;
            }
            return menu;
        }

        public List<ListField> GetListFields(string menuId)
        {
            List<ListField> lists = new List<ListField>();
            Menu menu = GetMenu(menuId);
            if (menu == null)
                return lists;
            string jsonFile = _dataListFolder + "/" + menu.ListID + ".json";
            return GetListFieldsFromFile(jsonFile);
        }

        public List<FTColumn> GetFTColumns(string menuId)
        {
            List<FTColumn> lists = new List<FTColumn>();
            

            Menu menu = GetMenu(menuId);
            if (menu == null)
                return lists;
            string jsonFile = _dataListFolder + "/" + menu.ListID + ".json";
            List<ListField> fields = GetListFieldsFromFile(jsonFile);

            string functionKey = KeyFunction(fields);

            lists = new List<FTColumn>();
            lists.Add(new FTColumn { name = "Key", title = "Key", visible = false});
            lists.Add(new FTColumn { type = "html", name = "Action", title = "Action", visible = true });
            fields.ForEach(d=> 
            {
                lists.Add(new FTColumn(d));
            });
            
            return lists;
        }

        public List<DTColumn> GetDTColumns(string menuId)
        {
            List<DTColumn> lists = new List<DTColumn>();
            Menu menu = GetMenu(menuId);
            if (menu == null)
                return lists;
            string jsonFile = _dataListFolder + "/" + menu.ListID + ".json";
            List<ListField> fields = GetListFieldsFromFile(jsonFile);
            lists = new List<DTColumn>();
            lists.Add(new DTColumn { name = "Key", title = "Key", visible = false });
            lists.Add(new DTColumn { type = "html", name = "Action", title = "Action", visible = true });
            
            fields.ForEach(d =>
            {
                lists.Add(new DTColumn(d));
            });
            
            return lists;
        }


        private List<ListField> GetListFieldsFromFile(string jsonFile)
        {
            using (StreamReader stream = System.IO.File.OpenText(jsonFile))
            {
                using (JsonTextReader reader = new JsonTextReader(stream))
                {
                    var x = Newtonsoft.Json.JsonSerializer.Create();
                    return x.Deserialize<IEnumerable<ListField>>(reader).ToList();
                }
            }
        }

        private string KeyFunction(List<ListField> fields)
        {
            string f = string.Empty;

            //rowData['{Name}'] + '#' +
            fields.ForEach(d =>
            {
                if (d.IsKey)
                    f += $"rowData['{d.FieldName}'] + '#' +";
            });
            if (!string.IsNullOrWhiteSpace(f))            
                f = "function(value, options, rowData){ return " + f.Substring(0, f.Length- 8) + ";}";
            
            return f;
        }

        
    }
}

