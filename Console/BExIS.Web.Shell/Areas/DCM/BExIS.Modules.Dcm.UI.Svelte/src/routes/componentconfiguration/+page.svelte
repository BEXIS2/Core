<script lang="ts">
  import {
    SvelteFlow,
    SvelteFlowProvider,
    Background,
    Controls,
    Position,
    SelectionMode,
    ConnectionLineType,
    MarkerType,
    type Node,
    type Edge,
    type EdgeTypes,
    type NodeTypes
  } from '@xyflow/svelte';
  import { writable, type Writable, get } from 'svelte/store';
  import '@xyflow/svelte/dist/style.css';
  import { onMount } from 'svelte';

  // import custom components
  import TreeComponent from './TreeComponent.svelte';
  import NodeWithItems from './NodeWithItems.svelte';
  import ItemNode from './ItemNode.svelte';
  import ButtonEdge from './ButtonEdge.svelte';
  import ComponentOverview from './ComponentOverview.svelte';
  import ConfigTab from './ConfigTab.svelte';
  import PreviewTab from './PreviewTab.svelte';
  import ModesTab from './ModesTab.svelte';
  import ComponentLibrary from './ComponentLibrary.svelte';
  import SectionNode from './SectionNode.svelte';
  import LeafNode from './LeafNode.svelte';
  import ResetViewButton from './ResetViewButton.svelte';

  // import file helpers for config management
  import { 
    downloadAllConfigs, 
    createEmptyConfig, 
    createEmptyPositions,
    // uploadJsonFile,
    loadConfigsFromDownloads,
    type ConfigFile,
    type PositionFile
  } from './Services/fileHelpers';
  
  import componentManifestJson from './componentManifest.json';

  // separate configs for edit/view modes
  let componentConfig_edit: ConfigFile = createEmptyConfig();
  let componentConfig_view: ConfigFile = createEmptyConfig();
  let componentPositions: PositionFile = createEmptyPositions();
  
  const componentManifest = componentManifestJson;

  // schema nodes from treecomponent
  let schemaNodes: any[] = [];

  let editModeNodes: Node[] = [];
  let viewModeNodes: Node[] = [];

  // initial interaction mode
  let currentInteractionMode = 'edit';
  let selectedMode: any = null;

  // store for node specific modes
  let nodeSpecificModes = new Map<string, any>();

  // popup state for mode switch warning
  let showModeChangeWarning = false;
  let pendingInteractionMode = '';

  let showCancelWarning = false;
  
  // delete confirmation dialog state
  let showDeleteWarning = false;
  let nodeToDelete: Node | null = null;

  // init counter for forcing node updates
  let nodeVersion = 0;

  // track last node state
  let lastNodesSnapshot = '';

  // load configs from file and apply component position & edges reconstruct on mount
  onMount(async () => {
    try {
      const loaded = await loadConfigsFromDownloads();
      
      if (loaded) {
        componentConfig_edit = JSON.parse(JSON.stringify(loaded.edit));
        componentConfig_view = JSON.parse(JSON.stringify(loaded.view));
        componentPositions = loaded.positions;
        
        editModeNodes = [];
        viewModeNodes = [];
        
        // apply saved positions to components once loaded
        forceNodeUpdate();
        setTimeout(() => {
          applyPositionsToNodes();
          reconstructEdgesForMode('edit'); // load only edit edges (view loads on mode switch)
        }, 500);
      }
    } catch (error) {
      // silent fail
    }
    

    // set initial sub-mode based on first manifest component
    const firstComponent = getCurrentConfig()?.components?.[0];
    if (firstComponent?.mode?.mode_name) {
      const manifestModes = componentManifest?.modes?.edit || [];
      const foundMode = manifestModes.find((mode: any) => mode.mode_name === firstComponent.mode.mode_name);
      if (foundMode) {
        selectedMode = foundMode;
        forceNodeUpdate();
      }
    }
  });

  // get current config based on interaction mode
  function getCurrentConfig(): ConfigFile {
    return currentInteractionMode === 'edit' ? componentConfig_edit : componentConfig_view;
  }

  // if a component config changes update the stored config
  function handleConfigChange(updatedConfig: any) {

    if (currentInteractionMode === 'edit') {
      updatedConfig.components.forEach((updatedComp: any) => {
        
        const existingIndex = componentConfig_edit.components.findIndex(
          (c: any) => c.meta.component_ui_id === updatedComp.meta.component_ui_id
        );
        
        if (existingIndex >= 0) {
          // update existing component
          componentConfig_edit.components[existingIndex] = updatedComp;
        } else {
          // add new component
          componentConfig_edit.components.push(updatedComp);
        }
      });

      componentConfig_edit = { ...componentConfig_edit };

    } else {
      // same for view mode
      updatedConfig.components.forEach((updatedComp: any) => {
        const existingIndex = componentConfig_view.components.findIndex(
          (c: any) => c.meta.component_ui_id === updatedComp.meta.component_ui_id
        );
        
        if (existingIndex >= 0) {
          componentConfig_view.components[existingIndex] = updatedComp;
        } else {
          componentConfig_view.components.push(updatedComp);
        }
      });
      
      componentConfig_view = { ...componentConfig_view };
    }
  }
  
  // apply saved component positions to nodes
  function updatePositionsFromNodes() {
    const currentNodes = get(nodes);
    
    currentNodes.forEach(node => {
      if (node.type === 'nodeWithItems') {
        componentPositions[node.id] = {
          x: Math.round(node.position.x),
          y: Math.round(node.position.y)
        };
      }
    });
  }
  
  // builds the component config object data from a node + edges, which will get saved to config files
  function buildComponentData(node: Node): any {
    if (!node || !node.data) return null;

    // structure as in config files
    const componentData: any = {
      meta: {
        component_name: node.data.componentName || componentManifest.meta.component_name,
        component_ui_id: node.id
      },
      globalSettings: {
        interaction_mode: node.data.interactionMode || currentInteractionMode,
        anchorpoint: String(node.data.anchorpoint || '').replace(/^\$\.?/, ''),
        globalsetting: []
      },
      mode: {
        mode_name: node.data.modeName || '',
        settings: {
          setting: []
        },
        variables: {
          variable: []
        }
      }
    };

    const modeKey = node.data.interactionMode || currentInteractionMode;
    const manifestModes = componentManifest?.modes?.[modeKey as string] || [];
    const manifestMode = manifestModes.find((m: any) => m.mode_name === node.data.modeName);
    const currentConfigRef = modeKey === 'edit' ? componentConfig_edit : componentConfig_view;
    
    const existingComponent = currentConfigRef.components?.find((c: any) => 
      c.meta.component_ui_id === node.id
    );
    
    // global settings
    const manifestGlobalSettings = componentManifest?.globalSettings?.globalsetting || [];
    const configGlobalSettings = existingComponent?.globalSettings?.globalsetting || [];
    
    // cycle through manifest global settings and fill from config or default
    componentData.globalSettings.globalsetting = manifestGlobalSettings.map((mgs: any) => {
      const cgs = configGlobalSettings.find((s: any) => s.target_variable === mgs.target_variable);
      return {
        target_variable: mgs.target_variable,
        value: String(cgs?.value ?? mgs.default_value?.value ?? '')
      };
    });
    
    // same for submode settings
    if (manifestMode?.settings?.setting) {
      const configSettings = existingComponent?.mode?.settings?.setting || [];
      componentData.mode.settings.setting = manifestMode.settings.setting.map((ms: any) => {
        const cs = configSettings.find((s: any) => s.target_variable === ms.target_variable);
        return {
          target_variable: ms.target_variable,
          value: String(cs?.value ?? ms.default_value?.value ?? '')
        };
      });
    }

    // get existing variables from config
    const existingVariables = existingComponent?.mode?.variables?.variable || [];
    const componentVariables = Array.isArray(node.data.componentVariables) ? node.data.componentVariables : [];
    
    const allEdges = get(edges);
    const processedVariables = new Set<string>(); // track processed variables

    // process each component variable and find connected edges
    componentVariables.forEach((variable: any) => {
      const variableId = variable.target_variable;
      const sourceHandle = `${node.id}-${variableId}-handle`;

      const connectedEdges = allEdges.filter(
        (edge: Edge) => edge.sourceHandle === sourceHandle || edge.targetHandle === sourceHandle
      );

      // case 1: has edges > create from edge data
      if (connectedEdges.length > 0) {
        connectedEdges.forEach((connectedEdge: Edge) => {
          const targetNode = get(nodes).find((n: Node) => n.id === connectedEdge.target);
          
          if (targetNode && targetNode.data) {
            let jsonPath = '';
            
            if (targetNode.data.jsonPath) {
              jsonPath = String(targetNode.data.jsonPath);
            } else if (targetNode.data.path) {
              jsonPath = String(targetNode.data.path);
            } else if (targetNode.data.label) {
              jsonPath = String(targetNode.data.label);
            }
            
            // prioritize interaction-mode specific visibility to prevent overwriting inactive mode settings with active mode state
            // uses key to store visibility per mode
            const modeVisibilityKey = `is_visible_${modeKey}`;
            const isVisible = targetNode.data[modeVisibilityKey] !== undefined ? targetNode.data[modeVisibilityKey] : (targetNode.data.is_visible !== false);
            const variableData: any = {
              target_variable: variable.target_variable,
              is_input: connectedEdge.data?.rightDirection || false,
              is_output: connectedEdge.data?.leftDirection || false,
              type: variable.type,
              JSONPath: jsonPath,
              is_visible: isVisible
            };

            // transfer regex from existingVariables
            const existingVar = existingVariables.find((v: any) => 
              v.target_variable === variable.target_variable
            );
            
            // write regex if exist
            if (existingVar) {
              if (existingVar.input_regex) {
                variableData.input_regex = existingVar.input_regex;
              }
              if (existingVar.output_regex) {
                variableData.output_regex = existingVar.output_regex;
              }
            }

            componentData.mode.variables.variable.push(variableData);
            processedVariables.add(variable.target_variable);
          }
        });
      }
    });

    // case 2: variables without edges but with regex in config
    existingVariables.forEach((existingVar: any) => {
      if (!processedVariables.has(existingVar.target_variable)) {
        // variable has no edges but might have regex
        if (existingVar.input_regex || existingVar.output_regex) {
          componentData.mode.variables.variable.push({
            ...existingVar
          });
        }
      }
    });    
    return componentData;
  }
  
  // extract mappings for a given node (NOT USED)
  function extractMappingsForNode(node: any): any[] {
    const currentEdges = get(edges);
    const mappings: any[] = [];
    
    node.data.componentVariables?.forEach((variable: any) => {
      const handleId = `${node.id}-${variable.target_variable}-handle`;
      
      const connectedEdges = currentEdges.filter(edge => 
        edge.sourceHandle === handleId || edge.targetHandle === handleId
      );
      
      if (connectedEdges.length > 0) {

        connectedEdges.forEach(edge => {
          const isSource = edge.sourceHandle === handleId;
          const schemaNodeId = isSource ? edge.target : edge.source;
          const jsonPath = buildJsonPathFromSchemaNode(schemaNodeId);
          
          const mapping: any = {
            target_variable: variable.target_variable,
            type: variable.type || 'string',
            is_visible: true,
            JSONPath: jsonPath,
            is_input: edge.data?.rightDirection || false,
            is_output: edge.data?.leftDirection || false
          };
          
          // add regex if exist
          if (edge.data?.inputRegex) mapping.input_regex = edge.data.inputRegex;
          if (edge.data?.outputRegex) mapping.output_regex = edge.data.outputRegex;
          
          mappings.push(mapping);
        });
      } else {

        mappings.push({
          target_variable: variable.target_variable,
          type: variable.type || 'string',
          is_visible: true,
          is_input: false,
          is_output: false
        });
      }
    });
    
    return mappings;
  }
  
  // build jsonpath from schema node id (NOT USED)
  function buildJsonPathFromSchemaNode(nodeId: string): string {
    if (nodeId.startsWith('schema-')) {
      let path = nodeId.replace('schema-', '');
      path = path.replace(/^(leaf|section)-/, '');
      path = path.replace(/-/g, '.');
      return path;
    }
    return '$';
  }

  function updateInteractionModeInConfig(newMode: string) {
    forceNodeUpdate();
  }

  function handleInteractionModeToggle(newMode: string) {
    if (newMode === currentInteractionMode) return;
    
    if (sidebarMode === 'edit') {
      pendingInteractionMode = newMode;
      showModeChangeWarning = true;
      return;
    }
    
    applyInteractionModeChange(newMode);
  }

  // switches between edit/view modes and saves current components, updates sidebar, rebuilds connections
  function applyInteractionModeChange(newMode: string) {
    const currentNodes = get(nodes);
    if (currentInteractionMode === 'edit') {
      editModeNodes = currentNodes.filter(n => 
        n.type === 'nodeWithItems' && n.data?.interactionMode === 'edit'
      );
    } else {
      viewModeNodes = currentNodes.filter(n => 
        n.type === 'nodeWithItems' && n.data?.interactionMode === 'view'
      );
    }
    
    currentInteractionMode = newMode; // switch mode
    updateInteractionModeInConfig(newMode);
    
    if (sidebarMode === 'edit') {
      sidebarMode = 'empty';
      selectedNode.set(null);
    }
    
    // get manifest submodes for new interaction mode
    const newModes = componentManifest?.modes?.[newMode];
    if (newModes && newModes.length > 0) {
      selectedMode = newModes[0];
    } else {
      selectedMode = null;
    }
    
    forceNodeUpdate();
    
    // set timeout to check nodes updated
    setTimeout(() => {
      const updatedNodes = get(nodes);
      const modesMatch = updatedNodes
        .filter(n => n.type === 'nodeWithItems')
        .every(n => n.data?.interactionMode === newMode);
      
      // reconstruct edges only if nodes updated correctly
      if (modesMatch && (newMode === 'edit' || newMode === 'view')) {
        reconstructEdgesForMode(newMode);
      } else {
        setTimeout(() => {
          if (newMode === 'edit' || newMode === 'view') {
            reconstructEdgesForMode(newMode);
          }
        }, 200);
      }
    }, 100);
  }

  function confirmModeChange() {
    applyInteractionModeChange(pendingInteractionMode);
    showModeChangeWarning = false;
    pendingInteractionMode = '';
  }

  function cancelModeChange() {
    showModeChangeWarning = false;
    pendingInteractionMode = '';
  }

  // save all component configurations and positions from both modes and download files

  function handleSaveEdit() {
    const currentNodes = get(nodes);
    
    // collect all nodes from both modes
    const allEditNodes = [
      ...editModeNodes,
      ...currentNodes.filter(n => 
        n.type === 'nodeWithItems' &&  n.data?.interactionMode === 'edit' && !editModeNodes.find(existing => existing.id === n.id)
      )
    ];
    
    const allViewNodes = [
      ...viewModeNodes,
      ...currentNodes.filter(n => 
        n.type === 'nodeWithItems' && n.data?.interactionMode === 'view' && !viewModeNodes.find(existing => existing.id === n.id)
      )
    ];

    // deduplicate nodes by id
    const editNodesMap = new Map<string, Node>();
    allEditNodes.forEach(node => editNodesMap.set(node.id, node));
    editModeNodes = Array.from(editNodesMap.values());
    
    const viewNodesMap = new Map<string, Node>();
    allViewNodes.forEach(node => viewNodesMap.set(node.id, node));
    viewModeNodes = Array.from(viewNodesMap.values());

    // save positions for all components
    const allComponentNodes = [...editModeNodes, ...viewModeNodes];
    allComponentNodes.forEach(node => {
      componentPositions[node.id] = {
        x: Math.round(node.position.x),
        y: Math.round(node.position.y)
      };
    });
    
    // update or add edit components to config
    editModeNodes.forEach(node => {
      const compIdx = componentConfig_edit.components.findIndex(
        (c: any) => c.meta.component_ui_id === node.id
      );
      
      // if component exists update variables
      if (compIdx >= 0) {
        const componentData = buildComponentData(node);
        if (componentData) {
          // merge instead of overwrite
          const existingVariables = componentConfig_edit.components[compIdx].mode.variables.variable || [];
          const newVariables = componentData.mode.variables.variable;
          
          const mergedVariables = newVariables.map((newVar: any) => {
            const existing = existingVariables.find((v: any) => 
              v.target_variable === newVar.target_variable
            );
            
            // keep existing regex if present
            if (existing) {
              return {
                ...newVar,
                input_regex: existing.input_regex || newVar.input_regex,
                output_regex: existing.output_regex || newVar.output_regex
              };
            }
            
            return newVar;
          });
          
          componentConfig_edit.components[compIdx].mode.variables.variable = mergedVariables;
        }
      } else {
        // add new component if missing
        const componentData = buildComponentData(node);
        if (componentData) {
          componentConfig_edit.components.push(componentData);
        }
      }
    });
    
    // update or add view components
    viewModeNodes.forEach(node => {
      const compIdx = componentConfig_view.components.findIndex(
        (c: any) => c.meta.component_ui_id === node.id
      );
      
      if (compIdx >= 0) {
        const componentData = buildComponentData(node);
        if (componentData) {
          componentConfig_view.components[compIdx].mode.variables = componentData.mode.variables;
        }
      } else {
        // add new component if missing
        const componentData = buildComponentData(node);
        if (componentData) {
          componentConfig_view.components.push(componentData);
        }
      }
    });

    // force config reactivity
    componentConfig_edit = { ...componentConfig_edit };
    componentConfig_view = { ...componentConfig_view };

    downloadAllConfigs(componentConfig_edit, componentConfig_view, componentPositions);
    
    alert(`Configuration saved!

    EDIT Mode: ${componentConfig_edit.components.length} component(s)
    VIEW Mode: ${componentConfig_view.components.length} component(s)
    Positions: ${Object.keys(componentPositions).length} saved
    
    Files downloaded:
    - componentConfig_edit.json
    - componentConfig_view.json
    - componentPositions.json`);
  }

  function handleCancelEdit() {
    showCancelWarning = true;
  }
  
  function confirmCancel() {
    sidebarMode = 'overview';
    activeTab = 0;
    showCancelWarning = false;
  }
  
  function rejectCancel() {
    showCancelWarning = false;
  }
  
  function handleDeleteComponent() {
    if (!$selectedNode) return;
    nodeToDelete = $selectedNode;
    showDeleteWarning = true;
  }
  
  // confirm deletion of component node and related data /edges
  function confirmDelete() {
    if (!nodeToDelete) return;
    
    const nodeId = nodeToDelete.id;
    nodes.update(allNodes => allNodes.filter(n => n.id !== nodeId));
    
    // remove from edit/view modes arrays
    if (nodeToDelete.data?.interactionMode === 'edit') {
      editModeNodes = editModeNodes.filter(n => n.id !== nodeId);
    } else {
      viewModeNodes = viewModeNodes.filter(n => n.id !== nodeId);
    }

    // remove from edit/view config objects
    componentConfig_edit.components = componentConfig_edit.components.filter(
      (c: any) => c.meta.component_ui_id !== nodeId
    );
    
    componentConfig_view.components = componentConfig_view.components.filter(
      (c: any) => c.meta.component_ui_id !== nodeId
    );
    
    delete componentPositions[nodeId]; // remove from positions
    
    // remove related edges from store
    edges.update(allEdges => allEdges.filter(edge => {
      const sourceMatch = edge.source === nodeId || edge.sourceHandle?.startsWith(nodeId + '-');
      const targetMatch = edge.target === nodeId || edge.targetHandle?.startsWith(nodeId + '-');
      return !sourceMatch && !targetMatch;
    }));
    
    componentConfig_edit = { ...componentConfig_edit };
    componentConfig_view = { ...componentConfig_view };
    
    sidebarMode = 'empty';
    selectedNode.set(null);
    showDeleteWarning = false;
    nodeToDelete = null;
  }
  
  function rejectDelete() {
    showDeleteWarning = false;
    nodeToDelete = null;
  }
  
  // save all but no settings (NOT USED)
  function handleSaveMappingsOnly() {

    updatePositionsFromNodes();

    const currentNodes = get(nodes);
    currentNodes.forEach(node => {
      if (node.type === 'nodeWithItems') {
        const componentData = buildComponentData(node);
        const currentConfig = getCurrentConfig();
        
        const existingIndex = currentConfig.components.findIndex(
          (c: any) => c.meta.component_name === node.data.componentName
        );
        
        if (existingIndex >= 0) {
          // only update variables keep other settings
          currentConfig.components[existingIndex].mode.variables = componentData.mode.variables;
        }
      }
    });
    
    // download all configs
    downloadAllConfigs(componentConfig_edit, componentConfig_view, componentPositions);
    
    // alert('Mappings saved!');
  }

  // apply loaded positions to nodes via ID
  function applyPositionsToNodes() {
    nodes.update(allNodes => allNodes.map(node => {
      if (componentPositions[node.id]) {
        return {
          ...node,
          position: componentPositions[node.id]
        };
      }
      return node;
    }));
  }
  
  
  // find schema node by JSONPath for edge creation
  function findSchemaNodeByJsonPath(jsonPath: string, allNodes: Node[]): Node | null {
    // remove leading "$."
    const cleanPath = jsonPath.replace(/^\$\.?/, '');
    
    const directMatch = allNodes.find(n => 
      n.type === 'leafNode' && n.data?.path === cleanPath
    );
    
    if (directMatch) {
      return directMatch;
    }
    
    // fallback: match by last part of path
    const pathParts = cleanPath.split('.');
    const leafName = pathParts[pathParts.length - 1];
    
    // return first matching leaf node
    return allNodes.find(n => 
      n.type === 'leafNode' && n.data?.label === leafName && typeof n.data?.path === 'string' && n.data.path === cleanPath
    ) || null;
  }
  
  // reconstruct edges for each node of a given interaction mode on mode change
  function reconstructEdgesForMode(mode: 'edit' | 'view') {
    const currentNodes = get(nodes);
    const config = mode === 'edit' ? componentConfig_edit : componentConfig_view;
    const existingEdges = get(edges);
    const existingIds = new Set(existingEdges.map(e => e.id));
    const newEdges: Edge[] = [];

    // create edges based on component variable mappings & filter components with current mode
    config.components?.filter((c: any) => c.globalSettings?.interaction_mode === mode).forEach((component: any) => {
        const componentNode = currentNodes.find(n =>
          n.id === component.meta.component_ui_id && n.type === 'nodeWithItems' && n.data?.interactionMode === mode
        );
        if (!componentNode) return;

        // create edges for each variable
        component.mode?.variables?.variable?.forEach((variable: any) => {
          if (!variable.JSONPath) return;
          const schemaNode = findSchemaNodeByJsonPath(variable.JSONPath, currentNodes);
          if (!schemaNode) return;

          const sourceHandle = `${componentNode.id}-${variable.target_variable}-handle`;
          const targetHandle = `${schemaNode.id}-handle`;
          const edgeId = `${sourceHandle}-${schemaNode.id}`;

          // add new edge if it doesn't exist
          if (!existingIds.has(edgeId)) {
            newEdges.push({
              id: edgeId,
              source: componentNode.id,
              target: schemaNode.id,
              sourceHandle,
              targetHandle,
              type: 'button',
              animated: false,
              style: 'stroke: #007acc; stroke-width: 2px;',
              markerEnd: variable.is_output ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined,
              markerStart: variable.is_input ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined,
              data: {
                leftDirection: variable.is_output || false,
                rightDirection: variable.is_input || false,
                sourceHandleId: sourceHandle,
                targetHandleId: targetHandle,
                sourceMode: mode
              }
            });
          }

          // update leafnode connected to this variable and set visibility from config
          nodes.update(ns => ns.map(n => {
            if (n.id === schemaNode.id && n.type === 'leafNode') {
              const modeVisibilityKey = `is_visible_${mode}`;
              const finalVisibility = variable.is_visible !== false;

              return {
                  ...n,
                  data: {
                    ...n.data,
                    [modeVisibilityKey]: finalVisibility, // store per mode via key
                    is_visible: finalVisibility,
                    edges,
                    nodes,
                    activeInteractionMode: currentInteractionMode,
                    onToggleVisibility: handleToggleLeafVisibility,
                    onSetAnchorpoint: handleSetAnchorpoint
                  }
                };
              }
              return n;
          }));
        });
      });

    if (newEdges.length > 0) {
      edges.set([...existingEdges, ...newEdges]);
    }

    // timeout to ensure nodes are updated
    setTimeout(() => {
      nodes.update(ns => ns.map(n =>
        n.type === 'leafNode'
          ? {
              ...n,
              data: {
                ...n.data,
                edges,
                nodes,
                onToggleVisibility: handleToggleLeafVisibility,
                onSetAnchorpoint: handleSetAnchorpoint
              }
            }
          : n
      ));
    }, 50);
  }

  // merge schema nodes with existing UI state, preserves user changes (visibility toggle...) (NOT USED)
  const enhancedSchemaNodes = schemaNodes.map(sn => {
    if (sn.type === 'leafNode') {
      const existing = $nodes.find(n => n.id === sn.id);
      const mergedData = existing ? { ...sn.data, ...existing.data } : sn.data;
      const preservedIsVisible = existing?.data?.is_visible ?? mergedData?.is_visible;
      
      return {
        ...sn,
        data: {
          ...mergedData,
          is_visible: preservedIsVisible,
          edges,
          nodes,
          activeInteractionMode: currentInteractionMode,
          onToggleVisibility: handleToggleLeafVisibility,
          onSetAnchorpoint: handleSetAnchorpoint
        }
      };
    }
    return sn;
  });

  $: componentVariables = selectedMode?.variables?.variable || [];
  $: componentSettings = selectedMode?.settings?.setting || [];

  // layout constants
  const childWidth = 140;
  const childHeight = 60;
  const colGap = 20;
  const rowGap = 12;
  const columns = 1;

  $: rows = Array.isArray(componentVariables) ? componentVariables.length : 0;
  $: parentWidth = Math.max(320, childWidth + 80);
  $: parentHeight = Math.max(140, rows * childHeight + (rows - 1) * rowGap + 100);

  // force node update by incrementing version > triggers reactive updates
  function forceNodeUpdate() {
    nodeVersion++;
  }

  // svelte stores for selected-/ nodes & edges
  let nodes: Writable<Node[]> = writable([]);
  let edges: Writable<Edge[]> = writable([]);
  let selectedNode: Writable<Node | null> = writable(null);
  let selectedEdge: Writable<Edge | null> = writable(null);
  // sidebar state and selected tab
  let sidebarMode: 'empty' | 'overview' | 'edit' = 'empty';
  let activeTab = 0;

  // receive and store generated schema nodes from tree component
  function handleSchemaNodesGenerated(event: any) {
    const generatedNodes = event.detail.nodes || [];
    schemaNodes = generatedNodes;
  }

  // dynamically create nodes
  $: configNodes = createConfigNodes(currentInteractionMode, getCurrentConfig(), componentManifest, nodeVersion, editModeNodes, viewModeNodes);

  // function to create config nodes on start or mode change based on config / saved nodes
  function createConfigNodes(
    interactionMode?: string, 
    config?: any, 
    manifest?: any, 
    version?: number,
    savedEditNodes?: Node[],
    savedViewNodes?: Node[]
  ): Node[] {
    const currentMode = interactionMode || currentInteractionMode;
    
    // use saved nodes if available (edit / view)
    if (currentMode === 'edit' && savedEditNodes && savedEditNodes.length > 0) {
      return savedEditNodes.map(n => ({
        ...n,
        data: { ...n.data, edges: get(edges), version }
      }));
    }
    
    if (currentMode === 'view' && savedViewNodes && savedViewNodes.length > 0) {
      return savedViewNodes.map(n => ({
        ...n,
        data: { ...n.data, edges: get(edges), version }
      }));
    }
    
    const components = config?.components || [];
    if (!components || !Array.isArray(components)) return [];
    
    const currentManifest = manifest || componentManifest;
    const configNodesArray: Node[] = [];
    
    // create nodes for each component in config matching current interaction mode
    components.forEach((component: any, index: number) => {
      if (component.globalSettings?.interaction_mode !== currentMode) {
        return;
      }
      
      const manifestModes = currentManifest?.modes?.[currentMode] || [];
      const modeDetails = manifestModes.find((mode: any) => mode.mode_name === component.mode?.mode_name);
      
      if (!modeDetails) {
        return;
      }
      
      // extract variables for child items
      const modeVariables = modeDetails.variables?.variable || [];
      const childItems = modeVariables.map((variable: any) => ({
        id: variable.target_variable,
        label: variable.target_variable,
        isInput: variable.is_input,
        isOutput: variable.is_output,
        type: variable.type
      }));

      const rows = modeVariables.length;
      const nodeWidth = Math.max(320, childWidth + 80);
      const nodeHeight = Math.max(140, rows * childHeight + (rows - 1) * rowGap + 100);

      // search for existing position via component_ui_id
      const nodeId = component.meta.component_ui_id;
      let position = { x: 400 + (index * 200), y: 100 }; // default position

      if (componentPositions[nodeId]) {
        position = componentPositions[nodeId]; // saved position
      }

      // create node object
      const node = {
        id: nodeId,
        type: 'nodeWithItems',
        data: {
          label: `${component.meta.component_name}`,
          componentName: component.meta.component_name,
          componentId: nodeId,
          modeName: component.mode?.mode_name || '',
          interactionMode: currentMode,
          childItems: childItems,
          componentVariables: modeVariables,
          edges: [],
          version: version || nodeVersion,
          anchorpoint: (component.globalSettings?.anchorpoint || '').replace(/^\$\.?/, '')
        },
        position: position,
        style: `width: ${nodeWidth}px; height: ${nodeHeight}px;`,
        selectable: true,
        deletable: true,
        selected: false,
        zIndex: 0,
        dragging: false,
        draggable: true
      };
      
      configNodesArray.push(node);
    });

    return configNodesArray;
  }

  // toggle visibility of leaf node and update connected component variable visibility
  function handleToggleLeafVisibility(leafNodeId: string) {
    nodes.update(allNodes => allNodes.map(node => {
      if (node.id === leafNodeId && node.type === 'leafNode') {
        const newIsVisible = !(node.data?.is_visible !== false);
        const modeVisibilityKey = `is_visible_${currentInteractionMode}`;
        const updatedNode = {
          ...node,
          data: {
            ...node.data,
            [modeVisibilityKey]: newIsVisible, // store per mode via key
            is_visible: newIsVisible,
            edges,
            nodes,
            onToggleVisibility: handleToggleLeafVisibility,
            onSetAnchorpoint: handleSetAnchorpoint
          }
        };

        const currentEdges = get(edges);
        const handleId = `${leafNodeId}-handle`;
        const connectedEdges = currentEdges.filter((edge: any) =>
          edge.sourceHandle === handleId || edge.targetHandle === handleId
        );

        // update for each connected component variable visibility
        connectedEdges.forEach((edge: any) => {
          const componentId = edge.source === leafNodeId ? edge.target : edge.source;
          const nodePath = node.data?.path;
          if (typeof nodePath === 'string') {
            updateComponentVariableVisibility(componentId, nodePath, newIsVisible);
          }
        });

        return updatedNode;
      }
      return node;
    }));
  }

  // set anchorpoint JSONPath for a component
  function handleSetAnchorpoint(componentId: string, jsonPath: string) {
    
    nodes.update(ns => ns.map(n => { // update node data
      if (n.id === componentId) {
        return { ...n, data: { ...n.data, anchorpoint: jsonPath } };
      }
      return n;
    }));

    // update selected node
    const sel = get(selectedNode);
    if (sel?.id === componentId) {
      const updated = get(nodes).find(n => n.id === componentId);
      if (updated) selectedNode.set(updated);
    }

    // update in config
    const currentConfig = getCurrentConfig();
    const componentIndex = currentConfig.components.findIndex(
      (c: any) => c.meta.component_ui_id === componentId
    );

    // set new anchorpoint if component found
    if (componentIndex >= 0) {
      currentConfig.components[componentIndex].globalSettings.anchorpoint = jsonPath;
      if (currentInteractionMode === 'edit') {
        componentConfig_edit = { ...componentConfig_edit };
      } else {
        componentConfig_view = { ...componentConfig_view };
      }
    }

    // refresh leaf props so anchor icon updates
    setTimeout(() => {
      nodes.update(ns => ns.map(n => {
        if (n.type === 'leafNode') {
          return {
            ...n,
            data: {
              ...n.data,
              edges,
              nodes,
              onToggleVisibility: handleToggleLeafVisibility,
              onSetAnchorpoint: handleSetAnchorpoint
            }
          };
        }
        return n;
      }));
    }, 30);
  }

  // sets is_visible for a component variable based on leaf path
  function updateComponentVariableVisibility(componentId: string, leafPath: string, isVisible: boolean) {
    const jsonPath = leafPath;
    const currentConfig = getCurrentConfig();
    // filter any component by id
    const componentIndex = currentConfig.components.findIndex(
      (c: any) => c.meta.component_ui_id === componentId
    );

    if (componentIndex >= 0) {
      const vars = currentConfig.components[componentIndex].mode?.variables?.variable || []; // all component variables
      
      // update all variables with matching jsonpath
      let updated = false;
      vars.forEach((v: any) => {
        if (v.JSONPath === jsonPath) {
          v.is_visible = isVisible;
          updated = true;
        }
      });

      if (updated) {
        if (currentInteractionMode === 'edit') {
          componentConfig_edit = { ...componentConfig_edit };
        } else {
          componentConfig_view = { ...componentConfig_view };
        }
      }
    }
  }

  // reactive block: merges component nodes from config with canvas state,
  // adds schema tree nodes, and updates store only when actual changes detected
  $: {
    const allNodes: Node[] = []; // array for store update
    const currentNodes = $nodes;
    
    // step 1: get all existing component nodes
    const existingComponentNodes = currentNodes.filter(n => 
      n.type === 'nodeWithItems' && n.data?.interactionMode === currentInteractionMode
    );
    
    // step 2: create map of config nodes for updates
    const configNodeMap = new Map<string, Node>(); // map that contains config nodes by ID
    if (configNodes && Array.isArray(configNodes) && configNodes.length > 0) {
      configNodes.filter(node => node.data?.interactionMode === currentInteractionMode).forEach(node => {
          configNodeMap.set(node.id, node);
        });
    }
    
    // step 3: merge using existing nodes, update with config data
    existingComponentNodes.forEach(existingNode => {
      const configNode = configNodeMap.get(existingNode.id);
      
      if (configNode) {
        // node exists in config: update with config data
        allNodes.push({
          ...configNode,
          position: existingNode.position,
          data: {
            ...configNode.data,
            anchorpoint: existingNode.data.anchorpoint || configNode.data.anchorpoint || '',
            edges,
            version: nodeVersion,
            isGrayedOut: isEditingComponent && existingNode.id !== $selectedNode?.id && existingNode.data?.interactionMode === currentInteractionMode,
            isEditMode: isEditingComponent
          }
        });
        configNodeMap.delete(existingNode.id); // delete if already processed
      } else {
        // node does NOT exist in config > keep existing node
        allNodes.push({
          ...existingNode,
          data: {
            ...existingNode.data,
            edges: $edges || [],
            version: nodeVersion,
            isGrayedOut: isEditingComponent && existingNode.id !== $selectedNode?.id && existingNode.data?.interactionMode === currentInteractionMode,
            isEditMode: isEditingComponent
          }
        });
      }
    });
    
    // step 4: add new config nodes not in nodes store yet
    configNodeMap.forEach(configNode => {
      allNodes.push({
        ...configNode,
        data: {
          ...configNode.data,
          edges: $edges || [],
          version: nodeVersion,
          isGrayedOut: isEditingComponent && configNode.id !== $selectedNode?.id && configNode.data?.interactionMode === currentInteractionMode,
          isEditMode: isEditingComponent
        }
      });
    });
    
    // step 5: add schema nodes
    if (schemaNodes && Array.isArray(schemaNodes) && schemaNodes.length > 0) {
      const enhancedSchemaNodes = schemaNodes.map(sn => {
        if (sn.type === 'leafNode') {
          // check if node already exists in node array
          const existing = currentNodes.find(n => n.id === sn.id);
          const modeVisibilityKey = `is_visible_${currentInteractionMode}`;
          const preservedModeVisibility = existing?.data?.[modeVisibilityKey];
          
          return {
            ...sn,
            data: {
              // preserve existing data to maintain state such as visibility per mode
              ...(existing ? existing.data : sn.data),
              [modeVisibilityKey]: preservedModeVisibility !== undefined ? preservedModeVisibility : sn.data?.is_visible,
              is_visible: preservedModeVisibility !== undefined ? preservedModeVisibility : sn.data?.is_visible,
              edges,
              nodes,
              activeInteractionMode: currentInteractionMode,
              onToggleVisibility: handleToggleLeafVisibility,
              onSetAnchorpoint: handleSetAnchorpoint
            }
          };
        }
        return sn;
      });
      allNodes.push(...enhancedSchemaNodes);
}
    
    // create snapshot to detect changes, store update only if changed
    const newSnapshot = JSON.stringify(
      allNodes.map(n => ({
        id: n.id,
        x: Math.round(n.position.x),
        y: Math.round(n.position.y),
        version: n.data?.version,
        childItemsCount: Array.isArray(n.data?.childItems) ? n.data.childItems.length : 0,
        isGrayedOut: n.data?.isGrayedOut,
        isEditMode: n.data?.isEditMode,
        selectedNodeId: $selectedNode?.id,
        edgesHash: JSON.stringify($edges.map(e => ({
          id: e.id,
          source: e.source,
          target: e.target,
          sourceHandle: e.sourceHandle,
          targetHandle: e.targetHandle,
          leftDir: e.data?.leftDirection,
          rightDir: e.data?.rightDirection
        })))
      }))
    );
    
    if (newSnapshot !== lastNodesSnapshot) {
      lastNodesSnapshot = newSnapshot;
      nodes.set(allNodes);
    }
  }

  // update edge style based on editing state (grey out non selected)
  $: {
    if (isEditingComponent && $selectedNode) {
      edges.update(allEdges => allEdges.map(edge => {
        const isConnectedToSelected = 
          edge.sourceHandle?.startsWith($selectedNode.id + '-') || 
          edge.targetHandle?.startsWith($selectedNode.id + '-');

        const edgeStyle = isConnectedToSelected ? 'stroke: #007acc; stroke-width: 2px;' : 'stroke: #cccccc; stroke-width: 2px; opacity: 0.3;';
        const shouldAnimate = isConnectedToSelected ? edge.animated : false;
        
        return {
          ...edge,
          style: edgeStyle,
          animated: shouldAnimate
        };
      }));
    } else {
      edges.update(allEdges => allEdges.map(edge => ({
        ...edge,
        style: 'stroke: #007acc; stroke-width: 2px;',
        animated: edge.animated
      })));
    }
  }

  // track if user is editing
  $: isEditingComponent = sidebarMode === 'edit' && $selectedNode !== null;

  let validationStatus = writable({
    connected: {
      total: 0,
      connected: 0,
      items: {}
    },
    isValid: false,
    typeValid: true
  });

  // update validation status based on selected node
  $: {
    if ($selectedNode && Array.isArray(componentVariables) && componentVariables.length > 0) {
      const selectedNodeVariables = $selectedNode.data?.componentVariables || [];
      validationStatus.update(status => ({
        ...status,
        connected: {
          ...status.connected,
          total: Array.isArray(selectedNodeVariables) ? selectedNodeVariables.length : 0
        }
      }));
      setTimeout(validateConnections, 10);
    }
  }

  $: currentNodeMode = $selectedNode ? getCurrentNodeMode() : selectedMode; // reactive current submode
  $: effectivePreviewMode = getEffectivePreviewMode();

  // determine submode to display in preview
  function getEffectivePreviewMode() {
    if ($selectedNode) {
      const nodeMode = getCurrentNodeMode();
      if (nodeMode) return nodeMode;
    }

    if (selectedMode) return selectedMode;

    // fallback to first available mode in manifest
    const availableModes = componentManifest?.modes?.[currentInteractionMode] || [];
    return availableModes[0] || null;
  }

  // determine current submode for selected node
  function getCurrentNodeMode() {
    if (!$selectedNode) return selectedMode;

    if (nodeSpecificModes.has($selectedNode.id)) {
      const stored = nodeSpecificModes.get($selectedNode.id);
      return stored;
    }

    if ($selectedNode.data?.modeName) {
      const interactionMode = $selectedNode.data?.interactionMode || currentInteractionMode;
      const manifestModes = componentManifest?.modes?.[interactionMode as string] || [];
      const nodeMode = manifestModes.find((mode: any) => mode.mode_name === $selectedNode.data.modeName);
      
      if (nodeMode) {
        nodeSpecificModes.set($selectedNode.id, nodeMode);
        return nodeMode;
      } else {
        // console.warn(` Mode "${$selectedNode.data.modeName}" not found in manifest for ${interactionMode}`);
      }
    }
    
    return selectedMode;
  }

  function handleModeChange(newMode: any) {
    if (!$selectedNode) {
      return;
    }

    // store current node position before mode change
    const currentPosition = $selectedNode.position;
    nodeSpecificModes.set($selectedNode.id, newMode); // store mode for this node

    // extract new variables and child items
    const newVariables = newMode.variables?.variable || [];
    const newChildItems = newVariables.map((variable: any) => ({
      id: variable.target_variable,
      label: variable.target_variable,
      isInput: variable.is_input,
      isOutput: variable.is_output,
      type: variable.type
    }));

    // dynamic component sizing on mode change
    const rows = newVariables.length;
    const newWidth = Math.max(320, childWidth + 80);
    const newHeight = Math.max(140, rows * childHeight + (rows - 1) * rowGap + 100);

    nodes.update(allNodes => allNodes.map(node => {
      if (node.id === $selectedNode.id) {
        return {
          ...node,
          position: currentPosition, // preserve position
          data: {
            ...node.data,
            modeName: newMode.mode_name,
            childItems: newChildItems,
            componentVariables: newVariables,
            version: nodeVersion + 1
          },
          style: `width: ${newWidth}px; height: ${newHeight}px;`
        };
      }
      return node;
    }));

    // filter dynamic edges & remove edges belonging to edited handles
    edges.update(allEdges => allEdges.filter(edge => 
      !edge.sourceHandle?.startsWith($selectedNode.id + '-') &&
      !edge.targetHandle?.startsWith($selectedNode.id + '-')
    ));
    
    selectedMode = newMode;
    componentVariables = newMode.variables?.variable || [];
    componentSettings = newMode.settings?.setting || [];
  }

  // calculate center position of current viewport in flow coordinates
  function getViewportCenter(): {x: number, y: number} {
    // get flow container element and its dimensions
    const flowContainer = document.querySelector('.svelte-flow');
    const flowRect = flowContainer?.getBoundingClientRect();
    
    if (!flowRect || !flowContainer) {
      return { x: 400, y: 300 }; // fallback position
    }

    // get viewport from flow container
    const viewportElement = flowContainer.querySelector('.svelte-flow__viewport');
    if (!viewportElement) {
      return { x: 400, y: 300 };
    }

    // parse transform matrix to get current pan/zoom
    const transform = window.getComputedStyle(viewportElement).transform; // get transform string matrix from DOM
    // EXAMPLE: Zoom 150%, displaced by 200px right & 100px down -> transform: matrix(1.5, 0, 0, 1.5, 200, 100);
    
    let x = 0, y = 0, zoom = 1;
    
    // extract values from matrix
    if (transform && transform !== 'none') {
      const matrix = transform.match(/matrix\(([^)]+)\)/);
      // extract matrix values from CSS transform string,  EXAMPLE: "matrix(1.5, 0, 0, 1.5, 200, 100)" >> ["1.5", "0", "0", "1.5", "200", "100"]
      
      if (matrix) {
        const values = matrix[1].split(',').map(v => parseFloat(v.trim())); // csv to float array
        zoom = values[0]; // zoom level
        x = values[4];    // horizontal
        y = values[5];    // vertical
      }
    }

    // calculate center pos in flow coordinates
    const centerX = (-x + flowRect.width / 2) / zoom;
    const centerY = (-y + flowRect.height / 2) / zoom;

    return { x: centerX - 160, y: centerY - 100 }; // return center adjusted for node size
  }

  // add new component node to canvas
  function handleAddComponent(component: any) {
    const centerPos = getViewportCenter();
    const modeVariables = component.mode?.variables?.variable || [];
    
    const childItems = modeVariables.map((variable: any) => ({
      id: variable.target_variable,
      label: variable.target_variable,
      isInput: variable.is_input,
      isOutput: variable.is_output,
      type: variable.type
    }));

    const rows = Array.isArray(modeVariables) ? modeVariables.length : 0;
    const newParentWidth = Math.max(320, childWidth + 80);
    const newParentHeight = Math.max(140, rows * childHeight + (rows - 1) * rowGap + 100);
    const newNodeId = `${component.meta.component_name}-${currentInteractionMode}-${component.mode.mode_name}-${Date.now()}`;
    
    const newNode = {
      id: newNodeId,
      type: 'nodeWithItems',
      data: {
        label: `${component.meta.component_name}`,
        componentName: component.meta.component_name,
        componentId: newNodeId,
        modeName: component.mode?.mode_name || '',
        interactionMode: currentInteractionMode,
        childItems: childItems,
        componentVariables: modeVariables,
        edges: [],
        version: 0
      },
      position: centerPos,
      style: `width: ${newParentWidth}px; height: ${newParentHeight}px;`,
      selectable: true,
      deletable: true,
      selected: false,
      zIndex: 0,
      dragging: false,
      draggable: true
    };

    nodes.update(current => [...current, newNode]);
  }

  // find initial edge direction on connection based on component variable settings
  function determineInitialDirection(sourceHandleId: string, targetHandleId: string) {

    let componentVariablesToCheck: any[] = []; // variables array to check
    let componentHandleId = '';
    
    const allNodes = get(nodes);
    
    // find dynamic component node
    for (const node of allNodes) {
      if (node.type === 'nodeWithItems') {
        // check if source or target handle belongs to this component
        if (sourceHandleId && sourceHandleId.startsWith(`${node.id}-`) && sourceHandleId.endsWith('-handle')) {
          componentVariablesToCheck = Array.isArray(node.data?.componentVariables) ? node.data.componentVariables : [];
          componentHandleId = sourceHandleId;
          break;
        } else if (targetHandleId && targetHandleId.startsWith(`${node.id}-`) && targetHandleId.endsWith('-handle')) {
          componentVariablesToCheck = Array.isArray(node.data?.componentVariables) ? node.data.componentVariables : [];
          componentHandleId = targetHandleId;
          break;
        }
      }
    }

    // check if component handle and variables are valid & set bools for arrow markers
    if (componentHandleId && componentVariablesToCheck.length > 0) {

      const handleParts = componentHandleId.split('-'); // extract dynamic variable name

      // expected format: componentId-variableName-handle
      if (handleParts.length >= 3) {
        const variableName = handleParts[handleParts.length - 2];

        // find dynamic variable
        const variable = componentVariablesToCheck.find((v: any) => v.target_variable === variableName);
        
        if (variable) {
          
          // automatic bidirectionality
          if (variable.is_input && variable.is_output) {
            const currentEdges = get(edges);
            const hasInputAlready = currentEdges.some(edge => {
              const sameHandle = edge.sourceHandle === componentHandleId || edge.targetHandle === componentHandleId;
              if (!sameHandle) return false;
              return edge.data?.rightDirection === true || (edge.data?.leftDirection === true && edge.data?.rightDirection === true);
            });
            return hasInputAlready ? { leftDirection: true, rightDirection: false } : { leftDirection: true, rightDirection: true };
          }
          // input-only
          if (variable.is_input && !variable.is_output) {
            // console.log('setting right direction for input-only parameter:', variableName);
            return { leftDirection: false, rightDirection: true };
          }
          // output-only
          if (!variable.is_input && variable.is_output) {
            // console.log('setting left direction for output-only parameter:', variableName);
            return { leftDirection: true, rightDirection: false };
          }
        } else {
          // console.log('variable not found for name:', variableName);
        }
      } else {
        // console.log('unexpected handle format, parts length:', handleParts.length);
      }
    } else {
      // console.log('no component handle found in connection');
    }
    
    return { leftDirection: false, rightDirection: true };
  }

  // checks edges for correct directions, if not -> calculate and set again
  function fixExistingEdges() {
    const currentEdges = get(edges);
    
    if (!Array.isArray(currentEdges)) {
      // console.log('no valid edges array');
      return;
    }
    
    let hasChanges = false;
    
    // iterate edges and fix missing direction data
    const fixedEdges = currentEdges.map(edge => {
      const hasValidDirections = edge.data && 
        (edge.data.leftDirection === true || edge.data.leftDirection === false) &&
        (edge.data.rightDirection === true || edge.data.rightDirection === false);
      
      if (hasValidDirections) {
        return edge;
      }
      
      // console.log('fixing edge with missing/invalid direction data:', edge.id, 'current data:', edge.data);
      const direction = determineInitialDirection(edge.sourceHandle ?? '', edge.targetHandle ?? '');
      // console.log('determined direction for edge:', edge.id, direction);
      
      // reconstruct edge with correct direction data and markers
      const fixedEdge = {
        ...edge,
        data: {
          ...edge.data,
          leftDirection: direction.leftDirection,
          rightDirection: direction.rightDirection,
          sourceHandleId: edge.sourceHandle,
          targetHandleId: edge.targetHandle
        },

        markerEnd: direction.leftDirection ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined,
        markerStart: direction.rightDirection ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined
      };

      hasChanges = true;
      return fixedEdge;
    });
    
    if (hasChanges) {
      edges.set(fixedEdges);
      setTimeout(validateConnections, 100);
    }
  }

  // reactive call to fix edges on variable change
  $: if (Array.isArray(componentVariables) && componentVariables.length > 0) {
    setTimeout(fixExistingEdges, 50);
  }

  // on node/edge click: set sidebar mode, clear selection, open overview
  function handleNodeClick(event: any) {
    const node = event.detail.node;
    selectedNode.set(node);
    selectedEdge.set(null);
    
    edges.update(eds => eds.map(edge => ({ ...edge, selected: false }))); // reset edge selection
    
    if (node.type === 'nodeWithItems' || node.type === 'group') {
      sidebarMode = 'overview';
    }
  }

  // handle edge click: select edge, clear node selection
  function handleEdgeClick(event: any) {
    const edge = event.detail.edge;
    
    // early return for edges not connected to selected node
    if (isEditingComponent && $selectedNode) {
      const isConnectedToSelected = 
        edge.sourceHandle?.startsWith($selectedNode.id + '-') || 
        edge.targetHandle?.startsWith($selectedNode.id + '-');
      
      if (!isConnectedToSelected) {
        return;
      }
    }
    
    selectedEdge.set(edge);
    selectedNode.set(null);
    
    edges.update(eds => eds.map(e => ({ ...e, selected: e.id === edge.id })));
  }

  // reset selection on pane click
  function handlePaneClick() {
    selectedNode.set(null);
    selectedEdge.set(null);
    sidebarMode = 'empty';
    
    edges.update(eds => eds.map(edge => ({ ...edge, selected: false })));
  }

  function handleEdit() {
    sidebarMode = 'edit';
    activeTab = 0;
  }

  // check if connection is valid before allowing 
  function isValidConnection(connection: any) {
    if (!connection) return true;
    
    // check input restriction
    if (connection.sourceHandle && connection.sourceHandle.includes('-handle')) {
      const currentEdges = get(edges);
      const currentNodes = get(nodes);
      const initialDirection = determineInitialDirection(connection.sourceHandle, connection.targetHandle);
      
      const sourceNode = currentNodes.find(n => connection.sourceHandle.startsWith(n.id + '-'));
      const isComponentNode = sourceNode?.type === 'nodeWithItems' || sourceNode?.id?.startsWith('config-component-') || sourceNode?.id?.startsWith('library-component-');
      
      // check flow direction for components
      if (isComponentNode && Array.isArray(currentEdges)) {
        const newEdgeWouldHaveInput = initialDirection.rightDirection || (initialDirection.leftDirection && initialDirection.rightDirection);
        
        if (newEdgeWouldHaveInput) {
          const sourceNodeMode = sourceNode?.data?.interactionMode;
          
          // check all existing input connections
          const existingInputConnections = currentEdges.filter(edge => {
            if (edge.sourceHandle !== connection.sourceHandle) return false;
            
            const hasInputFlow = edge.data?.rightDirection === true || (edge.data?.leftDirection === true && edge.data?.rightDirection === true);
            if (!hasInputFlow) return false;
            
            const edgeSourceNode = currentNodes.find(n => edge.sourceHandle?.startsWith(n.id + '-'));
            const edgeSourceNodeMode = edgeSourceNode?.data?.interactionMode;
            
            return edgeSourceNodeMode === sourceNodeMode;
          });
          
          // allow if variable is IN & OUT, but block for only IN
          if (existingInputConnections.length > 0) {
            const parts = (connection.sourceHandle || '').split('-');
            const variableName = parts.length >= 3 ? parts[parts.length - 2] : '';
            const variables = Array.isArray(sourceNode?.data?.componentVariables) ? sourceNode!.data.componentVariables : [];
            const variable = variables.find((v: any) => v.target_variable === variableName);
            const isInputOnly = variable?.is_input && !variable?.is_output;
            return isInputOnly ? false : true;
          }
        }
      }
    }
    
    return true;
  }

  // handles new edge connections between nodes: validates input restrictions, 
  // sets edge direction, updates leaf visibility, and syncs config with JSONPath mappings
  function onConnect(params: any) {
    if (!params) return;

    // input restriction validation, only one input per variable
    if (params.sourceHandle && params.sourceHandle.includes('-handle')) {
      const currentEdges = get(edges);
      const currentNodes = get(nodes);
      const initialDirection = determineInitialDirection(params.sourceHandle, params.targetHandle);
      const sourceNode = currentNodes.find(n => params.sourceHandle.startsWith(n.id + '-'));
      const isComponentNode = sourceNode?.type === 'nodeWithItems' || sourceNode?.id?.startsWith('config-component-') || sourceNode?.id?.startsWith('library-component-');

      // block duplicate input connections 
      if (isComponentNode && Array.isArray(currentEdges)) {
        const newEdgeWouldHaveInput = initialDirection.rightDirection || (initialDirection.leftDirection && initialDirection.rightDirection);

        if (newEdgeWouldHaveInput) {
          const sourceNodeMode = sourceNode?.data?.interactionMode;
          // check if handle already has input connections
          const existingInputConnections = currentEdges.filter(edge => {
            if (edge.sourceHandle !== params.sourceHandle) return false;

            const hasInputFlow = edge.data?.rightDirection === true || (edge.data?.leftDirection === true && edge.data?.rightDirection === true);
            if (!hasInputFlow) return false;

            const edgeSourceNode = currentNodes.find(n => edge.sourceHandle?.startsWith(n.id + '-'));
            const edgeSourceNodeMode = edgeSourceNode?.data?.interactionMode;

            return edgeSourceNodeMode === sourceNodeMode;
          });

          if (existingInputConnections.length > 0) {
            // allow IN & OUT variables to add more edges but as forced OUT-only, when input already exists
            const parts = (params.sourceHandle || '').split('-');
            const variableName = parts.length >= 3 ? parts[parts.length - 2] : '';
            const variables = Array.isArray(sourceNode?.data?.componentVariables) ? sourceNode!.data.componentVariables : [];
            const variable = variables.find((v: any) => v.target_variable === variableName);
            const isInputOnly = variable?.is_input && !variable?.is_output;
            if (isInputOnly) return; // block only input-only duplicates
          }
        }
      }
    }

    // determine edge properties (direction arrows, visibility)
    const initialDirection = determineInitialDirection(params.sourceHandle, params.targetHandle);
    const currentNodes = get(nodes);
    const currentEdges = get(edges);
    const sourceNode = currentNodes.find(n => params.sourceHandle?.startsWith(n.id + '-'));
    const targetNode = currentNodes.find(n => n.id === params.target);
    const sourceNodeMode = sourceNode?.data?.interactionMode;
    const componentUiId = sourceNode?.id;

    // special logic for leaf -> node connections (to set JSONPath & visibility)
    if (sourceNode?.type === 'nodeWithItems' && targetNode?.type === 'leafNode') {
      const targetJsonPath = targetNode.data?.path || '';
      const isVisible = initialDirection.leftDirection && !initialDirection.rightDirection;

      const existingLeafConnections = currentEdges.filter(edge => {
        const edgeSource = currentNodes.find(n => n.id === edge.source);
        const edgeTarget = currentNodes.find(n => n.id === edge.target);
        
        const isFromSameComponent = 
          (edge.source === sourceNode.id || edge.target === sourceNode.id) ||
          (edge.sourceHandle?.startsWith(sourceNode.id + '-') || edge.targetHandle?.startsWith(sourceNode.id + '-'));
        
        if (!isFromSameComponent) return false;
        
        const isLeafConnection = edgeTarget?.type === 'leafNode' || edgeSource?.type === 'leafNode';
        return isLeafConnection;
      });

      const currentAnchorpoint = sourceNode.data?.anchorpoint;
      
      if (targetJsonPath && existingLeafConnections.length === 0) {
        
        // update node state with new anchorpoint
        nodes.update(ns => ns.map(n =>
          n.id === sourceNode.id ? { ...n, data: {...n.data, anchorpoint: targetJsonPath}} : n
        ));

        // update selectedNode if selected
        selectedNode.update(sn => 
          sn && sn.id === sourceNode.id ? { ...sn, data: { ...sn.data, anchorpoint: targetJsonPath }} : sn
        );

        // update config directly
        const cfg = getCurrentConfig();
        let compIdx = cfg.components.findIndex((c: any) =>
          c.meta.component_ui_id === sourceNode.id
        );
        
        // update or create component entry
        if (compIdx >= 0) {
          // component exists > update
          cfg.components[compIdx].globalSettings.anchorpoint = targetJsonPath;
        } else {
          // component not yet in config > create with buildComponentData
          const newComponentData = buildComponentData(sourceNode);
          if (newComponentData) {
            newComponentData.globalSettings.anchorpoint = targetJsonPath;
            cfg.components.push(newComponentData);
            compIdx = cfg.components.length - 1;
          }
        }
        
        // force config reactivity
        if (currentInteractionMode === 'edit') {
          componentConfig_edit = { ...cfg };
        } else {
          componentConfig_view = { ...cfg };
        }
      }

      // update leaf node state
      nodes.update(allNodes =>
        allNodes.map(node => {
          if (node.id === targetNode.id) {
            return {
              ...node,
              data: {
                ...node.data,
                is_visible: node.data?.is_visible ?? isVisible,
                edges,
                nodes,
                activeInteractionMode: currentInteractionMode,
                onToggleVisibility: handleToggleLeafVisibility,
                onSetAnchorpoint: handleSetAnchorpoint
              }
            };
          }
          return node;
        })
      );

      // update component variable visibility
      if (componentUiId && targetNode.data?.path && typeof targetNode.data.path === 'string') {
        updateComponentVariableVisibility(componentUiId, targetNode.data.path, isVisible);
      }
    }

    // create new edge
    const newEdge = {
      id: `${params.source}-${params.target}`,
      source: params.source,
      target: params.target,
      sourceHandle: params.sourceHandle,
      targetHandle: params.targetHandle,
      type: 'button',
      animated: false,
      style: 'stroke: #007acc; stroke-width: 2px;',
      selected: false,
      markerEnd: initialDirection.leftDirection ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined,
      markerStart: initialDirection.rightDirection ? { type: MarkerType.ArrowClosed, color: '#007acc' } : undefined,
      data: {
        leftDirection: initialDirection.leftDirection,
        rightDirection: initialDirection.rightDirection,
        sourceHandleId: params.sourceHandle,
        targetHandleId: params.targetHandle,
        sourceMode: sourceNodeMode
      }
    };

    edges.update(all => [...all, newEdge]);

    // update component variable with JSONPath & directions
    if (sourceNode?.type === 'nodeWithItems' && targetNode?.type === 'leafNode') {
      const cfg = getCurrentConfig();
      const compIdx = cfg.components.findIndex((c: any) =>
        c.meta.component_ui_id === sourceNode.id
      );

      // update variable entry
      if (compIdx >= 0) {
        const varsArr = cfg.components[compIdx].mode?.variables?.variable || [];
        const parts = (params.sourceHandle || '').split('-');
        const varName = parts.length >= 3 ? parts[parts.length - 2] : '';
        const vIdx = varsArr.findIndex((v: any) => v.target_variable === varName);
        const jsonPath = targetNode.data?.path || '';
        
        // update variable data with new edge info
        if (vIdx >= 0 && jsonPath) {
          varsArr[vIdx].JSONPath = jsonPath;
          varsArr[vIdx].is_input = newEdge.data.rightDirection;
          varsArr[vIdx].is_output = newEdge.data.leftDirection;
          if (typeof targetNode.data?.is_visible === 'boolean') {
            varsArr[vIdx].is_visible = targetNode.data.is_visible;
          } else if (varsArr[vIdx].is_visible === undefined) {
            varsArr[vIdx].is_visible = true;
          }
        }
        if (currentInteractionMode === 'edit') {
          componentConfig_edit = { ...componentConfig_edit };
        } else {
          componentConfig_view = { ...componentConfig_view };
        }
      }
    }

    // force update leaf nodes
    setTimeout(() => {
      nodes.update(allNodes =>
        allNodes.map(node => {
          if (node.type === 'leafNode') {
            return {
              ...node,
              data: {
                ...node.data,
                edges,
                nodes,
                activeInteractionMode: currentInteractionMode,
                onToggleVisibility: handleToggleLeafVisibility,
                onSetAnchorpoint: handleSetAnchorpoint
              }
            };
          }
          return node;
        })
      );
      validateConnections();
    }, 50);
  }

  // validate connections for component variables and update validation status
  function validateConnections() {
    const currentEdges = get(edges);
    const allNodes = get(nodes);
    let totalConnectedCount = 0;
    let totalVariablesCount = 0;
    let allItems: { [key: string]: boolean } = {};
    let typeValid = true;

    // validate based on mode and sidebar state
    if ($selectedNode && sidebarMode === 'edit' && $selectedNode.type === 'nodeWithItems') {
      // edit mode: validate only selected component node
      const selectedNodeVariables = Array.isArray($selectedNode.data?.componentVariables) ? $selectedNode.data.componentVariables : [];
      
      // early return for no variables
      if (selectedNodeVariables.length === 0) {
        validationStatus.set({
          connected: {
            total: 0,
            connected: 0,
            items: {}
          },
          isValid: false,
          typeValid
        });
        return;
      }
      
      totalVariablesCount = selectedNodeVariables.length;

      // check each variable for connections
      selectedNodeVariables.forEach((variable: any) => {
        const handleId = `${$selectedNode.id}-${variable.target_variable}-handle`;
        const connectedEdges = currentEdges.filter(edge => {
          const isSourceMatch = edge.sourceHandle === handleId;
          const isTargetMatch = edge.targetHandle === handleId;
          return isSourceMatch || isTargetMatch;
        });

        let hasValidConnection = false;

        // check connection based on variable directionality from config
        if (variable.is_input && variable.is_output) {
          const bidirectionalEdges = connectedEdges.filter(edge => 
            edge.data?.leftDirection === true && edge.data?.rightDirection === true
          );
          
          // check if handle already has input connections
          if (bidirectionalEdges.length > 0) {
            hasValidConnection = true;
          } else {
            const inputConnections = connectedEdges.filter(edge => {
              const isIncomingNormal = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
              const isIncomingReverse = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
              const hasRightArrow = edge.data?.rightDirection === true && edge.data?.leftDirection === false;
              return (isIncomingNormal || isIncomingReverse) && hasRightArrow;
            });
            
            const outputConnections = connectedEdges.filter(edge => {
              const isOutgoingNormal = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
              const isOutgoingReverse = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
              const hasLeftArrow = edge.data?.leftDirection === true && edge.data?.rightDirection === false;
              return (isOutgoingNormal || isOutgoingReverse) && hasLeftArrow;
            });
            
            if (inputConnections.length > 0 && outputConnections.length > 0) {
              hasValidConnection = true;
            }
          }
          
        // check if input-only variable has valid input connection
        } else if (variable.is_input && !variable.is_output) {
          const inputConnections = connectedEdges.filter(edge => {
            const isIncomingNormal = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
            const isIncomingReverse = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
            const hasRightArrow = edge.data?.rightDirection === true;
            return (isIncomingNormal || isIncomingReverse) && hasRightArrow;
          });
          
          if (inputConnections.length > 0) {
            hasValidConnection = true;
          }
        
          // check if output-only variable has valid output connection
        } else if (!variable.is_input && variable.is_output) {
          const outputConnections = connectedEdges.filter(edge => {
            const isOutgoingNormal = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
            const isOutgoingReverse = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
            const hasLeftArrow = edge.data?.leftDirection === true;
            return (isOutgoingNormal || isOutgoingReverse) && hasLeftArrow;
          });
          
          if (outputConnections.length > 0) {
            hasValidConnection = true;
          }
        }
        
        allItems[`${$selectedNode.id}-${variable.target_variable}`] = hasValidConnection;
        if (hasValidConnection) {
          totalConnectedCount++;
        }
      });
      
    } else {
      // overview/view mode: validate all component nodes
      allNodes.forEach(node => {
        if (node.type === 'nodeWithItems' && node.data?.interactionMode === currentInteractionMode) {
          const nodeVariables = Array.isArray(node.data?.componentVariables) ? node.data.componentVariables : [];
          
          if (nodeVariables.length === 0) return;
          
          let nodeConnectedCount = 0;
          let nodeVariablesCount = nodeVariables.length;

          // check each variable for connections
          nodeVariables.forEach((variable: any) => {
            const handleId = `${node.id}-${variable.target_variable}-handle`;
            
            const connectedEdges = currentEdges.filter(edge => {
              const isSourceMatch = edge.sourceHandle === handleId;
              const isTargetMatch = edge.targetHandle === handleId;
              return isSourceMatch || isTargetMatch;
            });
            
            let hasValidConnection = false;
            
            if (variable.is_input && variable.is_output) {
              const bidirectionalEdges = connectedEdges.filter(edge => 
                edge.data?.leftDirection === true && edge.data?.rightDirection === true
              );
              
              // check if handle already has bidirectional connections
              if (bidirectionalEdges.length > 0) {
                hasValidConnection = true;
              } else {
                const inputConnections = connectedEdges.filter(edge => {
                  const isIncomingNormal = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
                  const isIncomingReverse = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
                  const hasRightArrow = edge.data?.rightDirection === true && edge.data?.leftDirection === false;
                  return (isIncomingNormal || isIncomingReverse) && hasRightArrow;
                });
                
                const outputConnections = connectedEdges.filter(edge => {
                  const isOutgoingNormal = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
                  const isOutgoingReverse = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
                  const hasLeftArrow = edge.data?.leftDirection === true && edge.data?.rightDirection === false;
                  return (isOutgoingNormal || isOutgoingReverse) && hasLeftArrow;
                });
                
                if (inputConnections.length > 0 && outputConnections.length > 0) {
                  hasValidConnection = true;
                }
              }
              
              // check if input-only variable has valid input connection
            } else if (variable.is_input && !variable.is_output) {
              const inputConnections = connectedEdges.filter(edge => {
                const isIncomingNormal = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
                const isIncomingReverse = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
                const hasRightArrow = edge.data?.rightDirection === true;
                return (isIncomingNormal || isIncomingReverse) && hasRightArrow;
              });
              
              if (inputConnections.length > 0) {
                hasValidConnection = true;
              }
              
              // check if output-only variable has valid output connection
            } else if (!variable.is_input && variable.is_output) {
              const outputConnections = connectedEdges.filter(edge => {
                const isOutgoingNormal = edge.sourceHandle === handleId && (edge.target.startsWith('schema-') || edge.target.startsWith('param-'));
                const isOutgoingReverse = edge.targetHandle === handleId && (edge.source.startsWith('schema-') || edge.source.startsWith('param-'));
                const hasLeftArrow = edge.data?.leftDirection === true;
                return (isOutgoingNormal || isOutgoingReverse) && hasLeftArrow;
              });
              
              if (outputConnections.length > 0) {
                hasValidConnection = true;
              }
            }
            
            allItems[`${node.id}-${variable.target_variable}`] = hasValidConnection;
            if (hasValidConnection) {
              nodeConnectedCount++;
            }
          });
          
          totalVariablesCount += nodeVariablesCount;
          totalConnectedCount += nodeConnectedCount;
        }
      });
    }

    // set validation status store
    validationStatus.set({
      connected: {
        total: totalVariablesCount,
        connected: totalConnectedCount,
        items: allItems
      },
      isValid: totalConnectedCount === totalVariablesCount,
      typeValid
    });
  }

  // trigger validation on edges change
  $: if ($edges) {
    validateConnections();
  }

  const nodeTypes: NodeTypes = {
    nodeWithItems: NodeWithItems as any,
    itemNode: ItemNode as any,
    sectionNode: SectionNode as any,
    leafNode: LeafNode as any,
    group: NodeWithItems as any
  };

  const edgeTypes: EdgeTypes = {
    button: ButtonEdge as any
  };
