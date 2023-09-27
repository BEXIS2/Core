
import type { listItemType } from '@bexis2/bexis2-core-ui';
import { displayPatternStore } from '../../store';
import { get } from 'svelte/store';

export function updateDisplayPattern(type, reset = true) {
 // currently only date, date tim e and time is use with display pattern.
 // however the serve only now datetime so we need to preselect the possible display pattern to date, time and datetime
 const allDisplayPattern = get(displayPatternStore);
 let displayPattern:listItemType[];

 if (type != undefined) {
  if (type.text.toLowerCase() === 'date') {
   // date without time
   displayPattern = allDisplayPattern.filter(
    (m) =>
     m.group.toLowerCase().includes(type.text) &&
     (!m.text.toLowerCase().includes('h') || !m.text.toLowerCase().includes('s'))
   );
  } else if (type.text.toLowerCase() === 'time') {
   // time without date
   displayPattern = allDisplayPattern.filter(
    (m) =>
     m.group.toLowerCase().includes(type.text) &&
     (!m.text.toLowerCase().includes('d') || !m.text.toLowerCase().includes('y'))
   );
  } else if (type.text.toLowerCase() === 'datetime') {
   // both
   displayPattern = allDisplayPattern.filter((m) => m.group.toLowerCase().includes(type.text));
  } else {
   displayPattern = [];
  }
 } else {
  displayPattern = [];
 }

 return displayPattern;
}


export function updateGroup(value: string, phrase: string) {
  // console.log("updateGroup",value,phrase);

  const othersText = 'other';

  if (value == othersText) {
    return phrase;
  } else {
    if (value.includes(phrase)) {
      return value;
    } else {
      return (value += ' | ' + phrase);
    }
  }
}
