# Integration Notes

These notes summarize how other systems in the project should interact with `global.GameData`.

## Spawning Entities

Use profile references directly when spawning. Example:

```gml
var profile = global.GameData.Entity.Pop.GEN1;
world_gen_spawn(profile, 5, global.GameData.SpawnFormations.Type.CLUSTERED, {x:100, y:100});
```

## Crafting Items

Recipes are stored in `GameData.Recipes`. Crafting logic should read required ingredients and produced items from these profiles rather than hard-coded values.

## Needs and Stats

Entity profiles can define base needs (`hunger`, `thirst`, `energy`) and stat ranges. When a pop initializes, copy these values from its profile into runtime variables.

## UI Display

UI components should use `display_name_key` and sprite references from item and entity profiles. This ensures localization and asset references remain consistent.

## Debugging

Consider adding utility scripts to print or visualize parts of `GameData` at runtime. A simple inspector window can help verify loaded data during development.
