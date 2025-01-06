


var getColumns = function (menuId) {
    return $.get(_API_URL + "api/list/columns?MenuID=" + menuId);
};

var getLOVColumns = function (menuId) {
    return $.get(_API_URL + "api/lov/columns?MenuID=" + menuId);
};

var getListInfo = function (menuId) {
    return $.get(_API_URL + "api/list/info/" + menuId);
};

var getSystemParameter = function (listID) {
    return $.get(_API_URL + "api/system/parameter");
};


Date.prototype.addDays = function (num) {
    var value = this.valueOf();
    value += 86400000 * num;
    return new Date(value);
}

Date.prototype.addSeconds = function (num) {
    var value = this.valueOf();
    value += 1000 * num;
    return new Date(value);
}

Date.prototype.addMinutes = function (num) {
    var value = this.valueOf();
    value += 60000 * num;
    return new Date(value);
}

Date.prototype.addHours = function (num) {
    var value = this.valueOf();
    value += 3600000 * num;
    return new Date(value);
}

Date.prototype.addMonths = function (num) {
    var value = new Date(this.valueOf());

    var mo = this.getMonth();
    var yr = this.getYear();

    mo = (mo + num) % 12;
    if (0 > mo) {
        yr += (this.getMonth() + num - mo - 12) / 12;
        mo += 12;
    }
    else
        yr += ((this.getMonth() + num - mo) / 12);

    value.setMonth(mo);
    value.setYear(yr);
    return value;
}


Array.prototype.arrayByProperty = function (propertyName) {
    var result = [];
    this.forEach(function (value) {
        result.push(value[propertyName]);
    });
    return result;
};

Array.prototype.sum = function (propertyName) {
    var result = 0;
    if (propertyName) {
        if (Array.isArray(propertyName) && propertyName.length > 0) {

            result = [];
            for (i = 0; i < propertyName.length; i++) {
                result.push(0);
            }

            this.forEach(function (value) {
                for (i = 0; i < propertyName.length; i++) {
                    result[i] += value[propertyName[i]];
                }
            });
        }
        else {
            this.forEach(function (value) {
                result += value[propertyName];
            });
        }
    }
    else {
        this.forEach(function (value) {
            result += value;
        });
    }
    return result;
};

Number.prototype.RoundNumber = function (decimalPlaces = 0) {
    var p = Math.pow(10, decimalPlaces);
    var n = (this * p) * (1 + Number.EPSILON);
    return Math.round(n) / p;
}



