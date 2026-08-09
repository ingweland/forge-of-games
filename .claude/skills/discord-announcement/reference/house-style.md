# House style

Distilled from the 48 posts in `social/Discord/en/`, weighted to the newest. Where the
corpus contradicts itself the recent form wins, and the superseded one is listed under
*Retired patterns* so it is not reintroduced by copying an old post.

Chronology comes from `git log --diff-filter=A`, not file mtimes — mtimes reflect checkout
and are misleading. Newest posts: `update-13.07.26.md` (2026-07-13),
`base-ac-announcement.md` (2026-03-25), `community-strategies-3.md` (2026-02-05),
`update-07.01.26.md` (2026-01-07).

---

## Roundup template

The opener is fixed. Reproduce it exactly, including both emoji:

```
📢 **Update Time!** 🚀
```

Then one `##` section per site area, blank line after each heading, verb-first bullets.
Then, optionally, a single `👉` line. Then the support block. Full shape, from
`update-13.07.26.md`:

```
📢 **Update Time!** 🚀

## Player profile

- Added **Heroes** section. It shows the heroes the player has.
- Added **Battle for Atlantis stats** section. It shows the stats of the player during the last 10 Battle for Atlantis events.

## Alliance profile

- Added **Battle for Atlantis ranking** section. It shows the points gained by the alliance during the last 10 Battle for Atlantis events.

## Browser extension

Added new options for data collection, which enables the functionality described above. Available starting from version 1.1.4.
- *Atlantis*: automatically collects different data related to Battle for Atlantis: player and alliance leaderboard information, map state, etc. We do not collect strategic data, such as the properties of the heroes you selected or the composition of your garrisons.
- *Heroes*: your heroes and other players' heroes. Only the hero ID (whether a player owns the hero) is collected. Hero level, equipment, relics, and other details are not collected.

👉 [Read more about the extension](https://forgeofgames.com/help/browser-extension)

## 💝 Support Our Mission

**[Support us](https://forgeofgames.com/support-us)** so Forge of Games can keep running and benefiting the whole community.
```

**Bullets.** Verb-first, past tense: *Added*, *Fixed*, *Improved*, *Reworked*, *Removed*.
Bold the feature name. Where the name alone doesn't explain the point, add one plain
sentence — "It shows the heroes the player has." Bullets that stop at the name read like a
changelog; the second sentence is what makes the corpus readable.

**Section headings.** Plain text, no emoji, no bold. Product and page names keep their
title case; the generic noun after them stays lowercase:

`City Planner` · `City Viewer` · `Battle Log` · `Stats Hub` · `Command Center` · `Heroes`
· `Player profile` · `Alliance profile` · `Browser extension` · `General`

`General` is where bug-fix-and-performance sweeps go, as one bullet
(`update-21.11.md`: "Various bug fixes and performance improvements.").

**Support block.** Verbatim, unchanged since Nov 2025:

```
## 💝 Support Our Mission

**[Support us](https://forgeofgames.com/support-us)** so Forge of Games can keep running and benefiting the whole community.
```

---

## Feature-launch template

Prose first. No fixed opener — but no greeting either. Either a title line or straight
into the news.

A title line may carry emoji; this is a *title*, not a body section heading, and the two
follow different rules. Both current forms:

```
## 📢 Introducing the "Most Used Equipment" Section of the Hero Profile 🚀
```
```
# Allied Culture Guides: {Allied Culture}
```

Then, in order:

1. **What it is**, one or two sentences. `We're thrilled to introduce a new addition to
   Forge of Games — the [Equipment Configurator](…).` / `We are pleased to share …`
2. **How to reach it**, as a navigation path, not just a link — "by navigating to the
   [Strategies Dashboard](…) and clicking on an item in the *Allied Cultures* section".
   Italicise the UI labels being named.
3. **What to expect / How to use** — a `-` list or a numbered list for step-by-step.
4. **Caveats, plainly.** Beta status, desktop-only, data freshness, extension required.
   `equipment-configurator.md` states the desktop limitation outright rather than letting
   mobile users discover it. Do the same.
5. **Where to report bugs** — Discord, sometimes GitHub.
6. **Link block** — `👉` lines, one per line, blank line between each.
7. **Support block** if it fits the occasion.

---

## Reusable lines

Copy these verbatim rather than rewording; they recur across the corpus.

```
👉 [Read more about the extension](https://forgeofgames.com/help/browser-extension)
👉 [Explore the Stats Hub](https://forgeofgames.com/stats-hub)
👉 [Read more about the Stats Hub](https://forgeofgames.com/help/stats-hub)
👉 [City Planner](https://forgeofgames.com/city-planner)
👉 [Battle Log](https://forgeofgames.com/battle-log)
👉 [Our Discord](https://discord.gg/4vFeeh7CZn)
👉 [Support us](https://forgeofgames.com/support-us)
```

On a roundup: at most one, and only when a help page genuinely adds something. On a
feature launch: a block of three to five.

---

## Canonical links

Verified against `PageRoutes` in `src/Application.Core/Helpers/FogUrlBuilder.cs`. Never
invent a route — check there if something is missing here.

