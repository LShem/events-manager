# Front Angular 22

> Mémoire imbriquée : ce fichier complète le CLAUDE.md racine quand le travail se passe sous web/.
> Les règles globales restent d'application : gouvernance des commits, sous-agents, boucle de vérification.

## Defaults v22 à respecter (ne génère PAS de legacy)

- Composants standalone, signals partout, OnPush implicite, Zoneless (pas de zone.js, jamais).
- Réactivité : `signal()`, `computed()`, `linkedSignal()` ; `effect()` avec parcimonie. Entrées/sorties : `input()`, `output()`, `model()`. PAS de décorateurs `@Input()`/`@Output()`, pas de `BehaviorSubject` pour de l'état local.
- Formulaires : Signal Forms (PAS de ReactiveForms RxJS legacy, pas de `FormBuilder`).
- Lecture de données : Resource API (`resource` / `httpResource`). Mutations : `HttpClient` injecté dans un service.
- Routing : `provideRouter` + lazy loading via `loadComponent`/`loadChildren` (PAS de `provideRoutes`, pas de `NgModule`).
- Templates : control flow natif `@if` / `@for` (avec `track` obligatoire) / `@switch`. PAS de `*ngIf` / `*ngFor`.
- Injection : `inject()` en initialisation de champ, pas d'injection par paramètres de constructeur.
- Bindings hôte : propriété `host` du décorateur, pas de `@HostBinding`/`@HostListener`.
- HttpClient : FetchBackend par défaut (limite connue : pas d'événements de progression d'upload).

## Structure sous web/

- `src/app/layout/` : shell back-office (toolbar, navigation, page 404).
- `src/app/features/<domaine>/` : une feature par domaine d'API (events, orders...), routes lazy par feature.
- `src/app/core/` : configuration d'app (provideHttpClient, base URL API, intercepteurs à venir).
- Modèles TS écrits à la main, alignés champ à champ sur les DTOs de l'API (contrats en primitifs). Pas de génération de client.

## Outillage

- **Gestionnaire de paquets : pnpm**, déclaré par le champ `packageManager` de `package.json` (lu par Corepack et par pnpm lui-même) et par `cli.packageManager` d'`angular.json` (ce qu'invoquent `ng add` / `ng update`). Ce sont des déclarations, pas des verrous : rien ne fait échouer un `npm install` lancé par erreur — d'où la règle ci-dessous. `pnpm-lock.yaml` est versionné ; il n'y a plus de `package-lock.json`. Ne jamais lancer `npm` ni `npx` ici — `pnpm install`, `pnpm <script>`, et `pnpm dlx` à la place de `npx`. Installation reproductible : `pnpm install --frozen-lockfile`.
- pnpm 10 **bloque par défaut les scripts d'install des dépendances** (surface d'attaque supply-chain). Les quatre paquets concernés ici (`@parcel/watcher`, `esbuild`, `lmdb`, `msgpackr-extract`) sont des accélérateurs natifs optionnels : build et tests sont verts sans eux, ils sont donc déclarés dans `pnpm.ignoredBuiltDependencies`. Conséquence utile : l'encadré d'avertissement à l'install reste vide, donc un nouveau paquet natif s'y verra immédiatement. Si un jour l'un d'eux devient nécessaire, le passer dans `pnpm.onlyBuiltDependencies` — jamais un blanc-seing global.
- Angular Material comme design system, thème M3 dérivé d'une couleur source (voir « Design tokens et theming »).
- Serveur MCP Angular (`angular-cli`) disponible : le consulter pour le contexte workspace et les bonnes pratiques avant de générer.
- TypeScript strict tel que scaffoldé : ne jamais l'affaiblir, aucun `any` non justifié.
- Polices Roboto et Material Icons chargées depuis les CDN Google (`index.html`). Le self-hosting fait l'objet d'une unité dédiée : ne pas l'introduire au passage.

## Design tokens et theming

Deux sources de vérité qui ne se recouvrent pas. **Ne jamais redéfinir en `--app-*` un token que Material fournit déjà** : un doublon, c'est deux vérités qui divergent au premier changement.

