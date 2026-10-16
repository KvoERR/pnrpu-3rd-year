import math
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


# ======================== Эль-Гамаль ========================


def generate_prime_elgamal(num_digits):
    """Генерирует простое число с ≥ num_digits десятичными цифрами."""
    bits = math.ceil(num_digits * math.log2(10))
    return generate_prime(bits)


def prime_factors(n):
    """Возвращает словарь {простой_множитель: степень} разложения n."""
    factors = {}
    d = 2
    while d * d <= n:
        while n % d == 0:
            factors[d] = factors.get(d, 0) + 1
            n //= d
        d += 1
    if n > 1:
        factors[n] = factors.get(n, 0) + 1
    return factors


def find_generator(p):
    """Находит наименьший примитивный корень (генератор) для простого p."""
    if p == 2:
        return 1
    phi = p - 1  # p простое
    factors = prime_factors(phi)
    factors_keys = list(factors.keys())

    for g in range(2, p):
        ok = True
        for q in factors_keys:
            if pow(g, phi // q, p) == 1:
                ok = False
                break
        if ok:
            return g
    raise CipherError(f"Не удалось найти генератор для p={p}")


def mod_inverse(a, m):
    """Вычисляет обратное a по модулю m."""
    g, x, _ = extended_gcd(a % m, m)
    if g != 1:
        raise CipherError("Обратное число не существует")
    return x % m


def generate_elgamal_keys(p=None, g=None, x=None):
    """Генерация ключей Эль-Гамаля.
    p — простое ≥ 20 десятичных цифр (модуль)
    g — простое ≥ 12 десятичных цифр (генератор)
    x — простое ≥ 12 десятичных цифр (закрытый ключ)
    Возвращает (p, g, x, y) где y = g^x mod p — публичный ключ.
    """
    if p is None:
        p = generate_prime_elgamal(20)
    if g is None:
        g = generate_prime_elgamal(12)
    if x is None:
        x = generate_prime_elgamal(12)

    y = pow(g, x, p)
    return p, g, x, y


def _text_to_blocks_elgamal(text, p):
    """Текст -> список целых чисел (каждое < p)."""
    data = text.encode('utf-8')
    # Размер блока в байтах: оставляем 1 байт под маркер \x01
    block_size = (p.bit_length() - 1) // 8 - 1
    if block_size < 1:
        block_size = 1

    blocks = []
    for i in range(0, len(data), block_size):
        chunk = data[i:i + block_size]
        chunk = chunk + b'\x01'
        m = int.from_bytes(chunk, byteorder='big')
        if m >= p:
            # Если всё ещё не влезает, уменьшаем блок на 1 байт
            block_size -= 1
            if block_size < 1:
                raise CipherError(
                    f"Число p={p} слишком мало для шифрования текста."
                )
            chunk = data[i:i + block_size]
            chunk = chunk + b'\x01'
            m = int.from_bytes(chunk, byteorder='big')
        blocks.append(m)
    return blocks


def _blocks_to_text_elgamal(blocks, p):
    """Список целых чисел -> текст."""
    # Вычисляем block_size так же, как в _text_to_blocks_elgamal
    block_size = (p.bit_length() - 1) // 8 - 1
    if block_size < 1:
        block_size = 1

    data = bytearray()
    for m in blocks:
        byte_len = (m.bit_length() + 7) // 8
        if byte_len > block_size + 1:
            byte_len = block_size + 1
        chunk = m.to_bytes(byte_len, byteorder='big')
        if chunk.endswith(b'\x01'):
            chunk = chunk[:-1]
        data.extend(chunk)

    try:
        return data.decode('utf-8')
    except UnicodeDecodeError:
        raise CipherError("Не удалось декодировать результат Эль-Гамаля")


def encrypt_elgamal(text, p, g, y, k=None):
    """Шифрование текста методом Эль-Гамаля.
    text — строка
    p, g, y — публичные ключи (p ≥ 20 цифр, g простое ≥ 12 цифр)
    k — временный ключ (≥ 20 цифр), генерируется случайно если None
    Возвращает список пар [(c1, c2), ...].
    """
    if not isinstance(text, str):
        raise TypeError("text должен быть строкой")
    if text == '':
        raise CipherError("text не может быть пустым")

    blocks = _text_to_blocks_elgamal(text, p)
    ciphertext = []

    for m in blocks:
        if k is None:
            k_val = generate_prime_elgamal(20)
        else:
            k_val = k

        c1 = pow(g, k_val, p)
        s = pow(y, k_val, p)
        c2 = (m * s) % p
        ciphertext.append((c1, c2))

    return ciphertext


def decrypt_elgamal(ciphertext_pairs, x, p):
    """Расшифрование текста методом Эль-Гамаля.
    ciphertext_pairs — список пар [(c1, c2), ...]
    x — закрытый ключ
    p — модуль
    Возвращает расшифрованный текст.
    """
    if not isinstance(ciphertext_pairs, list):
        raise TypeError("ciphertext_pairs должен быть списком пар")

    blocks = []
    for c1, c2 in ciphertext_pairs:
        s = pow(c1, x, p)
        s_inv = mod_inverse(s, p)
        m = (c2 * s_inv) % p
        blocks.append(m)

    return _blocks_to_text_elgamal(blocks, p)