var helper =
{
    
        displayData: function (object, elementPrefix) {
            for (var property in object) {
                if (object.hasOwnProperty(property)) {
                    if ($("#" + elementPrefix + property).val()) {
                        if (object[property])
                            $("#" + elementPrefix + property).val(object[property]);
                        else if (object[property] == 0)
                            $("#" + elementPrefix + property).val(0);

                        else
                            $("#" + elementPrefix + property).val("--");
                    }

                    else if ($("#" + elementPrefix + property).html()) {
                        if (object[property])
                            $("#" + elementPrefix + property).html(object[property]);
                        else if (object[property] == 0)
                            $("#" + elementPrefix + property).html(0);
                        else
                            $("#" + elementPrefix + property).html("--");
                    }

                }
            }
        },
        manualInputSelect2: function (term, results) {
            if ($(results).filter(function () {
                return term.localeCompare(this.text) === 0;
            }).length === 0) {
                return { id: term, tex: term };
            }
        },

        createTagSelect2: function (params, allowManualInput) {
            if (!allowManualInput)
                return null;
            var term = $.trim(params.term);

            if (term === '') {
                return null;
            }

            return {
                id: term,
                text: term
            };
        },
        callAjaxRequestJson: function (apiRouteUrl, dataFilter, ajaxMethod, successFunction, errorFunction) {
            if (!ajaxMethod)
                ajaxMethod = "get";
            var ajax = $.ajax({
                type: ajaxMethod,
                url: _API_URL + apiRouteUrl,
                data: dataFilter,
                headers: { "Authorization": authServices.getBearerToken() },
                success: successFunction,
                error: errorFunction
            });
            return ajax;
    },

    callRemoteAjaxRequestJson: function (apiHost, apiRouteUrl, dataFilter, ajaxMethod, successFunction, errorFunction) {
        if (!ajaxMethod)
            ajaxMethod = "get";
        var ajax = $.ajax({
            type: ajaxMethod,
            url: apiHost + apiRouteUrl,
            data: dataFilter,
            headers: { "Authorization": authServices.getBearerToken() },
            success: successFunction,
            error: errorFunction
        });
        return ajax;
    },

        initSelect2: function (elementId, allowManualInput) {
            $('#' + elementId).select2({
                tags: allowManualInput,
                createTag: function (params) { return helper.createTagSelect2(params, allowManualInput); }
            });
        },

        initSelect2Ajax: function (apiRoute, ajaxMethod, elementId, dataFilter, allowManualInput) {
            if (!ajaxMethod || ajaxMethod == '')
                ajaxMethod = "get";
            if (!apiRoute || apiRoute == '') {
                helper.initSelect2(elementId, allowManualInput);
                return;
            }


            $('#' + elementId).select2({
                ajax: {

                    url: _API_URL + apiRoute,
                    headers: { "Authorization": authServices.getBearerToken() },
                    type: ajaxMethod,
                    dataType: 'json',
                    delay: 250,
                    data: function (params) {
                        var searchParam = { searchTerm: params.term };
                        if (dataFilter)
                            return $.extend(searchParam, dataFilter);
                        return searchParam;
                    },
                    processResults: function (response) {
                        return {
                            results: $.map(response, function (item) {
                                return {
                                    text: item.Text,
                                    id: item.Id
                                };
                            })
                        };

                    },
                    cache: false,
                    tags: allowManualInput,
                    createTag: function (params) { return helper.createTagSelect2(params, allowManualInput); }
                }
            });

        },

        loadSelect2StaticFromAjax: function (apiRouteUrl, dataFilter, ajaxMethod, elementId, selectedValue, defaultText, allowManualInput, afterLoadFunction) {


            if (!ajaxMethod)
                ajaxMethod = "get";
            if (!apiRouteUrl || apiRouteUrl == '') {
                helper.initSelect2(elementId, allowManualInput);
                return;
            }

            var successFunction = function (responseData) {
                if (!selectedValue)
                    selectedValue = $("#" + elementId).val();
                $("#" + elementId).children().remove().end();
                helper.loadSelect2StaticFromArray(responseData, elementId, selectedValue, defaultText, allowManualInput);
            };

            var errorFunction = function (responseData) {
                $("#" + elementId).children().remove().end();
            };

            var ajax = helper.callAjaxRequestJson(apiRouteUrl, dataFilter, ajaxMethod, successFunction, errorFunction);
            if (afterLoadFunction)
                ajax.then(function () { afterLoadFunction(); });
            else
                ajax;
        },

        loadSelect2StaticFromAjax2: function (apiRouteUrl, dataFilter, ajaxMethod, elementId, selectedValue, defaultText, allowManualInput, dictData, idField, textField, afterLoadFunction) {

            if (!apiRouteUrl)
                return;
            if (!ajaxMethod)
                ajaxMethod = "get";
            if (!apiRouteUrl || apiRouteUrl == '') {
                helper.initSelect2(elementId, allowManualInput);
                return;
            }
            $("#" + elementId).children().remove().end();

            var successFunction = function (responseData) {
                if (Array.isArray(responseData) && responseData.length > 0) {
                    responseData.forEach(function (data) {
                        dictData["K" + data[idField]] = data;
                    });
                }
                helper.loadSelect2StaticFromDictionary(dictData, idField, textField, elementId, selectedValue, defaultText, allowManualInput);
            };

            var ajax = helper.callAjaxRequestJson(apiRouteUrl, dataFilter, ajaxMethod, successFunction, null);
            if (afterLoadFunction)
                ajax.then(function () {
                    afterLoadFunction();
                });
            else
                ajax;
        },

        loadSelect2StaticFromDictionary: function (optionItems, idField, textField, elementId, selectedValue, defaultText, allowManualInput) {
           

            $("#" + elementId).children().remove().end();            
            if (!optionItems)
                return false;

            for (var key in optionItems) {
                var item = optionItems[key];
                helper.loadSelect2FromOption(elementId, item[idField], item[textField]);
            }


            $('#' + elementId).select2({
                tags: allowManualInput,
                createTag: function (params) { return helper.createTagSelect2(params, allowManualInput); }
            });
            if (selectedValue === null) {
                $("#" + elementId).val(null).trigger('change');
                return;
            }
            $('#' + elementId).val(selectedValue.toString().trim()).trigger('change');
            

        },
        loadSelect2StaticFromArray: function (optionItems, elementId, selectedValue, defaultText, allowManualInput,mode,idField,textField) {

            
            if (optionItems === null)
                return false;

            if (!idField)
                idField = "Id";
            if (!textField)
                textField = "Text";
            if (!mode)
                mode = "replace";
            mode = mode.toLowerCase();

            var children;

            if (mode === "replace")
                $("#" + elementId).children().remove().end();

            
            if (mode === "insert") {
                children = $("#" + elementId).children();
                $("#" + elementId).children().remove().end();
            }

            if (Array.isArray(optionItems) && optionItems.length > 0) {

                optionItems.forEach(function (optionItem) {
                    helper.loadSelect2FromOption(elementId, optionItem[idField], optionItem[textField]);
                });
            }
            else {
                helper.loadSelect2FromOption(elementId, optionItems[idField], optionItems[textField]);
            }

            if (mode == "insert") {
                if (Array.isArray(children) && children.length > 0) {
                    children.forEach(function (optionItem) {
                        helper.loadSelect2FromOption(elementId, optionItem[idField], optionItem[textField]);
                    });
                }
                else {
                    helper.loadSelect2FromOption(elementId, children[idField], children[textField]);
                }
            }


            $('#' + elementId).select2({
                tags: allowManualInput,
                createTag: function (params) { return helper.createTagSelect2(params, allowManualInput); }
            });

            if (selectedValue === null) {
                $("#" + elementId).val(null).trigger('change');
                return;
            }
            $('#' + elementId).val(selectedValue).trigger('change');

        },

        loadSelect2FromOption: function (elementId, id, text) {
            if (!id) {
                if (id != 0 && id != false)
                    return;
            }

            if (typeof (id) === "string")
                id = id.trim();
            if (!text)
                text = "";
            if (typeof (id) === "string")
                text = text.trim();



            $('#' + elementId)
                .append($("<option></option>")
                    .attr("value", id)
                    .html(text));
        },

        initDateTimePicker: function (elementId, minDate, maxDate, format, readonly) {

            
            var dateOption = { format: format };
            if (minDate != null)
                $.extend(dateOption, { minDate: minDate });
            if (maxDate != null)
                $.extend(dateOption, { maxDate: maxDate });
            $('#' + elementId).datetimepicker(dateOption);

        },

        initDatePicker: function (elementId, minDate, maxDate) {
            this.initDateTimePicker(elementId, minDate, maxDate, 'DD MMM YYYY');
        },
        initTimePicker: function (elementId, minDate, maxDate) {
            this.initDateTimePicker(elementId, minDate, maxDate, 'HH:mm');
        },
        initDateRange: function (elementId, minDate, maxDate, format) {
            var elementIdStart = elementId + "Start";
            var elementIdEnd = elementId + "End";
            this.initDateTimePicker(elementIdStart, minDate, maxDate, format);
            $('#' + elementIdStart).on('dp.change', function (e) {
                var minDate = moment(e.date).format(format);
                $('#' + elementIdEnd).data('DateTimePicker').minDate(minDate);
                if ($('#' + elementIdEnd).data('DateTimePicker').date() < e.date)
                    $('#' + elementIdEnd).data('DateTimePicker').date(e.date);

                $('#' + elementIdEnd).data('DateTimePicker').minDate(minDate);
            });
            this.initDateTimePicker(elementIdEnd, minDate, maxDate, format);

        },
        initDateRange2: function (elementIdStart, elementIdEnd,format) {        
        $('#' + elementIdStart).on('dp.change', function (e) {
            var minDate = moment(e.date).format(format);
            $('#' + elementIdEnd).data('DateTimePicker').minDate(minDate);
            if ($('#' + elementIdEnd).data('DateTimePicker').date() < e.date)
                $('#' + elementIdEnd).data('DateTimePicker').date(e.date);

            $('#' + elementIdEnd).data('DateTimePicker').minDate(minDate);
        });
        

        },
        numberToSQLString: function (number, fieldName) {
            if (!number || number == '')
                return '';

            if (Array.isArray(number) && number.length > 0) {
                fieldName += " in (";
                number.forEach(function (item) {
                    fieldName += item + ",";
                });
                fieldName = fieldName.substring(0, fieldName.length - 1) + ")";
                return fieldName;
            }
            else {
                return fieldName + "=" + number;
            }
        },
        stringToSQLString: function (text, fieldName) {
            if (!text || text == '')
                return '';

            if (Array.isArray(text) && text.length > 0) {
                fieldName += " in (";
                text.forEach(function (item) {
                    fieldName += "'" + item + "',";
                });
                fieldName = fieldName.substring(0, fieldName.length - 1) + ")";
                return fieldName;
            }
            else {
                return fieldName + "='" + text + "'";
            }
        },

        loadArrayToFooTable: function (tableId, values, keys) {

            var ft;
            var intTimeOut;
            intTimeOut = setTimeout(function () {
                ft = FooTable.get('#' + tableId);
                if (ft) {
                    clearTimeout(intTimeOut);
                    if (keys) {
                        values.forEach(function (value) {
                            value.key = helper.footableRowKey(value, keys);
                        });
                    }

                    ft.rows.load(values);
                }
            }, 1000);


        },
        loadWebAPIDataToFooTable: function (apiRouteUrl, dataFilter, ajaxMethod, tableId) {
            if (!ajaxMethod)
                ajaxMethod = "get";

            $.ajax({
                type: ajaxMethod,
                url: _API_URL + apiRouteUrl,
                data: dataFilter,
                headers: { "Authorization": authServices.getBearerToken() },
                success: function (responseData, textStatus, jqXHR) {
                    helper.loadArrayToFooTable(tableId, responseData);
                }
            });
        },

        saveArrayFromFooTable: function (tableId, columns) {

            if (!columns)
                return [];

            if (!Array.isArray(columns))
                columns[0] = columns;

            var rows = [];
            var ft = FooTable.get('#' + tableId);

            $.each(ft.rows.all, function (i, row) {
                var rowData = {};
                var rowValue = row.val();
                columns.forEach(function (column) {
                    rowData[column] = rowValue[column];
                });
                rows.push(rowData);
            });
            return rows;
        },

        ajaxRows: function (apiUrl, data) {
            if (apiUrl && apiUrl != '') {
                var ajax = $.ajax({
                    type: "get",
                    url: _API_URL + apiUrl,
                    data: data,
                    headers: { "Authorization": authServices.getBearerToken() }
                });
                return ajax;
            }
            return null;
        },
        setInputReadOnly: function (id, attribute, readonly, replacement) {
            if (!readonly)
                readonly = false;
            if (!attribute)
                attribute = this.getInputReadOnlyAttr(id);
            $('#' + id).attr(attribute, readonly);
            if (replacement) {
                var value = $('#' + id).val();
                $('#' + id).replaceWith(replacement);

            }
        },

        getInputReadOnlyAttr: function (id) {

            var attribute = "disabled";
            if ($("#" + id).hasAttr("readonly"))
                attribute = "readonly";
            return attribute;
        },
        setValue: function (id, value, triggerEvent) {
            if ($('#' + id) && value) {
                if (triggerEvent)
                    $('#' + id).val(value).trigger(triggerEvent);
                else
                    $('#' + id).val(value);
            }
        },

    setValueDateTimePicker: function (id, value, formatValue, triggerEvent, readonly) {
        
        if (!formatValue)
            formatValue = "DD-MMM-YYYY HH:mm";

        var tmpValue = moment(value).format(formatValue);
        if (tmpValue === "Invalid date")
            tmpValue = moment(value, formatValue);

        value = tmpValue;
        if (!$('#' + id))
            return;
        
        if (triggerEvent)
            $('#' + id).data('DateTimePicker').date(value).trigger(triggerEvent);
        else
            $('#' + id).data('DateTimePicker').date(value);
        },
        setValueCheckBox: function (id, value, triggerEvent) {
            if (!value)
                value = false;
            $('#' + id).prop('checked', value);
        },
        getValue: function (id) {
            return $('#' + id).val();
        },

        getValueDatePicker: function (id) {
            return $('#' + id).data('DateTimePicker').date();
        },

        getValueCheckBox: function (id) {
            return $('#' + id).prop('checked');
        },
        //Format Columns {name:'',title:'',visible:true,formatter:fu,IsKey:true,EditorId:'',EditorReadOnlyAttr:'',EditorType:''}

        initFooTableEdit: function (tableId, title, allowAdd, allowEdit, allowDelete, rows, columns, displayRowToEditorFunction, saveEditorToRowFunction, rowValidFunction) {
            var readonly = !allowAdd && !allowEdit && !allowDelete,
                keys = [], hasKey = false, editRow = false,
                dictKeyEditors = {};

            if (Array.isArray(columns)) {
                columns.forEach(function (column) {
                    if (column.IsKey === true) {
                        keys.push(column.name);
                        hasKey = true;
                        if (column.EditorId)
                            dictKeyEditors[column.EditorId] = column;
                    }
                });
            }

            var $modal = $('#' + tableId + '_editor_modal'),
                $editor = $('#' + tableId + '_editor'),
                $editorTitle = $('#' + tableId + '_editor_title'),
                ft = FooTable.init('#' + tableId, {
                    editing: {
                        enabled: !readonly,
                        alwaysShow: !readonly,
                        allowAdd: allowAdd,
                        allowEdit: allowEdit,
                        allowDelete: allowDelete,
                        addRow: function () {
                            if (allowAdd) {
                                $modal.removeData('row');
                                //$editor[0].reset();
                                helper.resetForm($editor[0]);
                                $editorTitle.text('Add a new ' + title);
                                editRow = false;
                                setEditingKeyInputReadOnly(editRow);
                                $modal.modal('show');
                            }
                        },
                        editRow: function (row) {
                            if (allowEdit) {
                                $modal.removeData('row');
                                //$editor[0].reset();
                                helper.resetForm($editor[0]);
                                var values = row.val();
                                $editor.find('#' + tableId + '_rowid').val(values.rowid);
                                displayRowToEditor(displayRowToEditorFunction, values);
                                $modal.data('row', row);
                                $editorTitle.text('Edit ' + title);
                                editRow = true;
                                setEditingKeyInputReadOnly(editRow);
                                $modal.modal('show');
                            }
                        },
                        deleteRow: function (row) {
                            if (allowDelete) {
                                if (confirm('Are you sure you want to delete the row?')) {
                                    row.delete();
                                }
                            }
                        }
                    }
                }),
                uid = 10;

            if (ft && rows) {
                if (keys) {
                    rows.forEach(function (value) {
                        value.key = helper.footableRowKey(value, keys);
                    });
                }
                ft.rows.load(rows);
            }

            $editor.on('submit', function (e) {
                e.preventDefault();
                var row = $modal.data('row'),
                    values = { rowid: $editor.find('#' + tableId + '_rowid').val() };
                saveEditorToRow(saveEditorToRowFunction, values);
                var rowkey = { key: rowKey(values) };
                $.extend(values, rowkey);
                if (!editRow) {
                    if (rowExist(values) === true) {
                        alert('The row is already exists');
                        return;
                    }
                }
                if (!rowValid(rowValidFunction, values)) {
                    return;
                }

                if (row instanceof FooTable.Row)
                    row.val(values);
                else {
                    values[tableId + "_rowid"] = uid++;
                    ft.rows.add(values);
                }
                $modal.modal('hide');
            });

            var rowExist = function (values) {
                var ft = FooTable.get('#' + tableId), rowExistX = false;
                $.each(ft.rows.all, function (i, row) {
                    var rowVal = row.val();
                    if (values.key === rowVal.key) {
                        rowExistX = true;
                        return rowExistX;
                    }
                });
                return rowExistX;
            };

            var rowKey = function (values) {
                return helper.footableRowKey(values, keys);
            };

            var setEditingKeyInputReadOnly = function (readonly) {
                if (!hasKey)
                    return;
                for (var key in dictKeyEditors) {
                    var columnEditor = dictKeyEditors[key];
                    if (!columnEditor.EditorReadOnlyAttr)
                        columnEditor.EditorReadOnlyAttr = helper.getInputReadOnlyAttr(columnEditor.EditorId);
                    helper.setInputReadOnly(columnEditor.EditorId, columnEditor.EditorReadOnlyAttr, readonly);
                }

            };

            var displayRowToEditor = function (functionName, values) {
                functionName(values);
            };

            var saveEditorToRow = function (functionName, values) {
                if (!functionName || !values)
                    return;
                $.extend(values, functionName(values));
            };

            var rowValid = function (functionName, values) {
                if (!functionName || !values)
                    return true;
                return functionName(values);
            };

        },
        initFooTableLOV: function (tableId, title, rows, columns, rowSelectEvent) {
            helper.initFooTableEditFromArray(tableId, title, false, false, false, rows, columns);
            if (rowSelectEvent)
                $("#" + tableId + " tr.rows").click(function () {
                    var values = $(this).data('__FooTableRow__');
                    rowSelectEvent($(this), values);
                });
        },
        initFooTableLOVFromWebAPI: function (tableId, title, apiRouteUrl, columns, rowSelectEvent) {
            var successFunction = function (responseData) {
                helper.initFooTableEdit(tableId, title, false, false, false, responseData, columns);
                if (rowSelectEvent)
                    $("#" + tableId + " tr.rows").click(function () {
                        var values = $(this).data('__FooTableRow__');
                        rowSelectEvent($(this), values);
                    });
            },
                errorFunction = function (responseData) {
                    helper.initFooTableEdit(tableId, title, false, false, false, null, columns);
                };
            helper.callAjaxRequestJson(apiRouteUrl, null, 'get', successFunction, errorFunction);
        },
        initFooTableEditFromWebAPI: function (tableId, title, allowAdd, allowEdit, allowDelete, apiRouteUrl, columns, displayValuesFunction, saveValuesFunction, rowValidFunction) {
            var successFunction = function (responseData) {

                helper.initFooTableEditFromArray(tableId, title, allowAdd, allowEdit, allowDelete, responseData, columns, displayValuesFunction, saveValuesFunction, editingKeyControlFunction, rowValidFunction);
            },
                errorFunction = function (responseData) {
                    helper.initFooTableEdit(tableId, title, allowAdd, allowEdit, allowDelete, null, columns, displayValuesFunction, saveValuesFunction, editingKeyControlFunction, rowValidFunction);
                };
            helper.callAjaxRequestJson(apiRouteUrl, null, 'get', successFunction, errorFunction);
        },

        footableRowKey: function (values, keys) {
            if (!keys || !values)
                return '';
            if (Array.isArray(keys)) {
                if (keys.length < 0)
                    return '';
                if (keys.length === 1) {
                    return values[keys[0]];
                }
                var i, keyValue = '';

                for (i = 0; i < keys.length; i++) {
                    keyValue += values[keys[i]] + '#';
                }
                if (keyValue.length > 0)
                    keyValue = keyValue.substring(0, keyValue.length - 1);
                return keyValue;
            }
            else {
                return values[keys];
            }
        },

        footableRowExist : function (values, allrows, keys) {
            var rowExistX = false;
            var withKey = false;
            var hasRows = false;
            if (allrows && Array.isArray(allrows) && allrows.length > 0)
                hasRows = true;

            if (!hasRows)
                return false;

            if (keys && Array.isArray(keys) && keys.length > 0)
                withKey = true;
            if (withKey)
                values.key = helper.footableRowKey(values, keys);
            allrows.forEach(function(row){
                var rowVal = row.val();
                if (withKey)
                    rowVal.key = helper.footableRowKey(rowVal, keys);

                if (values.key === rowVal.key) {
                    rowExistX = true;
                    return;
                }
            });

            
            return rowExistX;
    },

    footableDisplayColumn: function (ftColumn) {
        
        if (!ftColumn.xtype)
            return ftColumn;
        
        var newColumn = {};
        newColumn.title = ftColumn.title;
        newColumn.style = ftColumn.style;
        newColumn.visible = true;
        newColumn.breakpoints = ftColumn.breakpoints;
        newColumn.filterable = ftColumn.filterable;
        newColumn.sortable = ftColumn.sortable;

        var hasDisplay = false;
        if (ftColumn.xtype === 'date') {
            newColumn.formatter = function (value, option, rowdata) {
                return helper.footableFormatterDate(rowdata, ftColumn.name, ftColumn.xformatString);
            };
            hasDisplay = true;
        }
        else if (ftColumn.xtype === 'boolean') {
            newColumn.type = 'html';
            newColumn.formatter = function (value, option, rowdata) {
                return helper.footableFormatterBoolean(rowdata, ftColumn.name);
            };
            hasDisplay = true;
        }
        else if (ftColumn.xtype === 'float') {
            newColumn.formatter = function (value, option, rowdata) {
                return helper.footableFormatterDecimal(rowdata, ftColumn.name,2);
            };
            hasDisplay = true;
        }
        else if (ftColumn.xtype === 'integer') {
            newColumn.formatter = function (value, option, rowdata) {
                return helper.footableFormatterInt(rowdata, ftColumn.name);
            };
            hasDisplay = true;
        }

        if (hasDisplay)
            return newColumn;
        return ftColumn;
    },

    footableFormatterBoolean: function (rowdata, name) {
        if (rowdata[name])
            return "<input type='checkbox' disabled checked/>";
        return "<input type='checkbox' disabled/>";
    },

    footableFormatterDate: function (rowdata, name,format) {
        if (rowdata[name])
            return moment(rowdata[name]).format(format);
        return null;
    },

    footableFormatterInt: function (rowdata, name) {
        if (rowdata[name])
            return parseInt(rowdata[name]);
        return null;
    },
    footableFormatterDecimal: function (rowdata, name, numdigit) {
        if (rowdata[name])
            return parseFloat(rowdata[name]).toFixed(numdigit);
        return null;
    },



        resetForm: function (form) {
            resetElements(form.Id);
        },
        setTableReadOnly: function (tableId) {
            var ft;
            var intTimeOut;
            intTimeOut = setTimeout(function () {
                ft = FooTable.get('#' + tableId);
                if (ft) {
                    clearTimeout(intTimeOut);
                    $("tr.footable-editing").hide();
                    $("button.footable-edit").prop("disabled", true);
                    $("button.footable-delete").prop("disabled", true);
                    //$(".footable-last-visible").html('');
                    //$(".footable-delete").css("display", "none");
                    //$(".footable-editing").css("display", "none");                
                    //var editingParam = {
                    //    editingAllowAdd: false,
                    //    editingAllowDelete: false,
                    //    editingAllowEdit:false
                    //};
                    //ft.data = editingParam;
                    var a = 1;
                }
            }, 1000);
        },
        loadLoV: function (LoVId, multiple, returnFunc, fixedFilter, largeModal) {
            if (typeof loadingLoV == "undefined")
                loadingLoV = false;
            if (!loadingLoV) {
                loadingLoV = true;
                
                $.get("/ListOfValue/Index?LoVMenuId=" + LoVId + "&multiple=" + multiple).done(function (responseData) {
                    $("#LoVContainer").html(responseData);
                    sessionStorage["FixedFilter" + LoVId] = JSON.stringify(fixedFilter);
                    
                    if (typeof largeModal !== "undefined" && largeModal.toLowerCase().trim() === "true")
                        $("#" + LoVId).find("[class='modal-dialog']").addClass("modal-lg");

                    $("#" + LoVId).modal("show");
                    activeLoV = LoVId;
                    LoV[LoVId].clearFilter();
                    var onClickEvent = function () {
                        if ($.isEmptyObject(LoV[activeLoV].selectedData)) {
                            alert("Please select at least one data");
                            return;
                        }

                        returnFunc(LoV[LoVId].selectedData);

                        $("#" + LoVId).modal("hide");
                    };

                    $("#btnSelect_" + LoVId).click(onClickEvent);
                    $("#btnSelect1_" + LoVId).click(onClickEvent);

                    $("#" + LoVId).on("hidden.bs.modal", function (e) {
                        activeLoV = null;
                    });

                    loadingLoV = false;
                });
            }               
    }

};

