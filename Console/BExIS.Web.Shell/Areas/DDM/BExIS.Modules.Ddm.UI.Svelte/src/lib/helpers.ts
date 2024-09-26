import type { Columns } from '@bexis2/bexis2-core-ui';

export const pascalToCamel = (str: string) => {
	return str
		.replace(/(?:^\w|[A-Z]|\b\w)/g, function (word, index) {
			return index === 0 ? word.toLowerCase() : word.toUpperCase();
		})
		.replace(/\s+/g, '');
};

export const convertTableData = (columns: Columns, rows: any, header: any, visibleHeaders: any) => {
	const tableData = rows.reduce((acc: any[], row: any) => {
		const rowData = row.Values.reduce((acc: any, value: any, index: number) => {
			acc[pascalToCamel(header[index].Name)] = value || '';

			if (!columns || !columns[pascalToCamel(header[index].Name)]) {
				const exclude =
					visibleHeaders.findIndex((item: any) => item.Name === header[index].Name) === -1;

				columns = {
					...columns,
					[pascalToCamel(header[index].Name)]: {
						header: header[index].DisplayName,
						exclude
					}
				};
			}

			return acc;
		}, {});
		acc.push(rowData);
		return acc;
	}, []);

	return {
		columns,
		data: tableData
	};
};
