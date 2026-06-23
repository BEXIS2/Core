<script>
	import HookContainer from '$lib/components/HookContainer.svelte';
	import DefaultViewHook from '$lib/components/DefaultViewHook.svelte';
	import Attachments from '$lib/hooks/Attachment.svelte';

	import { Spinner } from '@bexis2/bexis2-core-ui';
	import Link from '../../lib/hooks/Link.svelte';
	import Permission from '../../lib/hooks/Permission.svelte';
	import Publish from '../../lib/hooks/Publish.svelte';

	export let id;
	export let version;
	export let hooks = [];

	$: seperateHooks(hooks);

	let attachmentHook;
	// let linkHook;
	// let permissionsHook;
	// let publishHook;

	$: addtionalhooks = [];

	function seperateHooks(hooks) {
		console.log('h', hooks);
		hooks.forEach((element) => {
			if (element.name == 'attachments') {
				attachmentHook = element;
			} 
			//else if(element.name == 'link'){
			// 	linkHook = element;
			// } else if(element.name == 'permissions'){
			// 	permissionsHook = element;
			// } else if(element.name == 'publish'){
			// 	publishHook = element;
			// }
			else {
				// console.log(element.name)
				addtionalhooks.push(element);
			}
		});
	}

	let hookColor = 'bg-surface-200';
</script>

<div class={hookColor}>
	{#if addtionalhooks}
		<!-- if hooks list is loaded render hooks -->
<!-- 
		<HookContainer {...linkHook} color={hookColor}>
			<div>
				<Link
					{id}
					{version}
					hook={linkHook}
					on:error={(e) => errorHandler(e)}
					on:success={(e) => successHandler(e)}
				/>
			</div>
		</HookContainer> -->

		<HookContainer {...attachmentHook} let:errorHandler let:successHandler color={hookColor}>
			<div>
				<Attachments
					{id}
					{version}
					hook={attachmentHook}
					on:error={(e) => errorHandler(e)}
					on:success={(e) => successHandler(e)}
				/>
			</div>
		</HookContainer>
<!-- 
		<HookContainer {...permissionsHook} let:errorHandler let:successHandler color={hookColor}>
			<div>
				<Permission
					{id}
					{version}
					hook={permissionsHook}
					on:error={(e) => errorHandler(e)}
					on:success={(e) => successHandler(e)}
				/>
			</div>
		</HookContainer>

		<HookContainer {...publishHook} let:errorHandler let:successHandler color={hookColor}>
			<div>
				<Publish
					{id}
					{version}
					hook={publishHook}
					on:error={(e) => errorHandler(e)}
					on:success={(e) => successHandler(e)}
				/>
			</div>
		</HookContainer> -->

		

		

		{#each addtionalhooks as hook}
			<HookContainer {...hook} color={hookColor}>
				<div>
					<DefaultViewHook {id} {version} {...hook} start={hook.start} />
				</div>
			</HookContainer>
		{/each}
	{:else}
		<!-- while data is not loaded show a loading information -->
		<Spinner />
	{/if}
</div>