</script>

<style>
  .app-container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    width: 100%;
  }
  
  .top-bar {
    height: 60px;
    background: #ffffff;
    border-bottom: 2px solid #007acc;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 2rem;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    z-index: 1000;
  }
  
  .project-name {
    font-size: 1.2rem;
    font-weight: bold;
    color: #333;
  }
  
  .mode-controls {
    display: flex;
    gap: 0.5rem;
  }
  
  .mode-button {
    padding: 0.5rem 1.5rem;
    border: 2px solid #007acc;
    background: #ffffff;
    color: #007acc;
    border-radius: 6px;
    cursor: pointer;
    font-weight: bold;
    transition: all 0.2s;
  }
  
  .mode-button.active {
    background: #007acc;
    color: white;
  }
  
  .mode-button:hover:not(.active) {
    background: #f0f8ff;
  }
  
  .layout {
    display: flex;
    flex: 1;
    height: calc(100vh - 60px);
    width: 100%;
  }
  
  .flow-wrapper {
    flex: 1;
    position: relative;
  }
  
  .sidebar {
    width: 350px;
    background: #f5f5f5;
    border-left: 1px solid #ddd;
    display: flex;
    flex-direction: column;
  }
  
  .sidebar.empty {
    background: #fafafa;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #999;
    font-style: italic;
  }
  
  .tabs {
    display: flex;
    border-bottom: 1px solid #ddd;
  }
  
  .tab {
    flex: 1;
    padding: 0.5rem;
    background: #e0e0e0;
    border: none;
    cursor: pointer;
    font-size: 0.9rem;
    transition: background-color 0.2s;
  }
  
  .tab.active {
    background: #f5f5f5;
    border-bottom: 2px solid #007acc;
  }
  
  .tab:hover:not(.active) {
    background: #d5d5d5;
  }
  
  .tab-content {
    flex: 1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }

  /* save/cancel buttons styles */
  .edit-actions {
    display: flex;
    gap: 0.75rem;
    padding: 1rem;
    border-top: 1px solid #ddd;
    background: #f8f9fa;
    margin-top: auto;
  }
  
  .save-edit-button,
  .cancel-edit-button {
    flex: 1;
    padding: 0.75rem 1rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    font-weight: bold;
    transition: all 0.2s;
  }
  
  .save-edit-button {
    background: #28a745;
    color: white;
  }
  
  .save-edit-button:hover {
    background: #218838;
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(40, 167, 69, 0.3);
  }
  
  .cancel-edit-button {
    background: #6c757d;
    color: white;
  }
  
  .cancel-edit-button:hover {
    background: #5a6268;
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(108, 117, 125, 0.3);
  }

  :global(.svelte-flow__edges) {
    z-index: 10;
  }

  :global(.svelte-flow__nodes) {
    z-index: 5;
  }

  :global(.svelte-flow__edge) {
    pointer-events: all !important;
    cursor: pointer;
  }

  :global(.svelte-flow__edge path) {
    pointer-events: stroke !important;
    stroke-width: 2px !important;
  }

  :global(.svelte-flow__edge.selected path) {
    stroke: #ff6b35 !important;
    stroke-width: 3px !important;
  }

  :global(.svelte-flow__edge-label) {
    z-index: 9999 !important;
    pointer-events: all !important;
  }

  :global(.svelte-flow__handle) {
    z-index: 20 !important;
    width: 8px !important;
    height: 8px !important;
    background: #007acc !important;
    border: 2px solid white !important;
  }

  :global(.svelte-flow__handle:hover) {
    background: #005fa3 !important;
  }
  
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10000;
  }
  
  .modal {
    background: white;
    border-radius: 8px;
    padding: 2rem;
    max-width: 500px;
    width: 90%;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
  }
  
  .modal h3 {
    margin: 0 0 1rem 0;
    color: #333;
  }
  
  .modal p {
    margin: 0 0 1.5rem 0;
    color: #666;
    line-height: 1.5;
  }
  
  .modal-buttons {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
  }
  
  .cancel-button {
    padding: 0.75rem 1.5rem;
    background: #6c757d;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.2s;
  }
  
  .cancel-button:hover {
    background: #5a6268;
  }

  .confirm-button {
    padding: 0.75rem 1.5rem;
    background: #dc3545;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.2s;
  }
  
  .confirm-button:hover {
    background: #c82333;
  }
