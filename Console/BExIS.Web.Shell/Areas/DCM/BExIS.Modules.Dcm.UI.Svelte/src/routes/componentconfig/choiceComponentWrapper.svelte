<script lang="ts">
    import { createEventDispatcher } from 'svelte';
    import ComplexComponent from './complexComponentWrapper.svelte';
    import SimpleComponent from './simpleComponent.svelte';

    const dispatch = createEventDispatcher();

    export let choiceComponent: any;
    export let path: string;
    export let generateNodes: boolean = false;
    export let nodePosition: { x: number, y: number } = { x: 50, y: 50 };
    export let nodeSpacing: number = 80;

    let label = path.split('.').length > 1 ? path.split('.')[path.split('.').length - 1] : path;
    let choices: {key: string, value: string}[] = getChoices(choiceComponent);

    function getChoices(cComponent: any): {key: string, value: string}[] {
        let c: {key: string, value: string}[] = [];

        if (cComponent != undefined && cComponent != null) {
            let items: any[] = [];

            if (cComponent.anyOf != null && cComponent.anyOf != undefined && cComponent.anyOf.length > 0) {
                items = cComponent.anyOf;
            }
            else if (cComponent.oneOf != null && cComponent.oneOf != undefined && cComponent.oneOf.length > 0) {
                items = cComponent.oneOf;
            }
            else if (cComponent.allOf != null && cComponent.allOf != undefined && cComponent.allOf.length > 0) {
                items = cComponent.allOf;
            }

            items.forEach((item) => {
                const refKey = item['$ref'].split('/')[item['$ref'].split('/').length - 1];
                c.push({
                    key: refKey,
                    value: refKey
                });
            });
        }
        return c;
    }

    function generateChoiceNodes(): any[] {
        const nodes: any[] = [];
        let currentPosition = { ...nodePosition };

        // determine choice type
        const choiceType = choiceComponent.oneOf ? 'OneOf' : choiceComponent.anyOf ? 'AnyOf' : 'AllOf';

        // choice header node
        const headerNode = {
            id: `choice-wrapper-${path}`,
            type: 'itemNode',
            data: {
                label: `${choiceType}: ${label}`,
                description: `choice component: ${choiceType} - select from ${choices.length} options`,
                type: 'choice',
                format: 'selection',
                isHeader: true,
                path: path,
                choiceType: choiceType.toLowerCase(),
                optionsCount: choices.length
            },
            position: { ...currentPosition },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 220px; height: 60px; background: #f3e5f5; border: 2px solid #7b1fa2;'
        };
        nodes.push(headerNode);
        currentPosition.y += nodeSpacing;

        // generate option nodes
        choices.forEach((choice, index) => {
            const optionNode = {
                id: `choice-option-${path}-${index}`,
                type: 'itemNode',
                data: {
                    label: `Option: ${choice.key}`,
                    description: `choice option: ${choice.key}`,
                    type: 'choice_option',
                    format: 'option',
                    path: `${path}.${choice.key}`,
                    optionKey: choice.key,
                    optionValue: choice.value,
                    isChoiceOption: true
                },
                position: { ...currentPosition },
                draggable: true,
                selectable: true,
                deletable: false,
                style: 'width: 200px; height: 60px; background: #fce4ec;'
            };
            nodes.push(optionNode);
            currentPosition.y += nodeSpacing;

            // if choice has properties, generate nodes for them
            if (choiceComponent.properties && choiceComponent.properties[choice.key]) {
                const choiceProperty = choiceComponent.properties[choice.key];
                
                if (choiceProperty.type === 'object' && choiceProperty.properties && !choiceProperty.properties['#text']) {
                    // complex choice option
                    const complexOptionNodes = generateComplexChoiceOption(
                        choiceProperty, 
                        `${path}.${choice.key}`, 
                        currentPosition
                    );
                    nodes.push(...complexOptionNodes);
                    currentPosition.y += nodeSpacing * complexOptionNodes.length;

                } else if (choiceProperty.type === 'object' && choiceProperty.properties && choiceProperty.properties['#text']) {
                    // simple choice option
                    const simpleOptionNode = generateSimpleChoiceOption(
                        choiceProperty, 
                        `${path}.${choice.key}`, 
                        choice.key, 
                        currentPosition
                    );
                    if (simpleOptionNode) {
                        nodes.push(simpleOptionNode);
                        currentPosition.y += nodeSpacing;
                    }
                }
            }
        });

        return nodes;
    }

    function generateComplexChoiceOption(optionComponent: any, optionPath: string, position: { x: number, y: number }): any[] {
        const nodes: any[] = [];
        let currentPos = { ...position };

        if (optionComponent.properties) {
            Object.entries(optionComponent.properties).forEach(([key, value]: [string, any]) => {
                const currentPath = `${optionPath}.${key}`;

                if (value.type === 'object' && value.properties && value.properties['#text']) {
                    const simpleNode = {
                        id: `choice-complex-simple-${currentPath}`,
                        type: 'itemNode',
                        data: {
                            label: `${key} (Choice Field)`,
                            description: value.properties['#text'].description || `choice field: ${key}`,
                            type: value.properties['#text'].type || 'string',
                            format: value.properties['#text'].format || 'text',
                            required: optionComponent.required?.includes(key) || false,
                            path: currentPath,
                            isChoiceField: true
                        },
                        position: { ...currentPos },
                        draggable: true,
                        selectable: true,
                        deletable: false,
                        style: 'width: 200px; height: 60px; background: #f8bbd9;'
                    };
                    nodes.push(simpleNode);
                    currentPos.y += nodeSpacing;
                }
            });
        }

        return nodes;
    }

    function generateSimpleChoiceOption(optionComponent: any, optionPath: string, optionKey: string, position: { x: number, y: number }): any | null {
        if (!optionComponent.properties || !optionComponent.properties['#text']) {
            return null;
        }

        const textProps = optionComponent.properties['#text'];

        return {
            id: `choice-simple-option-${optionPath}`,
            type: 'itemNode',
            data: {
                label: `${optionKey} (Choice)`,
                description: textProps.description || `simple choice option: ${optionKey}`,
                type: textProps.type || 'string',
                format: textProps.format || 'text',
                required: false,
                path: optionPath,
                isChoiceField: true
            },
            position: { ...position },
            draggable: true,
            selectable: true,
            deletable: false,
            style: 'width: 200px; height: 60px; background: #f8bbd9;'
        };
    }

    // generate nodes when in node generation mode
    $: if (generateNodes && choiceComponent) {
        const generatedNodes = generateChoiceNodes();
        dispatch('choiceNodesGenerated', { nodes: generatedNodes });
    }
</script>
