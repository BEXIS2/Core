<script lang="ts">
  export let componentManifest: any;
  export let selectedNode: any;
  export let selectedMode: any;
  export let currentInteractionMode: string;
  export let componentConfig: any;
  
  $: modeVariables = selectedMode?.variables?.variable || [];
  
  // read default values from manifest
  function getVariableDefaultValue(variable: any) {
    return variable.default_value?.value || 'no default set';
  }
</script>

<div class="preview-tab">
  <h4>Default Values {selectedMode ? `(mode: ${selectedMode.mode_name})` : ''}</h4>
  
  {#if selectedMode && modeVariables.length > 0}
    <div class="section">
      <h5>Variables</h5>
      <div class="variables-list">
        {#each modeVariables as variable}
          <div class="variable-item">
            <div class="variable-header">
              <div class="variable-name">{variable.target_variable}</div>
            </div>
            <div class="variable-description">{variable.description || variable.name}</div>
            <div class="variable-details">
              <div class="variable-type">Type: {variable.type}</div>
              <div class="variable-direction">
                {variable.is_input && variable.is_output ? 'IN/OUT' : variable.is_input ? 'Input' : 'Output'}
              </div>
            </div>
            <div class="variable-default">
              <span class="label">Default Value:</span>
              <code>{getVariableDefaultValue(variable)}</code>
            </div>
          </div>
        {/each}
      </div>
    </div>
  {:else if selectedMode}
    <div class="section">
      <h5>Variables</h5>
      <div class="no-variables">
        No variables defined for mode "{selectedMode.mode_name}".
      </div>
    </div>
  {:else}
    <div class="section">
      <div class="no-variables">
        Error. No submode detected.
      </div>
    </div>
  {/if}
</div>

<style>
  .preview-tab {
    padding: 1rem;
    overflow-y: auto;
    height: 100%;
  }
  
  .preview-tab h4 {
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

  .variables-list {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .variable-item {
    background: #f8f9fa;
    border-radius: 6px;
    padding: 0.75rem;
    border: 1px solid #e9ecef;
  }
  
  .variable-name {
    font-weight: bold;
    margin-bottom: 0.25rem;
    color: #333;
  }
  
  .variable-description {
    font-size: 0.85rem;
    color: #666;
    margin-bottom: 0.5rem;
  }
  
  .variable-default {
    font-size: 0.8rem;
    margin-bottom: 0.25rem;
  }
  
  .variable-default:last-child {
    margin-bottom: 0;
  }
  
  .variable-header {
    margin-bottom: 0.25rem;
  }
  
  .variable-details {
    margin-bottom: 0.5rem;
  }
  
  .variable-type,
  .variable-direction {
    font-size: 0.8rem;
    color: #666;
    margin-bottom: 0.25rem;
  }
  
  .no-variables {
    text-align: center;
    color: #999;
    font-style: italic;
    padding: 2rem;
  }
  
  .label {
    font-weight: bold;
    color: #666;
  }
  
  code {
    background: #e9ecef;
    padding: 2px 4px;
    border-radius: 2px;
    font-family: monospace;
    font-size: 0.85em;
  }
</style>