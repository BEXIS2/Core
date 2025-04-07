<script lang="ts">
	import Fa from 'svelte-fa';
	import { faEye } from '@fortawesome/free-solid-svg-icons';

	export let row;
</script>

<a href="/ddm/data/Showdata/{row.iD}" target="_blank" class="btn variant-filled-primary">
	<Fa icon={faEye} size="xs" />
</a>
