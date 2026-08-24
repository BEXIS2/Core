<script lang="ts">
	import { onMount } from 'svelte';
	import ComplexComponent from '../../../routes/view/metadata/complexComponentWrapper.svelte';
	import * as apiCalls from '$services/MetadataCaller';
	import { schemaToJson, setConfigStore, setMetadataStore, setSchemaStore } from '$lib/components/utils/metadata/metadataComponentUtils';
	import { Spinner } from '@bexis2/bexis2-core-ui';

	export let id: number = 0;
	export let version: number = 0;
	export let tag: number = 0;

	let s: any;
	let m: any = null;
	let schema: any = s;
	$: schema = s;
	let loading = true;
	let error: string | null = null;

	onMount(async () => {
		await load();
	});

	async function load() {
		try {
			const res = await apiCalls.GetDatasetInfoById(id);
			if (res.status === 200) {
				const datasetInfos = res.data;
				s = await apiCalls.GetMetadataSchema(datasetInfos.metadataStructureId);
				setSchemaStore(s);

				if (id > 0) m = await apiCalls.GetMetadata(id, version, tag);
				else m = schemaToJson(s);
				setMetadataStore(m);

				const configJson = await apiCalls.GetComponentConfig(datasetInfos.entityTemplateId, 'view');
				setConfigStore(configJson);
			}
		} catch (e: any) {
			error = e.message || 'Failed to load metadata';
		} finally {
			loading = false;
		}
	}
</script>

{#if loading}
	<div class="flex justify-center py-8"><Spinner /></div>
{:else if error}
	<div class="text-sm text-error-500">{error}</div>
{:else if schema}
	<div id="metadata-content">
		<ComplexComponent complexComponent={schema} path={''} />
	</div>
{/if}
