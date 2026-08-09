from datetime import date
from pathlib import Path


def main():
    input_path = r"D:\Temp\My project\Assets"
    output_path = f"asset_names_{date.today():%Y-%m-%d}.txt"

    names = sorted(
        {
            p.stem
            for p in Path(input_path).rglob("*")
            if p.is_file() and p.suffix.lower() == ".asset"
        }
    )

    with open(output_path, "w", encoding="utf-8") as f:
        f.write("\n".join(names))

    print(f"Wrote {len(names)} names → {output_path}")


if __name__ == "__main__":
    main()
