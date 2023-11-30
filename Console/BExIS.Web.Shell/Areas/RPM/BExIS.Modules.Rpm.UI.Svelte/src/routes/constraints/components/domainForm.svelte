<script lang="ts">
	import { CodeEditor, Table } from '@bexis2/bexis2-core-ui';
	import { slide } from 'svelte/transition';
	import type { DomainConstraintListItem } from '../models';
	import { writable } from 'svelte/store';

	export let domainConstraint: DomainConstraintListItem;
	const domainItemsTableStore = writable<string[]>([]);
	$: domainItemsTableStore.set(setDomainItems(domainConstraint.domain));

	function setDomainItems(domain: string): string[] {
		let dis: string[] = [];
		let lines = domain.split('\n');
		lines.forEach(function (value) {
				dis.push(value);
		});
		return dis;
	}
</script>

{#if domainConstraint}
	<div class="grid grid-cols-3 gap-5 h-80" in:slide out:slide>
		<div class="pb-3">
			<CodeEditor
				id="domain"
				initialValue={domainConstraint.domain}
				toggle={false}
				bind:value={domainConstraint.domain}
				actions={false}
				styles={{
					'&': {
						borderRadius: '0.5rem',
						width: '100%',
						height: '20rem'
					},
					'.cm-scroller': {
						borderRadius: '0.5rem'
					}
				}}
			/>
		</div>
		<div class="pb-3 h-80">
			<div id="itemstable" class="table-container h-80">
				<table class="table w-full table-compact bg-tertiary-200 h-80">
					<thead>
						<tr class="bg-primary-300">
							<th class="!p-2">Domain List</th>
						</tr>
					</thead>
					<tbody>
						{#each $domainItemsTableStore as domainItem}
							<tr>
								<td>{domainItem}</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		</div>
		<div class="pb-3 w-48">put help here</div>
	</div>
{/if}
