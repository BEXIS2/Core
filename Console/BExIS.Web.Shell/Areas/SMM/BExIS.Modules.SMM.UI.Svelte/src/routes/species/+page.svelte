<script lang="ts">
	import { ErrorMessage, Page, pageContentLayoutType, positionType, Spinner } from "@bexis2/bexis2-core-ui";
	import { load } from "./services";
	import type { SpeciesModel } from "./types";


	let data:SpeciesModel;

	async function loadData(){

		data = await load();
	}

</script>
<Page 
	title="Species" 
	note=""
	contentLayoutType={pageContentLayoutType.center}
>
		{#await loadData()}
			<div class="text-surface-800">
				<Spinner position={positionType.center} label="loading species" />
			</div>
		{:then result}
			<b>{data.count}</b>
			<b>{data.name}</b>

		{:catch error}
			<ErrorMessage {error} />
		{/await}


</Page>