</style>

<div class="app-container">
  <div class="top-bar">
    <div class="project-name">
      Data Configuration Project
    </div>
    
    <div class="mode-controls">
      <button 
        class="mode-button" 
        class:active={currentInteractionMode === 'edit'}
        on:click={() => handleInteractionModeToggle('edit')}
      >
        EDIT
      </button>
      <button 
        class="mode-button" 
        class:active={currentInteractionMode === 'view'}
        on:click={() => handleInteractionModeToggle('view')}
      >
        VIEW
      </button>
    </div>
  </div>

  <div class="layout">
    <div class="flow-wrapper">
      <SvelteFlowProvider>
        <SvelteFlow
          {nodes}
          {edges}
          {nodeTypes}
          {edgeTypes}
          on:nodeclick={handleNodeClick}
          on:edgeclick={handleEdgeClick}
          on:paneclick={handlePaneClick}
          on:connect={onConnect}
          {isValidConnection}
          defaultEdgeOptions={{type: 'button'}}
          fitView
          selectionMode={SelectionMode.Partial}
          connectionLineType={ConnectionLineType.SmoothStep}
        >
          <Background />
          <Controls />
        </SvelteFlow>
        <ResetViewButton />
      </SvelteFlowProvider>
    </div>

    <div class="sidebar" class:empty={sidebarMode === 'empty'}>
      {#if sidebarMode === 'empty'}
        <ComponentLibrary 
          {currentInteractionMode}
          componentConfig={getCurrentConfig()}
          {componentManifest}
          onAddComponent={handleAddComponent}
          onSaveMappings={handleSaveMappingsOnly}
          onSave={handleSaveEdit}
        />
      {:else if sidebarMode === 'overview'}
        <ComponentOverview 
          selectedNode={$selectedNode} 
          onEdit={handleEdit}
          onSave={handleSaveEdit}
          onDelete={handleDeleteComponent}
        />
      {:else if sidebarMode === 'edit'}
        <div class="tabs">
          <button class="tab" class:active={activeTab === 0} on:click={() => activeTab = 0}>
            Modes
          </button>
          <button class="tab" class:active={activeTab === 1} on:click={() => activeTab = 1}>
            Config
          </button>
          <button class="tab" class:active={activeTab === 2} on:click={() => activeTab = 2}>
            Preview
          </button>
        </div>
        
        <div class="tab-content">
          {#if activeTab === 0}
            <ModesTab 
              {componentManifest}
              selectedMode={currentNodeMode}
              {currentInteractionMode}
              selectedNode={$selectedNode}
              onModeChange={handleModeChange}
            />
          {:else if activeTab === 1}
            <ConfigTab 
              componentConfig={getCurrentConfig()}
              {validationStatus}
              {edges}
              {nodes}
              selectedMode={currentNodeMode}
              {componentManifest}
              selectedNode={$selectedNode}
              onSetAnchorpoint={handleSetAnchorpoint}
              onConfigChange={handleConfigChange}
            />
          {:else if activeTab === 2}
            <PreviewTab 
              {componentManifest}
              selectedNode={$selectedNode}
              selectedMode={effectivePreviewMode}
              {currentInteractionMode}
              componentConfig={getCurrentConfig()}
            />
          {/if}
        </div>
        
        <!-- save/cancel buttons -->
        <div class="edit-actions">
          <button class="cancel-edit-button" on:click={handleCancelEdit}>
            Cancel
          </button>
          <button class="save-edit-button" on:click={handleSaveEdit}>
            Save
          </button>
        </div>
      {/if}
    </div>
  </div>
</div>

{#if showModeChangeWarning}
  <div class="modal-overlay">
    <div class="modal">
      <h3>Switch Interaction Mode</h3>
      <p>You are currently editing a component. Switching to {pendingInteractionMode.toUpperCase()} mode will close the editor and any unsaved changes will be lost.</p>
      <p>Do you want to continue?</p>
      <div class="modal-buttons">
        <button class="cancel-button" on:click={cancelModeChange}>Cancel</button>
        <button class="confirm-button" on:click={confirmModeChange}>Yes, Switch Mode</button>
      </div>
    </div>
  </div>
{/if}

<!-- cancel confirmation dialog -->
{#if showCancelWarning}
  <div class="modal-overlay">
    <div class="modal">
      <h3>Cancel Changes</h3>
      <p>Are you sure? All unsaved changes will be lost.</p>
      <div class="modal-buttons">
        <button class="cancel-button" on:click={rejectCancel}>No, Keep Editing</button>
        <button class="confirm-button" on:click={confirmCancel}>Yes, Cancel</button>
      </div>
    </div>
  </div>
{/if}

<!-- delete confirmation dialog -->
{#if showDeleteWarning}
  <div class="modal-overlay">
    <div class="modal">
      <h3>Delete Component</h3>
      <p>Are you sure you want to delete "{nodeToDelete?.data.componentName}"?</p>
      <p>This can remove all connections and cannot be undone.</p>
      <div class="modal-buttons">
        <button class="cancel-button" on:click={rejectDelete}>Cancel</button>
        <button class="confirm-button" on:click={confirmDelete}>Yes, Delete</button>
      </div>
    </div>
  </div>
{/if}

<!-- schema tree component for node generation -->
<TreeComponent on:nodesGenerated={handleSchemaNodesGenerated} />