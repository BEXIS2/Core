$(document).ready(function ()
{
	
		resetAllTelerikIconTitles();
		truncateTitle();
		//console.log("on document ready");
		//Bootstrat tooltip
		$('[data-toggle="tooltip"]').tooltip();

	setTimeout(function () {
		//get height of the menu and add this to the margin of the content
		var h = $(".navbar").height();
		console.log("h = " + h);
		//$("#information-container").css("top", h);
		//$(".main-content").css("margin-top", h);

		$(window).resize(function () {

			//var h = $(".navbar").height();
			//console.log("h = " + h);
			//$("#information-container").css("top", h);
			//$(".main-content").css("margin-top", h);
		});
    }, 1000);

	
});

function resetAllTelerikIconTitles()
{
	//$('.t-arrow-first, ' +
	//  '.t-arrow-prev,' +
	//  '.t-arrow-next,' +
	//  '.t-arrow-last,' +
	//  '.t-arrow-up,' +
	//  '.t-arrow-down,' +
	//  '.t-arrow-up-small,' +
	//  '.t-arrow-down-small,' +
	//  '.t-filter,'+
	//  '.t-group-delete,'+
	//  '.t-close,'+
	//  '.t-icon-calendar'
	//  ).each(function ()
	//  {
	//	$(this).empty();
	//});
}

/*Truncate Title*/
function truncateTitle()
{
	$('.bx-trunc-child').each(function ()
	{
		//$(this).trunk8();
		//if (!$(this).attr("title") == true) {
		var n = $(".bx-trunc-parent").width()-60;
		var text = $(this).text();
		console.log(text);

		var ntLast
		var nt
		//Link Breiter als/ oder gleich breit Container
		if ($(this).width() >= n) {
			//console.log("start truncate xxx");
			//console.log(this);
			//console.log("text:" + text);
			//console.log("n :" + n);
			//console.log("$(this).width() :" + $(this).width());

			$(this).width(n);

			//console.log("new .width() :" + $(this).width());

			//get text from title or text
			if ($(this).attr("title") !== null)
				t = $(this).attr("title");
			else
				t = $(this).text();

			nt = t.split(" ");

			//console.log("nt :" + nt);
			ntLast = nt.pop();
			//console.log("ntLast :" + ntLast);
			$(this).trunk8(
			{
				fill: "..." + ntLast
			});

			//console.log("new text:" + $(this).text());
		}
		//Link kürzer als Container
		else if (text.indexOf(".") !== -1 || text === '' || text === null)
		{
			var l = $(this).text().length;
			if (l === 0)
			{
				l = 1;
			}

			var w = $(this).width();
			if (w === 0)
			{
				w = 1;
			}

			var m = w / l;

			m = Math.round(m).toFixed(0);

			var t = $(this).attr("title");
			var maxWidth = t.length * m;
			maxWidth = maxWidth + 20;

			if (maxWidth >= n)
			{
				$(this).width(n);
			}
			else
			{
				$(this).width(maxWidth);
			}

			nt = t.split(" ");
			ntLast = nt.pop();

			$(this).trunk8(
				{
					fill: "..." + ntLast
				});
		}
	});
}

function addTooltips() {
	$(".t-grid > table > tbody > tr > td , .t-grid > table > thead > tr > th").each(function ()
	{
		var $this = $(this);
		var text = this.innerText;
		$this.attr("title", text);
	});
}

/**
 * TELERIK EXTENTIONS
 */

$(".t-grid").load(function () {
	$(".t-grid th").each(function () {
		var element = $(this);
		var div;
		if (element.find(".bx-header-title").length > 0) {
			div = element.find(".bx-header-title");
		}
		else {
			div = $(document.createElement("div"));
			div.addClass("bx-header-title");
			div.css({ "overflow": "hidden", "text-overflow": "ellipsis", "float": "left" });
			div.append(element.find("a"));
			element.prepend(div);
		}

		var filter = element.find(".t-grid-filter");
		filter.css("float", "right");
		filter.css("margin-top", "2px");
		div.width((element.innerWidth() - filter.outerWidth() - 5));
	});
});

