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


def generate_keys(p, q): #1
    """Генерация ключей RSA по шагам 1–6 описания.
    Возвращает (e, n, d).
    """
    n = p * q #2
    phi = (p - 1) * (q - 1)

    # 3
    for candidate in range(3, min(n, phi), 2):
        if math.gcd(candidate, phi) == 1:
            e = candidate
            break

    if e is None:
        raise CipherError("Не удалось подобрать e")

    # Шаг 4: находим d через уравнение e*d + (p-1)(q-1)*y = 1
    gcd, d, _ = extended_gcd(e % phi, phi)
    if gcd != 1:
        raise CipherError("Обратное число не существует (e и phi не взаимно просты)")
    d = d % phi

    return e, n, d


def _text_to_blocks(text, n):
    """Текст -> (blocks, k). Блок = k = floor(log2 n) бит (как в задании)."""
    k = n.bit_length() - 1
    if k < 8:
        raise CipherError(f"n={n} слишком мало (k={k} бит, нужно ≥8)")

    bits = ''.join(f'{b:08b}' for b in text.encode('utf-8'))
    total_bits = len(bits)
    bits += '0' * ((-total_bits) % k)          # паддинг до кратного k

    blocks = [int(bits[i:i+k], 2) for i in range(0, len(bits), k)]
    return blocks, k, total_bits


def _blocks_to_text(blocks, k, total_bits):
    """Блоки -> текст."""
    bits = ''.join(format(m, f'0{k}b') for m in blocks)[:total_bits]
    bits += '0' * ((-len(bits)) % 8)
    data = bytes(int(bits[i:i+8], 2) for i in range(0, len(bits), 8))
    try:
        return data.decode('utf-8')
    except UnicodeDecodeError:
        raise CipherError("Не удалось декодировать результат")


def encrypt(text, p, q):
    if not isinstance(text, str):
        raise TypeError("text должен быть строкой")
    if text == '':
        raise CipherError("text не может быть пустым")

    e, n, d = generate_keys(p, q)
    blocks, k, total_bits = _text_to_blocks(text, n)
    ciphertext = [pow(m, e, n) for m in blocks]
    return ciphertext, k, total_bits, e, n, d


def decrypt(ciphertext, k, total_bits, d, n):
    if not isinstance(ciphertext, (list, str)):
        raise TypeError("ciphertext должен быть списком чисел или строкой")
    if not ciphertext:
        raise CipherError("ciphertext не может быть пустым")

    if isinstance(ciphertext, str):
        ciphertext = [int(x.strip()) for x in ciphertext.split(',') if x.strip()]

    plaintext_blocks = [pow(c, d, n) for c in ciphertext]
    return _blocks_to_text(plaintext_blocks, k, total_bits)

