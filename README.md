# Amish Simulator

A Unity 6 WebGL sandbox life simulation game where you live an entire Amish life.

## Game Overview

Wake at dawn, do your chores, court a spouse, raise children, attend Gmay, grow your beard, and die surrounded by grandchildren. Humor comes from earnest characters who take simple chores with solemn pride.

## Difficulty Levels

| Level | Amish Name | Day Length | Description |
|---|---|---|---|
| Easy | Youngie | 18 min | Lenient Ordnung, forgiving NPCs |
| Medium | Ordnung | 12 min | Standard rules, balanced expectations |
| Hard | Gmay | 8 min | Strict punishments, scrutinizing community |

## Core Systems

- **Time System**: Day/night cycle with seasons (4 seasons × 28 days = 1 year)
- **Energy System**: Daily energy pool for chores (difficulty-driven)
- **Hunger/Survival**: Food management, starvation fail state
- **Aging & Beard**: Visual aging, beard growth tied to age and marriage
- **Ordnung System**: Rule violations and community consequences
- **Relationship System**: NPC affinity tracking (0-100)

## Mini-Games

- **Butter Churning**: Rhythm game — press at the right beat
- **Plowing Fields**: Navigation — guide horse-drawn plow in straight rows
- **Barn Raising**: Coordination event — place beams with NPC helpers
- **Quilting Bee**: Pattern matching — match the quilt template

## Development

Built with Unity 6 LTS, Universal Render Pipeline (URP), targeting WebGL.

### Running Tests

Open Unity Test Runner (Window → General → Test Runner) and run EditMode or PlayMode tests.

## Credits

Inspired by Stardew Valley, Zelda, and Weird Al's "Amish Paradise".
