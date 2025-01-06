using System.Collections.Generic;

namespace PMS.Web.UI.Code
{
    public class HtmlInputTable : HtmlInput
    {

        private Dictionary<string, HtmlInputTableColumn> _columns = new Dictionary<string, HtmlInputTableColumn>();


        public string ApiRouteUrl { get; set; }
        private bool _allowAdd = true;
        public bool AllowAdd
        {
            set
            {
                _allowAdd = value;
            }
            get
            {
                return _allowAdd && !ReadOnly;
            }
        }

        private string _addRowCaption = string.Empty;
        public string AddRowCaption
        {
            set { _addRowCaption = value; }
            get
            {
                return AllowAdd ? (string.IsNullOrWhiteSpace(_addRowCaption) ? (string.IsNullOrWhiteSpace(Caption) ? "New Row" : "New " + Caption) : _addRowCaption) : string.Empty;
            }
        }
        private bool _allowEdit = true;
        public bool AllowEdit
        {
            set
            {
                _allowEdit = value;
            }
            get
            {
                return _allowEdit && !ReadOnly;
            }
        }
        private bool _allowDelete = true;
        public bool AllowDelete
        {
            set
            {
                _allowDelete = value;
            }
            get
            {
                return _allowDelete && !ReadOnly;
            }
        }



        private void AddColumn(HtmlInputTableColumn column)
        {
            if (string.IsNullOrWhiteSpace(column.Id))
                return;

            try
            {
                if (_columns[column.Id] != null)
                {
                    return;
                }
            }
            catch { }

            if (column.Editor != null)
            {
                column.Editor.BindingField = column.Id;
                column.Editor.Id = "inputFor_" + Id + "_" + column.Id;
                column.Editor.Caption = column.Caption;
            }
            _columns.Add(column.Id, column);
        }

        private void AddColumns(IEnumerable<HtmlInputTableColumn> columns)
        {
            if (_columns == null)
                _columns = new Dictionary<string, HtmlInputTableColumn>();

            if (columns != null)
            {
                foreach (HtmlInputTableColumn column in columns)
                {
                    AddColumn(column);
                }
            }
        }

        private List<HtmlInputTableColumn> _listColumns = new List<HtmlInputTableColumn>();
        public List<HtmlInputTableColumn> Columns
        {
            get { return _listColumns; }
            set
            {
                _listColumns = new List<HtmlInputTableColumn>();
                _listColumns.AddRange(value);
            }
        }

        private HtmlJsonFormatter _jsonFormatter = new HtmlJsonFormatter();



        public string ApiURL { get; set; }
        public string EditorHtml
        {
            get
            {
                string editorHtml = @"
                    <div class='modal fade' id='{0}_editor_modal'>                        
                        <div class='modal-dialog' role='document'>
                            <form class='modal-content form-horizontal' id='{0}_editor'>
                                <div class='modal-header'>
                                    <button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>×</span></button>
                                    <h4 class='modal-title' id='{0}_editor_title'>Add Row</h4>
                                </div>
                                <div class='modal-body'>
                                    <div class='container-fluid'>
                                        <div class='col-xs-12'>
                                            <input type='number' id='{0}_rowid' name='{0}_rowid' class='hidden' />                                    
                                            {1}
                                        </div>
                                    </div>
                                </div>
                                <div class='modal-footer'>
                                    <button type='submit' class='btn btn-primary'>Save changes</button>
                                    <button type='button' class='btn btn-default' data-dismiss='modal'>Cancel</button>
                                </div>
                            </form>
                        </div>
                    </div>";

                string _controlsHtml = string.Empty;
                foreach (string key in _columns.Keys)
                {
                    HtmlInputTableColumn column = _columns[key];
                    if (column.Editor != null)
                    {
                        column.Editor.JsonFormatter = _jsonFormatter;
                        _controlsHtml += column.Editor;
                    }
                }

                return string.Format(editorHtml, Id, _controlsHtml);
            }
        }

        public string TableHtml
        {
            get
            {
                string tableCaption1 = (string.IsNullOrWhiteSpace(Caption) 
                    ? string.Empty
                    : "<tr><th colspan='{0}' class='text-center'>{1}</th></tr>");

                string tableCaption2 = string.Empty;


                string html = "<table id='" + Id + "' " +
                                     "class='table table-bordered table-hover' " +
                                     "data-paging='true' " +
                                     (ReadOnly ? string.Empty : "data-editing-always-show='true'") + " " +
                                     "data-editing-allow-add='" + (AllowAdd ? "true" : "false") + "' " +
                                     "data-editing-allow-edit='" + (AllowEdit ? "true" : "false") + "' " +
                                     "data-editing-allow-delete='" + (AllowDelete ? "true" : "false") + "' " +
                                     (string.IsNullOrWhiteSpace(AddRowCaption) ? string.Empty : "data-editing-add-text='" + AddRowCaption + "' ") +
                                 ">" +
                               "<thead>";
                string arrKey = string.Empty;
                tableCaption2 += "<tr>";
                int colSpan = 0;
                foreach (string key in _columns.Keys)
                {
                    tableCaption2 += _columns[key];
                    if (_columns[key].IsKey)
                        arrKey += "'" + key + "',";
                    colSpan++;
                }

                if (!string.IsNullOrWhiteSpace(arrKey))
                {
                    arrKey = "[" + arrKey.Substring(0, arrKey.Length - 1) + "]";
                }
                else
                    arrKey = null;
                tableCaption2 += "<th class='text-center' data-visible=false data-name=\"key\" data-formatter=\"function(data) = {helper.footableRowKey(data, " + arrKey + ")}\">Key</th>";
                tableCaption2 += "</tr>";
                if (colSpan>0 && !string.IsNullOrWhiteSpace(Caption) )                
                    tableCaption1 = string.Format(tableCaption1, colSpan+1, Caption.ToUpper());
                

                html += tableCaption1 + tableCaption2 +  "</thead></table>";
                return html;
            }

        }

