<script lang="ts">

	import { TextInput} from '@bexis2/bexis2-core-ui'
	import type { TagInfoModel } from '../types';

	export let value: string;
	export let row: any;
	export let dispatchFn;

let currentRow = row.original;

const eventDispatchFn = (type: string) => {
	
		return dispatchFn({ type, row:currentRow });
	};


</script>
<TextInput bind:value={value} on:change={() => eventDispatchFn("UPDATE")} > </TextInput>
