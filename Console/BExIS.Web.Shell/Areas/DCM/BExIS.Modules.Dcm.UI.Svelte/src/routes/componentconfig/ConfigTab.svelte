<script lang="ts">
  import { writable, get } from 'svelte/store';

  export let componentConfig: any;
  export let validationStatus: any;
  export let edges: any;
  export let selectedMode: any;
  export let componentManifest: any;
  export let selectedNode: any;
  export let nodes: any;
  export let onSetAnchorpoint: ((componentId: string, jsonPath: string) => void) | undefined = undefined;
  export let onConfigChange: ((config: any) => void) | undefined = undefined;

  let hasLoop = false;
  let loopDetails: Array<{path: string[]}> = [];
  let selectedParameter = '';
  let regexInputValue = '';
  let regexOutputValue = '';
  
  let showResetConfirm = false;

  $: connectedLeafNodes = getConnectedLeafNodes(get(edges), get(nodes), selectedNode?.id || '');
  $: selectedComponentNode = selectedNode ? (Array.isArray(get(nodes)) ? (get(nodes) as any[]).find((n: any) => n.id === selectedNode.id) : null) : null;
  $: currentAnchorpoint = (selectedComponentNode?.data?.anchorpoint ?? selectedNode?.data?.anchorpoint ?? '') || '';

  $: currentComponent = findCurrentComponent();

  $: manifestMode = getManifestMode();
  $: manifestSettings = manifestMode?.settings?.setting || [];
  $: manifestGlobalSettings = componentManifest?.globalSettings?.globalsetting || [];

  $: configSettings = currentComponent?.mode?.settings?.setting || [];
  $: configGlobalSettings = currentComponent?.globalSettings?.globalsetting || [];

  $: displaySettings = mergeSettingsWithConfig(manifestSettings, configSettings);
  $: displayGlobalSettings = mergeSettingsWithConfig(manifestGlobalSettings, configGlobalSettings);

  // get all available variables for the selected nodes component submode
  $: availableVariables = (selectedNode?.data?.componentVariables || []).map((v: any) => {
    const manifestVar = manifestMode?.variables?.variable?.find((mv: any) => 
      mv.target_variable === v.target_variable
    );
    
    return {
      ...v,
      allowregex_input: manifestVar?.allowregex_input || false,
      allowregex_output: manifestVar?.allowregex_output || false
    };
  });

  // filter variables that support regex input or output
  $: regexCapableVariables = availableVariables.filter((v: any) => 
    v.allowregex_input || v.allowregex_output
  );

  // currently selected variable with details
  $: selectedVariableDetails = selectedParameter ? availableVariables.find((v: any) => v.target_variable === selectedParameter) : null;

  // load input/ output regex reactively when variable changes
  $: if (selectedParameter && currentComponent) {
    loadRegexForSelectedVariable();
  }

  function loadRegexForSelectedVariable() {
    if (!currentComponent || !selectedParameter) {
      regexInputValue = '';
      regexOutputValue = '';
      return;
    }

    // find variable in current component config
    const variables = currentComponent.mode?.variables?.variable || [];
    const variable = variables.find((v: any) => v.target_variable === selectedParameter);

    if (variable) {
      regexInputValue = variable.input_regex || '';
      regexOutputValue = variable.output_regex || '';
    } else {
      regexInputValue = '';
      regexOutputValue = '';
    }
  }

  function findCurrentComponent() {
    if (!selectedNode || !componentConfig?.components) return null;
    const nodeId = selectedNode.id;
    
    // find component by ui_id
    return componentConfig.components.find((c: any) => 
      c.meta.component_ui_id === nodeId
    ) || null;
  }

  // filters edges to find connected leaf nodes for a given component id
  export function getConnectedLeafNodes($edges: any[], $nodes: any[], componentId: string) {
    const result: Array<{ id: string; label: string; path: string; jsonPath: string }> = [];
    if (!$edges || !$nodes || !componentId) return result;

    // iterate through edges to find those connected to the component
    $edges.forEach((edge: any) => {
      const isConnected = edge.source === componentId || edge.target === componentId;
      if (!isConnected) return;

      const leafId =
        edge.target?.startsWith('schema-leaf-') ? edge.target :
        edge.source?.startsWith('schema-leaf-') ? edge.source : null;
      if (!leafId) return;

      // find the leaf node object
      const leaf = $nodes.find((n: any) => n.id === leafId && n.type === 'leafNode');
      if (!leaf?.data?.path) return;

      // build jsonpath format and add to result
      const jsonPath = leaf.data.path;
      if (!result.find(r => r.jsonPath === jsonPath)) {
        result.push({
          id: leaf.id,
          label: leaf.data.label || leaf.data.path,
          path: leaf.data.path,
          jsonPath
        });
      }
    });

    return result;
  }

  // set new anchorpoint for the selected node
  export function handleAnchorpointChange(e: Event, selectedNode: any, onSetAnchorpoint?: (id: string, jp: string) => void) {
    const target = e.target as HTMLSelectElement;
    const newJsonPath = target?.value || '';
    if (!selectedNode?.id || !onSetAnchorpoint) return;
    onSetAnchorpoint(selectedNode.id, newJsonPath);
  }

  function getManifestMode() {
    if (!selectedMode) {
      // console.warn('No selectedMode available');
      return null;
    }
    
    const interactionMode = selectedNode?.data?.interactionMode || 'edit';
    const manifestModes = componentManifest?.modes?.[interactionMode] || [];
    
    const found = manifestModes.find((mode: any) => 
      mode.mode_name === selectedMode.mode_name
    );
    
    return found || selectedMode;
  }

  // merge manifest settings with config settings for correct setting values
  function mergeSettingsWithConfig(manifestSettings: any[], configSettings: any[]) {

    return manifestSettings.map((manifestSetting: any) => {
      const configSetting = configSettings.find(
        (cs: any) => cs.target_variable === manifestSetting.target_variable
      );
      
      const merged = {
        name: manifestSetting.name,
        target_variable: manifestSetting.target_variable,
        type: manifestSetting.type,
        description: manifestSetting.description,
        default_value: manifestSetting.default_value,
        value: configSetting?.value ?? manifestSetting.default_value?.value ?? ''
      };

      return merged;
    });
  }

  // store initial state for reset functionality
  let initialConfigState: any = null;

  // initialize initial state on first load
  $: if (componentConfig && !initialConfigState) {
    initialConfigState = JSON.parse(JSON.stringify(componentConfig));
  }

  $: allVariables = selectedNode?.data?.componentVariables || [];
  $: allSettings = selectedMode?.settings?.setting || [];
  $: globalSettings = componentManifest?.globalSettings?.globalsetting || [];

  // create default values lookup from manifest
  $: settingDefaults = createSettingDefaults();
  $: globalDefaults = createGlobalDefaults();

  function createSettingDefaults() {
    const defaults = {};
    if (selectedMode?.settings?.setting) {
      selectedMode.settings.setting.forEach(setting => {
        if (setting.default_value) {
          defaults[setting.target_variable] = setting.default_value.value;
        }
      });
    }
    return defaults;
  }

  function createGlobalDefaults() {
    const defaults = {};
    if (componentManifest?.globalSettings?.globalsetting) {
      componentManifest.globalSettings.globalsetting.forEach(setting => {
        if (setting.default_value) {
          defaults[setting.target_variable] = setting.default_value.value;
        }
      });
    }
    return defaults;
  }

  // initialize settings with default values if not set
  $: {
    if (configSettings.length > 0) {
      configSettings.forEach(setting => {
        if (setting.value === undefined && settingDefaults[setting.target_variable]) {
          setting.value = settingDefaults[setting.target_variable];
        }
      });
    }
  }

  // apply regex validation to selected variable
  function applyValidation() {
    if (!selectedParameter) {
      alert('Please select a variable first!');
      return;
    }

    if (!selectedVariableDetails) {
      alert('Variable details not found!');
      return;
    }

    const hasInputRegex = selectedVariableDetails.allowregex_input && regexInputValue.trim();
    const hasOutputRegex = selectedVariableDetails.allowregex_output && regexOutputValue.trim();

    if (!hasInputRegex && !hasOutputRegex) {
      alert('Please enter at least one regex pattern!');
      return;
    }

    if (!currentComponent) {
      ensureComponentInConfig();
    }

    const comp = findCurrentComponent();
    if (!comp) {
      return;
    }

    // ensure variables array exists
    if (!comp.mode.variables) {
      comp.mode.variables = { variable: [] };
    }

    const variablesArray = comp.mode.variables.variable;
    let variableIndex = variablesArray.findIndex((v: any) => 
      v.target_variable === selectedParameter
    );

    // ensure variable exists, create from manifest if not
    if (variableIndex < 0) { // -1 findIndex means not found

      const manifestVariable = manifestMode?.variables?.variable?.find((mv: any) => 
        mv.target_variable === selectedParameter
      );
      
      if (manifestVariable) {
        // create new variable with manifest data
        const newVariable: any = {
          target_variable: selectedParameter,
          is_input: manifestVariable.is_input || false,
          is_output: manifestVariable.is_output || false,
          type: manifestVariable.type || 'string',
          JSONPath: '',
          is_visible: true
        };
        
        variablesArray.push(newVariable);
        variableIndex = variablesArray.length - 1;
      } else {
        // alert(`Error: Variable ${selectedParameter} not found in manifest!`);
        return;
      }
    }

    // set regex patterns
    if (hasInputRegex) {
      variablesArray[variableIndex].input_regex = regexInputValue.trim();
    }
    if (hasOutputRegex) {
      variablesArray[variableIndex].output_regex = regexOutputValue.trim();
    }

    componentConfig = { ...componentConfig };
    if (onConfigChange) {
      onConfigChange(componentConfig);
    }
  }

  // handle changes to boolean settings
  function handleBooleanChange(setting, event) {
    const newValue = event.target.checked;
    setting.value = newValue;
    
    if (!currentComponent && selectedNode) {
      ensureComponentInConfig();
    }
    
    const comp = findCurrentComponent();
    if (comp) {
      if (!comp.mode.settings) {
        comp.mode.settings = { setting: [] };
      }
      
      const settingsArray = comp.mode.settings.setting;
      const settingIndex = settingsArray.findIndex(s => s.target_variable === setting.target_variable);
      
      // update or add setting 
      if (settingIndex >= 0) {
        settingsArray[settingIndex].value = String(newValue);
      } else {
        settingsArray.push({
          target_variable: setting.target_variable,
          value: String(newValue)
        });
      }

      componentConfig = { ...componentConfig };
      if (onConfigChange) {
        onConfigChange(componentConfig);
      }
    }
  }

  // handle changes to input string settings
  function handleInputChange(setting, event) {
    const newValue = event.target.value;
    setting.value = newValue;
    
    if (!currentComponent && selectedNode) {
      ensureComponentInConfig();
    }
    
    const comp = findCurrentComponent();
    if (comp) {
      if (!comp.mode.settings) {
        comp.mode.settings = { setting: [] };
      }
      
      const settingsArray = comp.mode.settings.setting;
      const settingIndex = settingsArray.findIndex(s => s.target_variable === setting.target_variable);
      
      // update or add setting
      if (settingIndex >= 0) {
        settingsArray[settingIndex].value = newValue;
      } else {
        settingsArray.push({
          target_variable: setting.target_variable,
          value: newValue
        });
      }
      
      componentConfig = { ...componentConfig };
      if (onConfigChange) {
        onConfigChange(componentConfig);
      }
    }
  }

  // handle changes to global boolean settings
  function handleGlobalBooleanChange(setting, event) {
    const newValue = event.target.checked;
    setting.value = newValue;
    
    if (!currentComponent && selectedNode) {
      ensureComponentInConfig();
    }
    
    const comp = findCurrentComponent();
    if (comp) {
      if (!comp.globalSettings.globalsetting) {
        comp.globalSettings.globalsetting = [];
      }
      
      const globalSettingsArray = comp.globalSettings.globalsetting;
      const settingIndex = globalSettingsArray.findIndex(s => s.target_variable === setting.target_variable);
      
      if (settingIndex >= 0) {
        globalSettingsArray[settingIndex].value = String(newValue);
      } else {
        globalSettingsArray.push({
          target_variable: setting.target_variable,
          value: String(newValue)
        });
      }
      
      componentConfig = { ...componentConfig };
      if (onConfigChange) {
        onConfigChange(componentConfig);
      }
    }
  }

  // handle changes to global input string settings
  function handleGlobalInputChange(setting, event) {
    const newValue = event.target.value;
    setting.value = newValue;
    
    if (!currentComponent && selectedNode) {
      ensureComponentInConfig();
    }
    
    const comp = findCurrentComponent();
    if (comp) {
      if (!comp.globalSettings.globalsetting) {
        comp.globalSettings.globalsetting = [];
      }
      
      const globalSettingsArray = comp.globalSettings.globalsetting;
      const settingIndex = globalSettingsArray.findIndex(s => s.target_variable === setting.target_variable);
      
      if (settingIndex >= 0) {
        globalSettingsArray[settingIndex].value = newValue;
      } else {
        globalSettingsArray.push({
          target_variable: setting.target_variable,
          value: newValue
        });
      }
      
      componentConfig = { ...componentConfig };
      if (onConfigChange) {
        onConfigChange(componentConfig);
      }
    }
  }

  // check and add component to config if not present via component id
  function ensureComponentInConfig() {
    if (!selectedNode || !componentManifest) return;
    
    const nodeId = selectedNode.id;
    const modeName = selectedNode.data?.modeName;
    const interactionMode = selectedNode.data?.interactionMode || 'edit';
    
    const exists = componentConfig.components.find((c: any) => 
      c.meta.component_ui_id === nodeId
    );
    
    if (exists) {
      return;
    }
    
    const manifestModes = componentManifest?.modes?.[interactionMode] || [];
    const manifestMode = manifestModes.find((m: any) => m.mode_name === modeName);
    
    if (!manifestMode) {
      return;
    }
    
    const newComponent = {
      meta: {
        component_name: componentManifest.meta.component_name,
        component_ui_id: nodeId
      },
      globalSettings: {
        interaction_mode: interactionMode,
        anchorpoint: selectedNode.data?.anchorpoint || '',
        globalsetting: (componentManifest?.globalSettings?.globalsetting || []).map((gs: any) => ({
          target_variable: gs.target_variable,
          value: gs.default_value?.value || ''
        }))
      },
      mode: {
        mode_name: modeName,
        settings: {
          setting: (manifestMode.settings?.setting || []).map((s: any) => ({
            target_variable: s.target_variable,
            value: s.default_value?.value || ''
          }))
        },
        variables: {
          variable: []
        }
      }
    };
    
    componentConfig.components.push(newComponent);
    componentConfig = { ...componentConfig };

    if (onConfigChange) {
      onConfigChange(componentConfig);
    }
  }

  function resetToDefaults() {
    showResetConfirm = true;
  }

  function confirmReset() {
    if (!currentComponent || !manifestMode) {
      showResetConfirm = false;
      return;
    }
    
    //reset mode settings to manifest defaults
    const manifestModeSettings = manifestMode?.settings?.setting || [];
    currentComponent.mode.settings.setting = manifestModeSettings.map((ms: any) => ({
      target_variable: ms.target_variable,
      value: ms.default_value?.value ?? ''
    }));
    
    // reset global settings to manifest defaults
    const manifestGlobalSettings = componentManifest?.globalSettings?.globalsetting || [];
    currentComponent.globalSettings.globalsetting = manifestGlobalSettings.map((mgs: any) => ({
      target_variable: mgs.target_variable,
      value: mgs.default_value?.value ?? ''
    }));
    
    componentConfig = { ...componentConfig };
    
    // clear regex form fields
    selectedParameter = '';
    regexInputValue = '';
    regexOutputValue = '';
    
    showResetConfirm = false;
    
    alert('Settings reset to default values!');
  }

  function cancelReset() {
    showResetConfirm = false;
  }

  // get setting type from manifest (NOT USED)
  function getSettingType(targetVariable) {
    const manifestSetting = selectedMode?.settings?.setting?.find(s => s.target_variable === targetVariable);
    return manifestSetting?.type || 'string';
  }

  // setting default from manifest (NOT USED)
  function getSettingDefaultValue(targetVariable) {
    const manifestSetting = selectedMode?.settings?.setting?.find(s => s.target_variable === targetVariable);
    if (manifestSetting?.default_value) {
      if (manifestSetting.type === 'boolean') {
        return manifestSetting.default_value.value === 'true';
      }
      return manifestSetting.default_value.value;
    }
    return manifestSetting?.type === 'boolean' ? false : '';
  }

  // get validation status for selected node
  $: nodeValidationStatus = getNodeValidationStatus();

  function getNodeValidationStatus() {
    if (!selectedNode || !$validationStatus.connected.items) {
      return { connected: 0, total: 0, items: {} };
    }

    const nodeId = selectedNode.id;
    const nodeVariables = Array.isArray(selectedNode.data?.componentVariables) ? selectedNode.data.componentVariables : [];
    
    if (nodeVariables.length === 0) {
      return { connected: 0, total: 0, items: {} };
    }
    
    let connectedCount = 0;
    let nodeItems = {};
    
    // check validation status for each variable
    nodeVariables.forEach((variable: any) => {
      const itemKey = `${nodeId}-${variable.target_variable}`;
      const isConnected = $validationStatus.connected.items[itemKey] || false;
      nodeItems[variable.target_variable] = isConnected;
      if (isConnected) {
        connectedCount++;
      }
    });
    
    return {
      connected: connectedCount,
      total: nodeVariables.length,
      items: nodeItems
    };
  }

  // check edges for loops and dependencies
  function checkForLoops() {
    if (!selectedNode || !$edges) {
      hasLoop = false;
      loopDetails = [];
      return;
    }
    const nodeId = selectedNode.id;
    const detectedLoops: Array<{path: string[]}> = [];
    const seenLoops = new Set<string>();

    const componentEdges = $edges.filter(edge => 
      edge.sourceHandle?.startsWith(nodeId + '-') || 
      edge.targetHandle?.startsWith(nodeId + '-')
    );

    if (componentEdges.length === 0) {
      hasLoop = false;
      loopDetails = [];
      return;
    }

    // recursive function to find all paths from a starting edge
    function findAllPaths(startEdge: any, currentPath: string[], visited: Set<string>): string[][] {
      const paths: string[][] = [];
      
      // get current edge end point
      let currentNode: string;
      let currentHandle: string;
      
      if (startEdge.target?.startsWith('schema-') || startEdge.target?.startsWith('param-')) {
        currentNode = startEdge.target;
        currentHandle = startEdge.targetHandle || '';
      } else {
        currentNode = startEdge.source;
        currentHandle = startEdge.sourceHandle || '';
      }
      
      // find all edges continuing from this node
      const continuingEdges = $edges.filter(edge => {
        if (visited.has(edge.id)) return false;
        
        const isFromSchema = (edge.source === currentNode || edge.sourceHandle === currentHandle);
        const isToSchema = (edge.target === currentNode || edge.targetHandle === currentHandle);
        
        return (isFromSchema || isToSchema) && edge.id !== startEdge.id;
      });
      
      // if no continuing edges check for loop
      if (continuingEdges.length === 0) {
        if (currentHandle?.startsWith(nodeId + '-')) {
          const finalHandle = currentHandle.replace(nodeId + '-', '').replace('-handle', '');
          paths.push([...currentPath, finalHandle]);
        }
        return paths;
      }
      
      // follow each continuing edge
      continuingEdges.forEach(edge => {
        const newVisited = new Set(visited);
        newVisited.add(edge.id);
        
        let nextPath = [...currentPath];
        
        // add intermediate node to path
        if (edge.target?.startsWith('schema-') || edge.target?.startsWith('param-')) {
          const schemaName = edge.target.replace('schema-leaf-', '').replace('schema-', '').replace('param-', '');
          nextPath.push(schemaName);
        } else if (edge.source?.startsWith('schema-') || edge.source?.startsWith('param-')) {
          const schemaName = edge.source.replace('schema-leaf-', '').replace('schema-', '').replace('param-', '');
          nextPath.push(schemaName);
        }
        
        // check if return to component
        if (edge.sourceHandle?.startsWith(nodeId + '-') || edge.targetHandle?.startsWith(nodeId + '-')) {
          const returnHandle = (edge.sourceHandle?.startsWith(nodeId + '-') ? edge.sourceHandle : edge.targetHandle)
            .replace(nodeId + '-', '').replace('-handle', '');
          nextPath.push(returnHandle);
          paths.push(nextPath);
        } else {
          // continue recursively
          const subPaths = findAllPaths(edge, nextPath, newVisited);
          paths.push(...subPaths);
        }
      });
      
      return paths;
    }

    // check output edges for feedback loops
    componentEdges.forEach(outEdge => {
      let componentHandle: string | null = null;
      let schemaNode: string | null = null;

      if (outEdge.sourceHandle?.startsWith(nodeId + '-')) {
        componentHandle = outEdge.sourceHandle;
        schemaNode = outEdge.target;
      } else if (outEdge.targetHandle?.startsWith(nodeId + '-')) {
        componentHandle = outEdge.targetHandle;
        schemaNode = outEdge.source;
      }

      if (!componentHandle || !schemaNode) return;

      const handle = componentHandle.replace(nodeId + '-', '').replace('-handle', '');
      const schema = schemaNode.replace('schema-leaf-', '').replace('schema-', '').replace('param-', '');
      
      // start path with component handle and schema node
      const startPath = [handle, schema];
      const visited = new Set([outEdge.id]);
      
      const allPaths = findAllPaths(outEdge, startPath, visited);
      
      // analyze found paths for loops
      allPaths.forEach(path => {
        if (path.length >= 3) {
          const sortedPath = [...path].sort();
          const loopKey = sortedPath.join('-');
          
          if (!seenLoops.has(loopKey)) {
            seenLoops.add(loopKey);
            detectedLoops.push({ path });
          }
        }
      });
    });

    // map containing counts of outputs
    const schemaOutputCounts = new Map<string, string[]>();
    
    // check connection to schema nodes for each edge
    $edges.forEach(edge => {
      const targetIsSchema = edge.target?.startsWith('schema-') || edge.target?.startsWith('param-');
      const sourceIsSchema = edge.source?.startsWith('schema-') || edge.source?.startsWith('param-');
      
      if (edge.data?.leftDirection) {
        if (targetIsSchema) {
          const schemaNode = edge.target;
          let sourceHandle = '';
          
          // extract variable name from source (component id as fallback)
          if (edge.sourceHandle?.startsWith(nodeId + '-')) {
            sourceHandle = edge.sourceHandle.replace(nodeId + '-', '').replace('-handle', '');
          } else {
            const sourceNode = edge.source;
            sourceHandle = sourceNode.replace('config-component-', 'Comp').replace('library-component-', 'Comp');
          }
          
          // add source handle to schema output counts
          if (!schemaOutputCounts.has(schemaNode)) {
            schemaOutputCounts.set(schemaNode, []);
          }
          schemaOutputCounts.get(schemaNode)!.push(sourceHandle);
        } else if (sourceIsSchema) {
          const schemaNode = edge.source;
          let targetHandle = '';
          
          // extract variable name from target (component id as fallback)
          if (edge.targetHandle?.startsWith(nodeId + '-')) {
            targetHandle = edge.targetHandle.replace(nodeId + '-', '').replace('-handle', '');
          } else {
            const targetNode = edge.target;
            targetHandle = targetNode.replace('config-component-', 'Comp').replace('library-component-', 'Comp');
          }
          
          // add target handle to schema output counts
          if (!schemaOutputCounts.has(schemaNode)) {
            schemaOutputCounts.set(schemaNode, []);
          }
          schemaOutputCounts.get(schemaNode)!.push(targetHandle);
        }
      }
    });

    // check for multiple outputs to one Leaf Node
    schemaOutputCounts.forEach((handles, schemaNode) => {
      if (handles.length > 1) {
        const schemaName = schemaNode.replace('schema-leaf-', '').replace('schema-', '').replace('param-', '');
        
        handles.forEach(handle => {
          const warningKey = `multiple-output-${schemaNode}-${handle}`;
          
          if (!seenLoops.has(warningKey)) {
            seenLoops.add(warningKey);
            detectedLoops.push({
              path: [handle, schemaName, `Connection Error`]
            });
          }
        });
      }
    });

    hasLoop = detectedLoops.length > 0;
    loopDetails = detectedLoops;
  }

  $: if ($edges || selectedNode) {
    checkForLoops();
  }
