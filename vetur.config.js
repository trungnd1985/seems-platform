// vetur.config.js
/** @type {import('vls').VeturConfig} */
module.exports = {
  settings: {
    "vetur.useWorkspaceDependencies": true,
    "vetur.experimental.templateInterpolationService": true
  },
  projects: [
    {
      root: './src/frontend/public',
      package: './package.json',
      tsconfig: './tsconfig.json',
      snippetFolder: './.vscode/vetur/snippets',
      globalComponents: [
        './components/**/*.vue'
      ]
    },
    {
      root: './src/frontend/admin',
      package: './package.json',
      tsconfig: './tsconfig.json',
      snippetFolder: './.vscode/vetur/snippets',
      globalComponents: [
        './src/components/**/*.vue'
      ]
    }
  ]
}