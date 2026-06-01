// Source:
// https://blog.devgenius.io/implementing-the-myers-diff-algorithm-in-typescript-character-level-precision-5aa0430f6727

export interface DiffPart {
  value: string;
  added?: boolean;
  removed?: boolean;
}

export function getDifference(oldStr: string, newStr: string): DiffPart[] {
  const oldLen = oldStr.length;
  const newLen = newStr.length;

  // Step 1: Create a 2D grid to compute the Longest Common Subsequence (LCS)
  const grid: number[][] = Array.from({ length: oldLen + 1 }, () =>
    new Array(newLen + 1).fill(0)
  );

  for (let i = 1; i <= oldLen; i++) {
    for (let j = 1; j <= newLen; j++) {
      if (oldStr[i - 1] === newStr[j - 1]) {
        grid[i][j] = grid[i - 1][j - 1] + 1;
      } else {
        grid[i][j] = Math.max(grid[i - 1][j], grid[i][j - 1]);
      }
    }
  }

  // Step 2: Backtrack through the grid to assemble the differences
  const result: DiffPart[] = [];
  let i = oldLen;
  let j = newLen;

  while (i > 0 || j > 0) {
    if (i > 0 && j > 0 && oldStr[i - 1] === newStr[j - 1]) {
      // Characters match -> part of the original/clean sequence
      result.unshift({ value: oldStr[i - 1] });
      i--;
      j--;
    } else if (j > 0 && (i === 0 || grid[i][j - 1] >= grid[i - 1][j])) {
      // Character was added in the new string
      result.unshift({ value: newStr[j - 1], added: true });
      j--;
    } else {
      // Character was removed from the old string
      result.unshift({ value: oldStr[i - 1], removed: true });
      i--;
    }
  }

  // Step 3: Optional optimization - merge adjacent parts of the same type
  // (e.g., merge separate 'a', 'b', 'c' objects into a single 'abc' object)
  const mergedResult: DiffPart[] = [];
  for (const part of result) {
    const last = mergedResult[mergedResult.length - 1];
    if (last && last.added === part.added && last.removed === part.removed) {
      last.value += part.value;
    } else {
      mergedResult.push({ ...part });
    }
  }

  return mergedResult;
}