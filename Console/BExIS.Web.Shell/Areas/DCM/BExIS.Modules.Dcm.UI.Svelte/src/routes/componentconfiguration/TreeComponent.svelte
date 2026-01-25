<script lang="ts">
    import * as ApiCalls from './Services/apiCalls';
    import ComplexComponent from './complexComponentWrapper.svelte';
    import { createEventDispatcher } from 'svelte';

    const dispatch = createEventDispatcher();

    let schema: any;
    let s: any;
    let currentSchema: number = 2;
    let generatedNodes: any[] = [];
    
    $: schema = s;

    async function loader(currentSchema: number) {
        try {
            schema = await ApiCalls.GetMetadataSchema(currentSchema);
            s = schema;
            
            // generate nodes after schema is loaded
            if (schema) {
                generatedNodes = generateNodesFromSchema(schema);
                dispatch('nodesGenerated', { nodes: generatedNodes });
            }
        } catch (error) {
            console.error('error loading metadata schema:', error);
        }
    }

    function generateNodesFromSchema(schema: any): any[] {
        const nodes: any[] = [];
        let yPosition = 50;
        const xPosition = 50;
        const nodeSpacing = 100; // space between leaf nodes

        if (schema && schema.type === 'object' && schema.properties) {
            traverseSchema(schema, '', nodes, { x: xPosition, y: yPosition }, nodeSpacing);
        }

        return nodes;
    }

    function traverseSchema(
        schemaNode: any, 
        path: string, 
        nodes: any[], 
        position: { x: number, y: number }, 
        spacing: number
    ): { x: number, y: number } {
        if (!schemaNode || !schemaNode.properties) return position;

        Object.entries(schemaNode.properties).forEach(([key, value]: [string, any]) => {
            const currentPath = path ? `${path}.${key}` : key;
            console.log('value  for key:', key, 'is:', value);
            if (value.type === 'object' && value.properties && !value.properties['#text']) {
                console.log('processing complex object for key:', key);
                console.log('complex object properties:', value.properties);

                // complex component: add section node without handle
                const sectionNode = {
                    id: `schema-section-${currentPath}`,
                    type: 'sectionNode',
                    data: {
                        label: `Section: ${key}`,
                        description: `complex section: ${key}`,
                        type: 'section',
                        isSection: true
                    },
                    position: { x: position.x, y: position.y },
                    draggable: false,
                    selectable: false,
                    deletable: false,
                    style: 'width: 200px; height: 40px;'
                };
                nodes.push(sectionNode);
                position.y += 40; // space between sections

                // recursively traverse nested properties
                if (value.oneOf || value.anyOf || value.allOf) {
                    // handle choice components
                    const choiceSectionNode = {
                        id: `schema-choice-section-${currentPath}`,
                        type: 'sectionNode',
                        data: {
                            label: `Choice: ${key}`,
                            description: `choice section: ${key}`,
                            type: 'choice_section',
                            isSection: true
                        },
                        position: { x: position.x, y: position.y },
                        draggable: false,
                        selectable: false,
                        deletable: false,
                        style: 'width: 200px; height: 40px;'
                    };
                    nodes.push(choiceSectionNode);
                    position.y += 50; // space between sections
                } else {
                    position.y += 20; // space between section and first leaf
                    position = traverseSchema(value, currentPath, nodes, position, spacing);
                }

            } else if (value.type === 'object' && value.properties && value.properties['#text']) {
                
                console.log('processing leaf node for key:', key);
                const leafNode = {
                    id: `schema-leaf-${currentPath}`,
                    type: 'leafNode',
                    data: {
                        label: key,
                        description: value.properties['#text'].description || `metadata field: ${key}`,
                        type: value.properties['#text'].type || 'string',
                        format: value.properties['#text'].format || 'text',
                        required: false, // toggle to render required leaf nodes red
                        path: currentPath,
                        isLeaf: true,
                        is_input: false,
                        is_output: true,
                        target_variable: key,
                        is_visible: true
                    },
                    position: { x: position.x + 30, y: position.y }, // indentation
                    draggable: true,
                    selectable: true,
                    deletable: false,
                    style: 'width: 220px; height: 60px;' //previously = 180
                };
                nodes.push(leafNode);
                position.y += spacing;

                   // ...existing code...
                    } else if (value.type === 'array' && value.items) {
                        // array - section + leaf items
                        const arraySectionNode = {
                            id: `schema-array-section-${currentPath}`,
                            type: 'sectionNode',
                            data: {
                                label: `Array: ${key}`,
                                type: 'array_section',
                                isSection: true
                            },
                            position: { x: position.x, y: position.y },
                            draggable: false,
                            selectable: false,
                            deletable: false,
                            style: 'width: 200px; height: 40px;'
                        };
                        nodes.push(arraySectionNode);
                        position.y += 50;  // space between sections
                        position.y += 20; // space between section and array items
        
                        // process array items
                        console.log('processing array items for key:', key);
        
                        // object items with properties -> recurse
                       // if (value.items.type === 'array' && value.items.properties) {
                       //     console.log('array items properties:', value.items.properties);
                       //     position = traverseSchema(value.items, currentPath, nodes, position, spacing);
        
                        // primitive item types -> create a leaf node for the array item
                       // } else 
                         if (value.items.type === 'object' && value.items.properties && value.items.properties['#text'])  {
                            const arrLeaf = {
                                id: `schema-array-item-${currentPath}`,
                                type: 'leafNode',
                                data: {
                                    label: `${key} (item)`,
                                    description: value.items.description || `array item: ${key}`,
                                    type: value.items.properties['#text'].type || 'string',
                                    format: value.items.properties['#text'].format || 'text',
                                    required: false,
                                    path: currentPath,
                                    isLeaf: true,
                                    is_input: false,
                                    is_output: true,
                                    target_variable: key,
                                    is_visible: true
                                },
                                position: { x: position.x + 30, y: position.y },
                                draggable: true,
                                selectable: true,
                                deletable: false,
                                style: 'width: 220px; height: 60px;'
                            };
                            nodes.push(arrLeaf);
                            position.y += spacing;
        
                        // items use oneOf/anyOf/allOf -> create nodes for each variant (either recurse or create leaf)
                        } else if (value.items.oneOf || value.items.anyOf || value.items.allOf) {
                            const variants = value.items.oneOf || value.items.anyOf || value.items.allOf;
                            variants.forEach((variant: any, idx: number) => {
                                const variantPath = `${currentPath}[${idx}]`;
                                if (variant.type === 'object' && variant.properties) {
                                    position = traverseSchema(variant, variantPath, nodes, position, spacing);
                                } else {
                                    const vLeaf = {
                                        id: `schema-array-variant-${variantPath}`,
                                        type: 'leafNode',
                                        data: {
                                            label: `${key} (variant ${idx})`,
                                            description: variant.description || `variant ${idx} of ${key}`,
                                            type: variant.type || 'string',
                                            format: variant.format || 'text',
                                            required: false,
                                            path: variantPath,
                                            isLeaf: true,
                                            is_input: false,
                                            is_output: true,
                                            target_variable: key,
                                            is_visible: true
                                        },
                                        position: { x: position.x + 30, y: position.y },
                                        draggable: true,
                                        selectable: true,
                                        deletable: false,
                                        style: 'width: 220px; height: 60px;'
                                    };
                                    nodes.push(vLeaf);
                                    position.y += spacing;
                                }
                            });
        
                        } else {
                            console.log('unhandled array.items type for key:', key, value.items);
                        }
                    } else {
                        console.log('unhandled schema node type for key:', key, 'type:', value.type);
                    }
        // ...existing code...
        });

        return position;
    }

    loader(currentSchema);

    // console.log('treecomponent: schema loaded, nodes generated');
</script>
