<script lang="ts">
  import TreeComponent from './TreeComponent.svelte';
  export let currentInteractionMode: string;
  export let componentConfig: any;
  export let componentManifest: any;
  export let onAddComponent: (component: any) => void;
  export let onSaveMappings: () => void;
  export let onSave: () => void;

  // get all submodes for current interaction mode from manifest
  $: availableModes = getAvailableModes(currentInteractionMode, componentManifest);
  let selectedModeForAdd: any = null;

  function getAvailableModes(interactionMode: string, manifest: any) {
    if (!manifest?.modes) return [];
    
    // return all submodes based on current interaction mode from manifest
    if (interactionMode === 'edit' && manifest.modes.edit) {
      return manifest.modes.edit;
    } else if (interactionMode === 'view' && manifest.modes.view) {
      return manifest.modes.view;
    }
    
    return [];
  }

  function handleModeClick(mode: any) {
    selectedModeForAdd = mode;
  }

  function handleAddComponent() {
    
    // get selected mode for current interaction mode, default = first
    const modeToAdd = selectedModeForAdd || availableModes[0];
    if (!modeToAdd) {
      return;
    }
    
    // create component object with selected mode
    const component = {
      id: `component-${modeToAdd.mode_name}-${Date.now()}`,
      meta: {
        component_name: componentManifest.meta.component_name,
        title: componentManifest.meta.title,
        description: componentManifest.meta.description
      },
      globalSettings: {
        interaction_mode: currentInteractionMode
      },
      mode: modeToAdd
    };
    
    onAddComponent(component);
    selectedModeForAdd = null;
  }
</script>

<div class="component-library">
  <h3>Component Library</h3>

  <div class="components-section">
    {#if componentManifest?.meta}
      <div class="component-card">
        <div class="component-header">
          <div class="component-title">{componentManifest.meta.title || componentManifest.meta.component_name}</div>
        </div>
        
        <div class="component-description">
          {componentManifest.meta.description || 'No description available'}
        </div>
        
        {#if availableModes.length > 0}
          <div class="component-modes">

            <div class="modes-list">
              {#each availableModes as mode}
                <div 
                  class="mode-item" 
                  class:selected={selectedModeForAdd?.mode_name === mode.mode_name}
                  on:click={() => handleModeClick(mode)}
                  role="button"
                  tabindex="0"
                  on:keypress={(e) => e.key === 'Enter' && handleModeClick(mode)}
                >
                  <div class="mode-main">
                    <span class="mode-name">{mode.mode_name}</span>
                    <div class="mode-description">{mode.description || 'No description available'}</div>
                  </div>
                </div>
              {/each}
            </div>
          </div>
          
          <div class="component-actions">
            <button 
              class="add-component-button" 
              on:click={handleAddComponent}
            >
              Add Component
            </button>
          </div>
        {:else}
          <div class="no-modes">
            <div class="no-modes-icon">📦</div>
            <div class="no-modes-text">
              No modes available for <strong>{currentInteractionMode.toUpperCase()}</strong> mode
            </div>
            <div class="no-modes-hint">
              {#if currentInteractionMode === 'view'}
                No view mode components defined in manifest
              {:else}
                No edit mode components defined in manifest
              {/if}
            </div>
          </div>
        {/if}
      </div>
    {:else}
      <div class="no-components">
        <div class="no-components-icon">📦</div>
        <div class="no-components-text">
          No component manifest available
        </div>
      </div>
    {/if}
  </div>
  
  <div class="save-mappings-section">
    <button class="save-mappings-button" on:click={onSave}>
      Save
    </button>
  </div>
</div>

<style>
  .component-library {
    padding: 1rem;
    height: 100%;
    overflow-y: auto;
  }
  
  .component-library h3 {
    margin: 0 0 0.5rem 0;
    color: #333;
    font-size: 1.2rem;
  }
  
  .component-card {
    background: white;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 1rem;
    transition: all 0.2s;
  }
  
  .component-card:hover {
    border-color: #007acc;
    box-shadow: 0 2px 8px rgba(0, 122, 204, 0.1);
  }
  
  .component-header {
    margin-bottom: 0.5rem;
  }
  
  .component-title {
    font-weight: bold;
    color: #333;
    font-size: 1.1rem;
    line-height: 1.3;
  }
  
  .component-description {
    color: #666;
    font-size: 0.85rem;
    margin-bottom: 0.75rem;
    line-height: 1.4;
  }
  
  .component-modes {
    margin-bottom: 1rem;
  }
  
  .modes-list {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .mode-item {
    background: #f8f9fa;
    padding: 0.5rem;
    border-radius: 4px;
    border: 1px solid #e9ecef;
    cursor: pointer;
    transition: all 0.2s;
  }
  
  .mode-item:hover {
    background: #e9ecef;
    border-color: #007acc;
  }
  
  .mode-item.selected {
    background: #d4e9f7;
    border-color: #007acc;
    border-width: 2px;
  }
  
  .mode-main {
    margin-bottom: 0;
  }
  
  .mode-name {
    font-weight: bold;
    color: #007acc;
    font-size: 0.85rem;
    display: block;
    margin-bottom: 0.25rem;
  }
  
  .mode-description {
    color: #666;
    font-size: 0.75rem;
    line-height: 1.3;
  }
  
  .component-actions {
    display: flex;
    justify-content: center;
  }
  
  .add-component-button {
    padding: 0.6rem 1.2rem;
    background: #007acc;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    font-weight: bold;
    transition: background-color 0.2s;
    width: 100%;
  }
  
  .add-component-button:hover {
    background: #005fa3;
  }
  
  .no-modes,
  .no-components {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 1rem;
    text-align: center;
    color: #666;
  }
  
  .no-modes-icon,
  .no-components-icon {
    font-size: 3rem;
    margin-bottom: 1rem;
    opacity: 0.5;
  }
  
  .no-modes-text,
  .no-components-text {
    font-size: 1rem;
    margin-bottom: 0.5rem;
  }
  
  .no-modes-hint {
    font-size: 0.85rem;
    color: #999;
    font-style: italic;
  }

  .save-mappings-section {
    padding: 1rem 0 0 0;
    border-top: 1px solid #ddd;
    margin-top: auto;
  }
  
  .save-mappings-button {
    width: 100%;
    padding: 0.75rem;
    background: #28a745;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: bold;
    transition: all 0.2s;
    font-size: 0.9rem;
  }
  
  .save-mappings-button:hover {
    background: #218838;
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(40, 167, 69, 0.3);
  }
</style>
