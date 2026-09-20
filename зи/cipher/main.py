class CipherError(Exception):
    pass


def _validate(word: str, key: str) -> None:
    if not isinstance(word, str) or not isinstance(key, str):
        raise TypeError("word и key должны быть строками")
    if not word:
        raise CipherError("word не может быть пустым")
    if not key:
        raise CipherError("key не может быть пустым")
    if sorted(key) != [str(i) for i in range(1, len(key) + 1)]:
        raise CipherError('Неверный ключ')


def encrypt(word: str, key: str) -> str:
    _validate(word, key)

    remainder = len(word) % len(key)
    if remainder:
        word += " " * (len(key) - remainder)
    cels = len(word)
    cols = len(key)
    word_matr = [""] * cols
    for i in range(0, cels, cols):
        for j in range(cols):
            col = int(key[j]) - 1 # получаем номер колонки в зависимости от ключа
            word_matr[col] += word[i:i + cols][j] # распихиваем по столбцам
    return "".join(word_matr)


def decrypt(word: str, key: str) -> str:
    _validate(word, key)

    remainder = len(word) % len(key)
    if remainder:
        word += " " * (len(key) - remainder)
    cels = len(word)
    cols = len(key)
    rows = cels // cols
    word_matr = [word[i:i + rows] for i in range(0, cels, rows)]
    result = ""
    for i in range(rows):
        for j in range(cols):
            col = int(key[j]) - 1
            result += word_matr[col][i]
    return result


if __name__ == "__main__":
    try:
        word = "сасиски в стакане"
        key = "51243"
        encrypted = encrypt(word, key)
        print(f"Зашифровано: {encrypted}")
        decrypted = decrypt(encrypted, key)
        print(f"Расшифровано: {decrypted}")
    except (CipherError, TypeError) as e:
        print(f"Ошибка: {e}")