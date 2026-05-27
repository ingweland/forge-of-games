import os


def main():
    input_path = "gamedesign.json"
    chunk_size = 500000

    base, ext = os.path.splitext(input_path)

    with open(input_path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    for i, start in enumerate(range(0, len(lines), chunk_size)):
        chunk = lines[start : start + chunk_size]
        out_path = f"{base}_part{i + 1}{ext}"
        with open(out_path, "w", encoding="utf-8") as f:
            f.writelines(chunk)
        print(f"Wrote {len(chunk)} lines → {out_path}")


if __name__ == "__main__":
    main()