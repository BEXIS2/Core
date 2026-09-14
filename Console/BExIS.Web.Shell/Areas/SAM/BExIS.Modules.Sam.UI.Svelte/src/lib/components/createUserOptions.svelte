<!-- src/lib/components/createGroupOptions.svelte -->
<script lang="ts">
	import type { CreateUserModel } from '../../routes/users/types';
	import type { ReadGroupModel } from '../../routes/groups/types';

	import { getContext } from 'svelte';
	export let row: ReadGroupModel;
	export let user = getContext('user') as CreateUserModel;

	function toggleGroup(groupId: number) {
		const index = user.groupIds?.indexOf(groupId) ?? -1;

		if (index === -1) {
			user.groupIds = [...(user.groupIds || []), groupId];
		} else {
			user.groupIds = (user.groupIds || []).filter((id) => id !== groupId);
		}
	}
</script>

<div class="flex flex-col gap-2">
	<label class="flex items-center gap-2 p-1 border rounded hover:bg-gray-50">
		<input
			type="checkbox"
			class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
			checked={user.groupIds?.includes(row.id)}
			on:change={() => toggleGroup(row.id)}
		/>
	</label>
</div>
