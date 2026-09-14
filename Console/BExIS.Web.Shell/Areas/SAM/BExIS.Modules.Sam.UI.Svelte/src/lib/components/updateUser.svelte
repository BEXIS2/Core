<!-- src/lib/components/createGroup.svelte -->
<script lang="ts">
	import { createEventDispatcher, SvelteComponent, setContext, type ComponentType } from 'svelte';
	import { groupsStore, updateGroupById } from '../../routes/groups/services';
	import type { ReadGroupModel, UpdateGroupModel } from '../../routes/groups/types';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';
	import { TextInput, TextArea, type TableConfig, Table } from '@bexis2/bexis2-core-ui';
	import type { ReadUserModel, UpdateUserModel } from '../../routes/users/types';
	import { updateUserById, usersStore } from '../../routes/users/services';
	import UpdateUserOptions from './updateUserOptions.svelte';
	import { writable } from 'svelte/store';
	import updateGroupValidation from './updateGroupValidation';
	import updateUserValidation from './updateUserValidation';

	const dispatch = createEventDispatcher();

	let isSubmitting = false;

	export let user: UpdateUserModel;

	const userStore = writable(user);
	setContext('user', userStore);

	$: userStore.set(user);
	

	async function handleSubmit(event: Event) {
		event.preventDefault();

		isSubmitting = true;

		try {
			await updateUserById(user.id, user);
			dispatch('success');
		} catch (error) {
			console.error('Fehler:', error);
		} finally {
			isSubmitting = false;
		}
	}

	const groupsTableConfig: TableConfig<ReadGroupModel> = {
		id: 'groupsTable',
		data: groupsStore,
		optionsComponent: UpdateUserOptions as ComponentType<SvelteComponent>,
		columns: {
			creationDate: { exclude: true },
			modificationDate: { exclude: true },
			userIds: { exclude: true }
		}
	};

			// validation
	let res = updateUserValidation.get();
	// flag to enable submit button
	$: disabled = !res.isValid();

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	async function onChangeHandler(e) {

		setTimeout(async () => {
			res = updateUserValidation(user, e.target.id);
		}, 10);
	}
</script>

<div class="card p-4">
	<form on:submit={handleSubmit} class="space-y-4">
		<div class="flex grow gap-4">
			<div class="grow">
				<TextInput
					id="userName"
					label="User Name"
					help={true}
					required={true}
					bind:value={user.userName}
					valid={res.isValid('userName')}
					invalid={res.hasErrors('userName')}
					feedback={res.getErrors('userName')}
					on:input={onChangeHandler}
				/>
			</div>

			<div class="grow">
				<TextInput
					id="email"
					label="Email"
					help={true}
					required={true}
					bind:value={user.email}
					valid={res.isValid('email')}
					invalid={res.hasErrors('email')}
					feedback={res.getErrors('email')}
					on:input={onChangeHandler}
				/>
			</div>
		</div>

		<div class="w-full">
			<label>Groups</label>
			<Table config={groupsTableConfig} />
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
				disabled={isSubmitting || disabled}
			>
				<Fa icon={faSave} />
			</button>
		</div>
	</form>
</div>