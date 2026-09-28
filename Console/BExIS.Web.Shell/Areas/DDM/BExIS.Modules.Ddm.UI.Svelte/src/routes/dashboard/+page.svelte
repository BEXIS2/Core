<script lang="ts">
	import { onMount } from 'svelte';
	import { writable } from 'svelte/store';
	import { Page, Spinner, Table, TablePlaceholder, notificationStore, notificationType } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import {
		faTable,
		faFile,
		faEye,
		faPenToSquare,
		faCopy,
		faTags,
		faCheck,
		faXmark,
		faCircleMinus,
		faTableColumns,
		faInbox,
		faClockRotateLeft
	} from '@fortawesome/free-solid-svg-icons';
	import {
		getMyDatasets,
		getMyRequests,
		getDecisions,
		getUseTags,
		getEntities,
		withdrawRequest,
		acceptDecision,
		rejectDecision,
		type MyDatasetModel,
		type RequestModel,
		type DecisionModel,
		type EntityModel
	} from './services';
	import TableOptions from './tableOptions.svelte';
	import TableTypeIcon from './tableTypeIcon.svelte';
	import TableTagIcon from './tableTagIcon.svelte';
	import TableDataIcon from './tableDataIcon.svelte';
	import TableValidBadge from './tableValidBadge.svelte';
	import { useTagsStore } from './stores';

	const TableOptionsCasted = TableOptions as any;
	const TableTypeIconCasted = TableTypeIcon as any;
	const TableTagIconCasted = TableTagIcon as any;
	const TableDataIconCasted = TableDataIcon as any;
	const TableValidBadgeCasted = TableValidBadge as any;

	type Tab = 'datasets' | 'requests' | 'decisions';

	let activeTab: Tab = 'datasets';
	let useTags = false;
	let datasetRightType: string = '';
	let entityName: string = 'Dataset';
	let entities: EntityModel[] = [];
	let loadingDatasets = false;
	let loadingRequests = false;
	let loadingDecisions = false;

	let datasets: MyDatasetModel[] = [];
	let requests: RequestModel[] = [];
	let decisions: DecisionModel[] = [];

	const datasetsStore = writable<any[]>([]);

	$: pendingDecisions = decisions.filter(d => d.status === 0).length;

	const datasetTabs = [
		{ key: 'write', label: 'Edit (Write Permission)' },
		{ key: 'grant', label: 'Own (Grant Permission)' },
		{ key: 'read', label: 'Download' }
	];

	function formatDate(dateStr: string): string {
		if (!dateStr) return '';
		return new Date(dateStr).toLocaleDateString();
	}

	onMount(async () => {
		useTags = await getUseTags();
		useTagsStore.set(useTags);
		entities = await getEntities();
		// load requests and decisions upfront for badge counts; datasets load on demand
		await Promise.all([loadRequests(), loadDecisions()]);
	});

	async function loadDatasets() {
		loadingDatasets = true;
		datasets = await getMyDatasets(datasetRightType, entityName);
		// sort descending by ID (newest first)
		datasets = datasets.sort((a, b) => b.id - a.id);
		// restructure objects so columns are in the right order — Table derives column order from data keys
		datasetsStore.set(datasets.map(ds => ({ id: ds.id, type: ds.type, hasTag: ds.tagNr > 0 ? ds.tagNr : 0, hasData: ds.hasData, title: ds.title, description: ds.description, isValid: ds.isValid, isOwn: ds.isOwn })));
		loadingDatasets = false;
	}

	async function loadRequests() {
		loadingRequests = true;
		requests = (await getMyRequests()).sort((a, b) => b.id - a.id);
		loadingRequests = false;
	}

	async function loadDecisions() {
		loadingDecisions = true;
		decisions = (await getDecisions()).sort((a, b) => b.id - a.id);
		loadingDecisions = false;
	}

	async function switchDatasetTab(key: string) {
		datasetRightType = key;
		await loadDatasets();
	}

	async function switchEntity(name: string) {
		entityName = name;
		await loadDatasets();
	}

	async function switchTab(tab: Tab) {
		activeTab = tab;
		if (tab === 'datasets' && datasets.length === 0) {
			await loadDatasets();
		} else if (tab === 'requests' && requests.length === 0) {
			await loadRequests();
		} else if (tab === 'decisions' && decisions.length === 0) {
			await loadDecisions();
		}
	}

	async function handleWithdraw(requestId: number) {
		const res = await withdrawRequest(requestId);
		if (res) {
			notificationStore.showNotification({ notificationType: notificationType.success, message: 'Request withdrawn.' });
			await loadRequests();
		}
	}

	async function handleAccept(decisionId: number) {
		const res = await acceptDecision(decisionId);
		if (res) {
			notificationStore.showNotification({ notificationType: notificationType.success, message: 'Request accepted.' });
			await loadDecisions();
		}
	}

	async function handleReject(requestId: number) {
		const res = await rejectDecision(requestId);
		if (res) {
			notificationStore.showNotification({ notificationType: notificationType.success, message: 'Request rejected.' });
			await loadDecisions();
		}
	}

	function onDatasetAction(e: any) {
		const { action, id } = e.detail.type;
		if (action === 'view') window.open(`/dcm/view/?id=${id}`, '_self');
		else if (action === 'edit') window.open(`/dcm/edit/?id=${id}`, '_self');
		else if (action === 'copy') {
			if (confirm('Do you really want to create a copy of this dataset? This will create a new dataset.'))
				window.open(`/dcm/create/copy?id=${id}`, '_blank');
		} else if (action === 'tags') window.open(`/ddm/taginfo/?id=${id}`, '_self');
	}
