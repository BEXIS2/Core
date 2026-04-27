<script lang="ts">
  import {useSvelteFlow} from '@xyflow/svelte';
  
  const {fitView} = useSvelteFlow();
  
  function handleResetView() {
    fitView({ 
      padding: 0.2,
      duration: 300 
    });
  }
</script>

<button 
  class="reset-view-button" 
  on:click={handleResetView}
  title="Reset view to fit all nodes"
>
  ⌖
</button>

<style>
  .reset-view-button {
    position: absolute;
    bottom: 20px;
    right: 20px;
    width: 40px;
    height: 40px;
    background: #007acc;
    color: white;
    border: none;
    border-radius: 50%;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
    transition: all 0.2s;
    z-index: 100;
  }

  .reset-view-button:hover {
    background: #005fa3;
    transform: scale(1.1);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  }

  .reset-view-button:active {
    transform: scale(0.95);
  }
</style>