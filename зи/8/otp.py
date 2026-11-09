import os


BLOCK_BYTES = 8
BLOCK_BITS = 64


class OtpError(Exception):
    """Ошибка шифрования или дешифрования."""


def generate_gamma(length: int) -> bytes:
    """Генерирует случайную гамму заданной длины."""
    if length < 0:
        raise OtpError("Длина гаммы не может быть отрицательной")
    return os.urandom(length)


def _xor_bytes(left: bytes, right: bytes) -> bytes:
    """Побитовый XOR двух байтовых строк одинаковой длины."""
    if len(left) != len(right):
        raise OtpError("Длины операндов XOR должны совпадать")
    return bytes(a ^ b for a, b in zip(left, right))


def encrypt(data: bytes, gamma: bytes = None) -> tuple:
    """
    Шифрует данные методом однократного гаммирования блоками по 64 бита.
    Возвращает (шифротекст, гамма).
    """
    if not data:
        raise OtpError("Данные пусты")

    padded = _pad_to_block(data, BLOCK_BYTES)

    if gamma is None:
        gamma = generate_gamma(len(padded))
    if len(gamma) != len(padded):
        raise OtpError(
            f"Длина гаммы ({len(gamma)} байт) должна совпадать "
            f"с длиной данных после дополнения ({len(padded)} байт)"
        )

    result = bytearray()
    for offset in range(0, len(padded), BLOCK_BYTES):
        block = padded[offset:offset + BLOCK_BYTES]
        gamma_block = gamma[offset:offset + BLOCK_BYTES]
        result.extend(_xor_bytes(block, gamma_block))

    return bytes(result), gamma


def decrypt(ciphertext: bytes, gamma: bytes) -> bytes:
    """Дешифрует данные методом однократного гаммирования."""
    if not ciphertext:
        raise OtpError("Шифротекст пуст")
    if len(ciphertext) != len(gamma):
        raise OtpError(
            f"Длина гаммы ({len(gamma)} байт) должна совпадать "
            f"с длиной шифротекста ({len(ciphertext)} байт)"
        )

    result = bytearray()
    for offset in range(0, len(ciphertext), BLOCK_BYTES):
        block = ciphertext[offset:offset + BLOCK_BYTES]
        gamma_block = gamma[offset:offset + BLOCK_BYTES]
        result.extend(_xor_bytes(block, gamma_block))

    return _unpad(bytes(result))


def _pad_to_block(data: bytes, block_bytes: int) -> bytes:
    """Дополняет данные до кратности размеру блока (PKCS#7)."""
    pad_length = block_bytes - (len(data) % block_bytes)
    if pad_length == 0:
        pad_length = block_bytes
    return data + bytes([pad_length] * pad_length)


def _unpad(data: bytes) -> bytes:
    """Снимает дополнение PKCS#7."""
    if not data:
        raise OtpError("Пустые данные")
    pad_length = data[-1]
    if not 1 <= pad_length <= BLOCK_BYTES:
        raise OtpError("Неверная гамма или повреждённые данные")
    if data[-pad_length:] != bytes([pad_length] * pad_length):
        raise OtpError("Неверная гамма или повреждённые данные")
    return data[:-pad_length]