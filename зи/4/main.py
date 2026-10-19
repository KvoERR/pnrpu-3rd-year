import base64


class CipherError(Exception):
    pass


# ==================== Вспомогательные функции ====================

def _bytes_to_bits(data: bytes) -> str:
    return ''.join(f'{b:08b}' for b in data)


def _bits_to_bytes(bits: str) -> bytes:
    bits += '0' * ((-len(bits)) % 8)
    return bytes(int(bits[i:i+8], 2) for i in range(0, len(bits), 8))


def _permute(bits: str, table: list) -> str:
    return ''.join(bits[i-1] for i in table)


def _xor_bits(a: str, b: str) -> str:
    return ''.join('1' if x != y else '0' for x, y in zip(a, b))


# ==================== Таблицы для 32-битного блока ====================

IP_TABLE = [
    2, 6, 10, 14, 18, 22, 26, 30,
    4, 8, 12, 16, 20, 24, 28, 32,
    1, 5, 9, 13, 17, 21, 25, 29,
    3, 7, 11, 15, 19, 23, 27, 31,
]

# FP_TABLE строим автоматически как обратную к IP_TABLE
_inv = [0] * 32
for _i, _pos in enumerate(IP_TABLE):
    _inv[_pos - 1] = _i + 1
FP_TABLE = _inv

E_TABLE = [
    2, 3, 4, 5, 6, 7,
    6, 7, 8, 9, 10, 11,
    10, 11, 12, 13, 14, 15,
    14, 15, 16, 1, 2, 3,
]

P_TABLE = [
    2, 6, 10, 14, 1, 5, 9, 13,
    4, 8, 12, 16, 3, 7, 11, 15,
]


# ==================== S-блоки 4×16 ====================

SBOXES = [
    [
        [14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7],
        [0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8],
        [4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0],
        [15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13],
    ],
    [
        [15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10],
        [3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5],
        [0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15],
        [13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9],
    ],
    [
        [10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8],
        [13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1],
        [13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7],
        [1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12],
    ],
    [
        [7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15],
        [13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9],
        [10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4],
        [3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14],
    ],
]


def _sbox_substitute(bits24: str) -> str:
    result = []
    for i in range(4):
        chunk = bits24[i*6:(i+1)*6]
        row = int(chunk[0] + chunk[5], 2)
        col = int(chunk[1:5], 2)
        val = SBOXES[i][row][col]
        result.append(f'{val:04b}')
    return ''.join(result)


# ==================== Генерация подключей ====================

def _generate_subkeys(key32: str) -> list:
    left = key32[:16]
    right = key32[16:]
    subkeys = []
    for r in range(8):
        shift = (r + 1) % 16
        left_rot = left[shift:] + left[:shift]
        right_rot = right[shift:] + right[:shift]
        combined = left_rot + right_rot
        round_const = f'{(r * 0x1F + 0x3A7) & 0xFFFF:016b}'
        subkey = _xor_bits(combined[:16], round_const)
        subkeys.append(subkey)
    return subkeys


# ==================== Функция Фейстеля ====================

def _feistel_f(right16: str, subkey16: str) -> str:
    expanded = _permute(right16, E_TABLE)
    xored = _xor_bits(expanded, subkey16 + subkey16[:8])
    substituted = _sbox_substitute(xored)
    return _permute(substituted, P_TABLE)


# ==================== Блочные операции ====================

def _encrypt_block(block32: str, subkeys: list) -> str:
    permuted = _permute(block32, IP_TABLE)
    left = permuted[:16]
    right = permuted[16:]
    for r in range(8):
        new_left = right
        new_right = _xor_bits(left, _feistel_f(right, subkeys[r]))
        left, right = new_left, new_right
    combined = right + left
    return _permute(combined, FP_TABLE)


def _decrypt_block(block32: str, subkeys: list) -> str:
    return _encrypt_block(block32, subkeys[::-1])


# ==================== Дополнение PKCS#7 ====================

def _pad(data: bytes, block_bytes: int = 4) -> bytes:
    pad_len = block_bytes - (len(data) % block_bytes)
    if pad_len == 0:
        pad_len = block_bytes
    return data + bytes([pad_len] * pad_len)


def _unpad(data: bytes) -> bytes:
    if not data:
        raise CipherError("Пустые данные")
    pad_len = data[-1]
    if pad_len < 1 or pad_len > 4:
        raise CipherError("НЕВЕРНЫЙ_КЛЮЧ")
    if data[-pad_len:] != bytes([pad_len] * pad_len):
        raise CipherError("НЕВЕРНЫЙ_КЛЮЧ")
    return data[:-pad_len]


# ==================== Шифрование/дешифрование байтов ====================

def _check_key(key32: int):
    if not isinstance(key32, int):
        raise CipherError("Ключ должен быть целым числом")
    if not (0 <= key32 < (1 << 32)):
        raise CipherError("Ключ должен быть 32-битным: 0 ≤ key < 2^32")


