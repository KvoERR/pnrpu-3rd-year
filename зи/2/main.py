import math
import sys
import random


class CipherError(Exception):
    pass


def is_prime_miller_rabin(n, k=20):
    """Проверка числа n на простоту тестом Миллера-Рабина."""
    if n < 2:
        return False
    if n == 2 or n == 3:
        return True
    if n % 2 == 0:
        return False

    # Представление n-1 = 2^r * d
    r, d = 0, n - 1
    while d % 2 == 0:
        r += 1
        d //= 2

    # k раундов теста
    for _ in range(k):
        a = random.randrange(2, n - 1)
        x = pow(a, d, n)

        if x == 1 or x == n - 1:
            continue

        for _ in range(r - 1):
            x = pow(x, 2, n)
            if x == n - 1:
                break
        else:
            return False

    return True


def generate_prime(bits):
    """Генерирует простое число разрядности bits бит."""
    while True:
        # Генерируем случайное число нужной разрядности
        n = random.getrandbits(bits)
        # Устанавливаем старший бит (чтобы было ровно bits бит)
        n |= (1 << (bits - 1))
        # Устанавливаем младший бит (чтобы было нечётным)
        n |= 1
        if is_prime_miller_rabin(n):
            return n


def extended_gcd(a, b):
    """Расширенный алгоритм Евклида.
    Возвращает (gcd, x, y) такие что a*x + b*y = gcd(a, b).
    """
    if a == 0:
        return b, 0, 1
    gcd, x1, y1 = extended_gcd(b % a, a)
    x = y1 - (b // a) * x1
    y = x1
    return gcd, x, y


def mod_inverse(e, phi):
    """Находит обратное число d: e*d ≡ 1 (mod phi)."""
    gcd, x, _ = extended_gcd(e % phi, phi)
    if gcd != 1:
        raise CipherError("Обратное число не существует (e и phi не взаимно просты)")
    return x % phi


def generate_keys(p, q):
    """Генерация ключей RSA по шагам 1–6 описания.
    Возвращает (e, n, d).
    """
    n = p * q
    phi = (p - 1) * (q - 1)

    e = 65537
    if math.gcd(e, phi) != 1:
        # Если 65537 не подошёл, ищем другой
        for candidate in range(3, min(n, phi), 2):
            if math.gcd(candidate, phi) == 1:
                e = candidate
                break

    if e is None:
        raise CipherError("Не удалось подобрать e")

    # Шаг 4: находим d через уравнение e*d + (p-1)(q-1)*y = 1
    d = mod_inverse(e, phi)

    return e, n, d


def _text_to_bytes_blocks(text, n):
    """Разбивает текст на блоки чисел, каждый < n.
    k = floor(log2(n)) бит на блок.
    """
    if n <= 1:
        raise CipherError("n должно быть > 1")

    k = n.bit_length() - 1  # floor(log2(n))
    block_size = max(1, k // 8)

    text_bytes = text.encode('utf-8')
    blocks = []

    for i in range(0, len(text_bytes), block_size):
        chunk = text_bytes[i:i + block_size]
        block_int = int.from_bytes(chunk, 'big')

        # Если блок >= n, уменьшаем размер блока
        while block_int >= n and len(chunk) > 1:
            chunk = chunk[:-1]
            block_int = int.from_bytes(chunk, 'big')

        if block_int >= n:
            raise CipherError(
                f"Значение блока {block_int} >= n={n}. "
                f"Увеличьте p и q (нужно n > {block_int})."
            )

        blocks.append(block_int)

    return blocks

def _blocks_to_text(blocks):
    """Собирает блоки обратно в текст."""
    all_bytes = b''
    for block in blocks:
        if block == 0:
            all_bytes += b'\x00'
            continue
        # Определяем количество байт
        byte_len = (block.bit_length() + 7) // 8
        all_bytes += block.to_bytes(byte_len, 'big')

    # Удаляем нулевые байти-заполнители (если текст не начинался с \x00)
    try:
        text = all_bytes.decode('utf-8')
        # Убираем trailing nulls и whitespace padding
        text = text.rstrip('\x00').rstrip()
        return text
    except UnicodeDecodeError:
        raise CipherError("Не удалось декодировать результат")


def encrypt(text, e, n):
    """Шифрование: c_i = pow(m_i, e, n) для каждого блока."""
    if not isinstance(text, str):
        raise TypeError("text должен быть строкой")
    if text == '':
        raise CipherError("text не может быть пустым")

    blocks = _text_to_bytes_blocks(text, n)
    ciphertext = [pow(m, e, n) for m in blocks]
    return ciphertext


def decrypt(ciphertext, d, n):
    """Расшифровка: m_i = pow(c_i, d, n) для каждого блока."""
    if not isinstance(ciphertext, (list, str)):
        raise TypeError("ciphertext должен быть списком чисел или строкой")
    if not ciphertext:
        raise CipherError("ciphertext не может быть пустым")

    # Если строка — парсим числа
    if isinstance(ciphertext, str):
        ciphertext = [int(x.strip()) for x in ciphertext.split(',') if x.strip()]

    plaintext_blocks = [pow(c, d, n) for c in ciphertext]
    return _blocks_to_text(plaintext_blocks)

