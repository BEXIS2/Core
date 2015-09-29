
$(document).ready(function ()
{
	resetAllTelerikIconTitles();
});

function resetAllTelerikIconTitles()
{
	$('.t-arrow-first, ' +
	  '.t-arrow-prev,' +
	  '.t-arrow-next,' +
	  '.t-arrow-last,' +
	  '.t-arrow-up,' +
	  '.t-arrow-down,' +
	  '.t-arrow-up-small,' +
	  '.t-arrow-down-small,' +
	  '.t-filter,'+
	  '.t-group-delete,'+
	  '.t-close,'+
	  '.t-icon-calendar'
	  ).each(function ()
	  {
		$(this).empty();
	});
}


/*List*/

$(".bx-list >li").click(function ()
{
	$(this).parent().find(".selected").removeClass("selected")

	if ($(this).hasClass("selected"))
	{
		$(this).removeClass("selected");
	}
	else
	{
		$(this).addClass("selected");
	}
})

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
})



