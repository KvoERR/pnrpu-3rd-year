from pathlib import Path

from password_hash import (
    DEFAULT_ITERATIONS,
    PasswordError,
    hash_from_hex,
    hash_password,
    hash_to_hex,
    verify_password,
)


class AppError(Exception):
    pass


def create_hash(password: str, iterations: int = DEFAULT_ITERATIONS) -> str:
    stored = hash_password(password, iterations=iterations)
    return hash_to_hex(stored)


def check_password(password: str, stored_hex: str, iterations: int = DEFAULT_ITERATIONS) -> bool:
    stored = hash_from_hex(stored_hex)
    return verify_password(password, stored, iterations=iterations)


def save_hash(path: str, stored_hex: str) -> None:
    Path(path).write_text(stored_hex, encoding="ascii")


def load_hash(path: str) -> str:
    return Path(path).read_text(encoding="ascii").strip()