var xSelectAttributes = {};

var xSelectInit = function (Id, readonly, value, apiUrl, apiUrlMode, allowManualInput, idField, textField, descendants, accendands, accendandFields) {

    if (accendands) {
        var xAttribute = {
            "readonly": readonly,
            "apiUrl": apiUrl,
            "apiUrlMode": apiUrlMode,
            "allowManualInput": allowManualInput,
            "idField": idField,
            "textField": textField,
            "descendants": descendants,
            "accendands": accendands,
            "accendandFields": accendandFields
        };
            
        xSelectAttributes[Id] = xAttribute;
    }
    
    if (readonly) {        
        xSelectSetValue(Id, true, value, apiUrl,apiUrlFilter);
    }
    else {
        if (apiUrl) {
            var apiUrlFilter = {};
            if (!apiUrlMode)
                apiUrlMode = "replace";

            if (!value)
                value = $("#" + Id).val();

            if (accendands) {
                
                var i = 0;
                accendands.forEach(function (x) {
                    var filterValue = $('#' + x).val();
                    var filterField = accendandFields[i];
                    if (filterValue === null) {
                        $('#' + Id).html('');
                        $('#' + Id).select2();
                        return;
                    }
                    apiUrlFilter[filterField] = filterValue;
                    i++;
                });
            }
            
            apiUrlMode = apiUrlMode.toLowerCase();
            helper.callAjaxRequestJson(apiUrl, apiUrlFilter, "get", function (responseData) {
                if (!idField)
                    idField = "Id";
                if (!textField)
                    textField = "Text";
                helper.loadSelect2StaticFromArray(responseData, Id, value, null, allowManualInput, apiUrlMode,idField,textField);
                if (typeof xEvent !== 'undefined') {
                    xEvent(Id, 'load');
                    xEvent(Id, 'change');
                }

                $('#' + Id).on('change', function () {
                    if (descendants) {
                        descendants.forEach(function (x) {
                            xSelectInit(x, xSelectAttributes[x].readonly, $('#' + x).val(), xSelectAttributes[x].apiUrl, xSelectAttributes[x].apiUrlMode, xSelectAttributes[x].allowManualInput, xSelectAttributes[x].idField, xSelectAttributes[x].textField, xSelectAttributes[x].descendants, xSelectAttributes[x].accendands, xSelectAttributes[x].accendandFields);
                        });
                    }
                    if (typeof xEvent !== 'undefined') {                        
                        xEvent(Id, 'change');
                    }
                    
                });
            });
        }
        else {
            if (!value)
                value = $("#" + Id).val();
            $('#' + Id).val(value).trigger("change");
            $('#' + Id).select2();

            if (typeof xEvent !== 'undefined') {
                xEvent(Id, 'load');
                xEvent(Id, 'change');
            }

            $('#' + Id).on('change', function () {
                if (typeof xEvent !== 'undefined') {
                    xEvent(Id, 'change');
                }
            });
        }
    }
};

