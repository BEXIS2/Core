<!-- src/lib/components/createGroup.svelte -->
<script lang="ts">
	import { createEventDispatcher, SvelteComponent, setContext, type ComponentType } from 'svelte';
	import Fa from 'svelte-fa';
	import { faXmark, faSave } from '@fortawesome/free-solid-svg-icons';
	import { TextInput, TextArea, Table } from '@bexis2/bexis2-core-ui';
	import type { CreateUserModel } from '../../routes/users/types';
	import { createUser } from '../../routes/users/services';
	import CreateUserOptions from './createUserOptions.svelte';
	import createUserValidation from './createUserValidation';
	import { groupsStore } from '../../routes/groups/services';

	const dispatch = createEventDispatcher();

	let isSubmitting = false;

	export let user: CreateUserModel;

	setContext('user', user);

	console.log('Initial user:', user);

	async function handleSubmit(event: Event) {
		event.preventDefault();

		isSubmitting = true;

		try {
			await createUser(user);
			dispatch('success');
		} catch (error) {
			console.error('Fehler:', error);
		} finally {
			isSubmitting = false;
		}
	}

	$: groupsTableConfig = {
		id: 'groupsTable',
		data: groupsStore,
		optionsComponent: CreateUserOptions as ComponentType<SvelteComponent>,
		columns: {
			creationDate: { exclude: true },
			modificationDate: { exclude: true },
			userIds: { exclude: true }
		}
	};

		// validation
	let validationResult = createUserValidation.get();
	// flag to enable submit button
	$: disabled = !validationResult.hasErrors();

	//change event: if input change check also validation only on the field
	// e.target.id is the id of the input component
	async function onChangeHandler(e) {

		setTimeout(async () => {
			validationResult = createUserValidation(user, e.target.id);
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
					valid={validationResult.isValid('userName')}
					invalid={validationResult.hasErrors('userName')}
					feedback={validationResult.getErrors('userName')}
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
					valid={validationResult.isValid('email')}
					invalid={validationResult.hasErrors('email')}
					feedback={validationResult.getErrors('email')}
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
