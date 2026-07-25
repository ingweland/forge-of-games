---
name: localize-resx
description: Translate newly added English .resx entries in forge-of-games into all supported locales, using translations.json produced by FogLocalizationHelper as the source of truth. Use this whenever the user mentions localization, translation, resx strings, new UI text that needs translating, FogResource.resx, or asks to "sync the locales" / "translate the new strings" — even if they don't name this skill explicitly.
---

# Localize new resx entries

The English resource file is edited by hand; every other locale is filled in by this
workflow. The point is to touch **only the entries that were just added**, and to reuse the
game's own official wording wherever it exists, so the app's terminology matches the game
instead of being freshly invented each time.

## Paths and conventions

| Thing | Location |
|---|---|
| Resx directory | `src/Application.Client.Core/Localization` |
| English (source) | `FogResource.resx` |
| Locale files | `FogResource.<locale>.resx` |
| Helper project | `src/Misc/FogLocalizationHelper/FogLocalizationHelper.csproj` |
| Lookup script | `scripts/find_translations.py` (bundled with this skill) |
| Verifier | `scripts/verify_resx.py` (bundled with this skill) |

Target locales: `cs-CZ, de-DE, es-ES, fr-FR, it-IT, ja-JP, ko-KR, nl-NL, pl-PL, zh-TW`

The bundled scripts are invoked below as `python3`. That is right on macOS and Linux; on
Windows use `python` or `py` instead. If the first spelling fails, try the others before
concluding Python is unavailable — all three are stdlib-only and need no packages installed.

## Hard stops

This workflow writes to ten files at once, so a wrong assumption is expensive to unpick.
Stop and ask the user in these cases. Do not decide any of them yourself.

1. **The English has grammar problems** (step 2).
2. **`translations.json` cannot be regenerated** for any reason — the command fails, `dotnet`
   is missing, a permission prompt is declined, or a project rule in `CLAUDE.md` forbids
   running applications. An existing file on disk is **not** a substitute; its age is
   unknown and its contents may predate the strings being translated.
3. **Anything else forces a deviation** from the steps below.

The general rule: **never silently substitute a degraded input for a specified one.** If a
step cannot be performed as written, say what blocked it, say what the alternative would
cost, and let the user choose. Project rules in `CLAUDE.md` outrank this skill — when they
conflict, surface the conflict, don't work around it and don't argue with it.

**Do not assume the locale file names.** Some use two-letter codes (`FogResource.de.resx`),
others need the region (`FogResource.zh-TW.resx`). Glob `FogResource.*.resx` in the resx
directory and build the locale → file mapping from what is actually there.

If a target locale has no file, say so and skip it rather than creating a new one.

---

## Step 1 — Get the new English entries from the diff

Never read or parse the whole `FogResource.resx`. The diff already says exactly what is
new, and reading the full file wastes context and invites retranslating things that were
already done.

```
git diff -U0 HEAD -- src/Application.Client.Core/Localization/FogResource.resx
```

`HEAD` covers both staged and unstaged edits, which is what "I just added some strings"
usually means. If it comes back empty, try `git diff -U0 HEAD~1 -- <file>` or ask which ref
to compare against — do not fall back to scanning the file.

Collect the added lines (`+`, ignoring the `+++` header). A complete addition looks like:

```xml
<data name="SomeKey" xml:space="preserve">
  <value>Some English text</value>
</data>
```

Two cases worth handling:

- **An added `<value>` with no surrounding `<data>` in the diff** — an existing entry was
  reworded. Re-run with `-U5` to find the enclosing `data name=`, and treat it as a
  retranslation (overwrite that key everywhere instead of inserting).
- **Removed entries** — mention them and offer to delete the matching keys from the locale
  files, but delete nothing without confirmation.

Report the keys and English values found before going further.

## Step 2 — Grammar gate

Review the English values for grammatical errors, typos, and inconsistent capitalization or
punctuation before anything is translated. Mistakes are cheap to fix now and expensive to
fix once they exist in ten languages.

- If anything is wrong: list the issues clearly and **stop**. Do not translate, do not edit
  any file. Wait for the user to fix the English or to say "go anyway".
