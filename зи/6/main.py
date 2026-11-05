import arithmetic
import lzw
from pathlib import Path


class CompressionError(Exception):
    """Ошибка сжатия или восстановления."""


def compress_file(input_path: str, output_path: str) -> dict:
    source = Path(input_path).read_bytes()
    if not source:
        raise CompressionError("Входной файл пуст")

    after_arithmetic = arithmetic.encode(source)
    after_lzw = lzw.encode(after_arithmetic)

    Path(output_path).write_bytes(after_lzw)

    return {
        "original": len(source),
        "arithmetic": len(after_arithmetic),
        "lzw": len(after_lzw),
    }


def decompress_file(input_path: str, output_path: str) -> int:
    packed = Path(input_path).read_bytes()
    if not packed:
        raise CompressionError("Входной файл пуст")

    after_lzw = lzw.decode(packed)
    restored = arithmetic.decode(after_lzw)

    Path(output_path).write_bytes(restored)
    return len(restored)


def verify(original_path: str, restored_path: str) -> bool:
    return Path(original_path).read_bytes() == Path(restored_path).read_bytes()