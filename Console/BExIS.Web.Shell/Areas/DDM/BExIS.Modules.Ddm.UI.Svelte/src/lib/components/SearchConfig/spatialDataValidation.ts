import { create, test, enforce } from 'vest';
import type { LocalSpatialMetadata } from '$lib/components/SearchConfig/SearchConfigModel';

interface SpatialDataListItem {
		id: string;
		template_name: string;
		type: 'bbox' | 'point';
		metadata_nodes: string[];
		config: LocalSpatialMetadata;
	}

const suite = create((data: SpatialDataListItem) => {
   


});
export default suite;