var xSelectSetValue = function (Id, readonly, value, apiUrl,apiUrlFilter) {
    if (readonly && value && apiUrl) {
            var data = { Id: value };
            helper.callAjaxRequestJson(apiUrl, apiUrlFilter, "get", function (responseData) {
                helper.loadSelect2StaticFromArray(responseData, Id, value, null, false, null);                
            });
    }
    else {
        $('#' + Id).val(value).trigger("change");
        
    }
};

var xSelectBoleanInit = function (Id, readonly, value) {

    if (readonly) {
        xSelectBooleanSetValue(Id, readonly, value);
    }
    else {

        $('#' + Id).html('');
        $('#' + Id).append("<option value=true>Yes</option>");
        $('#' + Id).append("<option value=false>No</option>");

        
        if (typeof xEvent !== 'undefined') {
            xEvent(Id, 'load');
            xEvent(Id, 'change');
        }
        

        $('#' + Id).on('change', function () {
            if (typeof xEvent !== 'undefined') {
                xEvent(Id, 'change');
            }
            
        });

        $('#' + Id).val(value).trigger("change");
    }
};

var xSelectBooleanSetValue = function (Id, readonly, value) {
    if (readonly) {
        $('#' + Id).html('');
        if (value)
            $('#' + Id).append("<option value=true selected>Yes</option>");
        else
            $('#' + Id).append("<option value=false selected>No</option>");
        $('#' + Id).val(value).trigger("change");
    }
    else {
        $('#' + Id).val(value).trigger("change");
    }
};


