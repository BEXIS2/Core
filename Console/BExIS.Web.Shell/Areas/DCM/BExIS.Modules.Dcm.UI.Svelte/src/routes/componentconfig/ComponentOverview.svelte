<script lang="ts">
  export let selectedNode: any;
  export let onEdit: () => void;
  export let onSave: () => void;
  export let onDelete: () => void;
</script>

<div class="overview">
  <h3>Overview</h3>
  
  {#if selectedNode}
    <div class="overview-content">
      <div class="node-info">
        <div class="info-row">
          <strong>Component:</strong> 
          <span>{selectedNode.data?.componentName || 'Unknown'}</span>
        </div>
        
        {#if selectedNode.data?.modeName}
          <div class="info-row">
            <strong>Mode:</strong> 
            <span>{selectedNode.data.modeName}</span>
          </div>
        {/if}
        
        {#if selectedNode.data?.componentId}
          <div class="info-row">
            <strong>ID:</strong> 
            <span>{selectedNode.data.componentId}</span>
          </div>
        {/if}
        
        <div class="info-row">
          <strong>Type:</strong> 
          <span>{selectedNode.type}</span>
        </div>
      </div>
      
      <button class="edit-button" on:click={onEdit}>
        Edit Component
      </button>
    </div>
  {/if}
  
  <!-- delete / save buttons-->
  <div class="action-buttons">
    <button class="delete-button" on:click={onDelete}>
      Delete
    </button>
    <button class="save-button" on:click={onSave}>
      Save
    </button>
  </div>
</div>

<style>
  .overview {
    padding: 1rem;
    height: 100%;
    display: flex;
    flex-direction: column;
  }
  
  .overview h3 {
    margin: 0 0 1rem 0;
    font-size: 1.2rem;
    color: #333;
  }
  
  .overview-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }
  
  .node-info {
    background: #f8f9fa;
    border-radius: 6px;
    padding: 1rem;
  }
  
  .info-row {
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
  }
  
  .info-row:last-child {
    margin-bottom: 0;
  }
  
  .info-row strong {
    display: inline-block;
    width: 80px;
    color: #555;
  }
  
  .edit-button {
    width: 100%;
    padding: 0.75rem;
    background: #007acc;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    font-weight: bold;
    transition: all 0.2s;
    margin-top: auto;
  }
  
  .edit-button:hover {
    background: #005fa3;
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(0, 122, 204, 0.3);
  }
  
  .action-buttons {
    display: flex;
    gap: 0.75rem;
    padding: 1rem 0 0 0;
    border-top: 1px solid #ddd;
    margin-top: auto;
  }
  
  .delete-button,
  .save-button {
    flex: 1;
    padding: 0.75rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: bold;
    font-size: 0.9rem;
    transition: all 0.2s;
    min-height: 44px;
    height: 44px;
    line-height: 1.2;
    box-sizing: border-box;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  
  .delete-button {
    background: #6c757d;
    color: white;
  }
  
  .delete-button:hover {
    background: #5a6268;
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(108, 117, 125, 0.3);
  }
  
  .save-button {
    background: #28a745;
    color: white;
  }
  
  .save-button:hover {
    background: #218838;
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(40, 167, 69, 0.3);
  }
</style>