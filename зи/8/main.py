from pathlib import Path

import otp


class OtpFileError(Exception):
    """Ошибка файловых операций."""


def encrypt_file(input_path: str, output_path: str) -> dict:
    """
    Шифрует файл. Сохраняет шифротекст и гамму в один файл:
    [8 байт: длина гаммы][гамма][шифротекст].
    """
    source = Path(input_path).read_bytes()
    if not source:
        raise OtpFileError("Входной файл пуст")

    ciphertext, gamma = otp.encrypt(source)

    payload = bytearray()
    payload.extend(len(gamma).to_bytes(8, "big"))
    payload.extend(gamma)
    payload.extend(ciphertext)

    Path(output_path).write_bytes(payload)

    return {
        "original": len(source),
        "padded": len(gamma),
        "ciphertext": len(ciphertext),
        "gamma": len(gamma),
    }


def decrypt_file(input_path: str, output_path: str) -> int:
    """Дешифрует файл. Читает длину гаммы, гамму и шифротекст."""
    packed = Path(input_path).read_bytes()
    if len(packed) < 8:
        raise OtpFileError("Файл слишком короткий")

    gamma_length = int.from_bytes(packed[:8], "big")
    if len(packed) < 8 + gamma_length:
        raise OtpFileError("Файл повреждён: не хватает гаммы")

    gamma = packed[8:8 + gamma_length]
    ciphertext = packed[8 + gamma_length:]

    restored = otp.decrypt(ciphertext, gamma)
    Path(output_path).write_bytes(restored)
    return len(restored)