import base64
import gzip
from pathlib import Path


def process_chunks(chunks: list[str], output_path: str):
    # 1) decode each base64 chunk and concatenate to rebuild the single zip
    step1 = b"".join(base64.b64decode(chunk) for chunk in chunks)

    # 2) unzip (gzip)
    step2 = gzip.decompress(step1)

    # 3) decode base64 again
    step3 = base64.b64decode(step2)

    # 4) save as bin file
    Path(output_path).write_bytes(step3)


# example usage
process_chunks(
    [
    ],  # ordered list of base64 chunks
    "output.bin")