| Page | URL |
|---|---|
| Home | `https://forgeofgames.com` |
| City Planner | `https://forgeofgames.com/city-planner` |
| Inspirations | `https://forgeofgames.com/city-planner/inspirations` |
| City Viewer | `https://forgeofgames.com/city-planner/viewer` |
| Strategies / City Guides Dashboard | `https://forgeofgames.com/city-planner/strategies` |
| Battle Log | `https://forgeofgames.com/battle-log` |
| A battle | `https://forgeofgames.com/battle-log/battles/<id>` |
| Heroes Database | `https://forgeofgames.com/heroes` |
| A hero | `https://forgeofgames.com/heroes/<HeroId>` |
| Stats Hub | `https://forgeofgames.com/stats-hub` |
| Top heroes | `https://forgeofgames.com/stats-hub/top-heroes` |
| Command Center | `https://forgeofgames.com/command-center` |
| Equipment | `https://forgeofgames.com/command-center/equipment` |
| Equipment Configurator | `https://forgeofgames.com/command-center/equipment-configurator` |
| My Battles | `https://forgeofgames.com/command-center/my-battles` |
| Tools | `https://forgeofgames.com/tools` |
| Research Calculator | `https://forgeofgames.com/tools/research-calculator` |
| Wonder Cost Calculator | `https://forgeofgames.com/tools/wonder-cost-calculator` |
| Building Cost Calculator | `https://forgeofgames.com/tools/building-cost-calculator` |
| Battle for Atlantis | `https://forgeofgames.com/battle-for-atlantis` |
| Campaign · Treasure Hunt · Buildings · Wonders | `/campaign` · `/treasure-hunt` · `/buildings` · `/wonders` |
| Support us | `https://forgeofgames.com/support-us` |
| Help index | `https://forgeofgames.com/help` |
| Discord | `https://discord.gg/4vFeeh7CZn` |
| GitHub (site) | `https://github.com/IngweLand/forge-of-games` |
| GitHub (extension) | `https://github.com/IngweLand/hoh-helper` |

Help pages, all under `https://forgeofgames.com/help/`:

`browser-extension` · `stats-hub` · `battle-log` · `equipment` · `city-planner` ·
`city-planner-app` · `city-planner-snapshots` · `city-strategy-builder-app` ·
`command-center` · `command-center/hero-profile` · `player-profile` · `alliance-profile` ·
`submission-id` · `my-battles` · `importing-hoh-data` · `tools`

---

## Retired patterns

Present in older posts, absent from the recent ones. Do not reintroduce any of these by
copying a nearby example.

| Retired | Last seen | Use instead |
|---|---|---|
| Patreon in the support block | `update-10.09.md`, 2025-09 | the `/support-us` block above |
| Emoji on roundup body headings — `## ⚔️ Battle Log` | `update-10.09.md`, 2025-09 | plain `## Battle Log` |
| Emoji on every bullet — `- 🔥 **X:** …` | `update-march-25.md`, 2025-03 | plain `-` bullets |
| "Hey Heroes!" / "Hey Commanders!" / "Attention …!" | `equipment-2.md`, 2025-07 | open on the news |
| Four-to-six `👉` links closing a roundup | `update-21.11.md`, 2025-11 | zero or one |
| `forgeofgames.com/cityPlanner/sharedCities/<id>` | `snapshots.md`, 2025-02 | `/city-planner/shares/<shareId>` |
| `/tools/hero-builder` | `hoh-tools.md`, 2024-12 | gone; the hero page is `/heroes/<HeroId>` |
| `/command-center/playgrounds/heroes/<Hero>` | 2025-03 | `/heroes/<HeroId>` — playgrounds merged into the hero profile |

The Patreon page still exists in `PageRoutes`; what changed is the support block, which
now points at `/support-us` in every post since Nov 2025.

---

## Discord rendering

- **No tables.** Discord does not render them, and the corpus contains none. Use bullets.
- `##` is the practical top level for a post body; `#` appears only as a standalone title.
  There is no `####`.
- `**bold**` for feature and section names, `*italics*` for UI option and setting names.
- `>` blockquote for a note or warning — `city-visit.md`, `city-inspirations.md`.
- Bare URLs render fine, but `[label](url)` is the corpus norm everywhere except a couple
  of early posts.
- Soft limit 2000 characters per message. Recent roundups run 1200–1400. Report an
  over-run rather than trimming the content yourself.

---

## UTM tagging

Off by default. Every 2026 roundup uses plain links.

Add tracking only when asked, or for a promo or cross-post aimed outside the FoG server.
Current form, from `social/Discord/base-ac-announcement.md`:

```
?utm_source=discord&utm_medium=social&utm_campaign=<campaign>&utm_content=<context>
```

The Reddit sibling swaps `utm_source=reddit` and uses a subreddit-flavoured `utm_content`
(`r-playHeroesOfHistory-{wonder}-announcement`). `utm_medium=hoh_server` from
`battle-log-promo.md` (2025-07) is superseded by `utm_medium=social`.

When tagging, tag every outbound `forgeofgames.com` link in the post, including the one in
the support block — `base-ac-announcement.md` does, with `utm_campaign=support-us`.
