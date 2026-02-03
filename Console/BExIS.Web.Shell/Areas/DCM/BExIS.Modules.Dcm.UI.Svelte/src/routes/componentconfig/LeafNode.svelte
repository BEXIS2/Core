<script lang="ts">
  import { Handle, Position } from '@xyflow/svelte';
  import { get } from 'svelte/store';
  import { onDestroy } from 'svelte';

  export let id: string;
  export let data: any;
  export let type: string;
  export let selected: boolean = false;
  export let dragging: boolean = false;
  export let draggable: boolean = true;
  export let selectable: boolean = true;
  export let deletable: boolean = true;
  export let zIndex: number = 0;

  export let isConnectable: boolean = true;
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
  export let connectable: boolean = true;

  // reactive assignments
  $: edges = data?.edges;
  $: nodes = data?.nodes;
  $: onToggleVisibility = data?.onToggleVisibility;
  $: onSetAnchorpoint = data?.onSetAnchorpoint;
  $: activeMode = data?.activeInteractionMode || null;

  // internal reactive snapshots for stores
  let unsubscribeEdges: (() => void) | null = null;
  let unsubscribeNodes: (() => void) | null = null;
  let currentEdges: any[] = [];
  let currentNodes: any[] = [];

  // reactive updates for edges and nodes subscriptions
  $: {
    if (unsubscribeEdges) unsubscribeEdges();
    if (edges && typeof edges.subscribe === 'function') {
      unsubscribeEdges = edges.subscribe((val: any[]) => {
        currentEdges = Array.isArray(val) ? val : [];
      });
    } else {
      currentEdges = Array.isArray(edges) ? edges : [];
      unsubscribeEdges = null;
    }

    if (unsubscribeNodes) unsubscribeNodes();
    if (nodes && typeof nodes.subscribe === 'function') {
      unsubscribeNodes = nodes.subscribe((val: any[]) => {
        currentNodes = Array.isArray(val) ? val : [];
      });
    } else {
      currentNodes = Array.isArray(nodes) ? nodes : [];
      unsubscribeNodes = null;
    }
  }

  onDestroy(() => {
    if (unsubscribeEdges) unsubscribeEdges();
    if (unsubscribeNodes) unsubscribeNodes();
  });

  // check if leaf node is connected to a component node in current interaction mode
  function isConnectedInCurrentMode(edges: any[], nodes: any[], currentMode: string | null): boolean {
    if (!Array.isArray(edges) || !Array.isArray(nodes) || !currentMode) {
      return false;
    }
    
    const handleId = `${id}-handle`;
    
    const connected = edges.some((edge: any) => {
      // check if edge is connected to this leaf node
      const isEdgeConnected = edge.sourceHandle === handleId || edge.targetHandle === handleId;
      if (!isEdgeConnected) return false;
      
      // find component by checking BOTH source AND target for nodeWithItems type
      const sourceNode = nodes.find((n: any) => n.id === edge.source);
      const targetNode = nodes.find((n: any) => n.id === edge.target);
      
      // component node is the one with type 'nodeWithItems'
      const componentNode = sourceNode?.type === 'nodeWithItems' ? sourceNode : (targetNode?.type === 'nodeWithItems' ? targetNode : null);
      
      if (!componentNode) {
        return false;
      }
      
      // check if component node is in current mode
      const isInMode = componentNode.data?.interactionMode === currentMode;
      
      return isInMode;
    });
    
    return connected;
  }

  $: isConnected = isConnectedInCurrentMode(currentEdges, currentNodes, activeMode);
  $: isAnchorpoint = checkIfAnchorpoint(currentNodes);
  
  // check if is_visible property allows visibility
  $: isVisible = data?.is_visible !== false;

  function checkIfAnchorpoint(nodesArr: any[]): boolean {
    if (!Array.isArray(nodesArr) || !activeMode) return false;
    const jsonPath = data?.path || '';
    if (!jsonPath) return false;
    
    // check if any component node has this leaf node as anchorpoint in current mode
    return nodesArr.some(node =>
      node.type === 'nodeWithItems' && node.data?.anchorpoint === jsonPath && node.data?.interactionMode === activeMode
    );
  }

  // click event handlers
  function handleToggleVisibility(event: MouseEvent) {
    event.stopPropagation();
    if (!isConnected || !onToggleVisibility) {
      return;
    }
    onToggleVisibility(id);
  }

  function handleSetAnchorpoint(event: MouseEvent) {
    event.stopPropagation();
    if (!isConnected || !onSetAnchorpoint) {
      return;
    }

    const jsonPath = data?.path || '';
    if (!jsonPath) return;

    const handleId = `${id}-handle`;
    const connectedEdge = currentEdges.find(
      (edge: any) =>
        (edge.sourceHandle === handleId || edge.targetHandle === handleId)
    );
    if (!connectedEdge) return;

    // find component node connected to this leaf node
    const componentNode = currentNodes.find(
      (n: any) =>
        n.type === 'nodeWithItems' && n.data?.interactionMode === activeMode &&
        (connectedEdge.source === n.id || connectedEdge.target === n.id)
    );
    
    if (!componentNode) {
      return;
    }

    onSetAnchorpoint(componentNode.id, jsonPath);
  }

  $: showControls = isConnected;
