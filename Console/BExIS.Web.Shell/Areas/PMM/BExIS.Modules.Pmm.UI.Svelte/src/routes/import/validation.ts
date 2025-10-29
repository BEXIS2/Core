import type { ValidationType, validationReturn, errorItem } from './models';

export function validateRows(data: any[], codeColumns: string[], dataColumns: string[], validationReturnObj: validationReturn): validationReturn {
    let columnErrors: { [key: string]: any[] } = {};

    data.forEach((row: any, rowIndex: number) => {
        if (!row || Object.values(row).every((val) => (val ?? '').toString().trim() === '')) {
        } else {
            let validationType: ValidationType = validateRow(row, codeColumns, dataColumns);

            validationType.cellError.forEach((ce) => {
                if (!columnErrors[ce.column]) {
                    columnErrors[ce.column] = [];
                }
                columnErrors[ce.column].push(rowIndex);
            });

            if (validationType.valid) {
                row = applyAvailableUponRequestRules(row);
                validationReturnObj.validData.push(row);
            } else {
                validationReturnObj.invalidDataCounter++;
                validationReturnObj.invalidData.push(row);
                validationReturnObj.errors.push({ rowIndex, cellErrors: validationType.cellError });
            }
            // console.log("valid", validData)
        }
    });
    return validationReturnObj;
}

function validateRow(row: any, codeColumns: string[], dataColumns: string[]): ValidationType {
    let cellError: errorItem[] = [];
    let rowValid: boolean = true;
    const checkColumns = [
        { columns: codeColumns, refColumn: 'Code URL' },
        { columns: dataColumns, refColumn: 'Data URL' }
    ];

    checkColumns.forEach(({ columns, refColumn }) => {
        const ref = row[refColumn];
        if (!ref) return;

        const commaCount = countCommas(ref);

        if (commaCount === 0) return; // gilt als valide

        columns.forEach((col) => {
            if (commaCount !== countCommas(row[col])) {
                cellError.push({
                    column: col,
                    errorMsg: `Number of entries does not match with ${refColumn}`
                });
                rowValid = false;
            }
        });
    });
    let validationType: ValidationType = { valid: rowValid, cellError: cellError };
    return validationType;
}

function countCommas(data: string): number {
    return data.split(',').length - 1;
}

function applyAvailableUponRequestRules(row: any) {
    function syncColumns(presentKey: string, targetKeys: string[], triggerValue: string) {
        if (!row[presentKey]) return;

        const presentValues = row[presentKey]
            .toString()
            .split(',')
            .map((v: string) => v.trim());

        for (const targetKey of targetKeys) {
            // Stelle sicher, dass die Zielzelle existiert (sonst leeres Array zum Füllen)
            let targetValues = (row[targetKey] || '')
                .toString()
                .split(',')
                .map((v: string) => v.trim());

            // Länge angleichen (falls Zielspalte kürzer ist)
            while (targetValues.length < presentValues.length) {
                targetValues.push('');
            }

            // Werte synchronisieren
            for (let i = 0; i < presentValues.length; i++) {
                if (presentValues[i].toLowerCase() === triggerValue.toLowerCase()) {
                    targetValues[i] = triggerValue;
                }
            }

            row[targetKey] = targetValues.join(', ');
        }
    }

    // Regel 1: available upon request
    syncColumns('Data present', ['Data License', 'Data Publisher'], 'available upon request');
    syncColumns('Code present', ['Code License', 'Code Publisher'], 'available upon request');

    // Regel 2: no access (nur für Data)
    syncColumns(
        'Data present',
        [
            'Data License',
            'Data Publisher',
            'Data URL',
            'Data URL resolves',
            'Data DOI',
            'Data DOI resolves'
        ],
        'no access'
    );

    syncColumns(
        'Code present',
        [
            'Code License',
            'Code Publisher',
            'Code URL',
            'Code URL resolves',
            'Code DOI',
            'Code DOI resolves'
        ],
        'no access'
    );
    return row;
}