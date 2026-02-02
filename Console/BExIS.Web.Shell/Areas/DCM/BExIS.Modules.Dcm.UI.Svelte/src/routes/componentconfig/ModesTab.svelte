<script lang="ts">
  export let componentManifest: any;
  export let selectedMode: any;
  export let onModeChange: (mode: any) => void;
  export let currentInteractionMode: string;
  export let selectedNode: any = null;

  // use node specific mode, else selectedMode
  $: nodeMode = getNodeSpecificMode();
  let tempSelectedMode = nodeMode;

  function getNodeSpecificMode() {
    // get specific mode from selectedNode if available
    if (selectedNode?.data?.modeName) {
      const manifestModes = getAvailableModes();
      const foundMode = manifestModes.find(mode => mode.mode_name === selectedNode.data.modeName);
      if (foundMode) {
        return foundMode;
      }
    }
    return selectedMode;
  }

  // update tempSelectedMode when nodeMode changes
  let lastNodeMode = nodeMode;
  $: if (nodeMode !== lastNodeMode) {
    tempSelectedMode = nodeMode;
    lastNodeMode = nodeMode;
  }

  // get available modes based on current interaction mode from manifest
  $: availableModes = getAvailableModes();

  function getAvailableModes() {
    if (!componentManifest?.modes) return [];
    
    // return modes based on current interaction mode from manifest
    if (currentInteractionMode === 'edit' && componentManifest.modes.edit) {
      return componentManifest.modes.edit;
    } else if (currentInteractionMode === 'view' && componentManifest.modes.view) {
      return componentManifest.modes.view;
    }
    
    return [];
  }

  let showSubmodeWarning = false;

  function applyChanges() {
    if (tempSelectedMode && hasChanges()) {
      showSubmodeWarning = true;
    } else if (tempSelectedMode) {
      onModeChange(tempSelectedMode);
    }
  }

  function confirmSubmodeChange() {
    if (tempSelectedMode) {
      onModeChange(tempSelectedMode);
    }
    showSubmodeWarning = false;
  }

  function cancelSubmodeChange() {
    showSubmodeWarning = false;
    tempSelectedMode = nodeMode; // reset to current
  }

  function hasChanges(): boolean {
    return tempSelectedMode?.mode_name !== nodeMode?.mode_name;
  }
</script>

