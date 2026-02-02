<script lang="ts">
    import { createEventDispatcher } from 'svelte';

    const dispatch = createEventDispatcher();

    export let simpleComponent: any;
    export let path: string;
    export let required: boolean = false;
    export let label: string;
    export let generateNodes: boolean = false;
    export let nodePosition: { x: number, y: number } = { x: 50, y: 50 };

    function generateLeafNode(): any {
        if (!simpleComponent || !simpleComponent.properties || !simpleComponent.properties['#text']) {
            return null;
        }

        const textProps = simpleComponent.properties['#text'];
        
        return {
            id: `leaf-${path}`,
            type: 'itemNode',
            data: {
                label: label,
                description: textProps.description || `leaf field: ${label}`,
                type: textProps.type || 'string',
                format: textProps.format || 'text',
                required: required,
                path: path,
                isLeaf: true
            },
            position: { ...nodePosition },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 200px; height: 60px; background: #e8f5e8; border: 2px solid #4caf50;'
        };
    }

    $: if (generateNodes && simpleComponent && path) {
        const generatedNode = generateLeafNode();
        if (generatedNode) {
            dispatch('leafNodeGenerated', { node: generatedNode });
        }
    }
</script>
