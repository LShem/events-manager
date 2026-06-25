# Projet : back-office événements + commandes

## But

Application web. Back-office (gestion d'événements et commandes, export Excel de stats) +
front public (formulaire de commande, QR de paiement EPC SEPA). Le projet est le terrain
d'apprentissage de l'agentic coding : maîtriser l'agentic est le but, l'appli est le véhicule.

## Stack

- API : C# / .NET 10, ASP.NET Core. EF Core 10. SQL Server.
- Front : Angular 22 (signals, standalone, Signal Forms, OnPush/Zoneless par défaut), Angular Material.
- Design tokens : export semi-manuel depuis Figma via Style Dictionary (variables CSS/SCSS). Sync automatique Figma→code **différée** : à évaluer quand > ~15 composants. Ne pas automatiser avant ce seuil.
- Tests : xUnit + Testcontainers SQL Server (intégration), Playwright (E2E fin, parcours critiques uniquement).

## Architecture (Clean Architecture, 4 couches)

- `Domain` : entités, value objects, invariants. **NE DÉPEND DE RIEN.**
- `Application` : commands/queries + handlers, validators (FluentValidation), mappers (méthodes d'extension). Dépend de Domain.
- `Infrastructure` : EF Core, DbContext. Dépend de Application.
- `Api` : endpoints, DI. Dépend de Application (et Infrastructure pour le wiring).
- Un test d'architecture (NetArchTest) garantit ces dépendances à chaque build.

## Conventions NON NÉGOCIABLES

- **PAS de MediatR**, **PAS d'AutoMapper**.
- CQRS light : objets `Command`/`Query` + handler dédié, dispatch par **injection DI directe**.
- Mappers = méthodes d'extension (`ToDto()`, `ToEntity()`), **aucune logique métier dedans**, testés.
- DDD tactique léger : `Money`, `EmailAddress`, IDs typés (`EventId`, `OrderId`) ; agrégats `Event` et `Order` avec invariants garantis dans la racine d'agrégat. Pas de DDD stratégique, pas de bus de domain events.
- Tests : **pyramide**. Gros volume en unitaire (domaine) + intégration (handlers + EF). E2E fin (3-5 parcours critiques max).

## Documentation à jour (MCP Context7)

Context7 est configuré dans `.claude/settings.json`. Avant de générer du code sur une librairie tierce, utilise-le pour récupérer la documentation versionnée à jour.

Librairies concernées : .NET 10, ASP.NET Core, EF Core 10, FluentValidation, xUnit, Testcontainers, Angular 22, Angular Material, Style Dictionary.

Pour l'activer dans un prompt : ajoute `use context7` ou demande explicitement la doc de la librairie visée.

## Workflow de vérification

- Après toute modification de code : lancer `/verify` et corriger jusqu'au vert **avant de s'arrêter**.
- Pour toute tâche non triviale : proposer un **plan d'abord**, attendre validation, puis exécuter.

## Structure du repo

```
/
  CLAUDE.md
  global.json         # pin du SDK .NET 10
  events-manager.sln  # solution à la racine
  .claude/
    commands/         # /verify, /slice, ...
    agents/           # reviewer, test-writer, ...
  src/                # projets .NET uniquement
    Api/
    Application/
    Domain/
    Infrastructure/
  web/                # app Angular (semaine 8), avec son propre CLAUDE.md
  tests/
    UnitTests/
    IntegrationTests/
    e2e/              # Playwright (semaine 12)
```

## Style de réponse attendu

Direct, sans fioritures. Explications techniques complètes et explicites.
Challenge les choix : si une direction est mauvaise ou qu'il y a mieux, le dire immédiatement.