<div class="modes-tab">
  <div class="section">
    <h5>Available Modes</h5>
    <div class="modes-list">
      {#each availableModes as mode}
        <label class="mode-option">
          <input 
            type="radio" 
            bind:group={tempSelectedMode} 
            value={mode}
          />
          <div class="mode-info">
            <div class="mode-name">{mode.mode_name}</div>
            <div class="mode-description">{mode.description}</div>
            <div class="mode-params">
              {#each mode.variables.variable as param}
                <div class="param-row">
                  <span class="param-name">{param.target_variable}</span>
                  <span class="param-type">{param.type}</span>
                  <span class="param-io">
                    {#if param.is_input}<span class="io in">IN</span>{/if}
                    {#if param.is_output}<span class="io out">OUT</span>{/if}
                  </span>
                </div>
              {/each}
            </div>
            <div class="mode-stats">
              <span class="stat">Variables: {mode.variables?.variable?.length || 0}</span>
              <span class="stat">Settings: {mode.settings?.setting?.length || 0}</span>
            </div>
          </div>
        </label>
      {/each}
      {#if availableModes.length === 0}
        <div class="no-modes">
          No modes available for {currentInteractionMode.toUpperCase()} mode.
        </div>
      {/if}
    </div>
  </div>

  <div class="section">
    <button 
      class="apply-button" 
      on:click={applyChanges}
      disabled={!hasChanges() || !tempSelectedMode}
    >
      Apply Mode Change
    </button>
    {#if hasChanges()}
      <div class="warning">
        <strong>Warning:</strong> Applying mode changes will clear all existing connections for this component.
      </div>
    {/if}
  </div>

  {#if showSubmodeWarning}
    <div class="modal-overlay">
      <div class="modal">
        <h3>Mode Change Warning</h3>
        <div class="modal-text">
          You are about to change the mode for this component. This will:
          <div class="modal-list">
            <div class="modal-list-item">• Remove all existing connections for this component</div>
            <div class="modal-list-item">• Reset all configurations and settings to their default values</div>
          </div>
          <div class="modal-strong">
            This action cannot be undone. Do you want to continue?
          </div>
        </div>
        <div class="modal-buttons">
          <button class="cancel-button" on:click={cancelSubmodeChange}>Cancel</button>
          <button class="confirm-button" on:click={confirmSubmodeChange}>Yes, Change Mode</button>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .modes-tab {
    padding: 1rem;
    overflow-y: auto;
    height: 100%;
  }
  .section {
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #eee;
  }
  .section:last-child {
    border-bottom: none;
  }
  .modes-list {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  .mode-option {
    display: flex;
    align-items: flex-start;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    cursor: pointer;
    transition: background-color 0.2s, border-color 0.2s;
  }
  .mode-option:hover {
    background: #f8f9fa;
    border-color: #007acc;
  }
  .mode-option input[type="radio"] {
    margin-right: 0.75rem;
    margin-top: 0.25rem;
  }
  .mode-info {
    flex: 1;
  }
  .mode-name {
    font-weight: bold;
    margin-bottom: 0.25rem;
    color: #333;
  }
  .mode-description {
    color: #666;
    font-size: 0.9rem;
    margin-bottom: 0.5rem;
  }
  .mode-params {
    margin-bottom: 0.5rem;
  }
  .param-row {
    display: flex;
    gap: 1rem;
    align-items: center;
    font-size: 0.85rem;
    margin-bottom: 2px;
  }
  .param-name {
    font-weight: bold;
    color: #007acc;
    min-width: 80px;
  }
  .param-type {
    color: #555;
    font-size: 0.85rem;
    min-width: 60px;
  }
  .param-io {
    display: flex;
    gap: 0.5rem;
  }
  .io {
    font-size: 0.8rem;
    font-weight: bold;
    padding: 2px 6px;
    border-radius: 3px;
    background: #eee;
    color: #888;
  }
  .io.in {
    background: #e3f2fd;
    color: #1976d2;
  }
  .io.out {
    background: #f3e5f5;
    color: #7b1fa2;
  }
  .mode-stats {
    display: flex;
    gap: 1rem;
    font-size: 0.8rem;
    color: #888;
    background: #f0f0f0;
    padding: 2px 6px;
    border-radius: 3px;
  }
  .no-modes {
    text-align: center;
    color: #999;
    font-style: italic;
    padding: 2rem;
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
    transition: background-color 0.2s;
  }
  .apply-button:hover:not(:disabled) {
    background: #005fa3;
  }
  .apply-button:disabled {
    background: #ccc;
    cursor: not-allowed;
  }
  .warning {
    margin-top: 0.5rem;
    padding: 0.5rem;
    background: #fff3cd;
    border: 1px solid #ffeaa7;
    border-radius: 4px;
    color: #856404;
    font-size: 0.85rem;
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
    max-width: 550px;
    width: 90%;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
  }
  
  .modal h3 {
    margin: 0 0 1rem 0;
    color: #333;
  }
  
  .modal-text {
    margin-bottom: 1rem;
    color: #666;
    line-height: 1.5;
  }
  .modal-list {
    margin: 0.5rem 0 1rem 1.5rem;
  }
  .modal-list-item {
    margin-bottom: 0.25rem;
  }
  .modal-strong {
    font-weight: bold;
    color: #333;
    margin-bottom: 1rem;
  }
  
  .modal-buttons {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    margin-top: 1.5rem;
  }
  .cancel-button, .confirm-button {
    padding: 0.75rem 1.5rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: bold;
    transition: background-color 0.2s;
  }
  
  .cancel-button {
    background: #6c757d;
    color: white;
  }
  
  .cancel-button:hover {
    background: #5a6268;
  }
  .confirm-button {
    background: #dc3545;
    color: white;
  }
  
  .confirm-button:hover {
    background: #c82333;
  }
</style>