</script>

<div class="config-tab">
  <h4>Configuration</h4>

  <!-- anchorpoint selection -->
  {#if connectedLeafNodes.length > 0}
    <div class="section">
      <div class="anchorpoint-selector">
        <label for="anchorpoint-select">
          Component Placement:
        </label>
        <select 
          id="anchorpoint-select" 
          value={currentAnchorpoint}
          on:change={(e) => handleAnchorpointChange(e, selectedNode, onSetAnchorpoint)}
        >
          <option value="">-- Select Anchorpoint --</option>
          {#each connectedLeafNodes as leaf}
            <option value={leaf.jsonPath}>
              {leaf.label} ({leaf.path})
            </option>
          {/each}
        </select>
        <div class="anchorpoint-info">
          The anchorpoint determines where the component will be rendered in the final form.
          
        </div>
      </div>
    </div>
  {/if}

  <!-- validation summary -->
  <div class="section">
    <h5>Validation Summary</h5>
    <div class="validation-summary" class:valid={nodeValidationStatus.connected === nodeValidationStatus.total && nodeValidationStatus.total > 0}>
      <div class="summary-stats">
        <span class="stat">Connected: {nodeValidationStatus.connected}/{nodeValidationStatus.total}</span>
        <span class="status-badge" class:valid={nodeValidationStatus.connected === nodeValidationStatus.total && nodeValidationStatus.total > 0}>
          {nodeValidationStatus.connected === nodeValidationStatus.total && nodeValidationStatus.total > 0 ? 'All Connected' : 'Missing Connections'}
        </span>
      </div>
    </div>
  </div>

  <!-- reset button -->
  <div class="section">
    <button class="reset-button" on:click={resetToDefaults}>
      Back to Default
    </button>
  </div>

  <!-- global settings -->
  {#if displayGlobalSettings.length > 0}
    <div class="section">
      <h5>Global Settings</h5>
      <div class="settings-list">
        {#each displayGlobalSettings as setting}
          <div class="setting-item" class:boolean={setting.type === 'boolean'}>
            {#if setting.type === 'boolean'}
              <input 
                type="checkbox" 
                id={`global-${setting.target_variable}`}
                checked={setting.value === true || setting.value === 'true'}
                on:change={(e) => handleGlobalBooleanChange(setting, e)}
              />
              <label for={`global-${setting.target_variable}`} class="setting-name">
                {setting.name || setting.target_variable}
              </label>
              <div class="default-info">
                Default: {setting.default_value?.value ?? 'false'}
              </div>
            {:else}
              <label class="setting-name" for={`global-text-${setting.target_variable}`}>{setting.name || setting.target_variable}:</label>
              <input 
                id={`global-text-${setting.target_variable}`}
                type="text" 
                value={setting.value || ''} 
                on:input={(e) => handleGlobalInputChange(setting, e)}
                placeholder={`Default: ${setting.default_value?.value || 'No default'}`}
              />
              <div class="default-info">
                Default: {setting.default_value?.value || 'No default'}
              </div>
            {/if}
          </div>
        {/each}
      </div>
    </div>
  {/if}

  <!-- settings -->
  {#if displaySettings.length > 0}
    <div class="section">
      <h5>Settings {manifestMode ? `(${manifestMode.mode_name})` : ''}</h5>
      <div class="settings-list">
        {#each displaySettings as setting}
          <div class="setting-item" class:boolean={setting.type === 'boolean'}>
            {#if setting.type === 'boolean'}
              <input 
                type="checkbox" 
                id={`setting-${setting.target_variable}`}
                checked={setting.value === true || setting.value === 'true'}
                on:change={(e) => handleBooleanChange(setting, e)}
              />
              <label for={`setting-${setting.target_variable}`} class="setting-name">
                {setting.name || setting.target_variable}
              </label>
              <div class="default-info">
                Default: {setting.default_value?.value ?? 'false'}
              </div>
            {:else}
              <label class="setting-name" for={`setting-text-${setting.target_variable}`}>{setting.name || setting.target_variable}:</label>
              <input 
                id={`setting-text-${setting.target_variable}`}
                type="text" 
                value={setting.value || ''} 
                on:input={(e) => handleInputChange(setting, e)}
                placeholder={`Default: ${setting.default_value?.value || 'No default'}`}
              />
              <div class="default-info">
                Default: {setting.default_value?.value || 'No default'}
              </div>
            {/if}
          </div>
        {/each}
      </div>
    </div>
  {:else if manifestMode}
    <div class="section">
      <h5>Settings {manifestMode ? `(${manifestMode.mode_name})` : ''}</h5>
      <div class="no-variables">
        No settings defined for this mode.
      </div>
    </div>
  {/if}

  <!-- variables validation -->
  <div class="section">
    <h5>Variables Validation</h5>
    <div class="parameters-list">
      {#each (selectedNode?.data?.componentVariables || []) as variable}
        {@const isConnected = nodeValidationStatus.items[variable.target_variable] || false}
        <div class="parameter-item" class:connected={isConnected} class:type-error={!$validationStatus.typeValid}>
          <div class="parameter-info">
            <div class="parameter-name">{variable.target_variable}</div>
            <div class="parameter-details">
              <span class="parameter-type">Type: {variable.type}</span>
              <span class="parameter-direction" class:input={variable.is_input} class:output={variable.is_output}>
                {variable.is_input && variable.is_output ? 'IN/OUT' : variable.is_input ? 'IN' : 'OUT'}
              </span>
            </div>
          </div>
          <div class="parameter-status">
            <div class="connection-status" class:connected={isConnected}>
              {isConnected ? '✓ Connected' : '✗ Not Connected'}
            </div>
            {#if !$validationStatus.typeValid}
              <div class="type-error">Type mismatch!</div>
            {/if}
          </div>
        </div>
      {/each}
      
      {#if (selectedNode?.data?.componentVariables || []).length === 0}
        <div class="no-variables">
          No variables defined for this mode.
        </div>
      {/if}
    </div>
  </div>

  <!-- loop detection -->
  <div class="bg-transparent border border-border rounded-md p-4">
    <div class="flex items-center gap-3">
      {#if hasLoop}
        <span class="text-xl font-bold text-danger">⚠</span>
        <span class="text-sm font-bold text-danger-dark">Error detected</span>
      {:else}
        <span class="text-xl font-bold text-success">✓</span>
        <span class="text-sm font-bold text-muted">No Error detected</span>
      {/if}
    </div>
    
    {#if hasLoop && loopDetails.length > 0}
      <div class="mt-4 pt-4 border-t border-danger-light">
        <div class="mb-2 text-xs text-danger-dark uppercase tracking-wider">
          Detected Errors:
        </div>
        <div class="flex flex-col gap-2">
          {#each loopDetails as loop}
            <div class="flex items-center gap-2 text-xs text-danger-dark">
              {#each loop.path as node, i}
                <span class="bg-white border border-danger-light px-1.5 py-0.5 rounded font-mono text-xs">
                  {node}
                </span>
                {#if i < loop.path.length - 1}
                  <span class="text-danger font-bold">→</span>
                {/if}
              {/each}
            </div>
          {/each}
        </div>
      </div>
    {/if}
  </div>

  <!-- regex -->
  <div class="section">
    <h5>Regex Validation</h5>
    
    {#if regexCapableVariables.length === 0}
      <div class="no-variables">
        No variables with regex support available.
      </div>
    {:else}
      <div class="validation-form">
        <!-- variable selection -->
        <div class="form-group">
          <label for="parameter-select">
            Variable:
          </label>
          <select id="parameter-select" bind:value={selectedParameter}>
            <option value="">Select Variable</option>
            {#each regexCapableVariables as variable}
              <option value={variable.target_variable}>
                {variable.target_variable}
                ({variable.allowregex_input && variable.allowregex_output ? 'IN+OUT' : 
                  variable.allowregex_input ? 'IN' : 'OUT'})
              </option>
            {/each}
          </select>
        </div>

        {#if selectedParameter && selectedVariableDetails}
          <!-- input regex -->
          {#if selectedVariableDetails.allowregex_input}
            <div class="form-group">
              
              <input 
                id="regex-input" 
                type="text" 
                bind:value={regexInputValue} 
                placeholder="Enter input regex pattern (e.g., ^[A-Z]{3}-\d{4}$)"
                class="regex-input"
              />
              
            </div>
          {/if}

          <!-- output regex -->
          {#if selectedVariableDetails.allowregex_output}
            <div class="form-group">
              
              <input 
                id="regex-output" 
                type="text" 
                bind:value={regexOutputValue} 
                placeholder="Enter output regex pattern (e.g., ^[a-z0-9-]+$)"
                class="regex-input"
              />
              
            </div>
          {/if}

          <!-- apply button -->
          <button 
            class="apply-button" 
            on:click={applyValidation}
            disabled={
              !selectedParameter || 
              (!regexInputValue.trim() && !regexOutputValue.trim()) ||
              (!selectedVariableDetails.allowregex_input && !selectedVariableDetails.allowregex_output)
            }
          >
            Apply Regex Validation
          </button>

          <!-- current Regex Info -->
          {#if currentComponent}
            {@const currentVar = currentComponent.mode?.variables?.variable?.find(v => v.target_variable === selectedParameter)}
            {#if currentVar && (currentVar.input_regex || currentVar.output_regex)}
              <div class="current-regex-info">
                <strong>Currently Applied:</strong>
                {#if currentVar.input_regex}
                  <div class="regex-display input">
                    
                    Input: <code>{currentVar.input_regex}</code>
                  </div>
                {/if}
                {#if currentVar.output_regex}
                  <div class="regex-display output">
                    
                    Output: <code>{currentVar.output_regex}</code>
                  </div>
                {/if}
              </div>
            {/if}
          {/if}
        {:else if selectedParameter}
          <div class="no-regex-support">
            Selected variable does not support regex validation
          </div>
        {/if}
      </div>
    {/if}
  </div>
</div>

<!-- reset confirmation -->
{#if showResetConfirm}
  <div class="modal-overlay">
    <div class="modal">
      <h3>Reset to Defaults</h3>
      <p>Are you sure you want to reset all settings to their default values? This action cannot be undone.</p>
      <div class="modal-buttons">
        <button class="cancel-button" on:click={cancelReset}>Cancel</button>
        <button class="confirm-button" on:click={confirmReset}>Yes, Reset</button>
      </div>
    </div>
  </div>
{/if}

<style>
  .config-tab {
    padding: 1rem;
    overflow-y: auto;
    height: 100%;
  }
  .config-tab h4 {
    margin: 0 0 1rem 0;
    color: #333;
  }
  .section {
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #eee;
  }
  .section:last-child {
    border-bottom: none;
  }
  .section h5 {
    margin: 0 0 0.75rem 0;
    color: #555;
    font-size: 0.9rem;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
  .anchorpoint-selector {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .anchorpoint-selector label {
    font-weight: bold;
    color: #333;
    font-size: 0.9rem;
  }
  
  .anchorpoint-selector select {
    width: 100%;
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 0.9rem;
    background: white;
  }
  
  .anchorpoint-selector select:focus {
    outline: none;
    border-color: #007acc;
    box-shadow: 0 0 0 2px rgba(0, 122, 204, 0.2);
  }
  
  .anchorpoint-info {
    font-size: 0.8rem;
    color: #666;
    line-height: 1.4;
    padding: 0.5rem;
    background: #f8f9fa;
    border-radius: 4px;
  }
  

  .reset-button {
    width: 100%;
    padding: 0.75rem;
    background: #ff742a;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.9rem;
    transition: background-color 0.2s;
  }
  .reset-button:hover {
    background: #fa550f;
  }
  .settings-list,
  .parameters-list {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  .setting-item {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    background: white;
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 0.75rem;
  }
  .setting-item.boolean {
    flex-direction: row;
    align-items: center;
    gap: 0.75rem;
  }
  .setting-item.boolean input[type="checkbox"] {
    margin: 0;
  }
  .setting-item.boolean label {
    margin: 0;
    cursor: pointer;
    flex: 1;
  }
  .setting-item.boolean .default-info {
    flex: none;
  }
  .setting-name {
    font-weight: bold;
    color: #333;
    margin: 0;
  }
  .setting-item input[type="text"] {
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
  }
  .default-info {
    font-size: 0.8rem;
    color: #666;
    font-style: italic;
    margin-top: 0.25rem;
  }
  .parameter-item {
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 0.75rem;
    background: white;
  }
  .parameter-item.connected {
    background: #f8fff8;
    border-color: #28a745;
  }
  .parameter-item.type-error {
    background: #fff5f5;
    border-color: #dc3545;
  }
  .parameter-name {
    font-weight: bold;
    margin-bottom: 0.25rem;
    color: #333;
  }
  .parameter-details {
    display: flex;
    gap: 1rem;
    margin-bottom: 0.5rem;
    flex-wrap: wrap;
  }
  .parameter-type {
    font-size: 0.8rem;
    color: #666;
    background: #f0f0f0;
    padding: 2px 6px;
    border-radius: 3px;
  }
  .parameter-direction {
    font-size: 0.8rem;
    color: #666;
    background: #f0f0f0;
    padding: 2px 6px;
    border-radius: 3px;
  }
  .parameter-direction.input {
    background: #e3f2fd;
    color: #1976d2;
  }
  .parameter-direction.output {
    background: #f3e5f5;
    color: #7b1fa2;
  }
  .connection-status {
    font-size: 0.8rem;
    font-weight: bold;
  }
  .connection-status.connected {
    color: #28a745;
  }
  .connection-status:not(.connected) {
    color: #dc3545;
  }
  .type-error {
    color: #dc3545;
    font-size: 0.8rem;
    font-weight: bold;
    margin-top: 0.25rem;
  }
  .validation-summary {
    background: #f8f9fa;
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 1rem;
  }
  .validation-summary.valid {
    background: #d4edda;
    border-color: #c3e6cb;
  }
  .summary-stats {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 1rem;
  }
  .stat {
    font-size: 0.9rem;
    font-weight: bold;
    color: #666;
  }
  .validation-summary.valid .stat {
    color: #155724;
  }
  .status-badge {
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.8rem;
    font-weight: bold;
    background: #6c757d;
    color: white;
  }
  .status-badge.valid {
    background: #28a745;
    color: white;
  }
  .no-variables {
    text-align: center;
    color: #999;
    font-style: italic;
    padding: 2rem;
  }
  .validation-form {
    background: #ffffff;
    padding: 1rem;
    border-radius: 6px;
  }
  .form-group {
    margin-bottom: 1rem;
  }
  .form-group:last-child {
    margin-bottom: 0;
  }
  .form-group label {
    display: block;
    margin-bottom: 0.25rem;
    font-weight: bold;
    color: #555;
    font-size: 0.9rem;
  }
  .regex-input {
    width: 100%;
    padding: 0.5rem;
    border: 2px solid #ddd;
    border-radius: 4px;
    font-size: 0.9rem;
    font-family: 'Courier New', monospace;
  }
  .regex-input:focus {
    outline: none;
    border-color: #007acc;
    box-shadow: 0 0 0 2px rgba(0, 122, 204, 0.2);
  }
  .current-regex-info {
    margin-top: 1rem;
    padding: 0.75rem;
    background: #ffffff;
    border: 1px solid #b3d9ff;
    border-radius: 4px;
    font-size: 0.85rem;
  }
  .current-regex-info strong {
    display: block;
    margin-bottom: 0.5rem;
    color: #004085;
  }
  .regex-display {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-top: 0.25rem;
    padding: 0.25rem;
  }
  .regex-display.input {
    color: #1976d2;
  }
  .regex-display.output {
    color: #7b1fa2;
  }
  .regex-display code {
    background: white;
    padding: 2px 6px;
    border-radius: 3px;
    font-family: 'Courier New', monospace;
    font-size: 0.85rem;
    border: 1px solid #ddd;
  }
  .regex-display.input{
    border: none !important;
    background: white !important;
    box-shadow: none !important;
    pointer-events: none;
  }
  .no-regex-support {
    text-align: center;
    padding: 1rem;
    color: #856404;
    background: #fff3cd;
    border: 1px solid #ffeaa7;
    border-radius: 4px;
    font-size: 0.85rem;
  }
  
  .form-group select,
  .form-group input {
    width: 100%;
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 0.9rem;
  }
  .form-group select:focus,
  .form-group input:focus {
    outline: none;
    border-color: #007acc;
    box-shadow: 0 0 0 2px rgba(0, 122, 204, 0.2);
  }
  .apply-button {
    width: 100%;
    padding: 0.75rem;
    background: #007acc;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.9rem;
    margin-top: 0.5rem;
    transition: background-color 0.2s;
  }
  .apply-button:hover:not(:disabled) {
    background: #005fa3;
  }
  .apply-button:disabled {
    background: #ccc;
    cursor: not-allowed;
  }

  /* modal styles */
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
    max-width: 400px;
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
    background: #ec3f0f;
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