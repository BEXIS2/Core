<script lang="ts">

	import { getValueByPath} from '$lib/components/utils/metadata/metadataComponentUtils';

	import SimpleComponent from './simpleComponent.svelte';

	export let simpleComponent: any;
	export let path: string;
	export let required: boolean = false;

	let label: string = !path
		? ''
		: path.split('.').length > 1
			? path.split('.')[path.split('.').length - 1]
			: path;

	let value = getValueByPath(path);
</script>

{#if path && simpleComponent.properties}
	<SimpleComponent {simpleComponent} {path} {required} {label} {value} />
{/if}

