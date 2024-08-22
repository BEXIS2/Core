<script lang="ts">

	import type { TagInfoModel } from '../types';
	import Fa from 'svelte-fa';
	import { faMinus, faPlus } from '@fortawesome/free-solid-svg-icons';


	export let value: string;
	export let row: any;	
	export let dispatchFn;

	let disabled:boolean = false;


let currentRow:TagInfoModel = row.original;
let currentValue:number = currentRow.tagNr;
$:currentValue;


	function add(){
		 currentValue = currentValue+0.1;
			currentValue.toFixed(1);
			currentRow.tagNr =	parseFloat(currentValue.toFixed(1));

			// activate the button
			if(currentValue>0 && disabled == true){
				disabled = false;
			}
	}

	function	sub(){

		if(currentValue>0){
			currentValue = currentValue-0.1;
			currentRow.tagNr =	parseFloat(currentValue.toFixed(1));
		}

		//deactivate the button
		if(currentValue===0){
					disabled = true;
		}

	}

</script>

<div class="flex items-center justify-center gap-2">


<button
class="btn btn-icon" 
on:click={add}
>
<Fa icon={faPlus} />
</button>

{currentValue.toFixed(1)}

<button
class="btn btn-icon" 
on:click={sub}
{disabled}
>
<Fa icon={faMinus} />
</button>
</div>

