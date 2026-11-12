from minisom import MiniSom
import numpy as np
from sklearn.datasets import load_iris

X = load_iris().data
X = (X - X.mean(0)) / X.std(0)

som = MiniSom(x=10, y=10, input_len=4,
              sigma=1.0, learning_rate=0.5,
              neighborhood_function='gaussian', random_seed=42)

som.random_weights_init(X)
som.train_random(X, num_iteration=5000)   # стохастическое обучение

# Ошибка квантования
print("QE:", som.quantization_error(X))
print("TE:", som.topographic_error(X))

# Визуализация
import matplotlib.pyplot as plt
plt.figure(figsize=(7,7))
plt.pcolor(som.distance_map().T, cmap='bone_r')  # U-Matrix
plt.colorbar()

for i, x in enumerate(X):
    w = som.winner(x)                              # BMU
    plt.plot(w[0]+0.5, w[1]+0.5, 'o',
             markeredgecolor='r', markerfacecolor='None', markersize=8)
plt.show()