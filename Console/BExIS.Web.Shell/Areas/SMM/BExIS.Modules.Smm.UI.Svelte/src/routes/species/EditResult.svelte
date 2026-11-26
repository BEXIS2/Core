<script lang="ts">
	export let row;
</script>

<div class="p-5 rounded-lg bg-white grid gap-2">
	<label for="resultName">Result name</label>
	<input type="text" id="resultName" class="input input-primary" bind:value={row.inputName} />
	<div class="flex gap-2 justify-end">
		<button class="btn variant-filled-error">Cancel</button>
		<button class="btn variant-filled-success">Done</button>
	</div>
</div>