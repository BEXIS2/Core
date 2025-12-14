## Svelte Flow UI-Component Configuration Tool

### Installation
Ensure a stable BEXIS2 Core environment with all packages installed correctly and verify all folders and files for component configuration exist.

Navigate to the DCM Module:
```bash
cd .\Console\BExIS.Web.Shell\Areas\DCM\BExIS.Modules.Dcm.UI.Svelte\ 
```

Install all dependencies (if not done already):
```bash
npm install
```

Install Svelte Flow (version 0.1.36 required for Svelte 4 compatibility):
```bash
npm install @xyflow/svelte@0.1.36
```
Run dev Server:
```bash
npm run dev
```
Click the "Component Configuration" button in the main interface.

### Core Concepts
- Choice between two interaction modes (toggleable via buttons): Edit Mode (configure component for data entry) and View Mode (configure component for data display)
- Base-Component definitions currently come from `componentManifest.json`. This file populates:
  - Component Library (separate components, each a Manifest)
  - Modes tab (submodes in each component, filtered by interaction mode)
  - Config tab (default settings, validation rules)
  - Preview tab (default variable values)

### Saving & Loading
Configured Components (e.g. changed settings, submode, mappings...) must be saved manually via the Save button in the sidebar, which downloads 3 files:

- `componentConfig_edit.json` (configured UI components for metadata input)
- `componentConfig_view.json` (configured UI components for metadata display)
- `componentPositions.json` (Svelte Flow component coordinates)

To restore state, manually move downloaded files to `src/routes/componentconfiguration/Downloads/` (filename supports numbered duplicates like `(1)`)

**Implementation:**

in the `+page.svelte`:
- download: `handleSaveAll()` calls `downloadAllConfigs()` from `Services/fileHelpers.ts`
- load: `onMount()` calls `loadConfigsFromDownloads()` from `Services/fileHelpers.ts`


### Metadata Schema
Current local load from `src/routes/componentconfiguration/Schema/` (e.g. `metadataSchema_x.json`)

To use API instead modify: `Services/apiCalls.ts`

The current metadata schema ID is hardcoded in `TreeComponent.svelte`:
```typescript
let currentSchema: number = 2;
```

### File Structure

**Schema Tree Generation**

`TreeComponent.svelte` - main schema parser
`arrayComponentWrapper.svelte`, `choiceComponentWrapper.svelte`, `complexComponentWrapper.svelte`, `simpleComponentWrapper.svelte` - recursive parsers, each for a corresponding Component (complex, simple, choice, array)

**Flow Elements**

- `NodeWithItems.svelte` - main UI component node containing variable items
- ( `ItemNode.svelte` - legacy node definition (currently unused) )
- `LeafNode.svelte` - metadata tree nodes (represents metadata schema elements)
- `SectionNode.svelte` - metadata tree sections (used as separators)
- `ButtonEdge.svelte` - custom edges with button menu
- `ResetViewButton.svelte` - button to reset zoom and fit all nodes

**Svelte Components (Sidebar)**

- `ComponentLibrary.svelte` - library containing components and submodes defined through manifest  
- `ModesTab.svelte` - submode selection with detailed descriptions
- `ConfigTab.svelte` - parameter configuration, error checking and progress display
- `PreviewTab.svelte` - intended to preview configured components, currently only displays default variable values

**Services**

`Services/fileHelpers.ts` - file operations

`Services/apiCalls.ts` - schema fetching

**Reference**
Config and Manifest schemas, as well as Bachelor's thesis, available on GitLab:
https://git.uni-jena.de/fusion/teaching/thesis/julius-halank-ba/fusion_bexis2_docs_thesis/-/tree/main/thesis/final_versions
