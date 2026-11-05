from collections import Counter


PRECISION_BITS = 32
FULL = 1 << PRECISION_BITS
HALF = FULL >> 1
QUARTER = HALF >> 1
MASK = FULL - 1


class ArithmeticError(Exception):
    """Ошибка арифметического кодирования."""


def _build_tables(data: bytes):
    counts = Counter(data)
    total = len(data)
    symbols = sorted(counts)
    low_table, high_table = {}, {}
    cumulative = 0
    for symbol in symbols:
        low_table[symbol] = cumulative
        cumulative += counts[symbol]
        high_table[symbol] = cumulative
    return low_table, high_table, total, symbols, counts


def encode(data: bytes) -> bytes:
    if not data:
        return b""

    low_table, high_table, total, symbols, counts = _build_tables(data)

    low, high, pending = 0, FULL - 1, 0
    bits = []

    def emit(bit: int) -> None:
        nonlocal pending
        bits.append(bit)
        while pending:
            bits.append(bit ^ 1)
            pending -= 1

    for symbol in data:
        width = high - low + 1
        high = low + width * high_table[symbol] // total - 1
        low = low + width * low_table[symbol] // total
        while True:
            if high < HALF:
                emit(0)
            elif low >= HALF:
                emit(1)
                low -= HALF
                high -= HALF
            elif low >= QUARTER and high < 3 * QUARTER:
                pending += 1
                low -= QUARTER
                high -= QUARTER
            else:
                break
            low = (low << 1) & MASK
            high = ((high << 1) | 1) & MASK

    pending += 1
    emit(0 if low < QUARTER else 1)
    while len(bits) % 8:
        bits.append(0)

    body = bytearray()
    for index in range(0, len(bits), 8):
        byte = 0
        for bit in bits[index:index + 8]:
            byte = (byte << 1) | bit
        body.append(byte)

    header = bytearray()
    header.extend(len(data).to_bytes(8, "big"))
    header.extend(len(symbols).to_bytes(2, "big"))
    for symbol in symbols:
        header.append(symbol)
        header.extend(counts[symbol].to_bytes(8, "big"))

    return bytes(header) + bytes(body)


def decode(payload: bytes) -> bytes:
    if len(payload) < 10:
        raise ArithmeticError("Слишком короткий поток")

    length = int.from_bytes(payload[:8], "big")
    symbol_count = int.from_bytes(payload[8:10], "big")
    offset = 10

    counts = {}
    for _ in range(symbol_count):
        symbol = payload[offset]
        count = int.from_bytes(payload[offset + 1:offset + 9], "big")
        counts[symbol] = count
        offset += 9

    total = sum(counts.values())
    low_table, high_table = {}, {}
    cumulative = 0
    for symbol in sorted(counts):
        low_table[symbol] = cumulative
        cumulative += counts[symbol]
        high_table[symbol] = cumulative

    body = payload[offset:]
    bits = []
    for byte in body:
        for shift in range(7, -1, -1):
            bits.append((byte >> shift) & 1)

    low, high, value, position = 0, FULL - 1, 0, 0
    for _ in range(PRECISION_BITS):
        bit = bits[position] if position < len(bits) else 0
        value = (value << 1) | bit
        position += 1

    result = bytearray()
    for _ in range(length):
        width = high - low + 1
        scaled = ((value - low + 1) * total - 1) // width
        symbol = None
        for candidate in sorted(counts):
            if low_table[candidate] <= scaled < high_table[candidate]:
                symbol = candidate
                break
        result.append(symbol)

        high = low + width * high_table[symbol] // total - 1
        low = low + width * low_table[symbol] // total

        while True:
            if high < HALF:
                pass
            elif low >= HALF:
                low -= HALF
                high -= HALF
                value -= HALF
            elif low >= QUARTER and high < 3 * QUARTER:
                low -= QUARTER
                high -= QUARTER
                value -= QUARTER
            else:
                break
            low = (low << 1) & MASK
            high = ((high << 1) | 1) & MASK
            bit = bits[position] if position < len(bits) else 0
            value = ((value << 1) | bit) & MASK
            position += 1

    return bytes(result)