

(
	function($){
		
		var ctTab = Object();
		var ctKey = Object();
		var tabList = Object();
		var tabPageTitles = {};
		var ctTabPageId = "";

		$.fn.dynatabs = function(options){
			
			var tabs = $('#' + options.tabBodyID + "_Nav");
			
			var settings = $.extend({
				
				tabBodyID : 'tabbody',
				defaultTab: 0, //default is 0 - first tab
				deactiveClass : 'unselected',
				activeClass : 'active',
				showCloseBtn : false, //shows the close button on the tabs
				closeableClass : 'closeable',
				tabLoaderClass : 'tabLoader',
				confirmDelete : false,
				confirmMessage : 'Delete Tab?',
				switchToNewTab : true,
				debug : false
				
			},options);
			
			$.fn.debug = function(message){
				if(settings.debug)
				{
					if($.browser.webkit || $.browser.mozilla)
					{
						console.log(message);
					}
					else
					{
						alert('You have debug enabled in settings. It is only supported in Firefox and Chrome now.');
					}
				}
			};
			
			/**
			 * Function to show a tab
			 */
			$.fn.showTab = function(event){
			
				var key = event.data.key;
				
				

				//if(ahref != null)
				//{
				//	$.fn.activateTab($(ahref).attr('href'), ahref, tab);
				//}
				if (key && key.length > 0)
				{
					$.fn.activateTab(key);
                }
				else
				{
					$.fn.debug('unable to show a null tab');
				}
			};
			
			$.fn.closeTab = function(event){
				
				if(event.data.key != null)
				{
					$.fn.debug('deleting tab');
					var canDelete = false;					
					//check if the tab can be deleted
					if(settings.confirmDelete)
					{
						var confirmMsg = settings.confirmMessage;
						if (!confirmMsg)
							confirmMsg = "";
						if (tabPageTitles[event.data.key])
							confirmMsg += " " + tabPageTitles[event.data.key];

						if (!confirmMsg)
							confirmMsg = "Tutup tab";
						if(confirm(confirmMsg))
						{
							canDelete = true;
						}
					}
					else
					{
						canDelete = true;
					}
					
					//delete the tab
					if(canDelete)
					{
						//find the ahref
						
						$prevTab = $('#li' + event.data.key).prev();
						if (ctTabPageId === event.data.key) {
							var a = $prevTab.find("a")[0];
							$.fn.activateTab($(a).data('tabpage'));
						}

						$('#li' + event.data.key).remove();
						$('#' + event.data.key).remove();

						
					}
				}
				
				return false;
			};
			
			
			
			$.fn.hideTab = function(key){
				
				if(key && key.length>0)
				{
					$prevTab = $('#li' + key).prev();
					if (ctTabPageId === key) {
						var a = $prevTab.find("a")[0];
						$.fn.activateTab($(a).data('tabpage'));
					}

					$('#li' + key).hide();
					$('#' + key).hide();

				}
				
			};
			
			/**
			 * Bind the on-click of each tab to showtab function
			 */
			$.fn.bindTabs = function(){
				$.each(tabs.find("li > a"), function(idx, a){
					//bind click function of the tab header

					var key = $(a).data('tabpage');
					$(a).bind('click',{ key:key}, $.fn.showTab);					
					//show the close button if enabled in settings
					if(settings.showCloseBtn && $(a).find("span").length == 0)
					{
						$.fn.addCloseBtn(a);
					}
				});
			};
			
			$.fn.addCloseBtn = function(ahref){
				
				if(ahref != null)
				{
					this.debug('adding close button');
					var key = $(ahref).data('tabpage');
					if(key.length > 0)
					{
						var closeButton = $('<span>&nbsp;&nbsp;<i class="glyphicon glyphicon-remove"></i></span>');
						$(closeButton).data('tabpage', key);
				
						$(ahref).append(closeButton);						
						$(ahref).find("span").bind('click', { key: key } ,$.fn.closeTab);
					}
				}
				
			};
			
			$.fn.addTabLoader = function(ahref){
				
				if(ahref != null)
				{
					this.debug('adding tab loader button');
					var key = $(ahref).data('tabpage');
					if(key.length > 0)
					{
						$(ahref).append('<span class="' + settings.tabLoaderClass + '"></span>');
						$(ahref).find("span").bind('click', { key: key } ,$.fn.closeTab);
					}
				}
				
			};

			$.fn.deactivateAllTabs = function (tabId,tabBody) {
				$('#' + tabBody).children('.tab-pane').removeClass(settings.activeClass);
				$('#' + tabId).children().removeClass(settings.activeClass);

			};

			$.fn.activateTab = function (tabPageId) {
				if (!tabPageId || tabPageId.length <= 0)
					return;
				if (ctTabPageId && ctTabPageId.length > 0 && ctTabPageId !== tabPageId) {
					$('#li' + ctTabPageId).removeClass(settings.activeClass);
					$('#' + ctTabPageId).removeClass(settings.activeClass);
				}
				ctTabPageId = tabPageId;
				$('#li' + ctTabPageId).addClass(settings.activeClass);
				$('#' + ctTabPageId).addClass(settings.activeClass);
			};

			$.fn.addNewTab = function(defaults, tabBody){
				
				settings.tabBodyID = tabBody;
				tabs = $("#"+defaults.tabID);
				$.fn.debug('Tab ID : ' + defaults.tabID);

				var tabPageId = "";
				if (typeof defaults.tabPageId !== 'undefined')
					tabPageId = defaults.tabPageId;
				if (!tabPageId) {
					var len = tabs.find("li").length + 1;
					tabPageId = 'tabview' + '_' + settings.tabBodyID + len;
				}

				

				tabPageTitles[tabPageId] = defaults.tabTitle;

				//create the li tag
				

				var li = $("<li/>");
				($(li)).attr("id", "li" + tabPageId);

				var a = $("<a />");
				$(a).attr('href', '#' + tabPageId);
				$(a).text(defaults.tabTitle);
				$(a).data('tabpage', tabPageId);
				$(li).append(a);
				tabs.append(li);
				
				//create data div
				var div = $("<div class='tab-pane'/>");
				$(div).attr('id', tabPageId);

				
				
				if(defaults.type === 'html')
				{
					if(defaults.html != null && defaults.html.length > 0)
					{
						var content = "<div class='panel panel-default'><div class='panel-body'>" + defaults.html + "</div></div>";
						//create the title tag
						$(div).html(content);
						//append to tab list
						
						//$(div).addClass(settings.activeClass);
						$('#' + settings.tabBodyID).append(div);
						
						//bind all click functions to tab headers
						$.fn.bindTabs();
						$.fn.activateTab(tabPageId);
					}
					else
					{
						$.fn.debug('No HTML content found for the new tab. Skipping new tab creation');
					}
				}
				else if(defaults.type === 'div')
				{
					if(defaults.divID != null && defaults.divID.length > 0 && $("#" + defaults.divID)[0])
					{
						var content = "<div class='panel panel-default'><div class='panel-body'>" + $("#" + defaults.divID).html()+ "</div></div>";
						//create the title tag
						$(div).html(content);
						//append to tab list

						$(div).html();
						//append to tab list						
						//$(div).addClass(settings.activeClass);
						$('#' + settings.tabBodyID).append(div);
						
						//bind all click functions to tab headers
						$.fn.bindTabs();
						$.fn.activateTab(tabPageId);
					}
					else
					{
						$.fn.debug('No Div found with id ' + defaults.divID + ' in the body for html content.');
					}
				}
				else if(defaults.type === 'ajax')
				{
					if(defaults.url.length > 0 && defaults.method.length > 0 && defaults.dtype.length > 0)
					{
						
						$(div).html("Loading...");
						//append to tab list
						
						//$(div).addClass(settings.activeClass);
						$('#' + settings.tabBodyID).append(div);
						
						
						//bind all click functions to tab headers
						$.fn.bindTabs();
						$.fn.activateTab(tabPageId);
						//remove close button
						$(a).find("span").remove();
						//add tab loader
						$.fn.addTabLoader(a);
						
						$.ajax({
							url : defaults.url,
							type: defaults.method,
							data: defaults.params,
							dataType: defaults.dtype
						}).done(function(response){
							$.fn.debug('obtained response..');
							var content = "<div class='panel panel-default'><div class='panel-body'>" + response + "</div></div>";
							//create the title tag
							$(div).html(content);

							
							//remove tab loader
							$(a).find("span").remove();
							$.fn.addCloseBtn(a);
						}).fail(function(){
							alert('Failed to load the ajax page');
							$(div).remove();
							$(li).remove();
						});
						
					}
					else
					{
						$.fn.debug('Could not load the ajax url given. Please verify parameters');
					}
				}
			};
			
			$.fn.initTabs = function(){
				
				//hide all tabs other than the default tab index
				var ct = 0;
				$.fn.debug('Tab Body ID -->' + settings.tabBodyID);
				$.each($("#" + settings.tabBodyID + " > div"), function(idx, div){
					if(ct != settings.defaultTab)
					{
						$.fn.debug('Hiding -- ' + div.outerHTML);
						$(div).hide();
						ct = ct + 1;
					}
					else
					{
						$.fn.debug('Showing -- ' + div.outerHTML);
						ct = ct + 1;
					}	
				});
				
				//add the selected class to the title also
				$.fn.debug(tabs);
				$.fn.debug('Tab Lengths --> ' + tabs.find("li").length);
				if(settings.defaultTab < tabs.find("li").length)
				{
					this.debug('setting active tab --> Index ' + settings.defaultTab);
					$(tabs.find("li")[settings.defaultTab]).removeClass(settings.deactiveClass);
					$(tabs.find("li")[settings.defaultTab]).addClass(settings.activeClass);
					ctTab[tabs.attr('id')] = $(tabs.find("li")[0]).find("a");
					ctKey[tabs.attr('id')] =  $(tabs.find("li")[0]).find("a").attr('href');
				}
				else
				{
					$.fn.debug('Index ' + settings.defaultTab + ' does not map to li');
				}
				
				//add close buttons as neccessary to all tabs and bind clicks
				this.bindTabs();
				//add tabs to the list
				tabList[tabs.attr('id')] = settings.tabBodyID;
			};
			
			this.initTabs();
		};
		
		$.addDynaTab = function(options){

			var settings = $.extend({
							type : 'html', //html or ajax or div
							url : '', //mandatory for ajax requests
							html : '', // mandatory for html content
							divID : '', //mandatory for div method
							method: 'post', //get or post for ajax urls
							dtype : 'html', //json/html/text for ajax urls
							params: {},
							tabID: '',
							tabPageId: '',
							tabTitle : 'New Tab'
						},options);
			if(settings.tabID.length > 0)
			{
				$.fn.addNewTab(settings, tabList[settings.tabID]);
			}
			else
			{
				$.fn.debug('Please enter the tab id parameter');
			}
			
		};
		
		$.fn.removeDynaTab = function(options){
			
		};
		
	}(jQuery)
);