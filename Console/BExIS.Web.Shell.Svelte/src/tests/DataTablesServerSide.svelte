<script>
 import jQuery from 'jquery'
 import { onMount } from 'svelte'
 import initDt from 'datatables.net-dt'

  initDt()

  let el;

  onMount(() => {
    jQuery(el).DataTable({
        serverSide: true,
        ajax: {
            "url": "/UiTest/Load",
            "type": "POST"
        },
        columns: [
          { data: 'Id', Name: "Id" },
          { data: 'Name', Name: "Name" },
          { data: 'Email', Name: "Email" }
          ]
      })
  })

</script>

<svelte:head>
	<link rel="stylesheet" href="//cdn.datatables.net/1.10.21/css/jquery.dataTables.min.css" />
</svelte:head>

<table bind:this={el} class="display" style="width:100%">
 <thead>
   <tr>
     <th>Id</th>
     <th>Name</th>
     <th>Email</th>
   </tr>
 </thead>
 
 <tfoot>
   <tr>
    <th>Id</th>
    <th>Name</th>
    <th>Email</th>
   </tr>
 </tfoot>
</table>