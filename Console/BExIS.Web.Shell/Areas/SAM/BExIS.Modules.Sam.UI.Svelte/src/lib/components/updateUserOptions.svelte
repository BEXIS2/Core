<!-- src/lib/components/createGroupOptions.svelte -->
<script lang="ts">
	import type { UpdateUserModel } from '../../routes/users/types';
	import type { ReadGroupModel } from '../../routes/groups/types';
	import { getContext } from 'svelte';
	import { type Writable } from 'svelte/store';
	
	export let row: ReadGroupModel;
	let userStore = getContext('user') as Writable<UpdateUserModel>;
	function toggleGroup(groupId: number) {
    userStore.update(u => {
        const index = u.groupIds?.indexOf(groupId) ?? -1;
        if (index === -1) {
            u.groupIds = [...(u.groupIds || []), groupId];
        } else {
            u.groupIds = (u.groupIds || []).filter(id => id !== groupId);
        }
        return u;
    });
	}
</script>

<div class="flex flex-col gap-2">
<label class="flex items-center gap-2 p-1 border rounded hover:bg-gray-50">
<input
    type="checkbox"
    checked={$userStore.groupIds?.includes(row.id)}
    on:change={() => toggleGroup(row.id)}
/>

		</label>
</div>