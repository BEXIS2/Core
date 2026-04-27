<script lang="ts">
  import { Handle, Position } from '@xyflow/svelte';
  import { onMount } from 'svelte';
  import { get } from 'svelte/store';

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

  // check if component should be disabled
  $: isGrayedOut = data?.isGrayedOut || false;
  // $: isEditMode = data?.isEditMode || false;

  // gets edges from parent context -> passed via data
  $: edges = data?.edges || [];
  
  // convert edges store to array if needed
  $: edgesArray = edges && typeof edges.subscribe === 'function' ? get(edges) : (Array.isArray(edges) ? edges : []);

  // update childItems on data changes
  $: childItems = data?.childItems || [];

  // helper function checks if component child item handle is properly connected based on arrow directions (NOT USED)
  function isHandleConnected(itemId: string, variable: any): boolean {
    if (!edgesArray || !Array.isArray(edgesArray) || edgesArray.length === 0) return false;
    
    const handleId = `${id}-${itemId}-handle`;
    
    // find all edges connected to handles
    const connectedEdges = edgesArray.filter(edge => {
      const isSourceMatch = edge.sourceHandle === handleId || edge.source === handleId;
      const isTargetMatch = edge.targetHandle === handleId || edge.target === handleId;
      return isSourceMatch || isTargetMatch;
    });
    
    // check connection requirements based on variable type
    if (variable.is_input && variable.is_output) {
      // input/output variable: needs either bidirectional arrow OR separate input+output arrows
      let hasValidConnection = false;
      
      // check for bidirectional arrows
      const bidirectionalEdges = connectedEdges.filter(edge => 
        edge.data?.leftDirection && edge.data?.rightDirection
      );
      
      if (bidirectionalEdges.length > 0) {
        hasValidConnection = true;
      } else {
        // check for separate input and output connections
        const inputEdges = connectedEdges.filter(edge => {
          const isIncoming = edge.targetHandle === handleId && edge.source.startsWith('param-');
          const hasRightArrow = edge.data?.rightDirection && !edge.data?.leftDirection;
          return isIncoming && hasRightArrow;
        });
        
        const outputEdges = connectedEdges.filter(edge => {
          const isOutgoing = edge.sourceHandle === handleId && edge.target.startsWith('param-');
          const hasLeftArrow = edge.data?.leftDirection && !edge.data?.rightDirection;
          return isOutgoing && hasLeftArrow;
        });
        
        if (inputEdges.length > 0 && outputEdges.length > 0) {
          hasValidConnection = true;
        }
      }

      return hasValidConnection;
      
    } else if (variable.is_input) {
      // input only: needs incoming arrow from external parameter
      const hasInputConnection = connectedEdges.some(edge => {
        const isIncoming = edge.targetHandle === handleId && edge.source.startsWith('param-');
        const hasRightArrow = edge.data?.rightDirection;
        return isIncoming && hasRightArrow;
      });
      return hasInputConnection;
      
    } else if (variable.is_output) {
      // output only: needs outgoing arrow to external parameter
      const hasOutputConnection = connectedEdges.some(edge => {
        const isOutgoing = edge.sourceHandle === handleId && edge.target.startsWith('param-');
        const hasLeftArrow = edge.data?.leftDirection;
        return isOutgoing && hasLeftArrow;
      });
      return hasOutputConnection;
    }
    
    return false;
  }

  // updates incoming connection status for IN/OUT badges
  function hasInputConnection(itemId: string, variable: any): boolean {
    if (!edgesArray || !Array.isArray(edgesArray) || edgesArray.length === 0) return false;
    
    const handleId = `${id}-${itemId}-handle`;
    const connectedEdges = edgesArray.filter(edge => {
      const isSourceMatch = edge.sourceHandle === handleId;
      const isTargetMatch = edge.targetHandle === handleId;
      return isSourceMatch || isTargetMatch;
    });
    
    return connectedEdges.some(edge => {
      const isTargetWithRightArrow = edge.targetHandle === handleId && (edge.source.startsWith('param-') || edge.source.startsWith('schema-')) && edge.data?.rightDirection === true;
      const isSourceWithRightArrow = edge.sourceHandle === handleId && (edge.target.startsWith('param-') || edge.target.startsWith('schema-')) && edge.data?.rightDirection === true;
      
      return isTargetWithRightArrow || isSourceWithRightArrow;
    });
  }

  // updates outgoing connection status for IN/OUT badges
  function hasOutputConnection(itemId: string, variable: any): boolean {
    if (!edgesArray || !Array.isArray(edgesArray) || edgesArray.length === 0) return false;
    
    const handleId = `${id}-${itemId}-handle`;
    const connectedEdges = edgesArray.filter(edge => {
      const isSourceMatch = edge.sourceHandle === handleId;
      const isTargetMatch = edge.targetHandle === handleId;
      return isSourceMatch || isTargetMatch;
    });
    
    return connectedEdges.some(edge => {
      const isSourceWithLeftArrow = edge.sourceHandle === handleId && (edge.target.startsWith('param-') || edge.target.startsWith('schema-')) && edge.data?.leftDirection === true;
      const isTargetWithLeftArrow = edge.targetHandle === handleId && (edge.source.startsWith('param-') || edge.source.startsWith('schema-')) && edge.data?.leftDirection === true;
      
      return isSourceWithLeftArrow || isTargetWithLeftArrow;
    });
  }

  // check if handle can accept new input connections
  function isHandleTargetable(itemId: string, _item: any): boolean {
    const handleId = `${id}-${itemId}-handle`;
    const variable = (data?.componentVariables || []).find((v: any) => `${id}-${v.target_variable}-handle` === handleId) || _item;
    
    if (!variable.is_input) return true;
    if (!variable) return true;
    if (!edgesArray || !Array.isArray(edgesArray) || edgesArray.length === 0) return true;

    const currentInteractionMode = data?.interactionMode;
    const isInputOnly = variable.is_input && !variable.is_output;

    // check if input connection already exists in current interaction mode for this handle
    const hasInputAlready = edgesArray.some((edge: any) => {
      const isOutgoingFromThisHandle = edge.sourceHandle === handleId;
      const isToSchema = edge.target?.startsWith('schema-') || edge.target?.startsWith('param-');
      const hasInputFlow = edge.data?.rightDirection === true || (edge.data?.leftDirection === true && edge.data?.rightDirection === true);
      const edgeSourceMode = edge.data?.sourceMode;
      const isSameMode = edgeSourceMode === currentInteractionMode;

      return isOutgoingFromThisHandle && isToSchema && hasInputFlow && isSameMode;
    });

    return isInputOnly ? !hasInputAlready : true;
  }
