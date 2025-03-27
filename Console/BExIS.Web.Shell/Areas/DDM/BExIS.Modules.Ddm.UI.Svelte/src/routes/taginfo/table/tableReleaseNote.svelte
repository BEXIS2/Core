<script lang="ts">

	import { TextArea} from '@bexis2/bexis2-core-ui'


	export let value: string;
	export let row: any;
	export let dispatchFn;

let currentRow = row.original;

</script>
<TextArea bind:value={currentRow.releaseNote} > </TextArea>