- If everything is fine: continue silently, without asking permission.

## Step 3 — Build translations.json

The helper resolves its output path against the **current working directory**, so it must be
run with the project's `bin` directory as the working directory. Run from anywhere else and it
writes the file somewhere else, silently leaving the expected copy stale.

Use whatever shell fits the platform — the steps are the same everywhere:

1. Note the current modified time of `src/Misc/FogLocalizationHelper/bin/translations.json`,
   if it exists. This is the baseline for step 4 below.
2. Create `src/Misc/FogLocalizationHelper/bin` if it does not exist — it is absent on a fresh
   clone and after `dotnet clean` or `git clean -xfd`. Creating it must be idempotent, and a
   failure to create or enter it must **abort**, never fall through to running the helper from
   the wrong directory.
3. With that directory as the working directory, run
   `dotnet run --project ../FogLocalizationHelper.csproj`. Restore the previous working
   directory afterwards, including when the run fails.
4. Confirm the file's modified time actually moved. Existence proves nothing — a stale copy
   from an earlier run is indistinguishable from a fresh one, and accepting one is the exact
   bug this check exists to catch. If the timestamp did not move, treat it as a failed
   regeneration and stop.

Never accept a `translations.json` from the repository root or from `bin/Debug/<tfm>/`. Either
is a leftover from a run with the wrong working directory, and its vintage is unknown.

Handle the outcome explicitly — this is hard stop #2, and the failure mode it guards against
is silent:

| Outcome | Action |
|---|---|
| Regenerated successfully | Timestamp moved. Use `src\Misc\FogLocalizationHelper\bin\translations.json` and note its time in the final summary. |
| Command fails or errors | **Stop.** Report the error verbatim. |
| Blocked — `dotnet` missing, permission declined, or a `CLAUDE.md` rule forbids running apps | **Stop.** Name the specific blocker. |

On any stop, report whether an existing `translations.json` was found and how old it is, then
offer the choices rather than picking one: (a) the user runs the `dotnet run` command
themselves and you continue, (b) proceed with the existing file, accepting that entries added
to the game since that timestamp will be missed, or (c) abandon the run. **Wait for an
answer.** Proceeding on the stale file without being told to is the specific bug this table
exists to prevent — an existing file is evidence that the helper ran *once*, not that it
reflects the game's current strings.

If the user picks (b), say so plainly in the final summary — "used translations.json from
<date>, not regenerated" — so the provenance survives into the commit message and review.

### What translations.json contains

```json
{
  "en-DK": [ { "Key": "Base.HeroPanel.Ability", "Strings": ["Ability"] },
             { "Key": "Base.BuildingTypes.barracks_Name", "Strings": ["Barracks", "Barracks"] } ],
  "de-DE": [ { "Key": "Base.BuildingTypes.barracks_Name", "Strings": ["Kaserne", "Kasernen"] } ],
  "cs-CZ": [ ... ], "es-ES": [ ... ], "fr-FR": [ ... ], "it-IT": [ ... ],
  "ja-JP": [ ... ], "ko-KR": [ ... ], "nl-NL": [ ... ], "pl-PL": [ ... ], "zh-TW": [ ... ]
}
```

Three things about this file drive everything below:

1. **English lives under `en-DK`**, not `en` or `en-US`.
2. **`Strings` is an ordered array of plural forms**, and its length is language-dependent —
   English 1–2, Czech and Polish up to 3, Japanese/Korean/Chinese always 1. Form indexes are
   therefore *not* portable between languages. Index 0 is always safe; anything past that
   needs a look at the actual forms.
3. **It is a small dictionary of game UI terms** (on the order of 100 keys), not a full
   translation memory. Expect most new strings to have no exact match. That is normal —
   partial matches and step 5 do most of the work.

## Step 4 — Look up the official wording

Use the bundled script rather than reading the JSON yourself; it indexes the file and
returns only the relevant slice, with the plural-form handling already sorted out.

