#!/usr/bin/env python3
"""
Verify the resx locale files after a translation pass.

Checks, in order:
  1. every FogResource*.resx parses as XML
  2. each key given with --keys exists exactly once in every locale file
  3. placeholders ({0}, {1}, ...) in each translation match the English entry

Reports every problem found rather than stopping at the first, and exits non-zero if
there is at least one. Silence plus exit 0 means the pass is clean.

Usage:
    python3 verify_resx.py --keys Foo.Bar Foo.Baz
    python3 verify_resx.py --all
"""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

DEFAULT_DIR = Path("src/Application.Client.Core/Localization")
ENGLISH = "FogResource.resx"
PLACEHOLDER = re.compile(r"\{\d+\}")


def load(path: Path) -> tuple[dict[str, list[str]], str | None]:
    """Return {key: [values]} and a parse error, if any. Values is a list so
    duplicate keys are visible rather than silently collapsing."""
    try:
        root = ET.parse(path).getroot()
    except ET.ParseError as exc:
        return {}, f"malformed XML: {exc}"
    except OSError as exc:
        return {}, f"unreadable: {exc}"

    entries: dict[str, list[str]] = {}
    for data in root.findall("data"):
        name = data.get("name")
        if name is None:
            continue
        value = data.find("value")
        entries.setdefault(name, []).append(value.text or "" if value is not None else "")
    return entries, None


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--dir", type=Path, default=DEFAULT_DIR)
    ap.add_argument("--keys", nargs="*", default=[],
                    help="keys added in this pass; omit with --all to check everything")
    ap.add_argument("--all", action="store_true",
                    help="check every key present in the English file")
    args = ap.parse_args()

    if not args.dir.is_dir():
        print(f"error: no such directory: {args.dir}", file=sys.stderr)
        return 1

    english_path = args.dir / ENGLISH
    if not english_path.is_file():
        print(f"error: no {ENGLISH} in {args.dir}", file=sys.stderr)
        return 1

    locale_paths = sorted(p for p in args.dir.glob("FogResource.*.resx"))
    if not locale_paths:
        print(f"error: no locale files found in {args.dir}", file=sys.stderr)
        return 1

    problems: list[str] = []

    english, err = load(english_path)
    if err:
        print(f"FAIL {english_path.name}: {err}", file=sys.stderr)
        return 1

    keys = sorted(english) if args.all else list(args.keys)
    if not keys:
        print("error: pass --keys <names...> or --all", file=sys.stderr)
        return 1

    missing_from_english = [k for k in keys if k not in english]
    problems += [f"{english_path.name}: key not present: {k}" for k in missing_from_english]
    keys = [k for k in keys if k in english]

    for path in locale_paths:
        entries, err = load(path)
        if err:
            problems.append(f"{path.name}: {err}")
            continue

        for key in keys:
            values = entries.get(key)
            if not values:
                problems.append(f"{path.name}: missing key {key}")
                continue
            if len(values) > 1:
                problems.append(f"{path.name}: key {key} appears {len(values)} times")

            expected = set(PLACEHOLDER.findall(english[key][0]))
            actual = set(PLACEHOLDER.findall(values[0]))
            if expected != actual:
                lost = ", ".join(sorted(expected - actual)) or "-"
                extra = ", ".join(sorted(actual - expected)) or "-"
                problems.append(
                    f"{path.name}: key {key} placeholder mismatch (missing: {lost}; unexpected: {extra})")

    checked = f"{len(keys)} key(s) across {len(locale_paths)} locale file(s)"
    if problems:
        for p in problems:
            print(f"FAIL {p}", file=sys.stderr)
        print(f"\n{len(problems)} problem(s); checked {checked}", file=sys.stderr)
        return 1

    print(f"OK - {checked}, XML well-formed, keys present once, placeholders match")
    return 0


if __name__ == "__main__":
    sys.exit(main())
