<script lang="ts">
  import { Handle, Position } from '@xyflow/svelte';

  export let id: string;
  export let data: any;
  export let type: string;
  export let selected: boolean = false;
  export let dragging: boolean = false;
  export let draggable: boolean = true;
  export let selectable: boolean = true;
  export let deletable: boolean = true;
  export let zIndex: number = 0;

  // svelteflow props needed to avoid warnings
  export let isConnectable: boolean = true;
  export let positionAbsoluteX: number = 0;
  export let positionAbsoluteY: number = 0;
  export let sourcePosition: Position | undefined = undefined;
  export let targetPosition: Position | undefined = undefined;
  export let width: number | undefined = undefined;
  export let height: number | undefined = undefined;
  export let dragHandle: string | undefined = undefined;
  export let parentId: string | undefined = undefined;
  export let parentNode: string | undefined = undefined;
  export let xPos: number = 0;
  export let yPos: number = 0;
  export let connectable: boolean = true;
</script>

<div class="item-node-content" class:selected class:required={data?.required}>
  <Handle 
    type="target" 
    position={Position.Right} 
    id={`${id}-handle`} 
    style="position: absolute; right: -4px; top: 50%; transform: translateY(-50%); width: 8px; height: 8px; background: #007acc; border: 2px solid white; border-radius: 50%;"
  />

  <div class="item-content">
    {#if data?.type}
      <div class="item-type">{data.type}</div>
    {/if}
    <div class="item-label">{data?.label || 'Item'}</div>
  </div>
</div>

<style>
  .item-node-content {
    border: 2px solid #888;
    border-radius: 6px;
    background: white;
    padding: 8px;
    min-width: 100px;
    position: relative;
    transition: all 0.2s;
    text-align: center;
  }
  .item-node-content.selected {
    border-color: #ff6b35;
    box-shadow: 0 0 8px rgba(255, 107, 53, 0.3);
  }
  .item-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
  }
  .item-label {
    font-weight: bold;
    color: #333;
    font-size: 0.8rem;
    line-height: 1.2;
  }
  .item-type {
    font-size: 0.65rem;
    color: #888;
    background: #f0f0f0;
    padding: 2px 6px;
    border-radius: 3px;
  }
</style>