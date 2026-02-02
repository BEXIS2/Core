<script lang="ts">
  export let id: string;
  export let data: any;
  
  // svelteflow props
  export let selected: boolean = false;
  export let dragging: boolean = false;
  export let draggable: boolean = false;
  export let selectable: boolean = false;
  export let deletable: boolean = false;
  export let zIndex: number = 0;
  export let isConnectable: boolean = false;
  export let positionAbsoluteX: number = 0;
  export let positionAbsoluteY: number = 0;
  export let sourcePosition: any = undefined;
  export let targetPosition: any = undefined;
  export let width: number | undefined = undefined;
  export let height: number | undefined = undefined;
  export let dragHandle: string | undefined = undefined;
  export let parentId: string | undefined = undefined;
  export let parentNode: string | undefined = undefined;
  export let xPos: number = 0;
  export let yPos: number = 0;
</script>

<div class="section-node">
  <div class="section-label">{data?.label || 'Section'}</div>
</div>

<style>
  .section-node {
    background: #f0f0f0;
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 8px 12px;
    font-weight: bold;
    text-align: center;
    color: #666;
    font-size: 0.9rem;
    pointer-events: none; /* no interaction */
  }
  
  .section-label {
    margin: 0;
  }
</style>