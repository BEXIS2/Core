<script lang="ts">
	export let value;
	export let row;
</script>

<input type="checkbox" name="isApproved" id={row.id} checked={value} disabled />
