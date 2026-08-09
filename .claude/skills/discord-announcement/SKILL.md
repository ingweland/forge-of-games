---
name: discord-announcement
description: Draft a Discord announcement for Forge of Games from rough notes or from commits, in the current house style, and save it to social/Discord/en/. Use this whenever the user wants to announce a release or feature, asks for an "update post", a "Discord announcement", or says something like "write the announcement for these commits" / "announce what changed since the last post" — even if they don't name this skill.
---

# Write a Discord announcement

`social/Discord/en/` holds every announcement posted since Dec 2024. It is the style
reference, but it is **not** uniformly current: conventions changed several times, and
copying the nearest example reproduces retired ones. Draft against
`reference/house-style.md`, which records only what the 2026 posts actually do.

The user supplies rough notes, commit references, or both. The output is one
Discord-ready markdown file.

## Paths

| Thing | Location |
|---|---|
| Announcements | `social/Discord/en/` |
| Style guide | `reference/house-style.md` (bundled) |
| Commit triage | `reference/from-commits.md` (bundled) |
| Cross-post template | `social/Discord/base-ac-announcement.md`, `social/Reddit/base-ac-announcement.md` |
| Route constants | `src/Application.Core/Helpers/FogUrlBuilder.cs` → `PageRoutes` |

Translations under `social/Discord/{de,fr,cz,es}/` are **out of scope**. They lapsed in
Feb 2026 and none of the last three roundups were translated. Only produce one if asked.

## Step 1 — Classify the post

Two shapes, and the choice governs everything downstream. Say which one you picked.

**Roundup** — several unrelated changes, or a commit range. Nine examples, all `update-*.md`.
Fixed opener, changes grouped under site-area headings, support block at the end.

**Feature launch** — one named thing being introduced. `equipment-configurator.md`,
`city-strategy-builder.md`, `city-viewer.md`, `alliance-ath.md`, `equipment-insights.md`.
Prose-first: what it is, how to reach it, what to expect, caveats, link block.

A single bug fix is neither. Say so and offer the roundup shape with one section, rather
than inflating it into a launch.

## Step 2 — Gather the material

**From notes:** take them at face value. Do not go hunting through the codebase to enrich
them — the user knows what shipped. Ask only when a note is too thin to turn into a
sentence a player would understand.

**From commits:** read `reference/from-commits.md` before touching `git log`. Most commits
in any range do not belong in an announcement, and the filtering rules matter more than
the drafting rules.

## Step 3 — Draft

Read `reference/house-style.md` and follow the template for the shape you chose.

Two things carry the voice, and both come from the newest posts:

- **Say what it is, then what it's for.** `- Added **Heroes** section. It shows the heroes the player has.`
  The second sentence is what makes the corpus readable; bullets that stop at the feature
  name read like a changelog.
- **State limits plainly, in the post.** Beta status, desktop-only, extension version,
  data freshness, what is *not* collected. `update-13.07.26.md` spends four lines on the
  privacy boundary of a new data collector. That is house style, not padding.

## Step 4 — Self-check before showing anything

Against the retired-pattern table in the style guide. The four that recur:

- No Patreon in the support block — it is `/support-us`, verbatim, since Nov 2025.
- No emoji immediately after `## ` in a roundup heading.
- No "Hey Heroes!" / "Hey Commanders!" opener.
- No more than one `👉` line on a roundup.

Then: every URL either appears in the style guide's link table or was given by the user.
Do not invent routes — check `PageRoutes` in `FogUrlBuilder.cs` if unsure.

Length: soft-target 2000 characters, Discord's non-Nitro message cap. The 2026 roundups
run 1200–1400. If a draft goes over, say so and let the user decide — do not truncate.

## Step 5 — Write the file and show the draft

Name it from the corpus convention:

- **Roundup** → `update-DD.MM.YY.md`, e.g. a post written on 9 Aug 2026 is
  `update-09.08.26.md`. The 2-digit year is current (`update-13.07.26.md`,
  `update-07.01.26.md`); the older year-less form (`update-21.11.md`) is not.
- **Feature launch** → kebab-case topic slug, e.g. `equipment-configurator.md`.

Check the name is free first. If it is taken, append a numeric suffix as the corpus
already does (`equipment-2.md`, `community-strategies2.md`) — never overwrite an existing
announcement.

Write to `social/Discord/en/`, then print the full draft in the reply so it can be read
without opening the file. Close with anything you were unsure about: a bullet you guessed
at, a link you could not verify, a commit you dropped that might have been user-facing.
Flagged doubt is cheap; a wrong claim in a published post is not.