var xLovSetValue = function (Id, readonly, value, text) {
    if (!text)
        text = value;
    if (readonly) 
        $('#' + Id).html('');
    if (value) {
        var newOption = new Option(text, value, true, true);
        $('#' + Id).append(newOption).trigger('change');
    }
};

var xStringToArray = function (text) {
    if (!text)
        return null;
     if (Array.isArray(text))
        return text;
    var result = text.split(",");

    if (!result || result.length<=0)
        return null;
    for (var i = 0; i < result.length; i++) {
        result[i] = result[i]
            .replace("'", "")
            .replace("\"", "")
            .replace("[", "")
            .replace("]", "");
    }
    return result;

};

var xDataListInit = function (Id, value,readOnly) {
    
    var option = $('#' + Id).data();
    if (option.url) {  
        if (!option.url.startsWith("http"))
            option.url = _API_URL + option.url;
        var data = {};
        if (option.relativeElements) {
            var relativeElements = xStringToArray(option.relativeElements),
                relativeFields = xStringToArray(option.relativeFields);
            if (relativeElements && relativeElements.length > 0) {

                for (var i = 0; i < relativeElements.length; i++) {                
                    data[relativeElements[i]] = relativeFields[i];
                    relativeElements[i] = "#" + relativeElements[i];
                }
                option.relatives = relativeElements;
                option.relativeFields = relativeFields;
            }
        }
        
        if (option.visibleProperties)
            option.visibleProperties = xStringToArray(option.visibleProperties);
        if (option.searchIn)
            option.searchIn = xStringToArray(option.searchIn);
        params = option.params;
        if (!params)
            params = {};
        params["bf"] = data;
        option.params = params;
        option.urlHeaders = { "Authorization": authServices.getBearerToken() };
    }
    if (!option.valueField)
        option.valueField = '*';
    option.selectionRequired = true;
    if (readOnly)
        option.readOnly = true;
    $('#' + Id).flexdatalist(option);
    if (value) {
        if ($('#' + Id).attr("multiple"))
            $('#' + Id).flexdatalist('add', value);
        else
            $('#' + Id).flexdatalist('value', value);
    }
};

var xFormDisplay = function (formId, data) {
    if (!data) {
        resetElements(formId);
        return;
    }
    var inputs = $("#" + formId).find("[data-bf]");
    inputs.each(function (index, input) {
        var bindingField = $(input).attr("data-bf"),
            readonly = $(input).attr("read-only"),
            xType = $(input).attr("x-type"),
            multiple = $(input).attr("multiple");
        

        if (bindingField) {
            var value = null;
            if (typeof data[bindingField] !== 'undefined')
                value = data[bindingField];

            
            if (xType === "select") {
                if ($(input).attr("selecttype") === "Boolean")
                {
                    if (value && (value === true || value === "true" || value === 1))
                        $(input).val("true").trigger('change');
                    else
                        $(input).val("false").trigger('change');
                }
                else
                    xSelectSetValue(input.id, readonly, value, $(input).attr("api-url"));
            }
            else if (xType === "xlov")
                xLovSetValue(input.id, readonly, value, value);
            
            else if (xType === "xdate") {
                if (value) {
                    if (readonly) {
                        value = moment(value).format($(input).attr("display-format"));
                        $(input).val(value);
                    }
                    else
                        helper.setValueDateTimePicker(input.id, value, $(input).attr("display-format"));
                }
                else
                    $(input).val(null);
                
            }
            else if (xType === "xdatalist") {
                $(input).flexdatalist('value', value);
            }
            else if (xType === "xcheckbox") {
                $(input).prop('checked', data[bindingField]);
            }
            else if (xType === "file-upload") {
                var propName = $(input).attr('name');
                displayFile(propName, value);
            }
            else
                $(input).val(value);
        }
    });
    
};

var xFormSave = function (formId) {
    var data = {};
    var inputs = $("#" + formId).find("[data-bf]");
    inputs.each(function (index,input) {
        var bindingField = $(input).attr("data-bf"),
            readonly = $(input).attr("read-only"),
            xType = $(input).attr("x-type");

        if (bindingField) {
            if (xType === 'xdatalist') {
                val = $(input).flexdatalist('value');
                if (val && typeof val.label !== 'undefined' && typeof val.value !== 'undefined')
                    val = val.value;
                data[bindingField] = val;
            }
            else
                data[bindingField] = $(input).val();
        }
    });
    return data;
};

