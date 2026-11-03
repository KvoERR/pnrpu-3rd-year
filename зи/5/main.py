class CodeError(Exception):
    """Ошибка кодирования или декодирования."""


INFORMATIONAL_BITS = 2
CODEWORD_BITS = 4

GENERATOR = [
    [1, 0, 0, 1],
    [0, 1, 1, 1],
]

CHECK = [
    [1, 0],
    [0, 1],
    [1, 1],
    [1, 0],
]

SYNDROME_TO_POSITION = {
    (0, 0): None,
    (1, 0): 1,
    (0, 1): 2,
    (1, 1): 3,
}


def _multiply(vector: list, matrix: list) -> list:
    columns = len(matrix[0])
    result = [0] * columns
    for column in range(columns):
        total = 0
        for row in range(len(vector)):
            total ^= vector[row] & matrix[row][column]
        result[column] = total
    return result


def _validate_bits(bits: list, expected: int, name: str) -> None:
    if len(bits) != expected:
        raise CodeError(f"{name} должно содержать ровно {expected} бит")
    if any(bit not in (0, 1) for bit in bits):
        raise CodeError("Биты должны быть 0 или 1")


def encode(message: list) -> list:
    """Кодирует 2 информационных бита в 4-битное слово."""
    _validate_bits(message, INFORMATIONAL_BITS, "Сообщение")
    return _multiply(message, GENERATOR)


def syndrome(received: list) -> tuple:
    """Вычисляет синдром принятого слова."""
    _validate_bits(received, CODEWORD_BITS, "Кодовое слово")
    return tuple(_multiply(received, CHECK))


def error_position(received: list):
    """Возвращает 1-based позицию ошибки или None."""
    result = syndrome(received)
    return SYNDROME_TO_POSITION.get(result)


def decode(received: list) -> tuple:
    """
    Декодирует принятое слово.
    Возвращает (сообщение, позиция_ошибки, исправленное_слово).
    """
    _validate_bits(received, CODEWORD_BITS, "Кодовое слово")

    corrected = list(received)
    position = error_position(received)
    if position is not None:
        corrected[position - 1] ^= 1

    return corrected[:INFORMATIONAL_BITS], position, corrected


def bits_to_text(bits: list) -> str:
    return "".join(str(bit) for bit in bits)


def text_to_bits(text: str) -> list:
    if not text or any(char not in "01" for char in text):
        raise CodeError("Строка должна содержать только 0 и 1")
    return [int(char) for char in text]