def encrypt_des32(data: bytes, key32: int) -> bytes:
    _check_key(key32)
    key_bits = f'{key32:032b}'
    subkeys = _generate_subkeys(key_bits)

    padded = _pad(data, 4)
    result = bytearray()
    for i in range(0, len(padded), 4):
        block = padded[i:i+4]
        block_bits = _bytes_to_bits(block)
        encrypted_bits = _encrypt_block(block_bits, subkeys)
        result.extend(_bits_to_bytes(encrypted_bits))
    return bytes(result)


def decrypt_des32(data: bytes, key32: int) -> bytes:
    _check_key(key32)
    if len(data) == 0:
        raise CipherError("Шифротекст пуст")
    if len(data) % 4 != 0:
        raise CipherError(
            f"Размер шифротекста ({len(data)} байт) не кратен 4. "
            f"Файл повреждён или это не шифротекст DES-32."
        )

    key_bits = f'{key32:032b}'
    subkeys = _generate_subkeys(key_bits)

    result = bytearray()
    for i in range(0, len(data), 4):
        block_bits = _bytes_to_bits(data[i:i+4])
        decrypted_bits = _decrypt_block(block_bits, subkeys)
        result.extend(_bits_to_bytes(decrypted_bits))

    try:
        return _unpad(bytes(result))
    except CipherError:
        raise CipherError("НЕВЕРНЫЙ_КЛЮЧ")


# ==================== Файловые операции (бинарный режим) ====================

def encrypt_file(input_path: str, output_path: str, key32: int):
    """Шифрует бинарный файл. Результат — бинарный файл."""
    with open(input_path, 'rb') as f:
        data = f.read()
    if len(data) == 0:
        raise CipherError("Входной файл пуст")
    encrypted = encrypt_des32(data, key32)
    with open(output_path, 'wb') as f:
        f.write(encrypted)


def decrypt_file(input_path: str, output_path: str, key32: int):
    """Расшифровывает бинарный файл."""
    with open(input_path, 'rb') as f:
        data = f.read()

    if len(data) == 0:
        raise CipherError("Входной файл пуст")
    if len(data) % 4 != 0:
        raise CipherError(
            f"Размер файла ({len(data)} байт) не кратен 4. "
            f"Файл повреждён или это не шифротекст DES-32."
        )
    if b'\xef\xbf\xbd' in data:
        raise CipherError(
            "Файл содержит символы замены (U+FFFD). "
            "Бинарный шифротекст был открыт и сохранён в текстовом "
            "редакторе. Перешифруйте файл заново или используйте "
            "Base64-режим."
        )

    try:
        decrypted = decrypt_des32(data, key32)
    except CipherError as e:
        if str(e) == "НЕВЕРНЫЙ_КЛЮЧ":
            raise CipherError(
                "Не удалось расшифровать: неверный ключ или "
                "повреждённый шифротекст."
            )
        raise

    with open(output_path, 'wb') as f:
        f.write(decrypted)


# ==================== Base64-режим (для передачи через текст) ====================

def encrypt_file_base64(input_path: str, output_path: str, key32: int):
    """Шифрует файл и сохраняет результат в Base64 (текстовый файл)."""
    with open(input_path, 'rb') as f:
        data = f.read()
    if len(data) == 0:
        raise CipherError("Входной файл пуст")
    encrypted = encrypt_des32(data, key32)
    b64 = base64.b64encode(encrypted).decode('ascii')
    with open(output_path, 'w', encoding='ascii') as f:
        f.write(b64)


def decrypt_file_base64(input_path: str, output_path: str, key32: int):
    """Расшифровывает файл, зашифрованный в Base64."""
    with open(input_path, 'r', encoding='ascii') as f:
        content = f.read().strip()

    if not content:
        raise CipherError("Входной файл пуст")
    if b'\xef\xbf\xbd' in content.encode('utf-8', errors='replace'):
        raise CipherError(
            "Файл повреждён: содержит символы замены. "
            "Base64-текст должен содержать только A–Z, a–z, 0–9, +, /, =."
        )

    try:
        encrypted = base64.b64decode(content, validate=True)
    except Exception:
        raise CipherError(
            "Не удалось декодировать Base64. "
            "Убедитесь, что файл не повреждён и содержит только "
            "символы A–Z, a–z, 0–9, +, /, =."
        )

    try:
        decrypted = decrypt_des32(encrypted, key32)
    except CipherError as e:
        if str(e) == "НЕВЕРНЫЙ_КЛЮЧ":
            raise CipherError(
                "Не удалось расшифровать: неверный ключ или "
                "повреждённый шифротекст."
            )
        raise

    with open(output_path, 'wb') as f:
        f.write(decrypted)