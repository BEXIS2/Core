<!-- src/lib/components/createGroupOptions.svelte -->
<script lang="ts">
	import Fa from 'svelte-fa';
	import { faPeopleGroup, faPen, faTrash } from '@fortawesome/free-solid-svg-icons';
	import type { ReadUserModel, UserModel } from '../../routes/users/types';
	import type { CreateGroupModel } from '../../routes/groups/types';
	import { usersStore } from '../../routes/users/services';

	import { getContext } from 'svelte';
	export let row: ReadUserModel;
	export let group = getContext('group') as CreateGroupModel;

	// ✅ Reaktive Liste: Alle ausgewählten User-IDs
	// $: selectedUserIds = (group.userIds || []).map(id => id);

	// ✅ Funktion: Toggle User
	function toggleUser(userId: number) {
		const index = group.userIds?.indexOf(userId) ?? -1;

		if (index === -1) {
			// ✅ Hinzufügen
			group.userIds = [...(group.userIds || []), userId];
		} else {
			// ✅ Entfernen
			group.userIds = (group.userIds || []).filter(id => id !== userId);
		}

		console.log('Aktualisierte User-IDs:', group.userIds);
	}
</script>

<div class="flex flex-col gap-2">
<label class="flex items-center gap-2 p-1 border rounded hover:bg-gray-50">
<!-- ✅ Reaktive Checkbox mit bind:checked -->
			<input
				type="checkbox"
				class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
				checked={group.userIds?.includes(row.id)}
				on:change={() => toggleUser(row.id)}
			/>

		</label>
</div>