</script>

<div 
  class="custom-node-content" 
  class:selected 
  class:grayed-out={isGrayedOut}
  style={isGrayedOut ? 'pointer-events: none;' : ''}
>
  <div class="node-header">
    <div class="node-title">
      {data?.label ?? 'Unnamed Node'}
    </div>

    {#if data?.modeName}
      <div class="mode-info">
        <small>Mode: {data.modeName}</small>
      </div>
    {/if}
  </div>

  <div class="node-items-container">
    {#if childItems && childItems.length > 0}
      <div class="node-items-grid">
        {#each childItems as item, i (item.id)}
          {@const variable = data?.componentVariables?.find(v => v.target_variable === item.id)}
          <div class="node-item">
            <!-- single handle on the left for all component child items -->
            <Handle 
              type="source" 
              position={Position.Left} 
              id={`${id}-${item.id}-handle`}
              style="left: 0px; top: 50%; transform: translate(-50%, -50%);"
              isConnectable={isHandleTargetable(item.id, item)}
            />
            
            <div class="item-content">
              <!-- only show IN badge if item can be input -->
              {#if item.isInput && item.isOutput}
                <span class="io in" class:connected={hasInputConnection(item.id, variable || item)}>IN</span>
              {:else if item.isInput}
                <span class="io in" class:connected={hasInputConnection(item.id, variable || item)}>IN</span>
              {/if}
              
              <span class="item-label">{item.label}</span>
              
              <!-- only show OUT badge if item can be output -->
              {#if item.isInput && item.isOutput}
                <span class="io out" class:connected={hasOutputConnection(item.id, variable || item)}>OUT</span>
              {:else if item.isOutput}
                <span class="io out" class:connected={hasOutputConnection(item.id, variable || item)}>OUT</span>
              {/if}
            </div>
            <!-- un/comment to toggle type hiding in childnodes -->
            <!-- 
            {#if item.type}
              <div class="item-type">{item.type}</div>
            {/if}
            -->
          </div>
        {/each}
      </div>
    {:else}
      <div class="no-items">
        <p>No child items available</p>
        <small>Mode: {data?.modeName || 'Unknown'}</small>
        <small>Data available: {JSON.stringify(Object.keys(data || {}))}</small>
        <small>Expected childItems, got: {typeof childItems} ({childItems?.length || 0} items)</small>
      </div>
    {/if}
  </div>
</div>

<style>
  .custom-node-content {
    border: 2px solid #007acc;
    border-radius: 8px;
    background: #eaf6ff;
    padding: 16px;
    min-width: 320px;
    transition: border-color 0.2s;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    position: relative;
  }
  .custom-node-content.selected {
    border-color: #ff6b35;
    box-shadow: 0 0 12px rgba(255, 107, 53, 0.4);
  }
    .custom-node-content.grayed-out {
    opacity: 0.4;
    filter: grayscale(0.8);
    cursor: not-allowed;
  }
  .node-header {
    text-align: center;
    margin-bottom: 16px;
    border-bottom: 1px solid #ddd;
    padding-bottom: 10px;
  }
  .node-title {
    font-weight: bold;
    color: #333;
    font-size: 1rem;
    margin-bottom: 4px;
  }
  .mode-info {
    margin-bottom: 2px;
    color: #666;
    font-size: 0.75rem;
  }
  .node-items-container {
    min-height: 80px;
  }
  .node-items-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 12px;
  }
  .node-item {
    background: white;
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 12px 16px;
    position: relative;
    cursor: pointer;
    transition: all 0.2s;
    min-height: 55px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    width: 100%;
  }
  .node-item:hover {
    background: #f0f8ff;
    border-color: #007acc;
    transform: translateY(-1px);
    box-shadow: 0 2px 6px rgba(0, 122, 204, 0.2);
  }
  .item-content {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 12px;
    width: 100%;
  }
  .io {
    font-size: 0.65rem;
    font-weight: bold;
    padding: 3px 5px;
    border-radius: 3px;
    background: #eee;
    color: #888;
    min-width: 22px;
    text-align: center;
    opacity: 0.4;
    transition: all 0.2s;
    flex-shrink: 0;
  }
  /* color settings for IN / OUT boxes = activated when connected to external parameters */
  .io.in {
    background: #e3f2fd;
    color: #1976d2;
    opacity: 1;
  }
  .io.out {
    background: #f3e5f5;
    color: #7b1fa2;
  }
  /* highlight when properly connected to external parameters */
  .io.in.connected {
    background: #1976d2;
    color: white;
    opacity: 1;
  }
  .io.out.connected {
    background: #7b1fa2;
    color: white;
    opacity: 1;
  }
  .item-label {
    font-weight: bold;
    color: #333;
    flex: 1;
    text-align: center;
    font-size: 0.85rem;
  }
  .no-items {
    text-align: center;
    color: #999;
    font-style: italic;
    padding: 20px;
    border: 2px dashed #ddd;
    border-radius: 6px;
    background: #fafafa;
  }
  .no-items p {
    margin: 0 0 5px 0;
  }
  .no-items small {
    display: block;
    color: #aaa;
    margin-bottom: 3px;
  }
</style>