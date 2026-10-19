# hamming.py
import numpy as np
from pathlib import Path
from PIL import Image
import json

IMG_SIZE = 64          # 64x64
VEC_LEN = IMG_SIZE * IMG_SIZE   # 4096 бит
THRESHOLD = 0.5        # порог бинаризации (0..1)


# ─────────────────────────────────────────────────────────────
# 1. Подготовка данных
# ─────────────────────────────────────────────────────────────

def load_binary_dataset(data_dir):
    """
    Загружает изображения из data/custom/ и переводит их
    в бинарные векторы длины 4096.
    Возвращает: X (n, 4096) из {0,1}, y (n,)
    """
    data_dir = Path(data_dir)
    X, y = [], []

    for filepath in sorted(data_dir.glob("*.png")):
        digit = int(filepath.stem.split("_")[0])

        img = Image.open(filepath).convert("L").resize((IMG_SIZE, IMG_SIZE))
        arr = np.asarray(img, dtype=np.float32) / 255.0
        binary = (arr > THRESHOLD).astype(np.float32).reshape(-1)

        X.append(binary)
        y.append(digit)

    if not X:
        raise FileNotFoundError(
            f"В папке {data_dir} нет изображений. "
            f"Положите .png файлы в data/custom/"
        )

    return np.stack(X), np.array(y, dtype=np.int64)


# ─────────────────────────────────────────────────────────────
# 2. Сеть Хемминга
# ─────────────────────────────────────────────────────────────

class HammingNetwork:
    """
    Двухслойная сеть Хемминга.

    Слой 1 (Хемминга):  вычисляет расстояние Хемминга до каждого эталона.
                        Выход = VEC_LEN - distance, т.е. число совпавших бит.
    Слой 2 (MAXNET):    итеративно подавляет слабые нейроны, пока не
                        останется один победитель.
    """

    def __init__(self, n_classes=10, max_iter=20, eps=0.1):
        self.n_classes = n_classes
        self.max_iter = max_iter
        self.eps = eps          # порог остановки MAXNET
        self.prototypes = None  # (n_classes, VEC_LEN) из {0,1}

    # ── обучение = запоминание эталонов ──────────────────────
    def fit(self, X, y):
        """
        Для каждого класса строим эталон: бит = 1, если он
        встречается чаще, чем 0, среди примеров этого класса.
        """
        self.prototypes = np.zeros((self.n_classes, VEC_LEN), dtype=np.float32)

        for c in range(self.n_classes):
            mask = (y == c)
            if mask.sum() == 0:
                continue
            # средняя яркость по классу чисел -> порог 0.5
            mean_img = X[mask].mean(axis=0)
            self.prototypes[c] = (mean_img > 0.5).astype(np.float32)

        return self

    # ── слой 1: сходство (число совпавших бит) ───────────────
    def _hamming_layer(self, x):
        """
        x: (VEC_LEN,) или (n, VEC_LEN)
        Возвращает: (n, n_classes) — число совпавших бит с каждым эталоном.
        """
        x = np.atleast_2d(x)
        # совпадение = 1, если биты равны
        #   XNOR: 1 - |x - p|  (для бинарных {0,1})
        diff = np.abs(x[:, None, :] - self.prototypes[None, :, :])
        matches = 1.0 - diff          # 1 = совпало, 0 = нет
        return matches.sum(axis=2)    # (n, n_classes)

    # ── слой 2: MAXNET ───────────────────────────────────────
    def _maxnet(self, activations):
        """
        Конкурентный слой: y_i <- relu(y_i - eps * sum_{j != i} y_j)
        Итерации до тех пор, пока не останется один активный нейрон.
        """
        y = activations.astype(np.float32).copy()
        for _ in range(self.max_iter):
            # сумма по всем, кроме себя
            total = y.sum(axis=1, keepdims=True) - y
            y = np.maximum(y - self.eps * total, 0.0)
            # если остался ровно один ненулевой нейрон — стоп
            if (y > 0).sum(axis=1).max() <= 1:
                break
        return y

    # ── предсказание ─────────────────────────────────────────
    def predict(self, X):
        """
        X: (n, VEC_LEN) бинарные векторы.
        Возвращает: (n,) классы и (n, n_classes) уверенности (0..1).
        """
        act = self._hamming_layer(X)       # слой 1
        out = self._maxnet(act)            # слой 2
        # нормируем выход в вероятности (сумма = 1)
        s = out.sum(axis=1, keepdims=True)
        s[s == 0] = 1.0
        probs = out / s
        return probs.argmax(axis=1), probs

    # ── сохранение / загрузка ────────────────────────────────
    def save(self, path="hamming_prototypes.npz"):
        np.savez(path, prototypes=self.prototypes,
                 n_classes=self.n_classes, max_iter=self.max_iter, eps=self.eps)
        print(f"{path} сохранён")

    @classmethod
    def load(cls, path="hamming_prototypes.npz"):
        data = np.load(path)
        net = cls(n_classes=int(data["n_classes"]),
                  max_iter=int(data["max_iter"]),
                  eps=float(data["eps"]))
        net.prototypes = data["prototypes"]
        return net


# ─────────────────────────────────────────────────────────────
# 3. Обучение (аналог train_model из main.py)
# ─────────────────────────────────────────────────────────────

def train_hamming():
    X, y = load_binary_dataset("./data/custom")
    print(f"Найдено {len(X)} примеров, вектор длины {VEC_LEN}")

    net = HammingNetwork(n_classes=10)
    net.fit(X, y)

    # оценка на обучающей выборке (сеть Хемминга не имеет отдельного
    # этапа обучения, поэтому accuracy на train — основной показатель)
    pred, probs = net.predict(X)
    acc = 100 * (pred == y).mean()
    print(f"Accuracy на обучающей выборке: {acc:.1f}%")

    net.save("hamming_prototypes.npz")


# ─────────────────────────────────────────────────────────────
# 4. Предсказание одной картинки (аналог predict из main.py)
# ─────────────────────────────────────────────────────────────

def predict_hamming(image_path):
    net = HammingNetwork.load("hamming_prototypes.npz")

    img = Image.open(image_path).convert("L").resize((IMG_SIZE, IMG_SIZE))
    arr = np.asarray(img, dtype=np.float32) / 255.0
    x = (arr > THRESHOLD).astype(np.float32).reshape(1, -1)

    pred, probs = net.predict(x)
    digit = int(pred[0])
    confidence = float(probs[0, digit])

    print(f"Распознана цифра: {digit}")
    print(f"Уверенность: {confidence:.2%}")
    print("\nСовпадения с эталонами (доля совпавших бит):")
    for d in range(10):
        bar = "█" * int(probs[0, d] * 30)
        print(f"  {d}: {probs[0, d]:.4f} {bar}")


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1 and sys.argv[1] == "train":
        train_hamming()
    elif len(sys.argv) > 1:
        predict_hamming(sys.argv[1])
    else:
        print("python hamming.py train        — обучить сеть Хемминга")
        print("python hamming.py <image.png>  — распознать цифру")