$(".t-grid th").click(function (e) {
	//var element = e.currentTarget;

	//var arrow = $(element).find("span")[0];
	//console.log(arrow);

	//var hasDownClass = $(arrow).hasClass("t-arrow-down");
	//var hasUpClass = $(arrow).hasClass("t-arrow-up");
	//var display = $(arrow).css("display");

	//console.log(hasDownClass);
	//console.log(display);

	//if ((hasDownClass && display.length == 0) ||
	//    (hasDownClass && display == "inline-block")
	//    ||
	//    (!hasDownClass && display == "none")
	//    ||
	//    (!hasDownClass && display.length == 0)) {
	//    $(arrow).removeClass();
	//    $(arrow).addClass("t-icon");
	//    $(arrow).addClass("t-arrow-up");
	//    $(arrow).attr("display", "inline-block");
	//    console.log("1");

	//} else
	//if ((hasDownClass && display.length == 0) ||
	//    (hasDownClass && display == "none")) {
	//    $(arrow).removeClass();
	//    $(arrow).addClass("t-icon");
	//    $(arrow).addClass("t-arrow-down");
	//    $(arrow).attr("display", "inline-block");
	//    console.log("2");

	//}
	//else
	//if (hasUpClass) {
	//    $(arrow).removeClass();
	//    $(arrow).addClass("t-icon");
	//    $(arrow).addClass("t-arrow-down");
	//    $(arrow).attr("display", "none");
	//    console.log("3");

	//}

	//console.log(arrow);

	//if (orderBy.indexOf("desc")>0) {
	//    $(arrow).removeClass();
	//    $(arrow).addClass("t-icon");
	//    $(arrow).addClass("t-icon t-arrow-up");
	//    console.log("desc");
	//}
	//else
	//    if (orderBy.indexOf("asc") > 0) {
	//    $(arrow).removeClass();
	//    $(arrow).addClass("t-icon");
	//    $(arrow).addClass("t-icon t-arrow-up");
	//    console.log("asc");
	//}
	//else
	//{
	//    $(arrow).removeClass();
	//    $(arrow).addClass("t-icon");
	//    $(arrow).addClass("t-icon t-arrow-down");
	//    console.log("---");
	//}
})

$(".t-grid").change(function () {
	$(".t-grid th").each(function () {
		var element = $(this);
		var div = $(document.createElement("div"));
		div.addClass("bx-header-title");
		div.css({ "overflow": "hidden", "text-overflow": "ellipsis", "float": "left" });

		var filter = element.find(".t-grid-filter");
		filter.css("float", "right");

		div.width((element.innerWidth() - filter.outerWidth()-5));

		var a = element.find("a");
		//var arrow = element.find("span");
		//console.log(arrow);
		//a.append(arrow);
		div.append(a);

		element.prepend(div);
	});
});

/*List*/

$(".bx-list > li").click(function ()
{
	$(this).parent().find(".selected").removeClass("selected");
	if ($(this).hasClass("selected"))
	{
		$(this).removeClass("selected");
	}
	else
	{
		$(this).addClass("selected");
	}
});
$(".bx-list-multi >li").click(function ()
{
	//$(this).parent().find(".selected").removeClass("selected")

	if ($(this).hasClass("selected"))
	{
		$(this).removeClass("selected");
	}
	else
	{
		$(this).addClass("selected");
	}
});

/**************************
 * *******PRELOADER********/

$.fn.extend({
	preloader: function(fontsize, text, height) {
		var h = $(this).height();
		if (height !== null && height > 0) {
			h = height;
		}

		//console.log(text);
		//console.log(height);
		//console.log(fontsize);

		if (typeof text === "undefined") {
			text = "Loading...";
		}

		if (typeof height === "undefined" || height === 0) {
			height = "auto";
		}

		if (typeof fontsize === "undefined") {
			fontsize = 10;
		}

		//console.log(text);
		//console.log(height);
		//console.log(fontsize);

		var loader = document.createElement('span');
		console.log(loader);

		$(loader).css("font-size", fontsize);
		$(loader).css("padding", 5);
		$(loader).addClass("fa fa-spinner fa-pulse");

		//    '<span style="font-size:'+fontsize+'px;" ' +
		//'class="preloader fa fa-spinner fa-pulse"></span> '+text+

		var div = document.createElement('div');

		$(div).append(loader);
		$(div).append(text);

		$(div).addClass("preloader");

		$(div).css("font-size", fontsize);
		$(div).css("height", h);
		$(div).css("padding", 2);

		console.log(div);

		$(this).children().hide();

		$(this).append(div);

		return this;
	},

	removePreloader: function () {
		$(".preloader").remove();
		$(this).children().show();
		return this;
	}
});

/* jQuery Validation Extension - CheckBox */
if (jQuery.validator) {
	// Checkbox Validation
	jQuery.validator.addMethod("checkrequired", function (value, element, params) {
		var checked = false;
		checked = $(element).is(':checked');
		return checked;
    }, '');

    jQuery.validator.addMethod('selectvalidation', function (value, element, params) {
        if (value !== "-1")
            return true;
        else
            return false;
    });

	if (jQuery.validator.unobtrusive) {
        jQuery.validator.unobtrusive.adapters.addBool("checkrequired");
        jQuery.validator.unobtrusive.adapters.add('selectvalidation', function (options) {
            options.rules['selectvalidation'] = {};
            options.messages['selectvalidation'] = options.message;
        });
	}
}