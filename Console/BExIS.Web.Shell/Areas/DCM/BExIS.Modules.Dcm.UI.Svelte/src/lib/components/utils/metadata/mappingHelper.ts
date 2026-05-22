import { getSystemMappingsStore, removeJsonPathIndices } from "./metadataComponentUtils";
import { MappingComponentConfig } from "./models";

export function getMappingComponentConfig(path:string, value: any): MappingComponentConfig {

 let mappingConfig: MappingComponentConfig = new MappingComponentConfig();
	const systemMappings = getSystemMappingsStore();
		
 if(systemMappings){
  const pathWithoutIndices = removeJsonPathIndices(path);

  if(systemMappings.partyMappings.some((mapping: any) => mapping.path == pathWithoutIndices)){

						mappingConfig.isMappedToParty = true;
						mappingConfig.partyMappingObject = systemMappings.partyMappings.find((mapping: any) => mapping.path == pathWithoutIndices);
						mappingConfig.isSelector = mappingConfig.partyMappingObject.selector;

						if(mappingConfig.isSelector)
						{
        mappingConfig.selectorValue = {
         "partyId": 0,
         "value": value
        }
						}

				
				}
 }

 return mappingConfig;
}