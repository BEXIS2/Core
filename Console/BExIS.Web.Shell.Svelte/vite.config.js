import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'


// https://vitejs.dev/config/
export default defineConfig(({ command, mode, ssrBuild }) => {

    return {
      //mode:'development',
      // build specific config
      build:{
        manifest:true,
        outDir:"public",
        //assetsDir:'build',
        rollupOptions:{
          input:[
            "src/menu.js"
          ],
          output: {
            inlineDynamicImports:true,
            chunkFileNames: `[name].js`,
            entryFileNames: "[name].js",
            assetFileNames: "[name].[ext]",
            dir: "../BExIS.Web.Shell/Scripts/svelte/"
          }

          }
      },

      plugins: [svelte()]
    }
  
})

