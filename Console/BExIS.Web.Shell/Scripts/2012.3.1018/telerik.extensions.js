/*
* @Copyright (c) 2011 John DeVight
* Permission is hereby granted, free of charge, to any person
* obtaining a copy of this software and associated documentation
* files (the "Software"), to deal in the Software without
* restriction, including without limitation the rights to use,
* copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the
* Software is furnished to do so, subject to the following
* conditions:
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
* OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
* HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
* WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
* OTHER DEALINGS IN THE SOFTWARE.
*/

/// <summary>
/// Extend the Telerik Extensions for ASP.NET MVC 2012 Q1.
/// </summary>
(function ($) {
    // Was the telerik.list.min.js added to the page by the telerik scrip registrar?
    if ($.telerik.dropDownList != undefined || $.telerik.combobox != undefined) {
        // Extend the dropDownList plugin.
        var dropDownListExtensions = {
            dataBindAsTable: function (e) {
                var ctrl = this;
                var bindData = [];

                ctrl._dataBindAsTableSettings = e;
                ctrl._dropdownInitialized = false;

                // Bind the data to the list control.
                $.each(e.data, function (idx, record) {
                    bindData.push({ Value: record[e.valueField], Text: record[e.selectedField] });
                });
                ctrl.dataBind(bindData);

                // Format each list item as a table based on e.displayFields.
                $.each(ctrl.dropDown.$items, function (idx, item) {
                    $(item).empty();
                    var tr = $('<tr/>');
                    $.each(e.displayFields, function (dfdx, displayField) {
                        var $td = $('<td/>').text(e.data[idx][displayField.fieldName]);
                        if (displayField.style != undefined) {
                            $td.attr('style', displayField.style);
                        }
                        $td.css('white-space', 'nowrap');
                        tr.append($td);
                    });
                    var table = $('<table/>')
                        .attr({
                            'cellpadding': '0px',
                            'cellspacing': '0px'
                        });
                    table.append(tr);
                    $(item).append(table);
                });

                ctrl._openHandler = function (e) {
                    setTimeout("$('#" + ctrl.element.id + "').data('" + ($(ctrl.element).parent().hasClass('t-dropdown') ? "tDropDownList" : "tComboBox") + "').formatDropDownAsTable();", 50);
                }
                $(ctrl.element).on('open', ctrl._openHandler);
            },
            /// <summary>
            /// When the dropdown is displayed in a dropdownlist; for each item, format the cells in each table
            /// so that each column has the same width and allow the table to be as wide as it needs to be.
            ///
            /// THIS IS AN INTERNAL FUNCTION AND IS NOT MEANT TO BE CALLED FROM ANYWHERE BESIDES THE dataBindAsTable
            /// FUNCTION.
            /// </summary>
            formatDropDownAsTable: function () {
                var ctrl = this;
                var $animationContainer = ctrl.dropDown.$element.closest('div.t-animation-container');

                $animationContainer.css('width', '');
                ctrl.dropDown.$element.css('width', '');

                if (ctrl._dropdownInitialized == false) {
                    var widths = [];

                    $.each(ctrl.dropDown.$element.find('li'), function (idx, item) {
                        $.each($(item).find('td'), function (tdx, td) {
                            var w = $(td).width() + 1;
                            if (tdx >= widths.length) {
                                widths.push(w);
                            } else {
                                if (widths[tdx] < w) {
                                    widths[tdx] = w;
                                }
                            }
                        });
                    });
                    $.each(ctrl.dropDown.$element.find('li'), function (idx, item) {
                        $.each($(item).find('td'), function (tdx, td) {
                            $(td).width(widths[tdx]);
                        });
                    });

                    if (ctrl._dataBindAsTableSettings.height != undefined) {
                        ctrl.dropDown.$element.height(ctrl._dataBindAsTableSettings.height);
                    }

                    ctrl._dropdownInitialized = true;
                }

                if ($animationContainer.height() < ctrl.dropDown.$element.find('ul').height()) {
                    ctrl.dropDown.$element.css('width', (ctrl.dropDown.$element.width() + $.getScrollbarWidth()));
                }
            },
            /// <summary>
            /// Disable list items in the dropdown list.
            /// </summary>
            /// <param type="array" name="items">Array of json objects where each json object has the following attributes:
            /// 1. text: the text that is displayed for a dropdown list item.
            /// OR
            /// 2. value: the value for a dropdown list item.
            /// </param>
            /// <example>
            /// $('#dropdownlist').data('tDropDownList').dataBind(
            ///   { Text:'Red', Value:'1' },
            ///   { Text:'Green', Value:'2' },
            ///   { Text:'Blue', Value:'3' },
            ///   { Text:'Yellow', Value:'4' },
            ///   { Text:'Orange', Value:'5' },
            /// );
            ///
            /// $('#dropdownlist').data('tDropDownList').disableListItems([{ text:'Red' },{ text:'Orange' }]);
            ///
            /// $('#dropdownlist').data('tDropDownList').disableListItems([{ value:'1' },{ text:'5' }]);
            /// </example>
            disableListItems: function (disableItems) {
                var ctrl = this;

                // Loop through the list of disabled items and look for them in the list of dropdown items.
                $.each(ctrl.dropDown.$items, function (idx, item) {
                    $.each(disableItems, function (didx, disableItem) {
                        if ((disableItem.text != undefined && disableItem.text == $(item).text()) ||
                            (disableItem.value != undefined && disableItem.value == ctrl.data[idx].Value)) {
                            $(item).removeClass('t-item').css('color', 'Grey');
                        }
                    });
                });
            },
            /// <summary>Select an item in the dropdown based on the value for an item.</summary>
            /// <param type="string" name="value">Value of the item to be selected.</summary>
            selectByValue: function (value) {
                var dropdown = this;

                $.each(dropdown.data, function (idx, item) {
                    if (item.Value == value) {
                        dropdown.select(idx);
                        return false;
                    }
                })
            },
            /// <summary>Select an item in the dropdown based on the text for an item.</summary>
            /// <param type="string" name="text">Text of the item to be selected.</summary>
            selectByText: function (text) {
                var dropdown = this;

                $.each(dropdown.data, function (idx, item) {
                    if (item.Text == text) {
                        dropdown.select(idx);
                        return false;
                    }
                })
            },
            /// <summary>Suppress/Unsuppress the call to the OnChange client event handler.</summary>
            /// <param type="bool" name="suppress">Flag indicating whether to suppress or unsupress the call to the OnChange client event handler.</summary>
            suppressOnChange: function (suppress) {
                var dropdown = this;
                if (dropdown.onChange != undefined) {
                    if (suppress) {
                        $(dropdown.element).unbind('valueChange', dropdown.onChange);
                    } else {
                        var bind = true;
                        if ($(dropdown.element).data('events') != undefined) {
                            $.each($(dropdown.element).data('events'), function (edx, event) {
                                $.each(event, function (hdx, eventHandler) {
                                    if (eventHandler.type == 'valueChange' && eventHandler.handler == dropdown.onChange) {
                                        bind = false;
                                        return false;
                                    }
                                });
                            });
                        }
                        if (bind) {
                            $(dropdown.element).bind('valueChange', dropdown.onChange);
                        }
                    }
                }
            }
        }

        if ($.telerik.dropDownList != undefined) {
            // Add the extensions to the dropDownList plugin.
            $.extend(true, $.telerik.dropDownList.prototype, dropDownListExtensions);
        }

        if ($.telerik.combobox != undefined) {
            // Add the extensions to the combobox plugin.
            $.extend(true, $.telerik.combobox.prototype, dropDownListExtensions);
        }
    };

    // Was the tekerik.grid.min.js added to the page by the telerik script registrar?
    if ($.telerik.grid != undefined) {
        // Extend the grid plugin.
        var gridExtensions = {
            /// <summary>
            /// Hide a column in the grid.
            /// </summary>
            /// <param type="int" name="index">Zero based index for the column.</param>
            /// <example>
            /// $('#MyGrid').data('tGrid').hideColumn(1);
            /// </example>
            hideColumn: function (index) {
                this.toggleColumnVisibility({ index: index, visible: false });
            },
            /// <summary>
            /// Show a column in the grid.
            /// </summary>
            /// <param type="int" name="index">Zero based index for the column.</param>
            /// <example>
            /// $('#MyGrid').data('tGrid').showColumn(1);
            /// </example>
            showColumn: function (index) {
                this.toggleColumnVisibility({ index: index, visible: true });
            },
            /// <summary>
            /// Toggle the visibility of a column in the grid.
            /// </summary>
            /// <param type="json object" name="e">Json object with the following attributes:
            /// 1. index (int) - Zero based index for the column.
            /// 2. visible (bool) - true to show, false to hide.
            /// <example>
            /// $('#MyGrid').data('tGrid').toggleColumnVisibility({ index: 1, visible: true });
            /// </example>
            toggleColumnVisibility: function (e) {
                var grid = this;
                var display = e.visible ? '' : 'none';

                $(grid.$header[0].cells[e.index]).css('display', display);

                $.each(grid.$tbody[0].rows, function (idx, row) {
                    if (e.index < row.cells.length) {
                        $(row.cells[e.index]).css('display', display);
                    }
                })

                if (grid.$footer.length > 0) {
                    $(grid.$footer.find('td')[e.index]).css('display', display);
                }
            },
            /// <summary>
            /// Enable client side sorting in the grid.
            /// </summary>
            /// <param type="json" name="o">JSON object with the following attributes:
            /// onlyColsDefinedInMarkup (defaults to true) [optional]:
            //      true - sorting should only be enabled for columns
            ///         that have Sortable set to true for columns defined in the HTML markup.
            ///     false - sorting should be enabled for all columns regardless of what
            ///         columns have Sortable set in the HTML markup.
            /// </param>
            /// <example>
            /// $('#MyGrid').data('tGrid').enableClientSort();
            /// $('#MyGrid').data('tGrid').enableClientSort({ onlyColsDefinedInMarkup:false });
            /// </example>
            enableClientSort: function (o) {
                o = $.extend({
                    onlyColsDefinedInMarkup: true
                }, o);
                var grid = this;
                if (o.onlyColsDefinedInMarkup == false) {
                    $.each(grid.$header[0].cells, function (idx, cell) {
                        var tlink = $(cell).children('a.t-link');
                        if (tlink.length == 0) {
                            var $a = $('<a />')
                            .attr('class', 't-link t-state-default')
                            .css('padding-right', '0px');
                            $a.text($(cell).text());
                            cell.innerHTML = $('<div>').append($a.clone()).html();
                        }
                    })
                }
                this.sort = function (h) {
                    var data = grid.data;

                    var sort_by = function (reverse, primer, displayFor) {
                        reverse = (reverse) ? -1 : 1;
                        return function (a, b) {
                            a = displayFor(a);
                            b = displayFor(b);

                            if (typeof (primer) != 'undefined') {
                                a = primer(a);
                                b = primer(b);
                            }

                            if (a < b) return reverse * -1;
                            if (a > b) return reverse * 1;
                            return 0;
                        }
                    }

                    if (h.length > 0) {
                        this.orderBy = h;
                        $.each(grid.sorted, function (idx, col) {
                            var datatype = col.type == undefined ? "String" : col.type;
                            var handler = datatype == "Number"
                                ? col.format == "{0:c}"
                                    ? function (a) { if (a != undefined && a != null) { return parseFloat(a.replace('$', '')); } return a; }
                                    : function (a) { if (a != undefined && a != null) { return parseFloat(a); } return a; }
                                : datatype == "Date"
                                    ? function (a) { if (a != undefined && a != null) { return new Date(a).getTime(); } return a; }
                                    : function (a) { if (a == undefined || a == null) { a = ""; } return a.toString().toUpperCase() }; /* Default is String */
                            data.sort(sort_by(col.order == 'desc', handler, col.display));
                        });
                        grid.dataBind(data);
                    }
                }
            },
            /// <summary>
            /// Hide columns that are grouped.
            /// </summary>
            /// <example>
            /// $('#MyGrid').data('tGrid').hideGroupColumns();
            /// </example>
            hideGroupColumns: function () {
                var grid = this;
                grid.columnstyles = [];
                $(grid.element).find('div.t-grid-header table col').each(function (idx, col) {
                    grid.columnstyles.push($(col).attr('style'));
                });

                $.telerik.bind(this, {
                    dataBound: function (e) {
                        var grid = $(e.target).data('tGrid');
                        var cnt = grid.groups.length;
                        var hCols = $(grid.element).find('div.t-grid-header table col');
                        var cCols = $(grid.element).find('div.t-grid-content table col');
                        var gridUsesDivs = $(grid.element).find('div.t-grid-header').length == 1;

                        grid.showAllColumnHeaders();

                        if (gridUsesDivs) {
                            $.each(grid.columnstyles, function (idx, columnstyle) {
                                if (columnstyle == undefined) {
                                    $(hCols[idx + cnt]).removeAttr('style');
                                    $(cCols[idx + cnt]).removeAttr('style');
                                } else {
                                    $(hCols[idx + cnt]).attr('style', columnstyle);
                                    $(cCols[idx + cnt]).attr('style', columnstyle);
                                }
                            });
                        }

                        if (cnt > 0) {
                            var width = 0;
                            $.each(grid.groups, function (gdx, group) {
                                var columnTitle = group.title;
                                $.each(grid.columns, function (idx, col) {
                                    if (col.title == columnTitle) {
                                        width += $(hCols[idx + cnt]).width();
                                        grid.hideColumn(idx + cnt);
                                        if (gridUsesDivs) {
                                            for (var cdx = idx; cdx < grid.columns.length; cdx++) {
                                                $(hCols[cdx + cnt]).attr('style', $(hCols[cdx + cnt + 1]).attr('style'));
                                                $(cCols[cdx + cnt]).attr('style', $(cCols[cdx + cnt + 1]).attr('style'));
                                            }
                                        }
                                    }
                                });
                            });
                            if (gridUsesDivs) {
                                width += $(hCols[cnt]).width();
                                for (var cdx = 1; cdx <= cnt; cdx++) {
                                    $(hCols[hCols.length - cdx]).css('width', '0px');
                                    $(cCols[cCols.length - cdx]).css('width', '0px');
                                }
                                $(hCols[cnt]).width(width);
                                $(cCols[cnt]).width(width);
                            }
                        }
                    }
                });
            },
            showAllColumnHeaders: function () {
                var grid = this;

                $.each(grid.$header[0].cells, function (idx, cell) {
                    $(cell).css('display', '');
                });
            },
            /// <summary>
            /// Enable double-click on a row.
            /// </summary>
            /// <example>
            /// $('#MyGrid').data('tGrid').enableRowDblClick();
            /// </example>
            enableRowDblClick: function (e) {
                if (e.callback != undefined) {
                    var grid = $(this.element);
                    grid.dblclick(function () {
                        if ($.browser.msie) {
                            var range = document.body.createTextRange();
                            range.setEndPoint('EndToStart', range);
                            range.select();
                        } else {
                            var selection = window.getSelection();
                            selection.removeAllRanges();
                        }
                        e.callback($(this).data('tGrid'));
                    });
                }
            }
        }

        // Add the extensions to the grid plugin.
        $.extend(true, $.telerik.grid.prototype, gridExtensions);
    }

    // Was the tekerik.tabstrip.min.js added to the page by the telerik script registrar?
    if ($.telerik.tabstrip != undefined) {
        // Extend the tabstrip plugin.
        var tabstripExtensions = {
            /// <summary>
            /// Get a tab.
            /// </summary>
            /// <param type="json object" name="o">json object with either the text or the index of the tab.</param>
            /// <return>jQuery object of the tab [li.t-item]</return>
            /// <example>
            /// var tab = $('#MyTabStrip').data('tTabStrip').getTab({ text: 'Tab 2' })
            /// var tab = $('#MyTabStrip').data('tTabStrip').getTab({ index: 1 })
            /// </example>
            getTab: function (o) {
                var tab = null;
                if (o.text != null) {
                    tab = $(this.element).children('.t-tabstrip-items').find('.t-item').find("a:contains('" + o.text + "')").parent();
                }
                else if (o.index != null) {
                    tab = $($(this.element).find('.t-item')[o.index]);
                }
                return tab;
            },
            /// <summary>
            /// Get index of tab.
            /// </summary>
            /// <param type="string" name="t">text of a tab.</param>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').getTabIndex('Tab 2')
            /// </example>
            getTabIndex: function (t) {
                var idx = 0;
                $.each($(this.element).children('.t-tabstrip-items').find('a.t-link'), function (i, a) {
                    if ($(a).text() == t) {
                        idx = i;
                        return false;
                    }
                })
                return idx;
            },
            /// <summary>
            /// Return a count on the number of tabs in the tabstrip.
            /// </summary>
            getTabCount: function () {
                return $(this.element).children('.t-tabstrip-items').find('a').length;
            },
            /// <summary>
            /// Hide a tab.
            /// </summary>
            /// <param type="json object" name="o">json object with either the text or the index of the tab.</param>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').hideTab({ text: 'Tab 2' })
            /// $('#MyTabStrip').data('tTabStrip').hideTab({ index: 1 })
            /// </example>
            hideTab: function (o) {
                var tab = this.getTab(o);
                if (tab != null) {
                    tab.css('visibility', 'hidden');
                    tab.css('display', 'none');
                }
            },
            /// <summary>
            /// Show a tab.
            /// </summary>
            /// <param type="json object" name="o">json object with either the text or the index of the tab.</param>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').showTab({ text: 'Tab 2' })
            /// $('#MyTabStrip').data('tTabStrip').showTab({ index: 1 })
            /// </example>
            showTab: function (o) {
                var tab = this.getTab(o);
                if (tab != null) {
                    tab.css('visibility', '');
                    tab.css('display', '');
                }
            },
            /// <summary>
            /// Select a tab.
            /// </summary>
            /// <param type="json object" name="o">json object with either the text or the index of the tab.</param>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').selectTab({ text: 'Tab 2' })
            /// $('#MyTabStrip').data('tTabStrip').selectTab({ index: 1 })
            /// </example>
            selectTab: function (o) {
                var tab = this.getTab(o);
                if (tab != null) {
                    this.select(tab);
                }
            },
            /// <summary>
            /// Change the text of a tab.
            /// </summary>
            /// <param type="json object" name="o">json object with either the text or the index of the tab and the newText for the tab.</param>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').setTabText({ text: 'Tab 2', newText: 'Second Tab' })
            /// $('#MyTabStrip').data('tTabStrip').setTabText({ index: 1, newText: 'Second Tab' })
            /// </example>
            setTabText: function (o) {
                var tab = this.getTab(o);
                if (tab != null) {
                    tab.find('a').text(o.newText);
                }
            },
            /// <summary>
            /// Add a tab.
            /// </summary>
            /// <param type="json object" name="t">json with the following properties:
            /// 1. text: text for the tab.
            /// 2. html: the html content for the tab.
            /// 3. url: url to retrieve the content for the tab.
            /// 4. data: data to post to the url.
            /// 5. complete: callback function to execute when the contents are loaded from the url.
            /// </param>
            /// <example>
            /// tabStrip.addTab({
            ///     text: 'New Tab',
            ///     url: "@Url.Content("~")TabStrip/GetTabContent",
            ///     data: { tabName: 'New Tab' },
            ///     complete: function() {
            ///         tabStrip.selectTab({ text: 'New Tab' });
            ///     }
            /// });
            /// </example>
            /// <example>
            /// var $span = $('<span/>').append('Content for the new tab');
            /// var tabStrip = $('#SampleTabStrip').data('tTabStrip');
            /// tabStrip.addTab({ text: 'New Tab', html: $span[0] });
            /// </example>
            addTab: function (t) {
                var tabstrip = $(this.element);
                var tabstripitems = tabstrip.children('.t-tabstrip-items');
                var cnt = tabstripitems.children().length;
                var tabname = tabstrip.attr('id');

                tabstripitems.append(
                    $('<li />')
                        .addClass('t-item')
                        .addClass('t-state-default')
                        .append(
                            $('<a />')
                                .attr('href', '#' + tabname + '-' + (cnt + 1))
                                .addClass('t-link')
                                .text(t.text)
                        )
                    );

                var $contentElement =
                    $('<div />')
                        .attr('id', tabname + '-' + (cnt + 1))
                        .addClass('t-content');

                tabstrip.append($contentElement);

                tabstrip.data('tTabStrip').$contentElements.push($contentElement[0]);

                if (t.url != undefined) {
                    this.loadTab($contentElement, t);
                } else if (t.html != undefined) {
                    $contentElement.append(t.html);
                }
            },
            /// <summary>
            /// Load the tab contents from a url.
            /// </summary>
            loadTab: function ($contentElement, t) {
                if (t.data == undefined) {
                    if (t.complete == undefined) {
                        $contentElement.load(t.url);
                    } else {
                        $contentElement.load(t.url, t.complete);
                    }
                } else {
                    if (t.complete == undefined) {
                        $contentElement.load(t.url, t.data);
                    } else {
                        $contentElement.load(t.url, t.data, t.complete);
                    }
                }
            },
            /// <summary>
            /// Remove a tab.
            /// </summary>
            /// <param type="json object" name="o">json object with either the text or the index of the tab.</param>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').removeTab({ text: 'Tab 2' })
            /// $('#MyTabStrip').data('tTabStrip').removeTab({ index: 1 })
            /// </example>
            removeTab: function (o) {
                var tabstrip = $(this.element);
                var tabname = tabstrip.attr('id');
                var tabstripitems = tabstrip.children('.t-tabstrip-items');
                var i = 0;

                if (o.index == undefined || o.index == null) {
                    i = this.getTabIndex(o.text);
                } else {
                    i = o.index;
                }

                // There must be atleast two tabs to remove a tab.
                if (tabstripitems.children().length > 1) {
                    var tab = this.getTab({ index: i });
                    // If the active tab is being removed, set another tab as active.
                    if (tab.hasClass('t-state-active') == true) {
                        var j = i == 0 ? 1 : (i - 1);
                        this.activateTab(this.getTab({ index: j }));
                    }
                    tab.remove();

                    // Remove the tab contents.
                    $(tabstrip.children()[i + 1]).remove();
                    tabstrip.data('tTabStrip').$contentElements.splice(i, 1);

                    // Rename the tab href.
                    $.each(tabstripitems.children(), function (idx, tab) {
                        $($(tab).children()[0]).attr('href', '#' + tabname + '-' + (idx + 1));
                    });

                    // Rename tab contents.
                    $.each(tabstrip.children(), function (idx, contentElement) {
                        if ($(contentElement).is('div')) {
                            $(contentElement).attr('id', tabname + '-' + idx);
                        }
                    })
                }
            },
            /// <summary>
            /// Hide the contents for all tabs in the tabstrip.
            /// </summary>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').hideContent()
            /// </example>
            hideContent: function () {
                var tabstrip = this;

                tabstrip.activateTab = function (d) {
                    d.parent().children().removeClass('t-state-active');
                    d.removeClass("t-state-default").addClass("t-state-active");
                };
                $(tabstrip.element).height($(this.element).children('.t-tabstrip-items').css('height'));
                $.each(tabstrip.$contentElements, function (idx, content) {
                    $(content).css('display', 'none');
                });
            },
            /// <summary>
            /// Show the contents for all tabs in the tabstrip.
            /// </summary>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').showContent()
            /// </example>
            showContent: function () {
                this.activateTab = $.telerik.tabstrip.prototype.activateTab;
                $(this.element).css('height', '');
                var t = this.getTab({ index: $(this.element).children('.t-tabstrip-items').find('li.t-state-active').index() });
                this.activateTab(t);
            },
            /// <summary>
            /// Remove the contents for all tabs in the tabstrip.
            /// Once this is done, there is no way of restoring the
            /// tab contents.
            /// </summary>
            /// <example>
            /// $('#MyTabStrip').data('tTabStrip').removeContent()
            /// </example>
            removeContent: function () {
                $(this.element).children('div.t-content', '.t-tabstrip').remove();
                $(this.element).height($(this.element).children('.t-tabstrip-items').css('height'));
            }
        };

        // Add the extensions to the tabstrip plugin.
        $.extend(true, $.telerik.tabstrip.prototype, tabstripExtensions);
    }

    // Was the tekerik.treeview.min.js added to the page by the telerik script registrar?
    if ($.telerik.treeview != undefined) {
        // Extend the treeview plugin.
        var treeviewExtensions = {
            /// <summary>
            /// Add a context menu to the treeview.
            /// </summary>
            /// <param type="json object" name="o">
            /// json object with a function to determine whether the context menu should be displayed
            /// for a node and a list of menu items for the context menu.
            /// </param>
            /// <example>
            ///     $('#MyTreeView').data('tTreeView').addContextMenu({
            /// 	    evaluateNode: function(treeview, node) {
            /// 		    var nodeValue = treeview.getItemValue(node);
            /// 		    return (nodeValue == 'editable');
            /// 	    },
            ///         contextMenuOpening: function (menu, node) {
            ///             // Find the 'Edit' context menu option and disable it.
            ///             $.each(menu.menuItems, function (idx, item) {
            ///                 if (item.text == 'Edit') {
            ///                     item.disabled = true;
            ///                     return false;
            ///                 }
            ///             });
            /// 	    },
            /// 	    menuItems: [{
            /// 		        text: 'Edit',
            /// 		        onclick: function(e) {
            ///                     alert('You Clicked ' + e.item.text() + ' for ' + e.treeview.getItemText(e.node) + ' with a value of ' + e.treeview.getItemValue(e.node));
            ///                 }
            /// 	        }, {
            ///                 separator: true
            ///             }, {
            /// 		        text: 'Delete',
            ///                 spriteCssClass: 'delete-icon',
            /// 		        onclick: function(e) {
            ///                     alert('You Clicked ' + e.item.text() + ' for ' + e.treeview.getItemText(e.node) + ' with a value of ' + e.treeview.getItemValue(e.node));
            ///                 },
            ///                 disabled: true
            /// 	        }]
            ///     });
            /// </example>
            addContextMenu: function (o) {
                if (this._contextMenus == undefined) {
                    this._contextMenus = [];

                    // subscribe to the contextmenu event to show a contet menu
                    $('.t-in', this.element).live('contextmenu', function (e) {
                        var treeview = $(this).parents('.t-treeview').data('tTreeView');

                        var span = $(this);

                        // prevent the browser context menu from opening
                        e.preventDefault();

                        // Remove any contect menus that are still being displayed.
                        var fx = $.telerik.fx.slide.defaults();
                        $('div#contextMenu').each(function (index) {
                            $.telerik.fx.rewind(fx, $(this).find('.t-group'), { direction: 'bottom' }, function () {
                                $(this).remove();
                            });
                        });

                        // the node for which the 'contextmenu' event has fired
                        var $node = span.closest('.t-item');

                        // default "slide" effect settings
                        /*var*/fx = $.telerik.fx.slide.defaults();

                        // Identify which context menu to use.
                        $.each(treeview._contextMenus, function (mdx, menu) {
                            // Does this context menu apply to this node?
                            if (menu.evaluateNode(treeview, $node) == true) {
                                if (menu.contextMenuOpening != undefined) {
                                    menu.contextMenuOpening(menu, $node);
                                }
                                var menuItems = '';
                                $.each(menu.menuItems, function (idx, item) {
                                    if (item.separator != undefined && item.separator == true) {
                                        menuItems += '<li class="t-item"><hr/></li>';
                                    } else {
                                        menuItems += '<li class="t-item' + (item.disabled != undefined ? ' t-state-disabled' : '') + '">' + '<a href="#" class="t-link" style="white-space:nowrap;">' + (item.spriteCssClass != undefined ? '<span class="t-sprite ' + item.spriteCssClass + '" style="margin-top:4px;"></span>' : '') + item.text + '</a></li>';
                                    }
                                    delete item.disabled;
                                });
                                if (menuItems.length > 0) {
                                    // context menu definition - markup and event handling
                                    var $contextMenu =
                                        $('<div class="t-animation-container" id="contextMenu">' +
                                            '<ul class="t-widget t-group t-menu t-menu-vertical" style="display:none">' +
                                                menuItems +
                                            '</ul>' +
                                        '</div>')
                                        .css( //positioning of the menu
                                        {
                                        position: 'absolute',
                                        left: e.pageX, // x coordinate of the mouse
                                        top: e.pageY   // y coordinate of the mouse
                                    })
                                        .appendTo(document.body)
                                        .find('.t-item') // select the menu items
                                        .mouseenter(function () {
                                            if ($(this).hasClass('t-state-disabled') == false) {
                                                // hover effect
                                                $(this).addClass('t-state-hover');
                                            }
                                        })
                                        .mouseleave(function () {
                                            // remove the hover effect
                                            $(this).removeClass('t-state-hover');
                                        })
                                        .click(function (e) {
                                            e.preventDefault();
                                            var li = $(this);
                                            if (li.hasClass('t-state-disabled') == false) {
                                                // dispatch the click
                                                $.each(menu.menuItems, function (idx, item) {
                                                    if (item.text == li.text()) {
                                                        item.onclick({ item: li, treeview: treeview, node: $node });
                                                        $contextMenu.remove();
                                                        return;
                                                    }
                                                });
                                            }
                                        })
                                        .end();

                                    // show the menu with animation
                                    $.telerik.fx.play(fx, $contextMenu.find('.t-group'), { direction: 'bottom' });

                                    // handle globally the click event in order to hide the context menu
                                    $(document).click(function (e) {
                                        // hide the context menu and remove it from DOM
                                        $.telerik.fx.rewind(fx, $contextMenu.find('.t-group'), { direction: 'bottom' }, function () {
                                            $contextMenu.remove();
                                        });
                                    });
                                }
                                return;
                            }
                        });
                    });
                }
                this._contextMenus.push(o);
            },
            findNodeByText: function (text) {
                var element = $(this.element).find('.t-in:contains("' + text + '")');
                if (element.length > 0) {
                    return this.createNode(this, element);
                } else {
                    return [];
                }
            },
            findNodeByValue: function (value) {
                var element = $(this.element).find('input.t-input[name="itemValue"][value="' + value + '"]').prev();
                if (element.length > 0) {
                    return this.createNode(this, element);
                } else {
                    return [];
                }
            },
            findNodeContainsValue: function (value) {
                var element = $(this.element).find('input.t-input[name="itemValue"][value*="' + value + '"]').prev();
                if (element.length > 0) {
                    return this.createNode(this, element);
                } else {
                    return null;
                }
            },
            createNode: function (treeview, element) {
                var node = {
                    treeview: treeview,
                    element: element,
                    select: function () {
                        element.click();
                        return this;
                    },
                    deselect: function () {
                        element.removeClass('t-state-selected');
                        return this;
                    },
                    selected: function () {
                        return this.element.hasClass('t-state-selected');
                    },
                    highlight: function () {
                        element.addClass('t-state-selected');
                        return this;
                    },
                    unhighlight: function () {
                        element.removeClass('t-state-selected');
                        return this;
                    },
                    expand: function () {
                        treeview.expand(element.closest('li'));
                        return this;
                    },
                    collapse: function () {
                        treeview.collapse(element.closest('li'));
                        return this;
                    },
                    enable: function () {
                        treeview.enable(element.closest('li'));
                        return this;
                    },
                    disable: function () {
                        treeview.disable(element.closest('li'));
                        return this;
                    },
                    check: function () {
                        treeview.nodeCheck(element.closest('li'), true);
                        return this;
                    },
                    uncheck: function () {
                        treeview.nodeCheck(element.closest('li'), false);
                        return this;
                    },
                    text: function () {
                        return element.text();
                    },
                    value: function () {
                        return element.next('input.t-input').val();
                    },
                    setText: function (text) {
                        element.text(text);
                        return this;
                    },
                    isVisible: function () {
                        return element.is(':visible');
                    },
                    parent: function () {
                        var parentElement = element.closest('ul.t-group').prev('div').find('.t-in');
                        if (parentElement.length > 0) {
                            return treeview.createNode(treeview, parentElement);
                        } else {
                            return [];
                        }
                    },
                    children: function () {
                        var childNodes = [];
                        var childrenElements = node.element.closest('div').next().children().children('div').find('.t-in');
                        $.each(childrenElements, function (idx, childElement) {
                            childNodes.push(treeview.createNode(treeview, $(childElement)));
                        });
                        return childNodes;
                    },
                    /// <summary>
                    /// Add a node to the tree.
                    /// </summary>
                    /// <param type='json object' name='attributes'>JSON object with the following parameters:
                    ///     - text: text for the node.
                    ///     - value: value for the node.
                    ///     - spriteCssClass [Optional]: css class for the node image.
                    ///     - image [Optional]: json object that contains the image attributes:
                    ///         - src: url for the image.
                    ///         - alt: alternate text for the image.
                    /// </param>
                    /// <example>
                    /// var node = $('#MyTreeView').data('tTreeView').findNodeByText('Free Tools');
                    /// var newNode = node.addNode({
                    ///     text:'New Node Text 1',
                    ///     value:101,
                    ///     spriteCssClass:'my-sprite-class'
                    /// });
                    /// </example>
                    /// <example>
                    /// var node = $('#MyTreeView').data('tTreeView').findNodeByText('Free Tools');
                    /// var newNode = node.addNode({
                    ///     text:'New Node Text 2',
                    ///     value:102,
                    ///     image: {
                    ///         src: 'http://demos.telerik.com/aspnet-mvc/Content/PanelBar/FirstLook/notesItems.gif',
                    ///         alt:'test'
                    ///     }
                    /// });
                    /// <example>
                    /// var node = $('#MyTreeView').data('tTreeView').findNodeByText('Free Tools');
                    /// var newNode = node.addNode({
                    ///     text:'New Node Text 2',
                    ///     value:102,
                    //      url: 'http://demos.telerik.com/aspnet-mvc/Content/PanelBar/FirstLook'
                    ///     image: {
                    ///         src: 'http://demos.telerik.com/aspnet-mvc/Content/PanelBar/FirstLook/notesItems.gif',
                    ///         alt:'test'
                    ///     }
                    /// });
                    /// </example>
                    addNode: function (attributes) {
                        var div = element.closest('div');
                        var ul = div.next('ul.t-group');

                        // If there are no child nodes for this parent node, then create the unordered list that will contain the child nodes.
                        if (ul.length == 0) {
                            ul = $('<ul/>').attr('class', 't-group');
                            div.closest('li.t-item').append(ul);
                            div.prepend($('<span/>').attr('class', 't-icon t-minus'));
                            // Alter the last child node to no longer be the last child node.
                        } else {
                            var lastChild = ul.children('li.t-last');
                            lastChild.removeClass('t-last').children('div.t-bot').removeClass('t-bot').addClass('t-mid');
                        }

                        // Create the child
                        var newChild = $('<li/>')
                            .attr('class', 't-item t-last')
                            .append($('<div/>')
                                .attr('class', 't-bot')
                                .append(attributes.url != undefined
                                    ? $('<a/>')
                                        .attr('class', 't-link t-in')
                                        .attr('href', attributes.url)
                                        .text(attributes.text)
                                    : $('<span/>')
                                        .attr('class', 't-in')
                                        .text(attributes.text)
                                )
                                .append($('<input/>')
                                    .attr('type', 'hidden')
                                    .attr('value', attributes.value)
                                    .attr('name', 'itemValue')
                                    .attr('class', 't-input')
                                )
                            );

                        // Was a sprite css class attribute passed in?
                        if (attributes.spriteCssClass != undefined) {
                            newChild.find('span.t-in').prepend($('<span/>').addClass('t-sprite').addClass(attributes.spriteCssClass));
                            // Was a image attribute passed in?
                        } else if (attributes.image != undefined) {
                            var img = $('<img/>').addClass('t-image').attr('src', attributes.image.src);
                            if (attributes.image.alt != undefined) {
                                img.attr('alt', attributes.image.alt);
                            }
                            newChild.find('span.t-in').prepend(img);
                        }
                        // Add the child node.
                        ul.append(newChild);

                        return treeview.createNode(treeview, newChild.find('span.t-in'));
                    },
                    remove: function () {
                        var parent = this.parent();
                        element.closest('li.t-item').remove();

                        var children = parent.element.closest('div').next().children('li');

                        if (children.length > 0) {
                            var lastChild = $(children[children.length - 1]);
                            lastChild.addClass('t-last');
                            lastChild.children('div').attr('class', 't-bot');
                        } else {
                            parent.element.closest('div').next().remove();
                            parent.element.closest('div').children('span.t-icon').remove();
                        }
                        return parent;
                    }
                }
                return node;
            }
        };

        // Add the extensions to the treeview plugin.
        $.extend(true, $.telerik.treeview.prototype, treeviewExtensions);
    }

    // Was the tekerik.window.min.js added to the page by the telerik script registrar?
    if ($.telerik.window != undefined) {
        // Extend the window plugin.
        var windowExtensions = {
            /// <summary>
            /// Set a new height for the window.
            /// </summary>
            /// <param type="int" name="h">New height for the window.</param>
            setHeight: function (h) {
                $(this.element).find('.t-window-content').height(h);
                return this;
            },
            /// <summary>
            /// Set a new width for the window.
            /// </summary>
            /// <param type="int" name="w">New width for the window.</param>
            setWidth: function (w) {
                $(this.element).find('.t-window-content').width(w);
                return this;
            },
            /// <summary>
            /// Set a new title for the window.
            /// </summary>
            /// <param type="string" name="t">New title for the window.</param>
            setTitle: function (t) {
                $(this.element).find('span.t-window-title').text(t);
                return this;
            }
        };

        // Add the extensions to the window plugin.
        $.extend(true, $.telerik.window.prototype, windowExtensions);
    }

    $.telerikExtensionsIncluded = true;
})(jQuery);