var xFormSaveToQueryString = function (formId) {
    var data = "";
    var inputs = $("#" + formId).find("[data-bf]");
    inputs.each(function (index, input) {
        var xType = $(input).attr("x-type");
        var bindingField = $(input).attr("data-bf");
        var val = $(input).val();
        if (xType === 'xdatalist') {
            val = $(input).flexdatalist('value');
            if (val && typeof val.label !== 'undefined' && typeof val.value !== 'undefined')
                val = val.value;
        }

        if (bindingField && val) {
            if (Array.isArray(val)) {
                if (val.length > 0) {
                    val.forEach(function (value) {
                        data += bindingField + "=" + value + "&";
                    });
                }
            }
            else 
                data += bindingField + "=" + val + "&";
        }
    });
    return data;
};

var xJsonToQueryString = function (json) {
    var data = "";
    Object.keys(json).forEach(function (key) {
        var val = json[key];
        if (val) {
            if (Array.isArray(val)) {
                if (val.length > 0) {
                    val.forEach(function (value) {
                        data += key + "=" + value + "&";
                    });
                }
            }
            else
                data += key + "=" + val + "&";
        }
    });
    return data;
};


var xGenerateFilter = function (filterFormId,filterTemplate) {    
    var inputs = $("#" + filterFormId).find("[data-bf]");
    inputs.each(function (index, input) {
        var bindingField = $(input).attr("data-bf"),
            readonly = $(input).attr("read-only"),
            xType = $(input).attr("x-type"),
            Id = $(input).attr("id");

        if (bindingField) {
            var xValue = null;
            if (xType === "xdatalist") {
                xValue = $(input).flexdatalist('value');
                if (xValue && typeof xValue.label !== 'undefined' && typeof xValue.value !== 'undefined')
                    xValue = xValue.value;
            }
            else
                xValue = $(input).val();
            filterTemplate = filterTemplate.replace("<" + Id + ">", xValue);
        }
    });
    return filterTemplate;
};

var xTabInit = function (Id) {
    var tabNavId = Id + "_Nav";
    var i = 0;
    $('#' + Id).children('.tab-pane').each(function (index, element) {        
        if ($(element).attr("id")) {
            var caption = $(element).attr("caption");
            
            var cssActive = "";
            if (i === 0) {
                cssActive = " class='active'";
                $(element).addClass("active");
            }
            else {
                $(element).removeClass("active");
            }
            $('#' + tabNavId).append("<li" + cssActive + "><a data-toggle='tab' href='#" + $(element).attr("id") + "'>" + caption + "</a></li>");
            i++;
        }
        
    });

};

var xRegisterEvent = function (Id, eventName) {
    if (!eventName)
        eventName = 'change';
    $('#' + Id).on('change', function () {
        if (typeof xEvent !== 'undefined')
            xEvent(Id, eventName);
    });
};


var checkAllCheckBox = function (thisCheckBox, name) {
    if (!name)
        name = thisCheckBox.name;
    $('[name = "' + name + '"]').not(this).prop('checked', thisCheckBox.checked);
};

var resetElements = function (parentId) {

    $('#' + parentId).find('input, textarea')
        .each(function () {
            var type = $(this).attr("type");
            if (type) {
                if (type === 'radio' || type === 'checkbox') {
                    $(this).removeAttr('checked');
                    $(this).removeAttr('selected');
                }
                else
                    $(this).val(null);
            }
            else
                $(this).val(null);
        });
    $('#' + parentId).find('select')
        .each(function () {
            $(this).val(null).trigger('change');
        });
};


var updateInputToRow = function (element,fieldName) {
    var $tr = $(element).closest('tr');
    var $row = FooTable.getRow($tr);    
    if (!fieldName)
        fieldName = $(element).attr("data-bf");
    if ($row && fieldName) {
        var rowVal = $row.val();
        rowVal[fieldName] = $(element).val();
        $row.val(rowVal, false);
    }
    return $row;
};

var saveFooTableRows = function (tableId) {
    var ft = FooTable.get('#' + tableId );
    if (!ft) {
        alert("invalid table");
        return null;
    }

    var rows = [];
    $.each(ft.rows.all, function (i, row) {
        rows.push(row.val());
    });
    return rows;
}

var isAny = function (arr) {
    if (arr && arr.length > 0 && Array.isArray(arr))
        return true;
    return false;
}

var parseJSONError = function (error) {
    var errorMessage = "";
    if (error) {
        if (error.responseJSON) {
            if (error.responseJSON.Message) 
                errorMessage = error.responseJSON.Message;            
            else 
                errorMessage = JSON.stringify(error.responseJSON);
        }
        else {
            errorMessage = JSON.stringify(error);
        }
    }
    else
        errorMessage = "Unknown error";
    return errorMessage;
}

var showError = function (errorMessage, errorTitle, errorContainer) {
    var htmlError = '';
    if (errorTitle && errorTitle != '')
        htmlError += "<h4>" + errorTitle + "</h4>";
    
    if (htmlError != '') {
        if (!errorContainer)
            errorContainer = "calloutErrorMessage";
        htmlError = "<div class='callout callout-danger'>" + htmlError + "</div>";
        $('#' + errorContainer).html(htmlError);
        $('#' + errorContainer).show();
        return;
    }
    hideError(errorContainer);
};
var hideError = function (errorContainer) {
    if (!errorContainer)
        errorContainer = "calloutErrorMessage";
    $('#' + errorContainer).html('');
    $('#' + errorContainer).hide();
};


var xShowHidePassword = function (inputGroupId) {
    var $a = $("#" + inputGroupId + " a");
    var $i = $("#" + inputGroupId + " i");
    var $input = $("#" + inputGroupId + " input");

    $a.on('click', function (event) {
        event.preventDefault();
        if ($input.attr("type") === "text") {
            $input.attr('type', 'password');
            $i.addClass("fa-eye-slash");
            $i.removeClass("fa-eye");
        }
        else if ($input.attr("type") === "password") {
            $input.attr('type', 'text');
            $i.removeClass("fa-eye-slash");
            $i.addClass("fa-eye");
        }
    });
};