</script>

<Page title="My Data">
	<div class="flex flex-col gap-4">

		<!-- Tab navigation -->
		<div class="flex gap-2 border-b border-surface-200 dark:border-surface-700">
			<button
				class="px-4 py-2 text-sm font-medium border-b-2 transition-colors {activeTab === 'datasets' ? 'border-primary-500 text-primary-700 dark:text-primary-300' : 'border-transparent text-surface-600 dark:text-surface-300 hover:text-surface-800'}"
				on:click={() => switchTab('datasets')}>
				<Fa icon={faTableColumns} class="mr-1" />
				My Datasets
				<span class="ml-1 text-xs text-surface-600 dark:text-surface-300">({datasets.length})</span>
			</button>
			<button
				class="px-4 py-2 text-sm font-medium border-b-2 transition-colors {activeTab === 'requests' ? 'border-primary-500 text-primary-700 dark:text-primary-300' : 'border-transparent text-surface-600 dark:text-surface-300 hover:text-surface-800'}"
				on:click={() => switchTab('requests')}>
				<Fa icon={faInbox} class="mr-1" />
				My Requests
				<span class="ml-1 text-xs text-surface-600 dark:text-surface-300">({requests.length})</span>
			</button>
			<button
				class="px-4 py-2 text-sm font-medium border-b-2 transition-colors {activeTab === 'decisions' ? 'border-primary-500 text-primary-700 dark:text-primary-300' : 'border-transparent text-surface-600 dark:text-surface-300 hover:text-surface-800'}"
				on:click={() => switchTab('decisions')}>
				<Fa icon={faClockRotateLeft} class="mr-1" />
				Decisions
				{#if pendingDecisions > 0}
					<span class="ml-1 badge variant-filled-error text-xs">{pendingDecisions}</span>
				{:else}
					<span class="ml-1 text-xs text-surface-600 dark:text-surface-300">({decisions.length})</span>
				{/if}
			</button>
		</div>

		<!-- Datasets tab -->
		{#if activeTab === 'datasets'}
			{#if entities.length > 1}
				<div class="flex gap-2">
					{#each entities as ent}
						<button
							class="badge {entityName === ent.name ? 'variant-filled-primary' : 'variant-soft-surface'} cursor-pointer"
							on:click={() => switchEntity(ent.name)}>
							{ent.name}
						</button>
					{/each}
				</div>
			{/if}
			<div class="flex gap-2">
				{#each datasetTabs as tab}
					<button
						class="badge {datasetRightType === tab.key ? 'variant-filled-primary' : 'variant-soft-surface'} cursor-pointer"
						on:click={() => switchDatasetTab(tab.key)}>
						{tab.label}
					</button>
				{/each}
				{#if datasetRightType === ''}
					<span class="text-sm text-surface-600 dark:text-surface-300">Select a right type to load datasets</span>
				{/if}
			</div>

			{#if loadingDatasets}
				<TablePlaceholder cols={6} />
			{:else if datasetRightType === ''}
				<div class="text-center py-8 text-surface-600 dark:text-surface-300 text-sm">Select a right type above to load datasets.</div>
			{:else if datasets.length > 0}
				<div class="table table-compact w-full">
					{#key datasetRightType + entityName}
						<Table
							on:action={onDatasetAction}
							config={{
								id: 'MyDatasets',
								data: datasetsStore,
								optionsComponent: TableOptionsCasted,
								columns: {
									id: { header: 'ID', disableFiltering: true, fixedWidth: 70 },
									type: { header: 'Type', fixedWidth: 60, disableFiltering: true, instructions: { renderComponent: TableTypeIconCasted, toStringFn: (v) => v, toSortableValueFn: (v) => v } },
									hasTag: { header: 'Tag', fixedWidth: 60, disableFiltering: true, exclude: !useTags, instructions: { renderComponent: TableTagIconCasted, toStringFn: (v) => v ? 'yes' : 'no', toSortableValueFn: (v) => v } },
									hasData: { header: 'Data', fixedWidth: 50, disableFiltering: true, disableSorting: true, instructions: { renderComponent: TableDataIconCasted, toStringFn: (v) => v ? 'yes' : 'no' } },
									title: { header: 'Title' },
									description: { header: 'Description', disableFiltering: true },
									isValid: { header: 'Valid', fixedWidth: 80, instructions: { renderComponent: TableValidBadgeCasted, toStringFn: (v) => v === 'yes' ? 'valid' : 'invalid', toFilterableValueFn: (v) => v === 'yes' ? 'valid' : 'invalid', toSortableValueFn: (v) => v } },
									isOwn: { exclude: true },
									optionsColumn: { fixedWidth: 160 }
								}
							}}
						/>
					{/key}
				</div>
			{:else}
				<div class="text-center py-8 text-surface-600 dark:text-surface-300 text-sm">No datasets found.</div>
			{/if}

		<!-- Requests tab -->
		{:else if activeTab === 'requests'}
			{#if loadingRequests}
				<div class="flex justify-center py-8"><Spinner /></div>
			{:else if requests.length > 0}
				<div class="flex flex-col gap-1.5">
					{#each requests as req}
						<div class="flex items-center gap-3 rounded-lg border border-surface-200 dark:border-surface-700 px-3 py-2.5 hover:bg-surface-50 dark:hover:bg-surface-800 transition-colors">
							<div class="shrink-0 w-16 text-sm text-surface-600 dark:text-surface-300">#{req.instanceId}</div>
							<div class="flex-1 min-w-0 flex flex-col gap-0.5">
								<div class="flex items-center gap-2 flex-wrap">
									<a href="/dcm/view/?id={req.instanceId}" class="text-sm font-medium text-primary-700 dark:text-primary-300 hover:underline truncate" title={req.title}>{req.title}</a>
									{#if req.intention}
										<span class="badge variant-soft-surface text-xs text-surface-700 dark:text-surface-200">{req.intention}</span>
									{/if}
								</div>
								<span class="text-xs text-surface-600 dark:text-surface-300">{formatDate(req.requestDate)}</span>
							</div>
							<div class="shrink-0 flex items-center gap-2">
								<span class="badge {req.requestStatus === 'Open' ? 'variant-soft-warning' : 'variant-soft-surface'} text-xs">{req.requestStatus}</span>
								<a href="/dcm/view/?id={req.instanceId}" class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="View"><Fa icon={faEye} /></a>
								{#if req.requestStatus === 'Open'}
									<button class="btn-icon variant-ghost-error" title="Withdraw request" on:click={() => handleWithdraw(req.id)}>
										<Fa icon={faCircleMinus} />
									</button>
								{/if}
							</div>
						</div>
					{/each}
				</div>
			{:else}
				<div class="text-center py-8 text-surface-600 dark:text-surface-300 text-sm">No requests found.</div>
			{/if}

		<!-- Decisions tab -->
		{:else if activeTab === 'decisions'}
			{#if loadingDecisions}
				<div class="flex justify-center py-8"><Spinner /></div>
			{:else if decisions.length > 0}
				<div class="flex flex-col gap-1.5">
					{#each decisions as dec}
						<div class="flex items-center gap-3 rounded-lg border border-surface-200 dark:border-surface-700 px-3 py-2.5 hover:bg-surface-50 dark:hover:bg-surface-800 transition-colors">
							<div class="shrink-0 w-16 text-sm text-surface-600 dark:text-surface-300">#{dec.instanceId}</div>
							<div class="flex-1 min-w-0 flex flex-col gap-0.5">
								<div class="flex items-center gap-2 flex-wrap">
									<a href="/dcm/view/?id={dec.instanceId}" class="text-sm font-medium text-primary-700 dark:text-primary-300 hover:underline truncate" title={dec.title}>{dec.title}</a>
									{#if dec.intention}
										<span class="badge variant-soft-surface text-xs text-surface-700 dark:text-surface-200">{dec.intention}</span>
									{/if}
								</div>
								<div class="flex items-center gap-2 text-xs text-surface-600 dark:text-surface-300">
									<span>by {dec.applicant}</span>
									<span>·</span>
									<span>{formatDate(dec.requestDate)}</span>
								</div>
							</div>
							<div class="shrink-0 flex items-center gap-2">
								{#if dec.status === 0}
									<span class="badge variant-soft-warning text-xs">pending</span>
									<a href="/dcm/view/?id={dec.instanceId}" class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="View"><Fa icon={faEye} /></a>
									<button class="btn-icon variant-ghost-success" title="Accept" on:click={() => handleAccept(dec.id)}>
										<Fa icon={faCheck} />
									</button>
									<button class="btn-icon variant-ghost-error" title="Reject" on:click={() => handleReject(dec.requestId)}>
										<Fa icon={faXmark} />
									</button>
								{:else if dec.status === 1}
									<span class="badge variant-soft-success text-xs">accepted</span>
									<a href="/dcm/view/?id={dec.instanceId}" class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="View"><Fa icon={faEye} /></a>
								{:else}
									<span class="badge variant-soft-error text-xs">rejected</span>
									<a href="/dcm/view/?id={dec.instanceId}" class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200" title="View"><Fa icon={faEye} /></a>
								{/if}
							</div>
						</div>
					{/each}
				</div>
			{:else}
				<div class="text-center py-8 text-surface-600 dark:text-surface-300 text-sm">No decisions found.</div>
			{/if}
		{/if}
	</div>
</Page>
