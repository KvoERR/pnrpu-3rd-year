"""Тест шифрования Эль-Гамаля с сообщением ≥ 256 символов."""
import math
from main import (
    generate_elgamal_keys,
    encrypt_elgamal,
    decrypt_elgamal,
    is_prime_miller_rabin,
)

# Тестовое сообщение длиной ≥ 256 символов
message = (
    "Это тестовое сообщение для шифрования методом Эль-Гамаля. "
    "Оно содержит достаточно символов для проверки корректности работы алгоритма. "
    "Здесь используется текст с разными символами, включая кириллицу: Привет, мир! "
    "Сообщение должно быть не менее 256 символов для соответствия требованиям задания. "
    "Продолжаем добавлять текст для достижения нужной длины. "
    "Каждое слово и символ учитываются при подсчёте длины сообщения. "
    "Эль-Гамаль — криптосистема с открытым ключом, основанная на сложности "
    "дискретного логарифмирования. Безопасность системы зависит от длины простого "
    "числа p и секретности закрытого ключа x. Генератор g должен быть примитивным "
    "корнем по модулю p. Временный ключ k генерируется случайно для каждого блока."
)

print(f"Длина сообщения: {len(message)} символов")
print(f"Сообщение: {message}")
print("=" * 80)

# Генерация ключей
print("Генерация ключей Эль-Гамаля...")
p, g, x, y = generate_elgamal_keys()

print(f"p = {p}")
print(f"  разрядность: {len(str(p))} цифр ({p.bit_length()} бит)")
print(f"  простое: {is_prime_miller_rabin(p)}")

print(f"g = {g}")
print(f"  разрядность: {len(str(g))} цифр ({g.bit_length()} бит)")
print(f"  простое: {is_prime_miller_rabin(g)}")

print(f"x = {x}")
print(f"  разрядность: {len(str(x))} цифр ({x.bit_length()} бит)")
print(f"  простое: {is_prime_miller_rabin(x)}")

print(f"y = {y}")
print(f"  разрядность: {len(str(y))} цифр ({y.bit_length()} бит)")

# Проверка требований
assert len(str(p)) >= 20, f"p имеет {len(str(p))} цифр, нужно ≥ 20"
assert len(str(g)) >= 12, f"g имеет {len(str(g))} цифр, нужно ≥ 12"
assert is_prime_miller_rabin(g), "g не является простым"
assert len(str(x)) >= 12, f"x имеет {len(str(x))} цифр, нужно ≥ 12"
assert is_prime_miller_rabin(x), "x не является простым"
assert len(message) >= 256, f"Сообщение {len(message)} символов, нужно ≥ 256"

print("\nВсе проверки требований пройдены!")
print("=" * 80)

# Шифрование
print("\nШифрование...")
ciphertext = encrypt_elgamal(message, p, g, y)
print(f"Зашифровано {len(ciphertext)} блоков")
print(f"Первая пара: c1={ciphertext[0][0]}, c2={ciphertext[0][1]}")
print("=" * 80)

# Расшифрование
print("\nРасшифровка...")
decrypted = decrypt_elgamal(ciphertext, x, p)
print(f"Расшифровано: {decrypted}")
print("=" * 80)

# Проверка
if decrypted == message:
    print("\n[OK] УСПЕХ: Расшифрованный текст совпадает с исходным!")
else:
    print("\n[FAIL] ОШИБКА: Тексты не совпадают!")
    print(f"Ожидалось ({len(message)} символов): {message[:100]}...")
    print(f"Получено ({len(decrypted)} символов): {decrypted[:100]}...")
    
    # Находим первое несовпадение
    for i, (a, b) in enumerate(zip(message, decrypted)):
        if a != b:
            print(f"Первое несовпадение на позиции {i}: '{a}' vs '{b}'")
            break
