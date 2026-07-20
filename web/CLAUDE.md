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

- Angular Material comme design system, thème prebuilt pour l'instant, composants layout en place. Theming et design tokens : semaine 9 (Style Dictionary, `web/tokens/`), ne pas anticiper.
- Serveur MCP Angular (`angular-cli`) disponible : le consulter pour le contexte workspace et les bonnes pratiques avant de générer.
- TypeScript strict tel que scaffoldé : ne jamais l'affaiblir, aucun `any` non justifié.

## Conventions

- Un service par domaine d'API. Pas de logique métier dans les composants ni les services front : elle vit dans l'API, le front orchestre l'affichage et la saisie.
- Nommage : conventions du générateur v22, pas de retour aux anciens suffixes si le style guide ne les génère plus.
- Tests : specs ciblées sur le comportement (runner du scaffold v22) ; E2E Playwright gérés au niveau racine (`tests/e2e`), fin de roadmap.

## Vérification

- La boucle `/check` du repo couvre web/ : elle lance `npm run build` puis `npm test` quand web/ est touché. La lancer en fin de modification comme partout ailleurs, zéro erreur et zéro warning avant de rendre la main. Pas de lint configuré avant la semaine 13.
