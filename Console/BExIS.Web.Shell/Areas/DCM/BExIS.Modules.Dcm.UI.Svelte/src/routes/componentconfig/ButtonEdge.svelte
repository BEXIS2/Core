
  <script lang="ts">
    import {
      BaseEdge,
      getBezierPath,
      Position,
      useEdges,
      useNodes,
      MarkerType
    } from '@xyflow/svelte';
  import { onMount } from 'svelte';

  export let id: string = '';
  export let source: string = '';
  export let target: string = '';
  export let sourcePosition: Position = Position.Bottom;
  export let targetPosition: Position = Position.Top;
  export let markerEnd: any = undefined;
  export let markerStart: any = undefined;
  export let sourceHandleId: string | undefined = undefined;
  export let targetHandleId: string | undefined = undefined;
  export let selected: boolean = false;
  export let data: any = undefined;
  export let type: string = '';
  export let animated: boolean = false;
  export let style: string = '';

  // edge positions
  let sourceX: number = $$props.sourceX || 0;
  let sourceY: number = $$props.sourceY || 0;
  let targetX: number = $$props.targetX || 0;
  let targetY: number = $$props.targetY || 0;
  
  // reactive position updates
  $: sourceX = $$props.sourceX || 0;
  $: sourceY = $$props.sourceY || 0;
  $: targetX = $$props.targetX || 0;
  $: targetY = $$props.targetY || 0;
  
  export let label: string | undefined = undefined;
  export let labelStyle: string | undefined = undefined; 
  export let interactionWidth: number | undefined = undefined;
  export let selectable: boolean | undefined = undefined;
  export let deletable: boolean | undefined = undefined;

  const edges = useEdges();
  const nodes = useNodes();

  let leftDirection = data?.leftDirection ?? false;
  let rightDirection = data?.rightDirection ?? true;
  let showPopup = false;
  let imageLoaded = true;

  // dynamically extract handle-ids from edge.data
  $: sourceHandleFromData = data?.sourceHandleId || sourceHandleId;
  $: targetHandleFromData = data?.targetHandleId || targetHandleId;

  // calculate edge path
  $: [edgePath, labelX, labelY] = getBezierPath({
    sourceX,
    sourceY,
    sourcePosition,
    targetX,
    targetY,
    targetPosition,
  });

  // helper function to determine component node and variable for this edge
  function findComponentNodeAndVariable() {
    let componentNode: any = null;
    let handleId = '';
    const sourceNode = $nodes.find(n => n.id === source);
    if (sourceNode?.type === 'nodeWithItems' && sourceHandleFromData) {
      componentNode = sourceNode;
      handleId = sourceHandleFromData;
    }
    const targetNode = $nodes.find(n => n.id === target);
    if (targetNode?.type === 'nodeWithItems' && targetHandleFromData) {
      componentNode = targetNode;
      handleId = targetHandleFromData;
    }
    if (!componentNode || !handleId) return { componentNode: null, variable: null, handleId: '' };

    const variables = componentNode.data?.componentVariables || [];
    const variable = variables.find((v: any) => `${componentNode.id}-${v.target_variable}-handle` === handleId) || null;
    return { componentNode, variable, handleId };
  }

  // initial directions validation
  function ensureInitialDirections() {
    const { variable, handleId } = findComponentNodeAndVariable();
    if (!variable || !handleId) return;

    const hasDataDirections =
      data && (typeof data.leftDirection === 'boolean' || typeof data.rightDirection === 'boolean');

    if (hasDataDirections) return; // already set

    let initLeft = false;
    let initRight = true;

    // determine initial directions based on variable properties
    if (variable.is_input && variable.is_output) {
      // if handle already has an IN -> force only OUT
      const currentEdges = $edges || [];
      const hasInputAlready = currentEdges.some(edge =>
        edge.id !== id &&
        (edge.sourceHandle === handleId || edge.targetHandle === handleId || edge.data?.sourceHandleId === handleId || edge.data?.targetHandleId === handleId) &&
        (edge.data?.rightDirection === true || (edge.data?.leftDirection === true && edge.data?.rightDirection === true))
      );
      initLeft = true;
      initRight = hasInputAlready ? false : true;
    } else if (variable.is_input && !variable.is_output) {
      initLeft = false;
      initRight = true;
    } else if (!variable.is_input && variable.is_output) {
      initLeft = true;
      initRight = false;
    }

    // update arrow directions in edges store & lock unidirectional arrow-menu buttons
    if (edges) {
      edges.update(eds => eds.map(e => { // update edge directions
        if (e.id !== id) return e;
        return {
          ...e,
          data: {
            ...e.data,
            leftDirection: initLeft,
            rightDirection: initRight,
            sourceHandleId: sourceHandleFromData,
            targetHandleId: targetHandleFromData
          },
          // block arrows if only one direction possible
          markerEnd: initLeft ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined,
          markerStart: initRight ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined
        };
      }));
    }

    leftDirection = initLeft;
    rightDirection = initRight;
  }

  onMount(() => {
    ensureInitialDirections();
  });

  $: if ($nodes) {
    ensureInitialDirections();
  }

  $: if ($edges) {
    isDirectionChangeable = checkDirectionChangeable();
  }

  // edge menu button logic 
  $: isDirectionChangeable = checkDirectionChangeable();

  function checkDirectionChangeable() {
    const { componentNode, variable, handleId } = findComponentNodeAndVariable();
    if (!componentNode || !handleId) {
      return { canChange: true, leftDisabled: false, rightDisabled: false };
    }
    if (!variable) {
      return { canChange: true, leftDisabled: false, rightDisabled: false };
    }

    // only input = buttons disabled, right shown as active
    if (variable.is_input && !variable.is_output) {
      return { 
        canChange: false, 
        leftDisabled: true, 
        rightDisabled: true,
        forceRightActive: true
      };
    }

    // only output = buttons disabled, left shown as active
    if (!variable.is_input && variable.is_output) {
      return { 
        canChange: false, 
        leftDisabled: true, 
        rightDisabled: true,
        forceLeftActive: true
      };
    }
 
    // both input & output = allow more edges if input exists, but only OUT arrows
    if (variable.is_input && variable.is_output) {
      const currentEdges = $edges || [];
      const hasInputAlready = currentEdges.some(edge => 
        edge.id !== id &&
        (edge.sourceHandle === handleId || edge.targetHandle === handleId || edge.data?.sourceHandleId === handleId || edge.data?.targetHandleId === handleId) &&
        (edge.data?.rightDirection === true || (edge.data?.leftDirection === true && edge.data?.rightDirection === true))
      );
      if (hasInputAlready) {
        return { 
          canChange: true, 
          leftDisabled: false, 
          rightDisabled: true
        };
      }
      return { canChange: true, leftDisabled: false, rightDisabled: false };
    }

    return { canChange: true, leftDisabled: false, rightDisabled: false };
  }

  // reactive update arrow directions based on data changes
  $: {
    if (isDirectionChangeable?.rightDisabled) {
      leftDirection = true;
      rightDirection = false;
    } else if (isDirectionChangeable?.forceRightActive) {
      leftDirection = false;
      rightDirection = true;
    } else {
      leftDirection = data?.leftDirection ?? leftDirection ?? false;
      rightDirection = data?.rightDirection ?? rightDirection ?? true;
    }
  }

  function deleteEdge(event: MouseEvent) {
    event.stopPropagation();
    if (edges) {
      edges.update(eds => eds.filter(edge => edge.id !== id));
    }
    showPopup = false;
  }

  function togglePopup(event: MouseEvent) {
    event.stopPropagation();
    showPopup = !showPopup;
  }

  function toggleLeftDirection(event: MouseEvent) {
    event.stopPropagation();
    if (!isDirectionChangeable.canChange || isDirectionChangeable.leftDisabled) return;
    if (leftDirection && !rightDirection) return; // never both false

    const newLeftDirection = !leftDirection;
    if (edges) {
      edges.update(eds => eds.map(edge => 
        edge.id === id ? { 
          ...edge, 
          data: { ...edge.data, leftDirection: newLeftDirection },
          markerEnd: newLeftDirection ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined
        } : edge
      ));
    }
  }

  function toggleRightDirection(event: MouseEvent) {
    event.stopPropagation();
    if (!isDirectionChangeable.canChange || isDirectionChangeable.rightDisabled) return;
    if (rightDirection && !leftDirection) return; // never both false

    const newRightDirection = !rightDirection;
    if (edges) {
      edges.update(eds => eds.map(edge => 
        edge.id === id ? { 
          ...edge, 
          data: { ...edge.data, rightDirection: newRightDirection },
          markerStart: newRightDirection ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined
        } : edge
      ));
    }
  }

  function handleClickOutside(event: MouseEvent) {
    const target = event.target as Element;
    if (showPopup && !target.closest('.edge-popup') && !target.closest('.edge-toggle-button')) {
      showPopup = false;
    }
  }

  function handleImageError() {
    imageLoaded = false;
  }

  function handlePopupClick(event: MouseEvent) {
    event.stopPropagation();
  }

  function handlePopupKeydown(event: KeyboardEvent) {
    event.stopPropagation();
  }