(function($) {
    /*! Copyright (c) 2008 Brandon Aaron (brandon.aaron@gmail.com || http://brandonaaron.net) * Dual licensed under the MIT (http://www.opensource.org/licenses/mit-license.php)  * and GPL (http://www.opensource.org/licenses/gpl-license.php) licenses. *//** * Gets the width of the OS scrollbar */
    $.getScrollbarWidth = function () {
        var scrollbarWidth = 0;
        if (!scrollbarWidth) { if ($.browser.msie) { var $textarea1 = $('<textarea cols="10" rows="2"></textarea>').css({ position: 'absolute', top: -1000, left: -1000 }).appendTo('body'), $textarea2 = $('<textarea cols="10" rows="2" style="overflow: hidden;"></textarea>').css({ position: 'absolute', top: -1000, left: -1000 }).appendTo('body'); scrollbarWidth = $textarea1.width() - $textarea2.width(); $textarea1.add($textarea2).remove(); } else { var $div = $('<div />').css({ width: 100, height: 100, overflow: 'auto', position: 'absolute', top: -1000, left: -1000 }).prependTo('body').append('<div />').find('div').css({ width: '100%', height: 200 }); scrollbarWidth = 100 - $div.width(); $div.parent().remove(); } } return scrollbarWidth;
    };
})(jQuery);