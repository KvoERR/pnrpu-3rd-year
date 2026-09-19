class CipherError(Exception):
    pass


def encrypt(word: str, key: str) -> str:
    if not isinstance(word, str) or not isinstance(key, str):
        raise TypeError("word и key должны быть строками")
    if not word:
        raise CipherError("word не может быть пустым")
    if not key:
        raise CipherError("key не может быть пустым")
    if int(max(key))>len(key):
        raise CipherError('Неверный ключ')

    remainder = len(word) % len(key)
    if remainder:
        word += " " * (len(key) - remainder)
    cels = len(word)
    cols = len(key)
    word_matr = [""] * cols
    for i in range(0, cels, cols):
        for j in range(cols):
            word_matr[j] += word[i:i + cols][j] #Распихиваем буквы по колонкам
    return "".join(word_matr)


def decrypt(word: str, key: str) -> str:
    if not isinstance(word, str) or not isinstance(key, str):
        raise TypeError("word и key должны быть строками")
    if not word:
        raise CipherError("word не может быть пустым")
    if not key:
        raise CipherError("key не может быть пустым")
    if int(max(key))>len(key):
        raise CipherError('Неверный ключ')

    
    remainder = len(word) % len(key)
    if remainder:
        word += " " * (len(key) - remainder)
    cels = len(word)
    cols = len(key)
    rows=cels//cols
    word_matr = [word[i:i+rows] for i in range(0, cels, rows)]
    word=""
    for i in range(rows):
        for j in range(cols):
            word+=word_matr[j][i]
    return word

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