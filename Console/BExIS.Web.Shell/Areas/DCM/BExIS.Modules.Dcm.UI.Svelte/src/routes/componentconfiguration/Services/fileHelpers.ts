export interface ConfigFile {
  components: any[];
}

export interface PositionFile {
  [nodeId: string]: {
    x: number;
    y: number;
  };
}

// download JSON data as file
export function downloadJsonFile(data: any, filename: string) {
  const jsonString = JSON.stringify(data, null, 2);
  const blob = new Blob([jsonString], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

// download all 3 config files at once
export function downloadAllConfigs(
  editConfig: ConfigFile, 
  viewConfig: ConfigFile, 
  positions: PositionFile
) {
  downloadJsonFile(editConfig, 'componentConfig_edit.json');
  setTimeout(() => {
    downloadJsonFile(viewConfig, 'componentConfig_view.json');
  }, 100);
  setTimeout(() => {
    downloadJsonFile(positions, 'componentPositions.json');
  }, 200);
}

export function createEmptyConfig(): ConfigFile {
  return { components: [] };
}

export function createEmptyPositions(): PositionFile {
  return {};
}

// load from config folder and autoselect latest versions
// looping over suffix from 100 to "version"
export async function loadConfigsFromDownloads(): Promise<{
  edit: ConfigFile;
  view: ConfigFile;
  positions: PositionFile;
} | null> {
  try {
    const basePath = '/src/routes/componentconfiguration/Downloads/';
    
    // adjust bounds as needed
    for (let version = 100; version >= 0; version--) {
      const suffix = version === 1 ? '' : ` (${version})`;
      
      try {
        const [editRes, viewRes, posRes] = await Promise.all([
          fetch(basePath + `componentConfig_edit${suffix}.json`),
          fetch(basePath + `componentConfig_view${suffix}.json`),
          fetch(basePath + `componentPositions${suffix}.json`)
        ]);
        
        if (editRes.ok && viewRes.ok && posRes.ok) {
          const [edit, view, positions] = await Promise.all([
            editRes.json(),
            viewRes.json(),
            posRes.json()
          ]);
          
          return { edit, view, positions };
        }
      } catch (error) {
        continue;
      }
    }
    
    return null;
    
  } catch (error) {
    return null;
  }
}