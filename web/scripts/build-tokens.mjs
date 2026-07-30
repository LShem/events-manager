// Genere src/styles/_tokens.scss depuis les fichiers de tokens de web/tokens/.
//
// Chaine : Figma -> tokens/app.tokens.json (DTCG) -> Style Dictionary -> CSS custom properties
// prefixees --app-*. Les couleurs semantiques sortent composees en light-dark(), sur le meme
// modele que les --mat-sys-* d'Angular Material, qui sont deja emis ainsi par mat.theme()
// (theme-type: color-scheme par defaut).
//
// A lancer via `npm run tokens`. Les hooks prebuild / prestart / prewatch / pretest de
// package.json l'appellent automatiquement : un clone frais n'a aucune etape manuelle a faire.

import StyleDictionary from 'style-dictionary';
import { fileHeader } from 'style-dictionary/utils';

// Chemins relatifs a web/, ou tournent les scripts npm.
const LIGHT_SOURCE = 'tokens/app.tokens.json';
const DARK_SOURCE = 'tokens/modes/dark.tokens.json';
const FORMAT_NAME = 'css/app-tokens';

// transformGroup 'css' seul : size/rem restitue telle quelle l'unite px deja portee par chaque
// token dimension, et color/css serialise les couleurs DTCG sRGB en hexadecimal. Aucune
// transformation d'unite a ecrire.
const cssPlatform = {
  transformGroup: 'css',
  prefix: 'app',
  buildPath: 'src/styles/',
};

/**
 * Resout un jeu de tokens sans rien ecrire sur le disque.
 *
 * Les deux jeux passent par la meme plateforme, donc par les memes transforms : les valeurs
 * claires et sombres sont strictement comparables en sortie.
 *
 * @param {string[]} source
 * @returns {Promise<Map<string, import('style-dictionary/types').TransformedToken>>} token
 *   transforme, indexe par chemin
 */
async function resolveTokens(source) {
  const dictionary = await new StyleDictionary({
    source,
    platforms: { css: { ...cssPlatform, files: [] } },
  }).getPlatformTokens('css');

  return new Map(dictionary.allTokens.map((token) => [token.path.join('.'), token]));
}

/**
 * Le fichier d'overrides sombres est resolu seul, pas fusionne avec la source.
 *
 * Deux raisons. D'abord il se suffit a lui-meme : il declare des $value litteraux complets, pas
 * des alias vers la source. Ensuite fusionner deux fichiers qui definissent les memes chemins
 * fait emettre a Style Dictionary un avertissement de collision de tokens, legitime dans le cas
 * general, et la boucle /check du depot traite tout avertissement comme un echec. Resoudre
 * separement supprime la cause de l'avertissement au lieu de le masquer.
 */
const darkTokens = await resolveTokens([DARK_SOURCE]);
const lightTokens = await resolveTokens([LIGHT_SOURCE]);

// Un chemin declare en sombre mais absent de la source est une erreur de saisie (cle renommee
// dans Figma, faute de frappe) : sans ce controle il serait silencieusement ignore et le mode
// sombre du token disparaitrait sans bruit.
const orphans = [...darkTokens.keys()].filter((path) => !lightTokens.has(path));
if (orphans.length > 0) {
  throw new Error(
    `${DARK_SOURCE} declare ${orphans.length} token(s) absent(s) de ${LIGHT_SOURCE} : ` +
      `${orphans.join(', ')}. Un override sombre ne peut porter que sur un token existant ` +
      `dans la source Figma.`,
  );
}

// light-dark() n'est valide qu'en contexte couleur. Un override sombre sur un token d'un autre
// type produirait par exemple `--app-spacing-md: light-dark(16px, 24px)` : syntaxe acceptee
// telle quelle dans une custom property, donc build vert, et invalide seulement au point
// d'utilisation. Controle symetrique de celui des orphelins, meme principe d'echec bruyant.
const nonColors = [...darkTokens.keys()].filter((path) => lightTokens.get(path).$type !== 'color');
if (nonColors.length > 0) {
  throw new Error(
    `${DARK_SOURCE} declare ${nonColors.length} override(s) sur un token qui n'est pas de type ` +
      `color : ${nonColors.join(', ')}. Seules les couleurs peuvent sortir en light-dark().`,
  );
}

// Regle d'emission, volontairement independante des valeurs : un token sort en
// light-dark(clair, sombre) si et seulement si son chemin est declare dans le fichier
// d'overrides sombres. Consequence assumee, --app-color-on-warning sort en
// light-dark(#2a1d00, #2a1d00) : cette couleur ne change pas entre modes, et l'ecrire
// explicitement vaut mieux qu'une sortie dont la forme dependrait des donnees.
StyleDictionary.registerFormat({
  name: FORMAT_NAME,
  format: async ({ dictionary, file }) => {
    const header = await fileHeader({ file });

    const declarations = dictionary.allTokens.map((token) => {
      const light = token.$value;
      const dark = darkTokens.get(token.path.join('.'))?.$value;
      const value = dark === undefined ? light : `light-dark(${light}, ${dark})`;

      return `  --${token.name}: ${value};`;
    });

    return `${header}:root {\n${declarations.join('\n')}\n}\n`;
  },
});

await new StyleDictionary({
  // Liste explicite, jamais un glob tokens/** : le fichier d'overrides sombres ne doit pas
  // entrer dans le jeu clair.
  source: [LIGHT_SOURCE],
  platforms: {
    css: {
      ...cssPlatform,
      files: [
        {
          destination: '_tokens.scss',
          format: FORMAT_NAME,
          options: {
            // En-tete sans horodatage (fileHeaderTimestamp est false par defaut en v5) : le
            // fichier genere est versionne, un horodatage le ferait diverger a chaque build.
            fileHeader: () => [
              'Do not edit directly, this file was auto-generated.',
              `Clair  : ${LIGHT_SOURCE} (export Figma, format DTCG)`,
              `Sombre : ${DARK_SOURCE}`,
              'Regenerer : npm run tokens',
            ],
          },
        },
      ],
    },
  },
}).buildAllPlatforms();
