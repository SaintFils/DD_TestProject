**English** | [Русский](README.ru.md)

# Turn-Based Battle Prototype

A small turn-based battle prototype in the spirit of Darkest Dungeon, made in Unity 6.

## Releases

- [v0.0.1](https://github.com/SaintFils/DD_TestProject/releases/tag/v0.0.1): the base version of the task without the visual polish and extra features (branch `dev/oleg/cleanup`).
- [v1.0.0](https://github.com/SaintFils/DD_TestProject/releases/tag/v1.0.0): the current `master` with a PC build attached as a zip.

## How to run

- Unity **6000.3.12f1**.
- After the first import the editor opens the menu scene by itself. If it doesn't, open `Assets/_src/Runtime/Scenes/MenuScene.unity`. Press Play.
- **Quick Battle** starts a player vs AI battle with the preset teams, **Play** opens the team builder.

### Controls: team builder

- Drag a unit card onto a team frame to put it into the first free slot, starting from the front.
- Click a card to see the unit's stats and abilities.
- Click a unit in a slot to remove it.
- Each team has a toggle that switches its controller: Player or AI ("Steve").
- **Random** fills all empty slots, **Play** starts the battle, **Back** returns to the menu.

### Controls: battle

- Hover an ability to highlight valid targets, click it to select.
- Hover a target to see which units will be affected, left-click to use the ability.
- Right-click or Esc cancels the selected ability.
- Esc or the pause button pauses the battle (resume, restart, exit to menu).
- Hover a card in the initiative queue to highlight that unit.

### Battle settings

`Assets/_src/Runtime/Configs/battle_config.asset`:

- **Left Team / Right Team**: preset teams from `Configs/Teams`, used by Quick Battle and when the battle scene is started directly.
- **Use Fixed Seed / Seed**: every battle uses this seed instead of a random one. The current seed is shown at the top of the battle log, so a specific battle can be replayed with the same teams (Quick Battle or the battle scene). Restart always keeps the current seed.
- **Reshuffle Equal Speed Each Round**: units with equal Speed are ordered randomly once per battle, with this option the order is re-rolled every round.

## What's done

Everything from the task, including both optional iterations: pause, restart and exit, and data-driven configuration.
With the remaining time, mostly for fun, I improved the visuals and added a few features: a team builder, hotseat and AI vs AI modes, sprite animations, a parallax background and ability icons.

## Key decisions

- **Logic is separated from visuals.** The model (`BattleState`, `BattleSide`, `BattleUnit`, `InitiativeQueue`) and the rules (`BattleRules`) are plain C#. The battle talks to the visuals only through `IBattlePresenter`, so the whole battle is covered by EditMode tests without a scene.
- **Async battle loop.** `BattleLoop` (UniTask) goes round → turn → command → resolve → animation, and every step is awaited, so actions never overlap and the next turn can't start early. After the player picks a target their input is reset, so extra clicks do nothing.
- **Resolve first, apply at impact.** `BattleRules.Resolve` returns an immutable `ActionResult` with one `ActionEffect` per affected unit. `BattleState.Apply` applies it between the windup and the follow-through, so HP changes exactly when the hit lands.
- **One interface for both sides.** `ISideController` is implemented by `PlayerController` and `AiController`, so the player and the AI obey the same rules. Hotseat and AI vs AI came almost for free.
- **Safe interruption.** `BattleSession` owns the battle's cancellation token and cancels it before any scene load. All tweens are awaited via `ToUniTask(KillAndCancelAwait)` and linked to their objects, R3 subscriptions are disposed together with their views, pause is `Time.timeScale`.
- **Data-driven abilities.** One configurable ability (actor position, target side and filter, area, effect, power, attack penalty) instead of a class per ability. Units, teams, AI and visuals are ScriptableObjects.
- **Stack.** Zenject is the composition root: the project context keeps session data (seed, teams, controllers), scene installers build the battle and the menu, factories create the views. R3 handles reactive state, DOTween and UniTask provide awaitable animations, NaughtyAttributes replaces Odin. Addressables or any other asset loading layer weren't used: everything is referenced directly, and for a project this small it wouldn't pay off.

Formulas: hit chance = 75% + 5% × (Attack - Defense), clamped between 10% and 95% (configurable). A ranged attack from the front line gets -3 Attack.
The AI heals an ally below 50% HP. Otherwise it picks the action with the best expected damage, with a bonus for kills, choosing randomly among options close to the best.

## Known limitations

- Screens (round banner, pause, result, team builder) are placed in the scenes in advance instead of being spawned by a screen system. This is a deliberate simplification for the scope of the task.
- The ability constructor doesn't validate nonsensical combinations, e.g. damage targeting self.
- The AI is intentionally simple: it evaluates only the current turn, without any lookahead. In case such simple decisions end up in a loop, an AI vs AI battle ends in a draw after 20 rounds (hardcoded).

## Third-party

- Libraries: Extenject (Zenject), UniTask, R3, DOTween, NaughtyAttributes, SoftMaskForUGUI, NuGetForUnity, Valhalla Public Getters Generator.
- Art:
  - [Platformer Fantasy Set1](https://assetstore.unity.com/packages/2d/environments/platformer-fantasy-set1-159063)
  - [Free RPG Fantasy Spell Icons](https://assetstore.unity.com/packages/2d/gui/icons/free-rpg-fantasy-spell-icons-200511)
  - [Fantasy Wooden GUI: Free](https://assetstore.unity.com/packages/2d/gui/fantasy-wooden-gui-free-103811)
  - [Hero and Opponents Animation](https://assetstore.unity.com/packages/2d/characters/hero-nad-opponents-animation-140776)
  - [Free Pixel Font: Thaleah](https://assetstore.unity.com/packages/2d/fonts/free-pixel-font-thaleah-140059)

## Time

About 15 hours for the task itself and 6-7 more for the extras.
