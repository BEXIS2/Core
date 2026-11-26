import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vitest/config';

export default defineConfig(({ command }) => ({
	plugins: [sveltekit()],
	test: {
		include: ['src/**/*.{test,spec}.{js,ts}']
	},
	ssr: {
		noExternal: ['@skeletonlabs/skeleton']
	},
	esbuild: {
		drop: command === 'build' ? ['console', 'debugger'] : []
	}
}));
