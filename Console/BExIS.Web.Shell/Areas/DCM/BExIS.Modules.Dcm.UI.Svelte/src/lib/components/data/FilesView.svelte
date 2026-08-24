<script lang="ts">
	import { FileInfo, type fileInfoType } from '@bexis2/bexis2-core-ui';
	import Fa from 'svelte-fa';
	import { faDownload, faEye, faXmark } from '@fortawesome/free-solid-svg-icons';
	import { fade } from 'svelte/transition';

	export let id = 0;
	export let files: fileInfoType[] = [];
	$: files;

	export let descriptionType: number = 0;
	$: descriptionType;

	// 'attachments' uses /dcm/attachments/download, 'data' uses /ddm/Data/DownloadFile
	export let downloadMode: 'attachments' | 'data' = 'attachments';
	export let versionId: number = 0;

	let previewFile: any = null;

	function formatSize(bytes: number): string {
		if (!bytes || bytes <= 0) return '';
		const units = ['B', 'KB', 'MB', 'GB'];
		let i = 0;
		let size = bytes;
		while (size >= 1024 && i < units.length - 1) {
			size /= 1024;
			i++;
		}
		return `${size.toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
	}

	function getFileUrl(file: any, preview: boolean = false): string {
		if (downloadMode === 'data') {
			return `/ddm/Data/DownloadFile?id=${id}&version=${versionId}&path=${encodeURIComponent(file.path || file.name)}&mimeType=${encodeURIComponent(file.type || '')}&preview=${preview}`;
		}
		return `/dcm/attachments/download?datasetId=${id}&fileName=${encodeURIComponent(file.name)}&preview=${preview}`;
	}

	function getPreviewType(type: string): 'image' | 'video' | 'audio' | 'pdf' | 'other' {
		if (!type) return 'other';
		const t = type.toLowerCase();
		if (t.startsWith('image/')) return 'image';
		if (t.startsWith('video/')) return 'video';
		if (t.startsWith('audio/')) return 'audio';
		if (t === 'application/pdf') return 'pdf';
		// text, xml, xsd, etc. are not reliably previewable in iframe — hide preview button
		return 'other';
	}

	function canPreview(type: string): boolean {
		return getPreviewType(type) !== 'other';
	}

	function openPreview(file: any) {
		previewFile = file;
	}

	function closePreview() {
		previewFile = null;
	}
</script>

{#if files.length > 0}
	<div class="flex flex-col gap-2">
		{#each files as file, index}
			<div class="flex items-center gap-3 rounded-lg border border-surface-300 dark:border-surface-600 px-3 py-2.5 hover:bg-surface-50 dark:hover:bg-surface-800 transition-colors">
				<div class="shrink-0"><FileInfo type={file.type} size="x-large" /></div>
				<div class="flex-1 min-w-0 flex flex-col gap-0.5">
					<div class="flex items-center gap-2 flex-wrap">
						<span class="text-sm font-medium text-surface-900 dark:text-surface-100 truncate" title={file.name}>{file.name}</span>
						{#if file.type}
							<span class="badge variant-soft-surface text-xs whitespace-nowrap text-surface-700 dark:text-surface-200">{file.type}</span>
						{/if}
						{#if file.length}
							<span class="text-xs text-surface-700 dark:text-surface-200 whitespace-nowrap">{formatSize(file.length)}</span>
						{/if}
					</div>
					{#if file.description}
						<span class="text-xs text-surface-700 dark:text-surface-200 truncate" title={file.description}>{file.description}</span>
					{/if}
				</div>
				<div class="shrink-0 flex items-center gap-1">
					{#if canPreview(file.type)}
						<button
							class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200 hover:text-primary-600"
							title="Preview {file.name}"
							on:click={() => openPreview(file)}>
							<Fa icon={faEye} />
						</button>
					{/if}
					<a href={getFileUrl(file)} download
						class="btn-icon variant-ghost-surface text-surface-700 dark:text-surface-200 hover:text-primary-600"
						title="Download {file.name}">
						<Fa icon={faDownload} />
					</a>
				</div>
			</div>
		{/each}
	</div>
{:else}
	<div class="flex justify-start">
		<span class="text-sm text-surface-700 dark:text-surface-200">No files available.</span>
	</div>
{/if}

{#if previewFile}
	<!-- svelte-ignore a11y-click-events-have-key-events -->
	<!-- svelte-ignore a11y-no-static-element-interactions -->
	<div
		class="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
		on:click={closePreview}
		transition:fade={{ duration: 150 }}>
		<div
			class="relative rounded-lg bg-white dark:bg-surface-900 shadow-xl flex flex-col"
			style="max-height: 90vh; width: 90vw; max-width: 90vw;"
			on:click|stopPropagation>
			<div class="flex items-center justify-between gap-4 border-b border-surface-300 dark:border-surface-600 px-4 py-2">
				<span class="text-sm font-medium truncate" title={previewFile.name}>{previewFile.name}</span>
				<button
					class="btn-icon variant-ghost-surface shrink-0 text-surface-700 dark:text-surface-200"
					title="Close preview"
					on:click={closePreview}>
					<Fa icon={faXmark} />
				</button>
			</div>
			<div class="overflow-auto p-4 flex items-center justify-center" style="max-height: calc(90vh - 3rem);">
				{#if getPreviewType(previewFile.type) === 'image'}
					<img src={getFileUrl(previewFile, true)} alt={previewFile.name} class="max-h-[80vh] max-w-full object-contain rounded" />
				{:else if getPreviewType(previewFile.type) === 'video'}
					<video controls class="max-h-[80vh] max-w-full">
						<source src={getFileUrl(previewFile, true)} type={previewFile.type} />
					</video>
				{:else if getPreviewType(previewFile.type) === 'audio'}
					<div class="flex flex-col items-center gap-4 p-8">
						<audio controls>
							<source src={getFileUrl(previewFile, true)} type={previewFile.type} />
						</audio>
					</div>
				{:else if getPreviewType(previewFile.type) === 'pdf'}
					<div class="w-full" style="height: 85vh;">
						<iframe src={getFileUrl(previewFile, true)} class="w-full h-full" title={previewFile.name}></iframe>
					</div>
				{/if}
			</div>
		</div>
	</div>
{/if}
