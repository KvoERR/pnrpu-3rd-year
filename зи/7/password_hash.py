import hashlib
import os

import des32


DEFAULT_ITERATIONS = 1000
SALT_BYTES = 4
MAX_PASSWORD_LENGTH = 13


class PasswordError(Exception):
    """Ошибка хеширования пароля."""


def _password_to_key(password: str) -> int:
    digest = hashlib.sha256(password.encode("utf-8")).digest()
    return int.from_bytes(digest[:4], "big")


def hash_password(password: str, salt: bytes = None, iterations: int = DEFAULT_ITERATIONS) -> bytes:
    if not password:
        raise PasswordError("Пароль не может быть пустым")
    if len(password) > MAX_PASSWORD_LENGTH:
        raise PasswordError(f"Пароль длиннее {MAX_PASSWORD_LENGTH} символов")

    if salt is None:
        salt = os.urandom(SALT_BYTES)
    if len(salt) != SALT_BYTES:
        raise PasswordError(f"Соль должна быть {SALT_BYTES} байта")

    key = _password_to_key(password)
    block = salt

    for _ in range(iterations):
        block = des32.encrypt_block(block, key)
        key = int.from_bytes(block, "big")

    return salt + block


def verify_password(password: str, stored: bytes, iterations: int = DEFAULT_ITERATIONS) -> bool:
    if len(stored) != SALT_BYTES + des32.BLOCK_BYTES:
        raise PasswordError("Некорректный формат хеша")

    salt = stored[:SALT_BYTES]
    expected = stored[SALT_BYTES:]

    actual = hash_password(password, salt=salt, iterations=iterations)
    return actual[SALT_BYTES:] == expected


def hash_to_hex(stored: bytes) -> str:
    return stored.hex()


def hash_from_hex(text: str) -> bytes:
    return bytes.fromhex(text)