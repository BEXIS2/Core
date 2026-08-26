<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { updateGroup } from '../../routes/groups/services';
	import type { UpdateGroupModel } from '../../routes/groups/types';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';

	export let group: UpdateGroupModel; // ← kommt vom Parent

	const dispatch = createEventDispatcher();
	let isSubmitting = false;

	async function handleSubmit(event: Event) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		const formData = new FormData(form);

		const model: UpdateGroupModel = {
			id: group.id,
			name: formData.get('name') as string,
			description: formData.get('description') as string
		};

		isSubmitting = true;

		try {
			await updateGroup(group.id, model); // Signatur ggf. anpassen
			dispatch('success');
		} catch (error) {
			console.error(error);
		} finally {
			isSubmitting = false;
		}
	}
</script>

<form on:submit={handleSubmit} class="space-y-4">
	<div>
		<label class="block text-sm font-medium">Id</label>
		<input
			type="number"
			name="id"
			value={group.id}
			required
			class="input input-bordered w-full"
			disabled={isSubmitting}
		/>
	</div>

	<div>
		<label class="block text-sm font-medium">Name</label>
		<input
			type="text"
			name="name"
			value={group.name}
			required
			class="input input-bordered w-full"
			disabled={isSubmitting}
		/>
	</div>

	<div>
		<label class="block text-sm font-medium">Description</label>
		<input
			type="text"
			name="description"
			value={group.description}
			required
			class="input input-bordered w-full"
			disabled={isSubmitting}
		/>
	</div>

<div class="flex gap-2 justify-end">
		<button
			type="button"
			class="btn variant-filled-warning h-9 w-16 shadow-md"
			disabled={isSubmitting}
			on:click={() => dispatch('close')}
		>
			<Fa icon={faXmark} />
		</button>

		<button
			type="submit"
			class="btn variant-filled-primary h-9 w-16 shadow-md"
			disabled={isSubmitting}
		>
			<Fa icon={faSave} />
		</button>
	</div>
</form>