</script>

<svelte:window on:click={handleClickOutside} />

<BaseEdge
  {id}
  path={edgePath}
  {markerEnd}
  {markerStart}
  {style}
/>

{#if selected}
  <foreignObject x={labelX - 15} y={labelY - 15} width="30" height="30" style="overflow: visible; z-index: 999999; pointer-events: all;">
    <div class="edge-label-container">
      <button class="edge-toggle-button" on:click={togglePopup}>
        {#if showPopup}×{:else}⚙️{/if}
      </button>
      
      {#if showPopup}
        <div class="edge-popup" role="menu" tabindex="-1" on:click={handlePopupClick} on:keydown={handlePopupKeydown}>
          <div class="popup-row">
            <button class="delete-button" on:click={deleteEdge} title="delete edge">
                <span class="trash-fallback">🗑️</span>
            </button>
            
            <div class="direction-controls">
              <button 
                class="direction-button" 
                class:active={leftDirection}
                class:disabled={!isDirectionChangeable.canChange || isDirectionChangeable.leftDisabled}
                disabled={!isDirectionChangeable.canChange || isDirectionChangeable.leftDisabled}
                on:click={toggleLeftDirection}
                title="component to external (output) {!isDirectionChangeable.canChange || isDirectionChangeable.leftDisabled ? '- disabled' : ''}"
              >
                ←
              </button>
              <button 
                class="direction-button" 
                class:active={rightDirection}
                class:disabled={!isDirectionChangeable.canChange || isDirectionChangeable.rightDisabled}
                disabled={!isDirectionChangeable.canChange || isDirectionChangeable.rightDisabled}
                on:click={toggleRightDirection}
                title="external to component (input) {!isDirectionChangeable.canChange || isDirectionChangeable.rightDisabled ? '- disabled' : ''}"
              >
                →
              </button>
            </div>
          </div>
        </div>
      {/if}
    </div>
  </foreignObject>
{/if}

<style>
  :global(.edge-label-container) {
    position: relative;
    z-index: 999999 !important;
    pointer-events: all !important;
  }

  :global(.edge-toggle-button) {
    width: 30px !important;
    height: 30px !important;
    background: #ffffff !important;
    border: 2px solid #007acc !important;
    border-radius: 50% !important;
    color: #007acc !important;
    font-size: 16px !important;
    cursor: pointer !important;
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
    padding: 0 !important;
    transition: all 0.2s !important;
    box-shadow: 0 2px 8px rgba(0,0,0,0.3) !important;
    z-index: 999999 !important;
    pointer-events: all !important;
    position: relative !important;
  }

  :global(.edge-toggle-button:hover) {
    background: #007acc !important;
    color: white !important;
    transform: scale(1.1) !important;
  }

  :global(.edge-popup) {
    position: absolute !important;
    top: 40px !important;
    left: 50% !important;
    transform: translateX(-50%) !important;
    background: white !important;
    border: 2px solid #007acc !important;
    border-radius: 8px !important;
    padding: 12px !important;
    box-shadow: 0 8px 32px rgba(0,0,0,0.5) !important;
    min-width: 160px !important;
    z-index: 999999 !important;
    pointer-events: all !important;
  }

  :global(.popup-row) {
    display: flex !important;
    align-items: center !important;
    gap: 12px !important;
  }

  :global(.delete-button) {
    width: 36px !important;
    height: 36px !important;
    background: #ff4444 !important;
    border: none !important;
    border-radius: 6px !important;
    cursor: pointer !important;
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
    transition: all 0.2s !important;
    position: relative !important;
  }

  :global(.delete-button:hover) {
    background: #cc0000 !important;
    transform: scale(1.05) !important;
  }

  :global(.trash-icon) {
    width: 18px !important;
    height: 18px !important;
    filter: brightness(0) invert(1) !important;
  }

  :global(.trash-fallback) {
    font-size: 16px !important;
    color: white !important;
  }

  :global(.direction-controls) {
    display: flex !important;
    gap: 6px !important;
  }

  :global(.direction-button) {
    width: 32px !important;
    height: 32px !important;
    background: #f0f0f0 !important;
    border: 1px solid #ccc !important;
    border-radius: 4px !important;
    cursor: pointer !important;
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
    font-size: 16px !important;
    font-weight: bold !important;
    transition: all 0.2s !important;
  }

  :global(.direction-button:hover:not([disabled])) {
    background: #e0e0e0 !important;
    border-color: #007acc !important;
  }

  :global(.direction-button.active:not([disabled])) {
    background: #007acc !important;
    color: white !important;
    border-color: #005fa3 !important;
  }

  /* disabled styles */
  :global(.direction-button:disabled),
  :global(.direction-button[disabled]) {
    opacity: 0.7 !important;
    cursor: not-allowed !important;
    pointer-events: none !important;
  }

  :global(.direction-button:disabled.active),
  :global(.direction-button[disabled].active) {
    background: #007acc !important;
    color: white !important;
    border-color: #005fa3 !important;
    opacity: 0.7 !important;
    cursor: not-allowed !important;
    pointer-events: none !important;
  }
</style>