var initFooTableEdit = function (tableId, title, allowAdd, allowEdit, allowDelete, rows, columns, keys, editorElement,rowValidFunction) {

    var readonly = !allowAdd && !allowEdit && !allowDelete,
        hasKey = false, editMode = "",        
        editorTitleElementId = tableId + "_editorTitle",
        editorFooterElementId = tableId + "_editorFooter",
        editorModalElementId = tblId + "_editorModal",
        editorFormId = tblId + "_editorForm",
        editorSaveCloseButtonId = tblId + "_btnSaveRowAndClose",
        editorSaveButtonId = tblId + "_btnSaveRow",
        editorResetButtonId = tblId + "_btnReset";
        

    if (keys) {
        if (Array.isArray(keys) && keys.length > 0)
            hasKey = true;
        else if (typeof keys === "string" && keys.length > 0) {
            hasKey = true;
            keys[0] = keys;
        }
    }
    else
        keys = [];

    editorElement = extend({        
        addRowButtonCaption: "Tambah",
        addRowCaption: "Tambah",
        editRowButtonCaption: "Ubah",
        editRowCaption: "Ubah",
        deleteRowButtonCaption: "Hapus",
        deleteRowCaption: "Hapus",
        editorBody:$('#' + tblId + "_editorBody")
    },
    editorElement);

    generateEditorForm(editorElement.editorBody);
    

    var $modal = $('#' + editorModalElementId),
        $editor = $('#' + editorFormId),
        $editorTitle = $('#' + editorTitleElementId),
        ft = FooTable.init('#' + tableId, {
            editing: {
                enabled: !readonly,
                alwaysShow: !readonly,
                allowAdd: allowAdd,
                allowEdit: allowEdit,
                allowDelete: allowDelete,
                addRow: function () {
                    if (prepareAdd())                    
                        $modal.modal('show');
                    
                },
                editRow: function (row) {
                    if (prepareEdit(row))
                        $modal.modal('show');
                },
                deleteRow: function (row) {
                    if (prepareDelete(row))
                        $modal.modal('show');
                }
            },
            columns: columns
        });

    if (ft && rows) {
        if (keys && keys.length > 0) {
            rows.forEach(function (value) {
                value.key = helper.footableRowKey(value, keys);
            });
        }
        ft.rows.load(rows);
    }

    var prepareAdd = function () {
        if (allowAdd) {
            $modal.removeData('row');
            resetElements(editorFormId);
            $('#' + editorTitleElementId).text(editorElement.addRowCaption + " " + editorElement.title);
            editMode = "add";
            setEditingKeyInputReadOnly(editMode);
            return true;
        }
        return false;
    };

    var prepareEdit = function (row) {
        if (allowEdit) {
            $modal.removeData('row');
            resetElements(editorElement.editor);
            $editorTitle.text(editorElement.editRowCaption + " " + editorElement.title);
            xFormDisplay(editorElement.editor, row.val());
            $modal.data('row', row);
            editMode = "edit";
            setEditingKeyInputReadOnly(editRow);
            return true;
            
        }
        return false;
    }; 

    var prepareDelete = function (row) {
        if (allowDelete) {
            $modal.removeData('row');
            resetElements(editorElement.editor);
            $editorTitle.text(editorElement.deleteRowCaption + " " + editorElement.title);
            xFormDisplay(editorElement.editor, row.val());
            $modal.data('row', row);
            editMode = "delete";
            setEditingKeyInputReadOnly(editRow);
            return true;
        }
        return false;
    };

    $('.editorbutton').on('click', function () {
        var dataId = $(this).data("id");
        switch (dataId) {
            case "reset":
                if (editMode === "add")
                    resetElements(editorFormId);
                break;
            case "save":
                if (saveRow())
                    if (editMode === "delete")
                        $modal.hide();
                    else if (editMode === "add" || editMode === "edit")
                        prepareAdd();
                break;
            case "saveclose":
                if (saveRow())
                    $modal.hide();
                break;
            case "close":
                $modal.hide();
                break;
        }
    });

    var saveRow = function () {
        var row = $modal.data('row');
        if (editMode === "delete") {
            row.delete();
            editMode = "";
            return true;
        }
        var updatedRow = xFormSave(editorFormId);
        if (rowValidFunction && !rowValid(rowValidFunction, updatedRow)) 
            return false;

        if (editMode === "edit")
            row.val(updatedRow);
        else if (editMode === "add") {
            updatedRow.key = rowKey(updatedRow);
            if (rowExist(updatedRow)) {
                alert('Data sudah ada');
                return false;
            }
            ft.rows.add(updatedRow);
        }
        editMode = "";
        return true;
    };

    var rowExist = function (values) {
        var rowExistX = false;
        if (keys && keys.length > 0) {
            var ft = FooTable.get('#' + tableId);
            if (ft) {
                $.each(ft.rows.all, function (i, row) {
                    var rowVal = row.val();
                    if (values.key === rowVal.key) {
                        rowExistX = true;
                        return;
                    }
                });
            }
        }
        return rowExistX;
    };

    var rowKey = function (values) {
        if (keys && keys.length>0)
            return helper.footableRowKey(values, keys);
        return null;
    };

    var setEditingKeyInputReadOnly = function (mode) {
        if (!keys || keys.length<=0)        
            return;
        keys.forEach(function (key) {
            var inputs = $("#" + formId).find("[data-bf]");
            inputs.each(function (index, input) {
                var bindingField = $(input).attr("data-bf"),
                    elementType = input.nodeName.toLowerCase(),
                    readonlyAttr = "readonly";

                if (elementType !== "input")
                    readonlyAttr = "disabled";
                
                if (bindingField === key)
                    $(input).attr(readonlyAttr) = true;
                else
                    $(input).removeAttr(readonlyAttr);
            });
        });
    };
    
    var rowValid = function (functionName, values) {
        if (!functionName || !values)
            return true;
        return functionName(values);
    };

    var generateEditorForm = function (editorBody) {
        var modalBody = $("<div class='modal-body'></div>").append(editorBody);
        var modalHeader =
            $("<div class='modal-header'>" +
            "<button type='button' class='close' data-dismiss='modal' aria-label='Close'> <span aria-hidden='true'>×</span></button>" +
            "<h4 class='modal-title' id='" + editorTitleElementId + "'></h4>" +
                "</div>");
        var modalFooter =
            $("<div class='modal-footer' id='" + editorFooterElementId + "'>" +
                "<button type='button' class='btn btn-primary editorbutton' data-id='save' id='" + editorSaveButtonId + "'>Simpan & Tambah</button>" +
                "<button type='button' class='btn btn-primary editorbutton' data-id='saveclose' id='" + editorSaveCloseButtonId + "'>Simpan & Tutup</button>" +
                "<button type='button' class='btn btn-default editorbutton' data-id='reset' id='" + editorResetButtonId +"'>Reset</button>" +
                "<button type='button' class='btn btn-default' data-dismiss='modal'>Tutup</button>" +
            "</div>");
        var editor =
            $("<div class='modal fade' id='" + editorModalElementId + "' tabindex='-1' role='dialog' aria-labelledby='" + editorTitleElementId + "'>" +
                "<style scoped>" +
                ".form-group.required .control-label:after {" +
                "content:'*';" +
                "color:red;" +
                "margin-left: 4px;" +
                "}" +
                "</style>" +
                "<div class='modal-dialog' role='document'>" +
                "<form class='modal-content form-horizontal' id='" + editorFormId + "'></form>" +
                "</div>" +
                "</div>").append(modalHeader, modalBody, modalFooter);
            
        $("body").append(editor);
    };

  
};


var displayFile = function (propertyName, fileID) {
    let fileInput = $("#i" + propertyName);
    let fileInputName = $("#i" + propertyName + "Name");
    let fileUploader = $("#i" + propertyName + "Upload");
    let fileDisplay = $("#i" + propertyName + "Display");

    if (fileID != null) {
        helper.callAjaxRequestJson("api/file/getinfo/" + fileID, null, 'get',
            function (responseData) {
                if (responseData.FileID != undefined) {
                    fileInput.val(responseData.FileID);
                    fileInputName.val(propertyName);
                    fileUploader.hide();
                    fileDisplay.show();
                }
            },
            function (error) {
                console.log("Error on Upload " + description);
                console.log(error);
            }
        );
    }
};

