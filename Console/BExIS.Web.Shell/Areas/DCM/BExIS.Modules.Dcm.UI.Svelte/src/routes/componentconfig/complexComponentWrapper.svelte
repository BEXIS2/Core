<script lang="ts">
    import { createEventDispatcher } from 'svelte';
    import SimpleComponent from './simpleComponentWrapper.svelte';
    import ArrayComponent from './arrayComponentWrapper.svelte';
    import ChoiceComponent from './choiceComponentWrapper.svelte';

    const dispatch = createEventDispatcher();

    export let complexComponent: any;
    export let path: string;
    export let required: boolean = false;
    export let generateNodes: boolean = false;
    export let nodePosition: { x: number, y: number } = { x: 50, y: 50 };
    export let nodeSpacing: number = 80;

    let label: string = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
    let requiredList = complexComponent && complexComponent.type === 'object' && complexComponent.required
        ? complexComponent.required
        : [];

    function generateComplexNodes(): any[] {
        const nodes: any[] = [];
        let currentPosition = { ...nodePosition };

        if (complexComponent && complexComponent.type === 'object' && complexComponent.properties) {
            Object.entries(complexComponent.properties).forEach(([key, value]: [string, any]) => {
                const currentPath = path ? `${path}.${key}` : key;
                const isRequired = requiredList.includes(key);

                if (value.type === 'object' && value.properties && !value.properties['#text']) {
                    // nested complex component
                    if (value.oneOf || value.anyOf || value.allOf) {
                        // choice component
                        const choiceNodes = generateChoiceNodes(value, currentPath, currentPosition);
                        nodes.push(...choiceNodes);
                        currentPosition.y += nodeSpacing * (choiceNodes.length || 1);
                    } else {
                        // regular complex component add header
                        const headerNode = {
                            id: `complex-header-${currentPath}`,
                            type: 'itemNode',
                            data: {
                                label: key,
                                description: `complex component: ${key}`,
                                type: 'header',
                                format: 'complex',
                                isHeader: true,
                                path: currentPath
                            },
                            position: { ...currentPosition },
                            draggable: true,
                            selectable: true,
                            deletable: false,
                            style: 'width: 200px; height: 60px; background: #e3f2fd; border: 2px solid #1976d2;'
                        };
                        nodes.push(headerNode);
                        currentPosition.y += nodeSpacing;

                        // recursively generate child nodes
                        const childNodes = generateNestedComplexNodes(value, currentPath, currentPosition);
                        nodes.push(...childNodes);
                        currentPosition.y += nodeSpacing * (childNodes.length || 1);
                    }

                } else if (value.type === 'object' && value.properties && value.properties['#text']) {
                    // simple component
                    const simpleNodes = generateSimpleNodes(value, currentPath, key, isRequired, currentPosition);
                    nodes.push(...simpleNodes);
                    currentPosition.y += nodeSpacing;

                } else if (value.type === 'array' && value.items) {
                    // array component
                    const arrayNodes = generateArrayNodes(value, currentPath, key, currentPosition);
                    nodes.push(...arrayNodes);
                    currentPosition.y += nodeSpacing * (arrayNodes.length || 1);
                }
            });
        }

        return nodes;
    }

    function generateNestedComplexNodes(component: any, basePath: string, position: { x: number, y: number }): any[] {
        const nodes: any[] = [];
        let currentPos = { ...position };

        if (component.properties) {
            Object.entries(component.properties).forEach(([key, value]: [string, any]) => {
                const currentPath = `${basePath}.${key}`;

                if (value.type === 'object' && value.properties && value.properties['#text']) {
                    const simpleNode = {
                        id: `simple-${currentPath}`,
                        type: 'itemNode',
                        data: {
                            label: key,
                            description: value.properties['#text'].description || `simple field: ${key}`,
                            type: value.properties['#text'].type || 'string',
                            format: value.properties['#text'].format || 'text',
                            required: component.required?.includes(key) || false,
                            path: currentPath
                        },
                        position: { ...currentPos },
                        draggable: true,
                        selectable: true,
                        deletable: false,
                        style: 'width: 200px; height: 60px;'
                    };
                    nodes.push(simpleNode);
                    currentPos.y += nodeSpacing;
                }
            });
        }

        return nodes;
    }

    function generateSimpleNodes(component: any, path: string, label: string, isRequired: boolean, position: { x: number, y: number }): any[] {
        return [{
            id: `simple-${path}`,
            type: 'itemNode',
            data: {
                label: label,
                description: component.properties['#text'].description || `simple field: ${label}`,
                type: component.properties['#text'].type || 'string',
                format: component.properties['#text'].format || 'text',
                required: isRequired,
                path: path
            },
            position: { ...position },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 200px; height: 60px;'
        }];
    }

    function generateArrayNodes(component: any, path: string, label: string, position: { x: number, y: number }): any[] {
        const nodes: any[] = [];
        let currentPos = { ...position };

        // array header
        const headerNode = {
            id: `array-header-${path}`,
            type: 'itemNode',
            data: {
                label: `Array: ${label}`,
                description: `array component: ${label}`,
                type: 'array',
                format: 'list',
                isHeader: true,
                path: path
            },
            position: { ...currentPos },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 200px; height: 60px; background: #fff3e0; border: 2px solid #f57c00;'
        };
        nodes.push(headerNode);
        currentPos.y += nodeSpacing;

        // process array items
        if (component.items) {
            if (component.items.type === 'object' && component.items.properties && !component.items.properties['#text']) {
                // complex array items
                const itemNodes = generateNestedComplexNodes(component.items, path, currentPos);
                nodes.push(...itemNodes);
            } else if (component.items.type === 'object' && component.items.properties && component.items.properties['#text']) {
                // simple array items
                const itemNode = {
                    id: `array-item-${path}`,
                    type: 'itemNode',
                    data: {
                        label: `${label} Item`,
                        description: component.items.properties['#text'].description || `array item: ${label}`,
                        type: component.items.properties['#text'].type || 'string',
                        format: component.items.properties['#text'].format || 'text',
                        required: false,
                        path: `${path}.item`
                    },
                    position: { ...currentPos },
                    draggable: true,
                    selectable: true,
                    deletable: false,
                    style: 'width: 200px; height: 60px;'
                };
                nodes.push(itemNode);
            }
        }

        return nodes;
    }

    function generateChoiceNodes(component: any, path: string, position: { x: number, y: number }): any[] {
        const nodes: any[] = [];
        let currentPos = { ...position };

        // choice header
        const choiceType = component.oneOf ? 'OneOf' : component.anyOf ? 'AnyOf' : 'AllOf';
        const headerNode = {
            id: `choice-header-${path}`,
            type: 'itemNode',
            data: {
                label: `${choiceType}: ${path.split('.').pop()}`,
                description: `choice component: ${choiceType}`,
                type: 'choice',
                format: 'selection',
                isHeader: true,
                path: path
            },
            position: { ...currentPos },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 200px; height: 60px; background: #f3e5f5; border: 2px solid #7b1fa2;'
        };
        nodes.push(headerNode);
        currentPos.y += nodeSpacing;

        // process choice options (simplified for flat structure)
        const choiceItems = component.oneOf || component.anyOf || component.allOf || [];
        choiceItems.forEach((item: any, index: number) => {
            if (item['$ref']) {
                const refKey = item['$ref'].split('/').pop();
                const choiceNode = {
                    id: `choice-option-${path}-${index}`,
                    type: 'itemNode',
                    data: {
                        label: `Option: ${refKey}`,
                        description: `choice option: ${refKey}`,
                        type: 'choice_option',
                        format: 'option',
                        path: `${path}.${refKey}`
                    },
                    position: { ...currentPos },
                    draggable: true,
                    selectable: true,
                    deletable: false,
                    style: 'width: 200px; height: 60px;'
                };
                nodes.push(choiceNode);
                currentPos.y += nodeSpacing;
            }
        });

        return nodes;
    }

    // generate nodes when component is used
    $: if (generateNodes && complexComponent) {
        const generatedNodes = generateComplexNodes();
        dispatch('nodesGenerated', { nodes: generatedNodes });
    }
</script>
