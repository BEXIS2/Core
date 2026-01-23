<script lang="ts">
	import PrimaryData from '$lib/components/SearchConfig/PrimaryData.svelte';
	import type { MeaningModel } from './SearchConfigModel';
	
	export let entitytemplates: any[] = [];
	export let meanings: MeaningModel[] = [];
	export let searchConfigData: any;
	export let onChangeHandler: (event: CustomEvent<any>) => void = () => {};
    export let res: any;
</script>

<PrimaryData {entitytemplates} {meanings} bind:searchConfigData {onChangeHandler} {res} />