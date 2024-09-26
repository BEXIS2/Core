<script lang="ts">

	import { TextInput} from '@bexis2/bexis2-core-ui'


	export let value: string;
	export let row: any;
	export let dispatchFn;

let currentRow = row.original;

</script>
<TextInput bind:value={currentRow.releaseNote} > </TextInput>