- **Material 3** couvre les couleurs de rôle (`--mat-sys-primary`, `surface`, `error`, `outline`...), l'échelle typographique complète, les rayons (`--mat-sys-corner-*`) et les élévations. Les tokens typographiques sont des **shorthands `font` complets** (graisse, taille, interligne, famille) : ne jamais les surcharger avec une simple taille.
- **Les `--app-*`** comblent ce que Material n'a pas : les espacements (`--app-spacing-xs` → `xxl`) et les couleurs sémantiques succès / avertissement (`--app-color-success`, `--app-color-on-success`, `--app-color-warning`, `--app-color-on-warning`).

Chaîne de génération :

```
Figma  ──►  tokens/app.tokens.json   ──┐
            (export DTCG, mode clair)   ├─►  scripts/build-tokens.mjs  ──►  src/styles/_tokens.scss
            tokens/modes/dark.tokens.json ┘        (Style Dictionary)          (--app-*, light-dark())
```

- `tokens/app.tokens.json` est la **source, produite par Figma : ne jamais la modifier**. Une valeur fausse se corrige en amont dans Figma puis se réexporte. La signaler, pas la corriger.
- `tokens/modes/dark.tokens.json` porte les valeurs sombres, saisies à la main (le plan Figma gratuit n'a pas les modes de variables). Mêmes chemins de clés que la source, valeurs littérales, aucun token qui n'existe pas dans la source — le script échoue s'il en trouve un.
- Export Figma → code **semi-manuel et assumé**. Sync automatique différée : à évaluer au-delà de ~15 composants, l'API REST Variables de Figma étant réservée au plan Enterprise.

Fichiers générés — **ne jamais les éditer, les ouvrir pour y toucher signale un pipeline cassé** :

- `src/styles/_tokens.scss` — Style Dictionary, via `pnpm tokens`.
- `src/styles/_theme-colors.scss` — palette M3 dérivée de `#B0413E` par `ng generate @angular/material:theme-color --primary-color='#B0413E' --directory=src/styles/ --interactive=false`. Régénéré à la main uniquement, pas au build : la reproductibilité du schematic dépend de sa non-modification. Seul `primary` est fixé, M3 dérive le reste.

Les deux sont **versionnés**. Corollaire, règle de non-dérive : `pnpm tokens` suivi de `git diff --exit-code src/styles/_tokens.scss` doit rendre un arbre propre. Un diff signifie que le généré commité est périmé (typiquement après un bump de `style-dictionary`) : le recommiter, jamais l'éditer. Ce contrôle deviendra un contrôle de CI.

Mode sombre : `mat.theme()` tourne avec son `theme-type` par défaut (`color-scheme`), donc Material émet déjà chaque `--mat-sys-*` en `light-dark(clair, sombre)`. Les `--app-color-*` reprennent la même convention, et `body { color-scheme: light dark; }` fait basculer les deux ensemble sur le réglage système. Ne pas introduire de sélecteur `.dark` ni de `@media (prefers-color-scheme)` en parallèle.

Aucune étape manuelle : les scripts `start` / `build` / `watch` / `test` de `package.json` enchaînent explicitement `pnpm tokens && ng ...`. Un clone frais fait `pnpm install` puis n'importe lequel des quatre.

**Ne pas revenir à des hooks `pre*`** (`prestart`, `prebuild`...) : pnpm ne les exécute pas par défaut (`enablePrePostScripts` vaut `false`). Comme `_tokens.scss` est versionné, le build resterait vert avec des tokens périmés — la panne serait silencieuse. L'enchaînement explicite ne dépend d'aucun réglage.

## Conventions

- Un service par domaine d'API. Pas de logique métier dans les composants ni les services front : elle vit dans l'API, le front orchestre l'affichage et la saisie.
- Nommage : conventions du générateur v22, pas de retour aux anciens suffixes si le style guide ne les génère plus.
- Tests : specs ciblées sur le comportement (runner du scaffold v22) ; E2E Playwright gérés au niveau racine (`tests/e2e`), fin de roadmap.

## Vérification

- La boucle `/check` du repo couvre web/ : elle lance `pnpm -C web run build` puis `pnpm -C web test --watch=false` quand web/ est touché (commandes faisant foi dans `.claude/commands/check.md` ; le `--watch=false` est obligatoire, `@angular/build` met `watch` à `true` en terminal interactif). La lancer en fin de modification comme partout ailleurs, zéro erreur et zéro warning avant de rendre la main. Pas de lint configuré avant la semaine 13.
