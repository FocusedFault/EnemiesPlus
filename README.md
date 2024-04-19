# Enemies Plus

## Enemy Changes

- Beetle Family - The debuff BeetleJuice is now stackable, lasting 5 seconds, and each stack applied resets the timer of previous stacks.
- Beetle - now spit a small projectile at short range instead of headbutting. The projectile applies a stack of BeetleJuice.
- Beetle Guard - has a new ability that increases own armor (+100) and increases nearby allies' attack/move speed (+25%). Does not apply if ally already has buff.
- Beetle Queen - spit projectiles and acid pools apply BeetleJuice. BeetleWard now keep stacking BeetleJuice the longer you're inside of it.

- Lemurian - Bite now has a slight lunge

- Lesser Wisp - Fires more "bullets" (3 -> 6) but has the same damage
- Greater Wisp - credit cost reduced (200 -> 120) (brass contraptions cost 60 as a reference)

- Imp - now throw 2 void spikes at a more distant range instead of slashing at close range

- Brass Contraption - new ability BuffBeam, makes 1 nearby Golem/BeetleQueen class (size) enemy invincible for a short time (cannot target other brass contraptions)

- Lunar Family - new debuff Helfire, stackable, lasting 10 seconds, non-lethal, burns 10% of your full combined HP (+shield) while preventing healing and regen
- Lunar Golem - new ability LunarShell, buffs self to have +200 armor and attacks inflict Helfire while buff is active
- Lunar Wisp - increased acceleration (15 -> 20) and move speed (18 -> 20), miniguns spin up at closer range, orb applies Helfire

## Enemies TODO (maybe)

- Stone Titan
- Wandering Vagrant
- Magma/Overloading Worm
- Grovetender
- Imp Overlord
- Xi Construct
- Solus Control Unit
- Grandparent

- Void Reaver
- Void Jailer
- Void Devastator

## Changelog

**1.0.1**

- Fixes bell's buffbeam applying VFX to unintented targets
- Improves bell's buffbeam search to guarantee a pick golem/beetlequeen class enemies in groups (if in range)
- Makes Helfire stackable
- Removes pest nerf
- Changes BeetleSpit to fire directly instead of using an arc
- Reduces BeetleSpit delay (1.25s -> 1s)
- Increases BeetleSpit max firing distance (15 -> 20)
- Reduces Beetle Guard RallyCry armor (200 -> 100)
- Increases RallyCry AoE (13m -> 16m)
- Increases RallyCry HP activation threshold from 80% -> 100%
- Increases Lunar Golem LunarShell armor (100 -> 200)
- Increases LunarShell HP activation threshold from 75% -> 100%
- Changes LunarShell move type (Stop -> Chase)
- Increases Imp AI blink distance (should prevent blinking when too close)
- Reduces Lunar Wisp acceleration (30 -> 20)
- Fixes Helfire being applied on modded wisp orbs (Umbral)

**1.0.0**

- Reworked and Improved
- Adds changes to more enemies
- Adds TweakAI as a dependency
