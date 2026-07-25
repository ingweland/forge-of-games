#!/usr/bin/env python3
"""
Look up English strings in translations.json (produced by FogLocalizationHelper)
and return the game's official translations for the requested locales.

translations.json shape:

    {
      "en-DK": [ { "Key": "Base.HeroPanel.Ability", "Strings": ["Ability"] }, ... ],
      "de-DE": [ { "Key": "Base.HeroPanel.Ability", "Strings": ["Fahigkeit"] }, ... ],
      ...
    }

English is stored under 'en-DK'. "Strings" is an ordered array of plural forms whose
length is language-dependent (English 1-2, Czech/Polish up to 3, Japanese/Korean/
Chinese always 1), so form indexes are NOT portable between languages.

Matching runs exact -> case-insensitive -> whitespace-trimmed -> fuzzy partial, and
compares against every plural form rather than just the first.

Usage:
    python3 find_translations.py -t translations.json -i newEnglish.json -o matches.json
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from difflib import SequenceMatcher
from pathlib import Path

DEFAULT_LOCALES = [
    "cs-CZ", "de-DE", "es-ES", "fr-FR", "it-IT",
    "ja-JP", "ko-KR", "nl-NL", "pl-PL", "zh-TW",
]

PLURAL_NOTE = (
    "Matched a plural form; form counts differ per language "
    "(en 1-2, cs/pl up to 3, ja/ko/zh always 1). Check \"forms\" and pick the right one."
)

_PLACEHOLDER = re.compile(r"\{[^}]*\}")
_MARKUP = re.compile(r"<[^>]*>")
_NONWORD = re.compile(r"[^\w]+", re.UNICODE)


def normalize(text: str) -> str:
    """Strip placeholders, markup and punctuation so wording can be compared."""
    t = text.lower()
    t = _PLACEHOLDER.sub(" ", t)
    t = _MARKUP.sub(" ", t)
    t = _NONWORD.sub(" ", t)
    return t.strip()


def forms_of(entry: dict) -> list[str]:
    """Pull the plural-form list out of an entry, tolerating a bare string."""
    raw = entry.get("Strings")
    if raw is None:
        return []
    if isinstance(raw, str):
        return [raw]
    if isinstance(raw, dict):  # defensive: shape changed to a bundle object
        keys = ("Singular", "One", "Plural", "Other", "Value", "Text")
        return [raw[k] for k in keys if isinstance(raw.get(k), str)]
    return [s for s in raw if isinstance(s, str)]


# A character-level ratio alone is noisy across unrelated strings, so it only
# qualifies a match on its own well above this bar (catches typos and inflections
# like "Barrack" vs "Barracks" without matching arbitrary text).
SEQ_ONLY_FLOOR = 0.62


def score(a_norm: str, a_tokens: set[str], b_norm: str, b_tokens: set[str]) -> float:
    """Token overlap with a containment bonus; character ratio only as a fallback."""
    if not a_norm or not b_norm:
        return 0.0
    inter = len(a_tokens & b_tokens)
    union = len(a_tokens | b_tokens)
    contained = a_norm in b_norm or b_norm in a_norm
    seq = SequenceMatcher(None, a_norm, b_norm).ratio()

    if not inter and not contained:
        return seq if seq >= SEQ_ONLY_FLOOR else 0.0

    jaccard = (inter / union if union else 0.0) + (0.25 if contained else 0.0)
    return min(1.0, max(jaccard, seq))


class Index:
    def __init__(self, data: dict):
        self.locales = list(data.keys())
        if not self.locales:
            raise ValueError("No locales at the top level - the file shape changed.")

        english = next((l for l in self.locales if re.match(r"^en([-_]|$)", l, re.I)), None)
        if english is None:
            raise ValueError(f"No English locale found. Saw: {', '.join(self.locales)}")
        self.english = english

        # key -> {locale: [forms]}
        self.by_key: dict[str, dict[str, list[str]]] = {}
        for locale, entries in data.items():
            for entry in entries:
                key = entry.get("Key")
                if not key:
                    continue
                self.by_key.setdefault(key, {})[locale] = forms_of(entry)

        if not self.by_key:
            raise ValueError("Indexed 0 keys.")

        # One record per (key, plural form) on the English side.
        self.en_forms = []
        for key, locales in self.by_key.items():
            for i, text in enumerate(locales.get(english, [])):
                norm = normalize(text)
                self.en_forms.append({
                    "key": key,
                    "index": i,
                    "text": text,
                    "norm": norm,
                    "tokens": set(norm.split()),
                })

    def locale_values(self, key: str, matched_index: int) -> dict:
        out = {}
        for locale in self.wanted:
            forms = self.by_key[key].get(locale) or []
            if not forms:
                out[locale] = None
                continue
            # Indexes are not portable across languages; clamp, and flag it upstream.
            out[locale] = {
                "value": forms[min(matched_index, len(forms) - 1)],
                "forms": forms,
            }
        return out

    def make_match(self, form: dict, sc: float | None) -> dict:
        match = {
            "key": form["key"],
            "matchedEnglish": form["text"],
            "formIndex": form["index"],
            "englishForms": self.by_key[form["key"]].get(self.english, []),
            "translations": self.locale_values(form["key"], form["index"]),
        }
        if sc is not None:
            match["score"] = round(sc, 3)
        if form["index"] > 0:
            match["pluralWarning"] = PLURAL_NOTE
        return match

    def lookup(self, src: str, max_partial: int, min_score: float) -> dict:
        tiers = [
            ("exact", lambda f: f["text"] == src),
            ("exact-ci", lambda f: f["text"].lower() == src.lower()),
            # resx values routinely differ from the game's by a trailing newline only.
            ("exact-trimmed", lambda f: f["text"].strip().lower() == src.strip().lower()),
        ]
        for match_type, predicate in tiers:
            hits = [f for f in self.en_forms if predicate(f)]
            if not hits:
                continue
            # One key can repeat the same text across forms; keep the first.
            seen, deduped = set(), []
            for f in sorted(hits, key=lambda f: f["index"]):
                if f["key"] not in seen:
                    seen.add(f["key"])
                    deduped.append(f)
            result = {
                "english": src,
                "matchType": match_type,
                "matches": [self.make_match(f, None) for f in deduped],
            }
            if len(deduped) > 1:
                result["note"] = ("Several game keys share this English text - "
                                  "compare them and pick the one whose context fits.")
            return result

        norm = normalize(src)
        tokens = set(norm.split())
        scored = []
        for f in self.en_forms:
            sc = score(norm, tokens, f["norm"], f["tokens"])
            if sc >= min_score:
                scored.append((sc, f))
        scored.sort(key=lambda pair: pair[0], reverse=True)
        top = scored[:max_partial]

        if top:
            return {
                "english": src,
                "matchType": "partial",
                "note": ("Terminology guidance only - reuse the wording for shared "
                         "nouns/verbs, then translate the actual string. Do not copy verbatim."),
                "matches": [self.make_match(f, sc) for sc, f in top],
            }
        return {
            "english": src,
            "matchType": "none",
            "note": "No match - fall back to the existing resx files, then translate.",
            "matches": [],
        }


def render_compact(payload: dict) -> str:
    """Dense text rendering - the JSON is ~4x heavier for identical information."""
    lines = [f"english={payload['englishLocale']} keys={payload['indexedKeys']}"]
    if payload["missingLocales"]:
        lines.append(f"MISSING LOCALES: {', '.join(payload['missingLocales'])}")

    for r in payload["results"]:
        lines.append("")
        lines.append(f'### {r["english"]!r}  [{r["matchType"]}]')
        if r["matchType"] in ("partial", "none"):
            lines.append(f"    {r['note']}")
        for m in r["matches"]:
            sc = f"{m['score']:.2f} " if "score" in m else ""
            form = f" (form {m['formIndex']})" if m["formIndex"] else ""
            lines.append(f"  {sc}{m['matchedEnglish']!r}{form}  <{m['key']}>")
            if "pluralWarning" in m:
                lines.append(f"    plural forms vary by language - verify: {m['englishForms']}")
            pairs = []
            for locale, val in m["translations"].items():
                if val is None:
                    continue
                short = locale.split("-")[0]
                pairs.append(f"{short}={val['value']!r}")
                if len(val["forms"]) > 1:
                    pairs[-1] += f"+{len(val['forms']) - 1}"
            lines.append("    " + "  ".join(pairs))
    return "\n".join(lines)


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("-t", "--translations", required=True, type=Path)
    ap.add_argument("-i", "--input", required=True, type=Path,
                    help="JSON file containing an array of English strings")
    ap.add_argument("-o", "--out", type=Path, help="write JSON here instead of stdout")
    ap.add_argument("--locales", nargs="+", default=DEFAULT_LOCALES)
    ap.add_argument("--max-partial", type=int, default=4)
    ap.add_argument("--min-score", type=float, default=0.34)
    ap.add_argument("--format", choices=("compact", "json"), default="compact",
                    help="compact is ~4x cheaper in context; json for programmatic use")
    args = ap.parse_args()

    data = json.loads(args.translations.read_text(encoding="utf-8"))
    index = Index(data)
    index.wanted = args.locales

    missing = [l for l in args.locales if l not in index.locales]
    if missing:
        print(f"warning: locales absent from translations.json: {', '.join(missing)}",
              file=sys.stderr)

    inputs = json.loads(args.input.read_text(encoding="utf-8"))
    if isinstance(inputs, str):
        inputs = [inputs]

    results = [index.lookup(s, args.max_partial, args.min_score)
               for s in inputs if isinstance(s, str)]

    payload = {
        "source": str(args.translations.resolve()),
        "englishLocale": index.english,
        "availableLocales": index.locales,
        "missingLocales": missing,
        "indexedKeys": len(index.by_key),
        "results": results,
    }

    text = (json.dumps(payload, ensure_ascii=False, indent=2)
            if args.format == "json" else render_compact(payload))
    if args.out:
        args.out.write_text(text, encoding="utf-8")
        tally: dict[str, int] = {}
        for r in results:
            tally[r["matchType"]] = tally.get(r["matchType"], 0) + 1
        summary = ", ".join(f"{k}={v}" for k, v in sorted(tally.items()))
        print(f"Wrote {args.out}  ({summary})")
    else:
        print(text)
    return 0


if __name__ == "__main__":
    sys.exit(main())
