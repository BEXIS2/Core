<!-- src/lib/components/createGroupOptions.svelte -->
<script lang="ts">
	import type { ReadUserModel, UserModel } from '../../routes/users/types';
	import type { UpdateGroupModel } from '../../routes/groups/types';
	import { getContext } from 'svelte';
	import { type Writable } from 'svelte/store';
	
	export let row: ReadUserModel;
	let groupStore = getContext('group') as Writable<UpdateGroupModel>;
	function toggleUser(userId: number) {
    groupStore.update(g => {
        const index = g.userIds?.indexOf(userId) ?? -1;
        if (index === -1) {
            g.userIds = [...(g.userIds || []), userId];
        } else {
            g.userIds = (g.userIds || []).filter(id => id !== userId);
        }
        return g;
    });
	}
</script>

<div class="flex flex-col gap-2">
<label class="flex items-center gap-2 p-1 border rounded hover:bg-gray-50">
<input
    type="checkbox"
    checked={$groupStore.userIds?.includes(row.id)}
    on:change={() => toggleUser(row.id)}
/>

		</label>
</div>