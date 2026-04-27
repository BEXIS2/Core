<script lang="ts">
	import Curation from '$lib/components/curation/Curation.svelte';
	import { RadioGroup, RadioItem } from '@skeletonlabs/skeleton';
	import Fa from 'svelte-fa';
	import {
		faAngleLeft,
		faCircleDot,
		faCodeCommit,
		faCodeCompare,
		faPlus
	} from '@fortawesome/free-solid-svg-icons';
	import { fade, slide } from 'svelte/transition';
	import type { Readable } from 'svelte/store';
	import { tick } from 'svelte';
	import MetadataDiffTool from '$lib/components/metadataDiffTool/MetadataDiffTool.svelte';
	import {
		CurationEntryType,
		type CurationEntryHelperModel,
		type CurationEntryModel
	} from '$lib/models/CurationEntry';

	export let datasetId: number;

datasetId = 0



</script>

<!-- How the component might be used with data next to it -->

	

	
		
				<MetadataDiffTool {datasetId} />
		



<style lang="postcss">
	.max-h-fit-screen {
		max-height: calc(100vh - 12rem);
	}
</style>
