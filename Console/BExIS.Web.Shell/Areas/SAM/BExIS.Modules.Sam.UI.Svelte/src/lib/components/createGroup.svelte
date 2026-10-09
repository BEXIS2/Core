<!-- src/lib/components/createGroup.svelte -->
<script lang="ts">
	import { createEventDispatcher, SvelteComponent, setContext, type ComponentType } from 'svelte';
	import { createGroup } from '../../routes/groups/services';
	import type { CreateGroupModel } from '../../routes/groups/types';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';
	import { TextInput, TextArea, type TableConfig, Table } from '@bexis2/bexis2-core-ui';
	import type { ReadUserModel } from '../../routes/users/types';
	import { getUsers, usersStore } from '../../routes/users/services';
	import CreateGroupOptions from './createGroupOptions.svelte';
	import createGroupValidation from './createGroupValidation';

	const dispatch = createEventDispatcher();

	let isSubmitting = false;

	export let group: CreateGroupModel;

	setContext('group', group);

	async function handleSubmit(event: Event) {
		event.preventDefault();

		isSubmitting = true;

		try {
			await createGroup(group);
			dispatch('success');
		} catch (error) {
			console.error('Fehler:', error);
		} finally {
			isSubmitting = false;
		}
	}

	$: usersTableConfig = {
		id: 'usersTable',
		data: usersStore,
		optionsComponent: CreateGroupOptions as ComponentType<SvelteComponent>,
		columns: {
			creationDate: { exclude: true },
			modificationDate: { exclude: true },
			groupIds: { exclude: true }
		}
	};

		// validation
	// validation
	let validationResult = createGroupValidation.get();
	// flag to enable submit button
	$: disabled = validationResult.hasErrors() || !validationResult.isValid();

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	async function onChangeHandler(e) {

		setTimeout(async () => {
			validationResult = createGroupValidation(group, e.target.id);
		}, 10);
	}
</script>

<div class="card p-4">
	<form on:submit={handleSubmit} class="space-y-4">
		<div class="flex grow gap-4">
			<div class="grow">
				<TextInput
					id="name"
					label="Name"
					help={true}
					required={true}
					bind:value={group.name}
					valid={validationResult.isValid('name')}
					invalid={validationResult.hasErrors('name')}
					feedback={validationResult.getErrors('name')}
					on:input={onChangeHandler}
				/>
			</div>

			<div class="grow">
				<TextArea
					id="description"
					label="Beschreibung"
					help={true}
					required={true}
					bind:value={group.description}
					valid={validationResult.isValid('description')}
					invalid={validationResult.hasErrors('description')}
					feedback={validationResult.getErrors('description')}
					on:input={onChangeHandler}
				/>
			</div>
		</div>

		<div class="w-full">
			<label>Users</label>
			<Table config={usersTableConfig} />
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
