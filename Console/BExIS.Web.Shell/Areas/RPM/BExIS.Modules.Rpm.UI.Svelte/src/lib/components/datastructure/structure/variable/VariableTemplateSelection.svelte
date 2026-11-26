<script lang="ts">
	import type { templateListItemType } from "../../types";
 import TableListString from "./table/tableListString.svelte";
 import TableOptions from "./table/tableOptions.svelte";
 import Fa from 'svelte-fa';
	import { faAdd } from '@fortawesome/free-solid-svg-icons';
 import { createEventDispatcher, SvelteComponent } from 'svelte';

	import { Drawer, getDrawerStore } from '@skeletonlabs/skeleton';
	const drawerStore = getDrawerStore();
	import type { DrawerSettings } from '@skeletonlabs/skeleton';
	import { Table, type TableConfig } from "@bexis2/bexis2-core-ui";

	import {	templateStore 	} from '../../store';

	const dispatch = createEventDispatcher();

 export let list:templateListItemType[];
	$: list, open();

function select(type: any) {
  console.log("selected", type.id);
  drawerStore.close();
  dispatch('selected', type.id);
}

function open(): void {

	 if(list==undefined || list.length==0){
			return;
		}

			const drawerSettings: DrawerSettings = {
				id: 'example-2',
				meta: { foo: 'bar', fizz: 'buzz', age: 40 }
			};
			drawerStore.open(drawerSettings);
		}

 function close() {
		console.log('close selection');
  dispatch('close');
  drawerStore.close();

		list	= [];
  
	}

 const tc: TableConfig<templateListItemType> = {
		id: 'VariableTemplates',
		data: templateStore,
  optionsComponent: TableOptions as unknown as typeof SvelteComponent,
		columns: {
			id: {
				disableFiltering: true,
				exclude: true
			},
   group: {
				disableFiltering: true,
				exclude: true
			},
   dataTypes: {
				disableFiltering: true,
				exclude: true
			},
   units: {
				disableFiltering: true,
				exclude: true
			},
   meanings: {
				instructions: {
					renderComponent: TableListString as unknown as typeof SvelteComponent
				},
				disableFiltering: true,
				exclude: false
			},
   constraints: {
				instructions: {
					renderComponent: TableListString as unknown as typeof SvelteComponent
				},
				disableFiltering: true,
				exclude: false
			}
  }
	};

</script>

<div style="z-index:150">
 <Drawer position="right" width="10"  on:backdrop={close} on:touchend={close}>
  <div class="p-5 space-y-5"> 
			<h1 class="h2">Select Template</h1>
   	<Table on:action={(obj) => select(obj.detail.type)} config={tc} />
  </div>
 </Drawer>
</div>
 
