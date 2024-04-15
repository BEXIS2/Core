<script>
	import { onMount } from 'svelte';

	import { Alert, Spinner } from '@bexis2/bexis2-core-ui';

	import { hooksStatus } from '../../routes/edit/stores';

	import Fa from 'svelte-fa';
	import { faLock } from '@fortawesome/free-solid-svg-icons';

	import TimeDuration from '$lib/components/utils/TimeDuration.svelte';

	export let name;
	export let displayName;
	export let content = 9;
	export let visible = true;
	export let status = 0;
	export let color = '';

	$: error = [];
	$: success = null;
	$: warnings = [];

	$: date = '';

	$: active = false;
	$: wait = false;

	onMount(async () => {
		//active = setActive(status);
		hooksStatus.subscribe((h) => {
			if (h[name] != undefined) {
				setStatus(h[name]);
			}
		});
		error = [];
		success = null;
		warnings = [];
	});

	function errorHandler(e) {
		resetInformations();
		error = e.detail.messages;
	}

	function successHandler(e) {
		resetInformations();
		success = e.detail.text;
	}

	function warningHandler(e) {
		resetInformations();
		warnings[0] = e.detail.text;
	}

	function dateHandler(e) {
		date = e.detail.lastModification;
	}

	function resetInformations() {
		error = [];
		warnings = [];
		success = null;
	}

	// visibility
	function setStatus(status) {
		if (status == 0 || status == 1 || status == 5) {
			// disabled || access denied || inactive
			active = false;
		} else {
			active = true; // every other status enable the hook
		}

		wait = status == 6 ? true : false; // wait for somthing

		if (wait) {
			resetAlerts();
		}
	}

	function resetAlerts() {
		error = [];
		success = null;
		warnings = [];
	}
</script>

{#if visible && active}
	<div class="flex p-5 m-2 gap-5">
		<div class="flex-none w-48">
			<h4 class="h4">{displayName}</h4>
			<div class="text-sm py-2">
				{#if date}
					<TimeDuration milliseconds={new Date(date)} />
				{/if}
			</div>
		</div>
		<div class="grow space-y-2">
			{#if error}
				{#each error as item}
					<Alert cssClass="variant-filled-error" message={item} />
				{/each}
			{/if}
			{#if warnings}
				{#each warnings as item}
					<Alert cssClass="variant-filled-warning" message={item} />
				{/each}
			{/if}
			{#if success}
				<Alert cssClass="variant-filled-success" message={success} />
			{/if}
			<div class="h-full w-full">
				{#if !wait}
					<slot {errorHandler} {successHandler} {warningHandler} {dateHandler}
						>render view</slot
					>
				{:else}
					<div class="flex gap-2 text-surface-600">
						<Fa icon={faLock} size="lg" />
						<span>this area is locked, because data is uploading</span>
					</div>
				{/if}
			</div>
		</div>
	</div>
{/if}
