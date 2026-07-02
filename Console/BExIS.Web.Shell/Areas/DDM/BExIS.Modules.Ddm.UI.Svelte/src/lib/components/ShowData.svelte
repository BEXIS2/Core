<script lang="ts">
	import Fa from 'svelte-fa';
	import { faEye } from '@fortawesome/free-solid-svg-icons';

	export let row;
</script>

<a href="/dcm/view?id={row.iD}" target="_blank" class="btn variant-filled-primary" title="View dataset details in a new tab">
	<Fa icon={faEye} size="xs" />
</a>
