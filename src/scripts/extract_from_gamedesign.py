#!/usr/bin/env python3
"""
Filter objects from a JSON file.

Loads a JSON file, looks inside response.content (an array of objects),
finds all objects where:
  - "@type" == [TARGET_TYPE]
  - "id" starts with [ID_PREFIX]

Writes the matching objects (as a JSON array) to an output file.

Usage:
    python extract_from_gamedesign.py input.json output.json
"""

import json
import sys


TARGET_TYPE = ""
ID_PREFIX = ""


def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def find_matching_objects(data):
    """
    Navigate to data['response']['content'] and return all objects
    matching the target @type and id prefix.
    """
    try:
        content = data["response"]["content"]
    except (KeyError, TypeError) as e:
        raise ValueError(
            "Could not find 'response.content' in the JSON data"
        ) from e

    if not isinstance(content, list):
        raise ValueError("'response.content' is not a list/array")

    matches = []
    for obj in content:
        if not isinstance(obj, dict):
            continue
        obj_type = obj.get("@type")
        obj_id = obj.get("id", "")
        if obj_type == TARGET_TYPE and isinstance(obj_id, str) and obj_id.startswith(ID_PREFIX):
            matches.append(obj)

    return matches


def save_json(data, path):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)


def main():
    if len(sys.argv) != 3:
        print("Usage: python extract_from_gamedesign.py <input.json> <output.json>")
        sys.exit(1)

    input_path = sys.argv[1]
    output_path = sys.argv[2]

    data = load_json(input_path)
    matches = find_matching_objects(data)

    save_json(matches, output_path)

    print(f"Found {len(matches)} matching object(s).")
    print(f"Saved to: {output_path}")


if __name__ == "__main__":
    main()