```
# newEnglish.json = JSON array of the English values from step 1
python3 "${CLAUDE_SKILL_DIR}/scripts/find_translations.py" \
  -t <translations.json path from step 3> \
  -i newEnglish.json \
  -o matches.txt
```

Default output is a dense text format, roughly 6x cheaper in context than the equivalent
JSON. Add `--format json` only if something downstream needs to parse it. If a run pulls in
too much, `--max-partial 2` halves the candidate lists.

Each result carries a `matchType`:

| matchType | What to do |
|---|---|
| `exact` | Use the game's translation **verbatim**. This is the whole reason the file exists — do not "improve" it. |
| `exact-ci` | Same, but fix the casing to match your English string. |
| `exact-trimmed` | Same; differs only in surrounding whitespace. Keep *your* whitespace, take *their* wording. |
| `partial` | Terminology guidance only. Lift how the game renders the shared nouns and verbs, then write a real translation of the actual string. Never paste a partial match in as if it were the translation. |
| `none` | Go to step 5. |

Each match line shows the score, the matched English text, the game key, and then every
locale as `xx='value'`. A `+N` suffix means that language has N further plural forms; rerun
with `--format json` to see them all. When a match is on a form other than the first, the
output says so — form counts differ per language, so verify rather than trusting the index.

Partial matches are ranked, and a match needs at least one shared word (or containment) to
appear at all, so an empty result really does mean nothing relevant exists.

If the script throws because the shape changed, inspect the first ~60 lines of
`translations.json`, adjust the script, and say what you changed.

## Step 5 — Fall back to the existing resx files

This is the workhorse, not the exception. For anything still unmatched, find related
English strings already in `FogResource.resx` and pull the corresponding values out of the
sibling locale files — that is what keeps a new "Building level" string consistent with an
existing "Building name" string.

Grep `FogResource.resx` for a keyword from the new string to find related entries.

Then read just those keys from each locale file. If that turns up nothing either, translate
independently: natural, idiomatic, UI-appropriate register, consistent with neighbouring
strings.

## Step 6 — Write the translations into the locale files

Edit each locale file in place with targeted edits. Do not rewrite whole files, do not
reformat, do not reorder existing entries — a clean diff is what makes this reviewable.

- **Ordering mirrors English.** Find the key immediately preceding the new one in
  `FogResource.resx` and insert after that same key in each locale file. If it isn't there,
  insert directly before `</root>`.
- **Keep `xml:space="preserve"`** and the exact attribute form the English entry uses.
- **Escape XML**: `&` → `&amp;`, `<` → `&lt;`, `>` → `&gt;`. Leave quotes alone.
- **Preserve placeholders exactly.** `{0}`, `{1}` and any markup must survive unchanged, in
  a position that is grammatical for the target language — word order moves, placeholders
  don't disappear.
- **Never touch `<resheader>` blocks**, the schema preamble, or the file encoding/BOM.
  Windows line endings stay CRLF.
- Leading/trailing spaces and newlines in the English value are usually deliberate — carry
  them over.
- If a key already exists in a locale file (retranslation), replace its `<value>` rather
  than adding a duplicate.

## Step 7 — Verify

Run the bundled checker — it parses every locale file, confirms each new key is present
exactly once, and compares placeholder sets against English:

```
python3 "${CLAUDE_SKILL_DIR}/scripts/verify_resx.py" --keys Key.One Key.Two
```

It reports every problem it finds rather than stopping at the first, and exits non-zero if
there is at least one. Fix and re-run until it is clean; do not hand back a run it rejects.

Then check the diff shape: `git diff --stat` should show only the expected files, with roughly
3 added lines per new key per locale. Anything larger means a file got reformatted or
reordered, which buries the real change.

Then summarize: keys added, which came from exact matches, which were partial-guided, which
were translated from scratch, and any locale skipped or uncertain. Flag the shaky ones
explicitly — a quietly wrong translation is worse than a flagged one.

State the provenance of `translations.json` in the summary every time — regenerated now, or
reused from a given date — along with any other step that had to deviate from this document
and what the user chose instead. A reviewer reading only the summary should be able to tell
whether the run was clean.
