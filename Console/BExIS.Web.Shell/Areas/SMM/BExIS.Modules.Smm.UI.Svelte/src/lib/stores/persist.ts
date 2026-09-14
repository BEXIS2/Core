import { writable } from 'svelte/store';

export function persisted(key, initialValue) {
    // 1. Check if we have a saved value in localStorage
    const saved = typeof window !== 'undefined' ? localStorage.getItem(key) : null;

    // 2. Use saved value if it exists, otherwise use initialValue
    const data = saved ? JSON.parse(saved) : initialValue;

    const store = writable(data);

    // 3. Listen for changes and save them to localStorage
    if (typeof window !== 'undefined') {
        store.subscribe(value => {
            localStorage.setItem(key, JSON.stringify(value));
        });
    }

    return store;
}