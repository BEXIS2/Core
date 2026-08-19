<script lang="ts">
	import { goTo } from '$services/BaseCaller';
	import Fa from 'svelte-fa';
	import {
		faArrowUpRightFromSquare,
		faCircleCheck,
		faChevronDown,
		faChevronUp,
		faClockRotateLeft,
		faLink
	} from '@fortawesome/free-solid-svg-icons';
	import type { ReferenceModel } from '../../../models/View';

	export let links: ReferenceModel[] = [];

	const maxVisible = 3;
	let expandedTypes: Set<string> = new Set();

	$: types = links
		.map((link) => link.linkType)
		.filter((value, index, self) => self.indexOf(value) === index);

	function getTypeLabel(type: string): string {
		if (!type) return 'General';
		return type.charAt(0).toUpperCase() + type.slice(1).toLowerCase();
	}

	function getLinksForType(type: string): ReferenceModel[] {
		return links
			.filter((link) => link.linkType === type)
			.sort((a, b) => (a.referenceType || '').localeCompare(b.referenceType || ''));
	}

	function toggle(type: string) {
		if (expandedTypes.has(type)) {
			expandedTypes = new Set([...expandedTypes].filter((t) => t !== type));
		} else {
			expandedTypes = new Set([...expandedTypes, type]);
		}
	}
</script>

{#if types.length > 0}
	<h3 class="h3">Related Work</h3>
	<div class="flex flex-col gap-4">
		{#each types as type}
			{@const typeLinks = getLinksForType(type)}
			{@const isExpanded = expandedTypes.has(type)}
			{@const visibleLinks = isExpanded ? typeLinks : typeLinks.slice(0, maxVisible)}
			{@const hiddenCount = typeLinks.length - maxVisible}
			<div class="flex flex-col gap-2">
				<span
					class="text-xs font-semibold uppercase tracking-wider text-surface-500 flex items-center gap-1"
				>
					<Fa icon={faLink} class="text-surface-400" />
					{getTypeLabel(type)}
					<span class="font-normal text-surface-400">({typeLinks.length})</span>
				</span>
				<div class="flex flex-col gap-1.5">
					{#each visibleLinks as link, i}
						{@const showBadge =
							i === 0 || (visibleLinks[i - 1].referenceType || '') !== (link.referenceType || '')}
						<div
							class="flex items-center gap-2 rounded-lg border border-surface-200 dark:border-surface-700 px-3 py-2 hover:bg-surface-50 dark:hover:bg-surface-800 transition-colors"
						>
							<span
								class="badge {showBadge
									? 'variant-soft-surface'
									: ''}  text-xs whitespace-nowrap shrink-0 w-28 text-center"
								>{showBadge ? link.referenceType : ''}</span
							>
							<div class="flex items-center gap-2 flex-wrap min-w-0 flex-1">
								{#if link.target.type.toLocaleLowerCase() == 'extension'}
									<!-- svelte-ignore a11y-missing-attribute -->
									<a
                    role="button"
                    tabindex="0"
                    title="Open latest version"
										class="text-sm font-medium text-primary-700 dark:text-primary-300 hover:underline cursor-pointer inline-flex items-center gap-1"
										on:click={() =>
											goTo(
												'/dcm/view/?id=' + link.target.id + '&version=' + link.target.version,
												true
											)}
										on:keydown={(e) =>
											e.key === 'Enter' &&
											goTo(
												'/dcm/view/?id=' + link.target.id + '&version=' + link.target.version,
												true
											)}
									>
										{link.target?.title ? link.target.title : 'No title available'}
									</a>
								{:else}
									<a
										class="text-sm font-medium text-primary-700 dark:text-primary-300 hover:underline inline-flex items-center gap-1"
										href="/dcm/view?id={link.target.id}&version={link.target.version}"
										target="_blank"
									>
										{link.target?.title ? link.target.title : 'No title available'}
										<Fa icon={faArrowUpRightFromSquare} class="text-xs opacity-60" />
									</a>
									{#if link.target.latestVersion}
									<!--	<span
											class="inline-flex items-center gap-1 text-xs text-success-600 dark:text-success-400"
											title="Linked to the latest version"
										>
											<Fa icon={faCircleCheck} />
											latest
										</span>-->
									{:else}
										<span
											class="inline-flex items-center gap-1 text-xs text-warning-600 dark:text-warning-400"
											title="Originally linked to an older version (v{link.target.version})"
										>
										
											<Fa icon={faClockRotateLeft} />
										<a	
												class="dark:text-primary-300 hover:underline inline-flex items-center gap-1"
												href="/dcm/view?id={link.target.id}&version={link.target.version}"
												target="_blank"
											>older version
                      <Fa icon={faArrowUpRightFromSquare} class="text-xs opacity-60" />
												
											</a>										</span>
									{/if}
								{/if}
								{#if link.context}
									<span class="text-xs text-surface-400 ml-auto truncate" title={link.context}
										>{link.context}</span
									>
								{/if}
							</div>
						</div>
					{/each}
				</div>
				{#if hiddenCount > 0}
					<button
						class="btn btn-sm variant-ghost-surface self-start mt-1 inline-flex items-center gap-1.5 text-xs"
						on:click={() => toggle(type)}
					>
						{#if isExpanded}
							<Fa icon={faChevronUp} />
							Show less
						{:else}
							<Fa icon={faChevronDown} />
							Show {hiddenCount} more
						{/if}
					</button>
				{/if}
			</div>
		{/each}
	</div>
{/if}