</script>

<div class="leaf-node-content" class:selected>
  <Handle 
    type="target" 
    position={Position.Right} 
    id={`${id}-handle`} 
    style="position: absolute; right: -4px; top: 50%; transform: translateY(-50%); width: 8px; height: 8px; background: #888; border: 2px solid white; border-radius: 50%;"
  />

  <div class="leaf-content">
    <div class="leaf-header">
      <div class="leaf-label">{data?.label || 'Metadata Field'}</div>
      
      {#if showControls}
        <div class="leaf-controls">
          <button 
            class="control-btn anchorpoint-btn"
            class:is-anchor={isAnchorpoint}
            on:click={handleSetAnchorpoint}
            title={isAnchorpoint ? 'Current anchorpoint' : 'Set as anchorpoint'}
          >
            ⚓
          </button>
          
          <button 
            class="control-btn visibility-btn" 
            class:visible={isVisible}
            on:click={handleToggleVisibility}
            title={isVisible ? 'Visible in form' : 'Hidden in form'}
          >
            {#if isVisible}
              👁️
            {:else}
              <span class="eye-crossed">👁️</span>
            {/if}
          </button>
        </div>
      {/if}
    </div>
    
    {#if data?.type}
      <div class="leaf-type">{data.type}</div>
    {/if}
    {#if data?.path}
      <div class="leaf-path">{data.path}</div>
    {/if}
  </div>
</div>

<style>
  .leaf-node-content {
    border: 2px solid #888;
    border-radius: 6px;
    background: #f8f8f8;
    padding: 8px 12px;
    min-width: 160px;
    position: relative;
    transition: all 0.2s;
  }
  
  .leaf-node-content.selected {
    border-color: #ff6b35;
    box-shadow: 0 0 8px rgba(255, 107, 53, 0.3);
  }
  
  .leaf-content {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }
  
  .leaf-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
  }
  
  .leaf-label {
    font-weight: bold;
    color: #333;
    font-size: 0.85rem;
    line-height: 1.2;
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .leaf-controls {
    display: flex;
    gap: 4px;
    align-items: center;
    flex-shrink: 0;
  }
  
  .control-btn {
    width: 22px;
    height: 22px;
    border: 1px solid #ccc;
    border-radius: 3px;
    background: white;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 13px;
    transition: all 0.2s;
    padding: 0;
  }
  
  .control-btn:hover {
    border-color: #007acc;
    transform: scale(1.15);
  }
  
  .anchorpoint-btn {
    opacity: 0.35;
  }
  .anchorpoint-btn.is-anchor {
    background: #ffd700;
    border-color: #ffa500;
    opacity: 1;
    box-shadow: 0 0 6px rgba(255, 215, 0, 0.6);
  }
  
  .anchorpoint-btn:hover {
    opacity: 1;
    background: #fff4cc;
  }
  
  .visibility-btn.visible {
    background: #e8f5e9;
    border-color: #4caf50;
  }

  .visibility-btn:not(.visible) {
    background: #ffebee;
    border-color: #f44336;
  }
  .eye-crossed {
    position: relative;
    display: inline-block;
  }
  .eye-crossed::after {
    content: '';
    position: absolute;
    top: 50%;
    left: 0;
    right: 0;
    height: 2px;
    background: red;
    transform: translateY(-50%) rotate(-45deg);
  }
  
  .leaf-type {
    font-size: 0.7rem;
    color: #666;
    background: #f0f0f0;
    padding: 2px 4px;
    border-radius: 2px;
  }
  
  .leaf-path {
    font-size: 0.65rem;
    color: #888;
    font-style: italic;
    word-break: break-all;
  }

  .leaf-node-content:not(.selected) .leaf-controls {
    opacity: 0.85;
  }
</style>