var currentlyUploading = [];
var InitializeFileUploader = function (propertyName, readonly) {
    let fileInput = $("#i" + propertyName);
    let fileInputName = $("#i" + propertyName + "Name");
    let fileUploader = $("#i" + propertyName + "Upload");
    let fileDisplay = $("#i" + propertyName + "Display");
    let btnDelete = $("#i" + propertyName + "BtnDelete");



    fileInputName.on("click", function () {
        if (fileInput.val() == "")
            return;
        helper.callAjaxRequestJson("api/file/getbase64/" + fileInput.val(), null, 'get',
            function (data) {
                var bytes = base64ToArrayBuffer(data.Base64);
                var blob = new Blob([bytes], { type: data.MimeType });
                //var blob = new Blob([bytes]);
                var link = window.URL.createObjectURL(blob);
                var windowName = Math.random().toString();
                window.open(link, windowName);
                
            },
            function (error) {
                console.log(error);
            }
        );
    });

    if (readonly) {
        fileInputName.css("width", "100%");
        fileUploader.hide();
        btnDelete.hide();
        fileDisplay.show();
        true;
    }

    fileUploader.on("change", function () {
        if (fileUploader.val() == "")
            return;

        let file = fileUploader[0].files[0];
        let description = fileUploader.data("file-description");

        // add to the currently uploading list
        if (typeof (currentlyUploading) == "undefined")
            currentlyUploading = [];
        currentlyUploading.push(propertyName);
        // ---

        fileUploader.prop("disabled", true);

        

        callAjaxRequestAsForm("api/fileupload/file", { File: file, Description: description }, 'post',
            function (responseData) {
                fileUploader.prop("disabled", false);
                fileInput.val(responseData.ID);
                fileInputName.val(responseData.Name);
                fileUploader.hide();
                fileDisplay.show();

                currentlyUploading.splice(currentlyUploading.indexOf(propertyName), 1); // remove from currently uploading list
            },
            function (error) {
                fileUploader.prop("disabled", false);
                alert("Error on Upload " + description);
                console.log("Error on Upload " + description);
                console.log(error);

                currentlyUploading.splice(currentlyUploading.indexOf(propertyName), 1); // remove from currently uploading list
            }
        );
    });

    btnDelete.on("click", function () {
        if (confirm("Attachment will be deleted, continue?")) {
            fileUploader.val("");
            fileInput.val("");
            fileInputName.val("");
            fileUploader.show();
            fileDisplay.hide();
        };
    });
};

var base64ToArrayBuffer = function (base64) {
    var binaryString = window.atob(base64);
    var binaryLen = binaryString.length;
    var bytes = new Uint8Array(binaryLen);
    for (var i = 0; i < binaryLen; i++) {
        var ascii = binaryString.charCodeAt(i);
        bytes[i] = ascii;
    }
    return bytes;
};


var callAjaxRequestAsForm = function (apiRouteUrl, data, ajaxMethod, successFunction, errorFunction) {
        if (!ajaxMethod)
            ajaxMethod = "get";
        let formData = new FormData();
        $.each(data, function (key, value) {
            formData.append(key, value);
        });
        $.ajax({
            type: ajaxMethod,
            url: _API_URL + apiRouteUrl,
            data: formData,
            processData: false,
            contentType: false,
            headers: { "Authorization": authServices.getBearerToken() },            
            success: successFunction,
            error: errorFunction
        });
            
}

function disableDataList(Id) {
    var option = $('#' + Id).data();
    option.readOnly = true;
    $('#' + Id).flexdatalist(option);

    //$("#" + id).flexdatalist("destroy");
    //$("#" + id).attr("readonly", true);
    //xDataListInit(id, null, true);
    
}


function enableDataList(Id) {
    
    //var value = $("#" + id).val();
    //$("#" + id).flexdatalist("destroy");
    //$("#" + id).removeAttr("readonly");
    //xDataListInit(id, value, false);

    var option = $('#' + Id).data();
    option.readOnly = false;
    $('#' + Id).flexdatalist(option);
}

var tickCrossTabulator = function (cell, formatterParams, onRendered) {
    var value = cell.getValue(),
        element = cell.getElement(),
        empty = formatterParams.allowEmpty,
        truthy = formatterParams.allowTruthy,
        tick = typeof formatterParams.tickElement !== "undefined" ? formatterParams.tickElement : '<svg enable-background="new 0 0 24 24" height="14" width="14" viewBox="0 0 24 24" xml:space="preserve" ><path fill="#2DC214" clip-rule="evenodd" d="M21.652,3.211c-0.293-0.295-0.77-0.295-1.061,0L9.41,14.34  c-0.293,0.297-0.771,0.297-1.062,0L3.449,9.351C3.304,9.203,3.114,9.13,2.923,9.129C2.73,9.128,2.534,9.201,2.387,9.351  l-2.165,1.946C0.078,11.445,0,11.63,0,11.823c0,0.194,0.078,0.397,0.223,0.544l4.94,5.184c0.292,0.296,0.771,0.776,1.062,1.07  l2.124,2.141c0.292,0.293,0.769,0.293,1.062,0l14.366-14.34c0.293-0.294,0.293-0.777,0-1.071L21.652,3.211z" fill-rule="evenodd"/></svg>',
        cross = typeof formatterParams.crossElement !== "undefined" ? formatterParams.crossElement : '<svg enable-background="new 0 0 24 24" height="14" width="14"  viewBox="0 0 24 24" xml:space="preserve" ><path fill="#CE1515" d="M22.245,4.015c0.313,0.313,0.313,0.826,0,1.139l-6.276,6.27c-0.313,0.312-0.313,0.826,0,1.14l6.273,6.272  c0.313,0.313,0.313,0.826,0,1.14l-2.285,2.277c-0.314,0.312-0.828,0.312-1.142,0l-6.271-6.271c-0.313-0.313-0.828-0.313-1.141,0  l-6.276,6.267c-0.313,0.313-0.828,0.313-1.141,0l-2.282-2.28c-0.313-0.313-0.313-0.826,0-1.14l6.278-6.269  c0.313-0.312,0.313-0.826,0-1.14L1.709,5.147c-0.314-0.313-0.314-0.827,0-1.14l2.284-2.278C4.308,1.417,4.821,1.417,5.135,1.73  L11.405,8c0.314,0.314,0.828,0.314,1.141,0.001l6.276-6.267c0.312-0.312,0.826-0.312,1.141,0L22.245,4.015z"/></svg>';

    if (truthy && value || value === true || value === "true" || value === "True" || value === 1 || value === "1") {
        element.setAttribute("aria-checked", true);
        return tick || "";
    } else {
        if (empty && (value === "null" || value === "" || value === null || typeof value === "undefined")) {
            element.setAttribute("aria-checked", "mixed");
            return "";
        } else {
            element.setAttribute("aria-checked", false);
            return cross || "";
        }
    }
};


let maxElement = (arrayofObject, propertyName) => arrayofObject.reduce((m, x) => m[propertyName] > x[propertyName] ? m : x);
let minElement = (arrayofObject, propertyName) => arrayofObject.reduce((m, x) => m[propertyName] < x[propertyName] ? m : x);

var progressLoading = {};
var showProgressWaiting = function (elementId, waitingText) {
    if (!elementId)
        elementId = "divContentContainer";
    if (!waitingText)
        waitingText = "Harap tunggu...";

    var isLoaded = progressLoading[elementId];
    if (!isLoaded) {
        progressLoading[elementId] = true;
        $('#' + elementId).waitMe({
            effect: 'ios',
            text: waitingText,
            bg: 'rgba(255,255,255,0.7)',
            color: '#000'
        });
    }
}


var hideProgressWaiting = function (elementId) {
    if (!elementId)
        elementId = "divContentContainer";

    var isLoaded = progressLoading[elementId];
    if (isLoaded) {
        $('#' + elementId).waitMe('hide');
        progressLoading[elementId] = false;
    }
}