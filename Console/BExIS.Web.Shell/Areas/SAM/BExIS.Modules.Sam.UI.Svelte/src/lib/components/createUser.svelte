<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { createUser } from '../../routes/users/services';
	import type { CreateUserModel } from '../../routes/users/types';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';

	const dispatch = createEventDispatcher();

	let isSubmitting = false;

	async function handleSubmit(event: Event) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		const formData = new FormData(form);

		const model: CreateUserModel = {
			userName: formData.get('userName') as string,
			email: formData.get('email') as string
		};

		isSubmitting = true;

		try {
			await createUser(model);
			dispatch('success'); // Parent schließt + lädt neu
		} catch (error) {
			console.error(error);
			// Optional: Fehlermeldung anzeigen
		} finally {
			isSubmitting = false;
		}
	}
</script>

<form on:submit={handleSubmit} class="space-y-4">
	<div>
		<label class="block text-sm font-medium">Name</label>
		<input
			type="text"
			name="userName"
			required
			class="input input-bordered w-full"
			disabled={isSubmitting}
		/>
	</div>

	<div>
		<label class="block text-sm font-medium">Email</label>
		<input
			type="email"
			name="email"
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