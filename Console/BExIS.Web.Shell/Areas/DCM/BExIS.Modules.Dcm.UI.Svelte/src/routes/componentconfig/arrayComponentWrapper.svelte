<script lang="ts">
    import { createEventDispatcher } from 'svelte';
    import ComplexComponent from './complexComponentWrapper.svelte';
    import SimpleComponent from './simpleComponent.svelte';

    const dispatch = createEventDispatcher();

    export let arrayComponent: any;
    export let path: string;
    export let generateNodes: boolean = false;
    export let nodePosition: { x: number, y: number } = { x: 50, y: 50 };
    export let nodeSpacing: number = 80;

    let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
    let requiredList = arrayComponent.items && arrayComponent.items.type === 'object' && arrayComponent.items.required
        ? arrayComponent.items.required
        : [];

    let maxItems: number = arrayComponent.maxItems ? arrayComponent.maxItems : 2147483647;
    let minItems: number = arrayComponent.minItems ? arrayComponent.minItems : 1;

    function generateArrayNodes(): any[] {
        const nodes: any[] = [];
        let currentPosition = { ...nodePosition };

        if (!arrayComponent.items) return nodes;

        // array header node
        const headerNode = {
            id: `array-wrapper-${path}`,
            type: 'itemNode',
            data: {
                label: `Array: ${label}`,
                description: `array component: ${label} (${minItems}-${maxItems} items)`,
                type: 'array',
                format: 'list',
                isHeader: true,
                path: path,
                minItems: minItems,
                maxItems: maxItems
            },
            position: { ...currentPosition },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 220px; height: 60px; background: #fff3e0; border: 2px solid #f57c00;'
        };
        nodes.push(headerNode);
        currentPosition.y += nodeSpacing;

        // process array items based on type
        if (arrayComponent.items.type === 'object' && arrayComponent.items.properties && !arrayComponent.items.properties['#text']) {
            // complex array items sare traversed recursively
            const complexNodes = generateComplexArrayItems(arrayComponent.items, `${path}.items`, currentPosition);
            nodes.push(...complexNodes);

        } else if (arrayComponent.items.type === 'object' && arrayComponent.items.properties && arrayComponent.items.properties['#text']) {
            // simple array items
            const simpleNode = generateSimpleArrayItem(arrayComponent.items, `${path}.item`, currentPosition);
            if (simpleNode) {
                nodes.push(simpleNode);
            }
        }

        return nodes;
    }

    function generateComplexArrayItems(itemsComponent: any, basePath: string, position: { x: number, y: number }): any[] {
        const nodes: any[] = [];
        let currentPos = { ...position };

        if (itemsComponent.properties) {
            Object.entries(itemsComponent.properties).forEach(([key, value]: [string, any]) => {
                const currentPath = `${basePath}.${key}`;
                const isRequired = requiredList.includes(key);

                if (value.type === 'object' && value.properties && value.properties['#text']) {
                    // simple property in complex array item
                    const simpleNode = {
                        id: `array-complex-simple-${currentPath}`,
                        type: 'itemNode',
                        data: {
                            label: `${key} (Array Item)`,
                            description: value.properties['#text'].description || `array item field: ${key}`,
                            type: value.properties['#text'].type || 'string',
                            format: value.properties['#text'].format || 'text',
                            required: isRequired,
                            path: currentPath,
                            isArrayItem: true
                        },
                        position: { ...currentPos },
                        draggable: true,
                        selectable: true,
                        deletable: false,
                        style: 'width: 220px; height: 60px; background: #fff8e1;'
                    };
                    nodes.push(simpleNode);
                    currentPos.y += nodeSpacing;

                } else if (value.type === 'object' && value.properties && !value.properties['#text']) {
                    // nested complex component in array
                    const nestedHeaderNode = {
                        id: `array-nested-header-${currentPath}`,
                        type: 'itemNode',
                        data: {
                            label: `${key} (Nested in Array)`,
                            description: `nested complex component in array: ${key}`,
                            type: 'header',
                            format: 'complex',
                            isHeader: true,
                            path: currentPath,
                            isArrayItem: true
                        },
                        position: { ...currentPos },
                        draggable: true,
                        selectable: true,
                        deletable: false,
                        style: 'width: 220px; height: 60px; background: #e3f2fd; border: 2px solid #1976d2;'
                    };
                    nodes.push(nestedHeaderNode);
                    currentPos.y += nodeSpacing;

                    // recursively process nested properties
                    const nestedNodes = generateComplexArrayItems(value, currentPath, currentPos);
                    nodes.push(...nestedNodes);
                    currentPos.y += nodeSpacing * nestedNodes.length;
                }
            });
        }

        return nodes;
    }

    function generateSimpleArrayItem(itemComponent: any, itemPath: string, position: { x: number, y: number }): any | null {
        if (!itemComponent.properties || !itemComponent.properties['#text']) {
            return null;
        }

        const textProps = itemComponent.properties['#text'];

        return {
            id: `array-simple-item-${itemPath}`,
            type: 'itemNode',
            data: {
                label: `${label} Item`,
                description: textProps.description || `simple array item: ${label}`,
                type: textProps.type || 'string',
                format: textProps.format || 'text',
                required: false,
                path: itemPath,
                isArrayItem: true
            },
            position: { ...position },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 220px; height: 60px; background: #fff8e1;'
        };
    }

    // generate nodes when in node generation mode
    $: if (generateNodes && arrayComponent && arrayComponent.items) {
        const generatedNodes = generateArrayNodes();
        dispatch('arrayNodesGenerated', { nodes: generatedNodes });
    }
</script>

