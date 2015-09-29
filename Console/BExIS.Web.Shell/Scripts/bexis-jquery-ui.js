/*Dialog*/

function bx_openDialog(id, type)
{
	if (type == "alert")
	{
		$(id).closest(".ui-dialog").addClass("alert");
	}

	$(id).dialog(
	{
		buttons: {
			Ok: function ()
			{
				$(this).dialog("close");
			}
		}

	});

}