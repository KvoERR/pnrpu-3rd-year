BLOCK_BYTES = 4
BLOCK_BITS = 32
HALF_BITS = 16
ROUNDS = 8


IP_TABLE = [
    2, 6, 10, 14, 18, 22, 26, 30,
    4, 8, 12, 16, 20, 24, 28, 32,
    1, 5, 9, 13, 17, 21, 25, 29,
    3, 7, 11, 15, 19, 23, 27, 31,
]

FP_TABLE = [0] * BLOCK_BITS
for source_pos, target_pos in enumerate(IP_TABLE):
    FP_TABLE[target_pos - 1] = source_pos + 1

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


def _to_bits(data: bytes) -> str:
    return "".join(f"{byte:08b}" for byte in data)


def _to_bytes(bits: str) -> bytes:
    bits = bits + "0" * ((-len(bits)) % 8)
    return bytes(int(bits[i:i + 8], 2) for i in range(0, len(bits), 8))


def _permute(bits: str, table: list) -> str:
    return "".join(bits[position - 1] for position in table)


def _xor(left: str, right: str) -> str:
    return "".join("1" if a != b else "0" for a, b in zip(left, right))


def _apply_sboxes(bits: str) -> str:
    result = []
    for index in range(4):
        block = bits[index * 6:(index + 1) * 6]
        row = int(block[0] + block[5], 2)
        column = int(block[1:5], 2)
        value = SBOXES[index][row][column]
        result.append(f"{value:04b}")
    return "".join(result)


def _round_keys(key: int) -> list:
    key_bits = f"{key:032b}"
    left, right = key_bits[:HALF_BITS], key_bits[HALF_BITS:]
    keys = []
    for round_index in range(ROUNDS):
        shift = (round_index + 1) % HALF_BITS
        left_shifted = left[shift:] + left[:shift]
        right_shifted = right[shift:] + right[:shift]
        combined = left_shifted + right_shifted
        round_constant = f"{(round_index * 0x1F + 0x3A7) & 0xFFFF:016b}"
        keys.append(_xor(combined[:HALF_BITS], round_constant))
    return keys


def _feistel(half: str, round_key: str) -> str:
    expanded = _permute(half, E_TABLE)
    mixed = _xor(expanded, round_key + round_key[:8])
    substituted = _apply_sboxes(mixed)
    return _permute(substituted, P_TABLE)


def _encrypt_block(block: str, keys: list) -> str:
    permuted = _permute(block, IP_TABLE)
    left, right = permuted[:HALF_BITS], permuted[HALF_BITS:]
    for round_key in keys:
        left, right = right, _xor(left, _feistel(right, round_key))
    return _permute(right + left, FP_TABLE)


def _decrypt_block(block: str, keys: list) -> str:
    return _encrypt_block(block, list(reversed(keys)))


def encrypt_block(block: bytes, key: int) -> bytes:
    if len(block) != BLOCK_BYTES:
        raise ValueError(f"Блок должен быть {BLOCK_BYTES} байта")
    if not 0 <= key < (1 << BLOCK_BITS):
        raise ValueError("Ключ должен быть 32-битным")
    keys = _round_keys(key)
    return _to_bytes(_encrypt_block(_to_bits(block), keys))


def decrypt_block(block: bytes, key: int) -> bytes:
    if len(block) != BLOCK_BYTES:
        raise ValueError(f"Блок должен быть {BLOCK_BYTES} байта")
    if not 0 <= key < (1 << BLOCK_BITS):
        raise ValueError("Ключ должен быть 32-битным")
    keys = _round_keys(key)
    return _to_bytes(_decrypt_block(_to_bits(block), keys))