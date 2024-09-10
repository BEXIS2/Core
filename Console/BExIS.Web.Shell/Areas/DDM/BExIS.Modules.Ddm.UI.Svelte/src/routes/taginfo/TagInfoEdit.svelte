<script lang="ts">

	import { TablePlaceholder, Table, notificationStore, notificationType, ErrorMessage} from "@bexis2/bexis2-core-ui";
	import { add, get, save, updateSearch } from './services'

	import { tagInfoModelStore } from './stores.js'
	import TablePublish from "./table/tablePublish.svelte";
	import TableShow from "./table/tableShow.svelte";
	import TableNr from "./table/tableNr.svelte";
	import TableText from "./table/tableText.svelte";
	import TableOptions from "./table/tableOptions.svelte";
	import TableDate from "./table/tableDate.svelte";
	import  { type TagInfoEditModel, TagType } from "./types";
	import TableReleaseNote from "./table/tableReleaseNote.svelte";


	let container;
	let id: number = 0;

	async function reload(){
		container = document.getElementById('taginfo');
		id = Number(container?.getAttribute('dataset'));

		const tagInfos = await get(id);
		tagInfoModelStore.set(tagInfos);

		console.log("🚀 ~ reload ~ tagInfoModelStore:", $tagInfoModelStore)
	}

 const tableActions = (action: CustomEvent<{ row: TagInfoEditModel; type: string }>) => {
		// See tableCRUDActions tab for more details
		const { type, row } = action.detail;
		console.log("🚀 ~ tableActions ~ action.detail:", action.detail)

		switch (type) {
			case 'SAVE':
				saveFn(row);
				break;
			case 'MINOR':
				 addFn(row, TagType.Minor);
					break;
			case 'MAJOR':
		  	addFn(row, TagType.Major);
					break;
			default:
			 break;
	}

	};

	async function saveFn(tagInfo:	TagInfoEditModel) {

		const responce = await save(tagInfo);
		
		if(responce.status ===	200){

			// trigger to update search index
	
				const res = await updateSearch(id);
				console.log("🚀 ~ saveFn ~ res:", res)
			
				notificationStore.showNotification({
					notificationType: notificationType.success,
					message: 'Tag is saved.'
				})
			console.log('ok');
		}else{
			notificationStore.showNotification({
					notificationType: notificationType.error,
					message: 'Tag is not saved.'
				})
		}

	}

	async function addFn(tagInfo:	TagInfoEditModel, type:TagType) {

		const responce = await add(tagInfo,type);
		console.log("🚀 ~ addFn ~ responce:", responce)
		
		if(responce.status ===	200){

				notificationStore.showNotification({
					notificationType: notificationType.success,
					message: 'Tag is created.'
				})


			reload()

		}else{
			notificationStore.showNotification({
					notificationType: notificationType.error,
					message: 'Tag is not created.'
				})
		}

	}





</script>


{#await reload()}
	<div class="table-container w-full">
		<TablePlaceholder cols={7} />
	</div>
{:then model}
<div class="table table-compact w-full">

	<Table
	 on:action={tableActions}
		config={{
			id:"taginfos",
			search:false,
			data: tagInfoModelStore,
			optionsComponent: TableOptions,
			columns:{
				versionId: {
							disableFiltering: true,
							exclude: true
						},
					tagId: {
							disableFiltering: true,
							exclude: true
						},
					versionNr: {
							fixedWidth:10,
							header:"Internal version",
							disableFiltering:true,
							disableSorting:true
						},
					tagNr: {
							fixedWidth:80,
							header:"Tag",
							disableFiltering:true,
							disableSorting:true,
							instructions:{
								renderComponent:TableNr
							}
						},
						releaseNote: {
							header:"Release Note",
							fixedWidth:400,
							disableFiltering:true,
							disableSorting:true,
							instructions:{
								renderComponent:TableReleaseNote
							}
						},
						releaseDate: {
							header:"Release Date",
							instructions:{
								renderComponent:TableDate
							},
							disableFiltering:true,
							disableSorting:true
						},
						systemDescription: {
							header:"System Description",
							disableFiltering:true,
							disableSorting:true
						},
						systemAuthor: {
							header:"System Author",
							disableFiltering:true,
							disableSorting:true
						},
						systemDate: {
							header:"System Date",
							instructions:{
								renderComponent:TableDate
							},
							disableFiltering:true,
							disableSorting:true
						},
						show:{
							header:"Show history",
							fixedWidth:10,
							instructions:{
								renderComponent:TableShow
							},
							disableFiltering:true,
							disableSorting:true
						},
						publish:{
							header:"Publish Tag",
							fixedWidth:10,
							instructions:{
								renderComponent:TablePublish
							},
							disableFiltering:true,
							disableSorting:true
						}
			}
		}}>
		</Table>

</div>
{:catch error}
		<ErrorMessage {error} />
{/await}

