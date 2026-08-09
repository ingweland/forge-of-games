# Turning commits into an announcement

Read this before running `git log`. The hard part is not drafting — it is deciding which
commits are announcements and which are plumbing. In the two ranges behind the last two
roundups, roughly six in seven commits did not appear in the post.

## Choosing the range

If the user named commits, a tag, or a range, use it. Otherwise the default is everything
since the last announcement was committed:

```bash
git log --diff-filter=A --format=%ad --date=short -1 -- social/Discord/en/
```

Then `git log --format="%ad %s" --date=short --since=<that date>`.

Derive the boundary this way rather than parsing filenames — the corpus mixes
`update-13.07.26.md`, `update-21.11.md` and `update-july-30.md`, and no single pattern
covers them.

Read subjects first (`--format="%ad %s"`). Only open a diff when a subject is too cryptic
to classify, and prefer `git show --stat` over the full patch.

## What survives

Keep a commit only if a player would notice the change without being told where to look.

Drop by default:

- Refactors, extractions, renames, DI wiring
- Azure Functions, orchestration, processors, fetchers, nightly jobs, queue and `host.json` config
- Data-layer work, EF migrations, caching, logging, analytics plumbing
- Enum additions, in-game data syncs, proto changes
- Fixes to code that shipped inside the same range — the bug never reached anyone
- The `Add Discord announcement` commit itself

Keep, and describe the effect rather than the mechanism:

- New pages, sections, panels, columns, filters, toggles
- Behaviour a player can observe: something now sorts differently, loads faster in a way
  worth claiming, or stops requiring a workaround
- New heroes, cities, cultures, ages, events
- Browser-extension capabilities, with the minimum version
- Removals of things people used

A backend commit paired with a UI commit is **one** bullet, written from the UI side.
"Add support for WoA player stats processing and persistence" plus "Add Battle for
Atlantis player's stats" became a single line about a new profile section.

**Dropped does not mean invisible.** When a range contains a cluster of small fixes and
performance work — none individually worth a line — roll them up into a single `## General`
bullet rather than letting them vanish. `update-21.11.md` closes with "Various bug fixes and
performance improvements." standing in for a batch-limit increase, an orchestrator tuning
pass and a processing-delay reduction. Use it when the cluster is real; omit the section
entirely when it isn't.

## Code vocabulary → post vocabulary

Internal names must be translated. The recurring ones:

| In commits and code | In the post |
|---|---|
| `WoA`, `Woa`, `woa` | Battle for Atlantis |
| `ATH` | Treasure Hunt (alliance) |
| `Pvp*` — `PvpRankings`, `PvpBattle` | Arena — Arena Rankings, Arena Battles |
| `PvpTier`, `PvpEliteTier` | Arena Tier, Elite Arena |
| city map entity | building |
| wonder ranking | Wonder levels |
| player/alliance *ranking* services | the section on the profile page that shows it |
| `HohHelper`, extension | Browser extension |

Section names on a profile page are proper nouns in the post and get bolded — **Heroes**,
**Battle for Atlantis stats**, **Wonder levels**, **City properties**. Fix typos from
commit subjects silently: "Add alliance's battle for attlantis ranking" became "Added
**Battle for Atlantis ranking** section".

## Grouping

Group by site area, not by commit order or date, then apply the roundup template in
`house-style.md`. Section vocabulary is listed there.

Worked example — 2026-07-06 → 2026-07-13, 25 commits → `update-13.07.26.md`, four bullets
in three sections:

| Surviving commit | Became |
|---|---|
| Add player heroes to the player's profile page | `## Player profile` — Added **Heroes** section |
| Add Battle for Atlantis player's stats | `## Player profile` — Added **Battle for Atlantis stats** section |
| Add alliance's battle for attlantis ranking | `## Alliance profile` — Added **Battle for Atlantis ranking** section |
| Add support for heroes data collection and processing | `## Browser extension` — the *Heroes* collection option |

Dropped from that same range: the heroes data layer, the nightly processor, the hero-id
mapping fix, the `GetWakeupsAsync` refactor, `WorkerType_FISHER`, a log-message fix, the
domain-restriction middleware, and a `BattleStatsFetcher` optimisation.

Second example — 2025-12-20 → 2026-01-07 → `update-07.01.26.md`. Note how the code nouns
disappear: "Add support for city map entity selection and stats in city viewer" became
"You can now select individual buildings and see their stats"; three separate city-planner
commits (upgrading state, building counts, premium expansion counts) became three bullets
under one `## City Planner`; "Order production stats by priority keys and add custom
comparer" became "Fixed the sorting of the city stats so that coins and food … always
appear at the top."

## Surface the judgement calls, don't make them

Some user-facing commits are deliberately left out — a feature held for its own post, or
one not yet live on the main server. Both recent ranges contain examples:

- 2026-07-06 "Add Ithaka allied culture support with new cities, wonders, buildings, and
  expansions" — user-facing, omitted from the 13 July post.
- 2026-07-12 "Remove wonder ranking from the player's profile page" — a visible removal,
  omitted.
- 2025-12-20 "Add City Strategy Builder" — got its own feature-launch post instead.
- 2025-12-23 "Hide building customization selector because customizations are no longer
  used in the game" — visible, omitted.

So do not decide these silently in either direction. Draft the post from the clear cases,
then list the borderline ones under it: what the commit was, why it might belong, and why
it might not. Let the user rule on release timing — that is information the log does not
contain.

Same rule for anything the commit subject does not actually support. If a bullet would
need a detail you are inferring — which screen it appears on, what the option is called,
whether it is live — ask rather than inventing it.