        public string JsTableColumns
        {
            get
            {
                string template = "{{name:'{0}',title:'{1}',visible:{2},breakpoints:'{3}',IsKey:{4},EditorId:'{5}',EditorReadOnlyAttr:'{6}'}}";
                string html = string.Empty;
                foreach (string key in _columns.Keys)
                {
                    HtmlInputTableColumn column = _columns[key];
                    html += string.Format(template,
                        column.Id,
                        column.Caption,
                        column.Hidden ? "false" : "true",
                        column.BreakPoints,
                        column.IsKey ? "true" : "false",
                        column.Editor == null?string.Empty : column.Editor.Id,
                        column.Editor == null?string.Empty : column.Editor.ReadOnlyAttribute) + ",";
                }

                if (string.IsNullOrWhiteSpace(html))
                    return "[]";
                else
                    return "[" + html.Substring(0, html.Length - 1) + "]";

            }
        }

        public string JsIsRowValidFunctionName
        {
            get
            {
                return (string.IsNullOrWhiteSpace(Id) ? string.Empty : Id) + "_isRowValid";
            }
        }



        public override string InitScript
        {
            get
            {
                string script = "<script>";
                script += _jsonFormatter.JsFunctionDisplay + "\r\n\r\n" + _jsonFormatter.JsFunctionSave + "\r\n\r\n";
                if (string.IsNullOrWhiteSpace(ApiRouteUrl))
                {
                    script += string.Format(@"helper.initFooTableEdit(
                        '{0}','{1}',{2},{3},{4},null,{5},{6},{7},{8});",
                        Id,
                        Caption,
                        AllowAdd ? "true" : "false",
                        AllowEdit ? "true" : "false",
                        AllowDelete ? "true" : "false",
                        JsTableColumns,
                        _jsonFormatter.JsFunctionDisplayName,
                        _jsonFormatter.JsFunctionSaveName,
                        JsIsRowValidFunctionName);
                }
                else
                {
                    script += string.Format(@"helper.initFooTableEditFromWebAPI(
                        '{0}','{1}',{2},{3},{4},'{5}',{6},{7},{8},{9});",
                        Id,
                        Caption,
                        AllowAdd ? "true" : "false",
                        AllowEdit ? "true" : "false",
                        AllowDelete ? "true" : "false",
                        ApiRouteUrl,
                        JsTableColumns,
                        _jsonFormatter.JsFunctionDisplayName,
                        _jsonFormatter.JsFunctionSaveName,
                        JsIsRowValidFunctionName);
                }
                script += "</script>";
                return script;
            }
        }

        public override string ToString()
        {
            
            if (string.IsNullOrWhiteSpace(_jsonFormatter.Id))
                _jsonFormatter.Id = Id;
            _columns = new Dictionary<string, HtmlInputTableColumn>();
            AddColumns(Columns);
            if (WithInitScript)
            {
                return TableHtml + EditorHtml + InitScript;
            }
            else
                return TableHtml;
        }

        public override string JsDisplayFromJson(string jsonObjectName)
        {
            
            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;
            string script = string.Empty;
            if (!string.IsNullOrWhiteSpace(PreDisplayScripts))
                script += PreDisplayScripts + "\r\n";
            if (UseCustomDisplayScripts)
            {
                if (!string.IsNullOrWhiteSpace(CustomDisplayScripts))
                    script += CustomDisplayScripts;
            }
            else
            {
                string keyColumnNames = string.Empty;
                foreach (string key in _columns.Keys)
                {
                    if (_columns[key].IsKey)
                        keyColumnNames += "'" + _columns[key].Id + "',";
                }
                if (string.IsNullOrWhiteSpace(keyColumnNames))
                    keyColumnNames = "var keys=null;";
                else
                    keyColumnNames = "var keys = [" + keyColumnNames.Substring(0, keyColumnNames.Length - 1) + "];";

                

                script += keyColumnNames + string.Format("helper.loadArrayToFooTable('{0}',{1},keys);", Id, jsonObjectName + "." + BindingField);
            }
                
            if (!string.IsNullOrWhiteSpace(PostDisplayScripts))
                script += PostDisplayScripts + "\r\n";
            return script;


            
        }

        public override string JsSaveToJson(string jsonObjectName)
        {
            if (string.IsNullOrWhiteSpace(jsonObjectName) || string.IsNullOrWhiteSpace(BindingField))
                return string.Empty;
            string columnNames = string.Empty;
            foreach (string key in _columns.Keys)
            {
                columnNames += "'" + _columns[key].Id + "',";
            }
            if (string.IsNullOrWhiteSpace(columnNames))
                columnNames = "var columnNames=null;";
            else
                columnNames = "var columnNames = [" + columnNames.Substring(0, columnNames.Length - 1) + "];";

            return columnNames 
                + string.Format("\r\n{1}=helper.saveArrayFromFooTable('{0}',columnNames);", Id, jsonObjectName + "." + BindingField,columnNames)
                + string.Format("\r\n{0}_COUNT = 0;\r\nif ({0}.length) {0}_COUNT={0}.length;", jsonObjectName + "." + BindingField);
        }
    }


    public class HtmlInputTableColumn
    {
        public bool IsKey { get; set; }
        public string Id { get; set; }
        public string Caption { get; set; }
        public bool Hidden { get; set; }
        HtmlInput _editor = new HtmlInput();
        public virtual HtmlInput Editor
        {
            get;
            set;
        }
        public string BreakPoints { get; set; }
        //public IHtmlInputTableColumnType ColumnType {get;set;}

        public override string ToString()
        {
            string th = "<th class='text-center' ";
            if (Hidden)
                th += " data-visible=false";
            if (!string.IsNullOrWhiteSpace(BreakPoints))
                th += " data-breakpoints='" + BreakPoints + "'";
            if (Editor == null)
            {
                if (!string.IsNullOrWhiteSpace(Id))
                    th += " data-name='" + Id + "'";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Editor.BindingField))
                    th += " data-name='" + Editor.BindingField + "'";
                else
                    th += " data-name='" + Id + "'";
            }
            //if (ColumnType != null)
            //{
            //    th += " data-type='" + ColumnType.Type + "'";
            //    if (!string.IsNullOrWhiteSpace(ColumnType.Format))
            //        th += " data-format='" + ColumnType.Format + "'";
            //}

            th += ">" + Caption + "</th>";

            return th;
        }
    }

    public class HtmlInputTableColumnSelector : HtmlInputTableColumn
    {
        public override HtmlInput Editor { get => null; set => base.Editor = null; }
        public override string ToString()
        {
            string th = "<th ";
            if (Hidden)
                th += " data-visible=false";
            if (!string.IsNullOrWhiteSpace(BreakPoints))
                th += " data-breakpoints='" + BreakPoints + "'";

            
            th += "><input type = 'checkbox' class='global-checkbox'></th>";
            return th;
        }
    }


    //public interface IHtmlInputTableColumnType
    //{
    //    string Type { get; }   
    //    string Format { get; set; }
    //}

    //public class HtmlInputTableColumnTypeText:IHtmlInputTableColumnType
    //{
    //    public string Type { get { return "text"; } }
    //    public string Format
    //    {
    //        get { return string.Empty; }
    //        set { }
    //    }
    //}

    //public class HtmlInputTableColumnTypeHtml : IHtmlInputTableColumnType
    //{

    //    public string Type { get { return "html"; } }
    //    public string Format
    //    {
    //        get { return string.Empty; }
    //        set { }
    //    }


    //}

    //public class HtmlInputTableColumnTypeDate:IHtmlInputTableColumnType
    //{

    //    public string Type { get { return "date"; } }
    //    public string Format
    //    {
    //        get;
    //        set;
    //    }
    //}

    //public class HtmlInputTableColumnTypeNumber : IHtmlInputTableColumnType
    //{

    //    public string Type { get { return "number"; } }
    //    public string Format
    //    {
    //        get;
    //        set;
    //    }

    //}


    //public class HtmlInputLOV:HtmlInputTable
    //{
        
    //    public override string ToString()
    //    {

    //        ReadOnly = true;            
    //        return base.ToString();
    //    }
        
    //    public string JsAfterRowClickFunctionName
    //    {
    //        get { return Id + "_afterRowClick"; }
    //    }

        
    //    public override string InitScript
    //    {
    //        get
    //        {
    //            //              string script = "$(document).ready(function() { " +
    //            //// Handler for .ready() called.
    //            //                  "var " + ToBindingField + "; " +
    //            //                  "$('#" + Id + " tr').click(function() {" +
    //            //                  ToBindingField + " = $(this).data('__FooTableRow__').val();" +
    //            //                  "if (" + JsAfterRowClickFunctionName + ") " + JsAfterRowClickFunctionName + "(" + ToBindingField + ");" +
    //            //              "});" +
    //            //              "});";
    //            //              return base.InitScript + "<script>" + script + "</script>";
    //            return base.InitScript;
    //        }
    //    }
    //}

}