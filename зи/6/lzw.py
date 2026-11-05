class LzwError(Exception):
    """Ошибка LZW-кодирования."""


def encode(data: bytes) -> bytes:
    if not data:
        return b""

    dictionary = {bytes([symbol]): symbol for symbol in range(256)}
    next_code = 256

    codes = []
    current = bytes([data[0]])

    for byte in data[1:]:
        candidate = current + bytes([byte])
        if candidate in dictionary:
            current = candidate
        else:
            codes.append(dictionary[current])
            dictionary[candidate] = next_code
            next_code += 1
            current = bytes([byte])

    codes.append(dictionary[current])

    header = bytearray()
    header.extend(len(data).to_bytes(8, "big"))
    header.extend(len(codes).to_bytes(8, "big"))

    body = bytearray()
    for code in codes:
        body.extend(code.to_bytes(2, "big"))

    return bytes(header) + bytes(body)


def decode(payload: bytes) -> bytes:
    if len(payload) < 16:
        raise LzwError("Слишком короткий поток")

    original_length = int.from_bytes(payload[:8], "big")
    code_count = int.from_bytes(payload[8:16], "big")

    codes = []
    offset = 16
    for _ in range(code_count):
        codes.append(int.from_bytes(payload[offset:offset + 2], "big"))
        offset += 2

    dictionary = {symbol: bytes([symbol]) for symbol in range(256)}
    next_code = 256

    current = dictionary[codes[0]]
    result = bytearray(current)

    for code in codes[1:]:
        if code in dictionary:
            entry = dictionary[code]
        elif code == next_code:
            entry = current + current[:1]
        else:
            raise LzwError(f"Неизвестный код {code}")

        result.extend(entry)
        dictionary[next_code] = current + entry[:1]
        next_code += 1
        current = entry

    return bytes(result[:original_length])