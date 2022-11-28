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
            "src/main.js"

          ],
          output: {
            inlineDynamicImports:false,
            chunkFileNames: `assets/[name].js`,
            entryFileNames: "[name].js",
            assetFileNames: "assets/[name].[ext]",
            dir: "../BExIS.Modules.Rpm.UI/Scripts/svelte/"
          }

          }
      },

      plugins: [svelte()]
    }
  
})

