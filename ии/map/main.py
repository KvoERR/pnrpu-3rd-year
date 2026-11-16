from minisom import MiniSom
import numpy as np
from sklearn.datasets import load_iris
import matplotlib.pyplot as plt

# ---------- 1. Данные ----------
X = load_iris().data
y = load_iris().target
X = (X - X.mean(0)) / X.std(0)

# ---------- 2. Утилита U-Matrix ----------
def u_matrix(weights):
    gx, gy, _ = weights.shape
    u = np.zeros((gx, gy))
    for i in range(gx):
        for j in range(gy):
            neigh = []
            for di, dj in [(-1,0),(1,0),(0,-1),(0,1)]:
                ni, nj = i+di, j+dj
                if 0 <= ni < gx and 0 <= nj < gy:
                    neigh.append(np.linalg.norm(weights[i,j] - weights[ni,nj]))
            u[i,j] = np.mean(neigh) if neigh else 0.0
    return u

# ---------- 3. Обучение с сохранением снапшотов ----------
som = MiniSom(x=10, y=10, input_len=4,
              sigma=3.0, learning_rate=0.5,
              neighborhood_function='gaussian', random_seed=42)

som.random_weights_init(X)

weights_init = som.get_weights().copy()   # состояние ДО обучения

snapshots = [weights_init]
total_iters = 10000
chunk = 2000
for step in range(total_iters // chunk):
    som.train_random(X, num_iteration=chunk)
    snapshots.append(som.get_weights().copy())

weights_final = som.get_weights()

# ---------- 4. Метрики ----------
print("QE (quantization error):", som.quantization_error(X))
print("TE (topographic error): ", som.topographic_error(X))

# ---------- 5. U-Matrix: до vs после ----------
fig, axes = plt.subplots(1, 2, figsize=(14, 6))

im0 = axes[0].pcolor(u_matrix(weights_init).T, cmap='bone_r')
axes[0].set_title('U-Matrix ДО обучения')
axes[0].set_xlabel('x'); axes[0].set_ylabel('y')
plt.colorbar(im0, ax=axes[0], label='U-distance')

im1 = axes[1].pcolor(u_matrix(weights_final).T, cmap='bone_r')
axes[1].set_title('U-Matrix ПОСЛЕ обучения')
axes[1].set_xlabel('x'); axes[1].set_ylabel('y')
plt.colorbar(im1, ax=axes[1], label='U-distance')

plt.tight_layout()
plt.show()

# ---------- 6. Процесс: снапшоты ----------
n = len(snapshots)
fig, axes = plt.subplots(1, n, figsize=(4*n, 4))
for k, w in enumerate(snapshots):
    axes[k].pcolor(u_matrix(w).T, cmap='bone_r')
    axes[k].set_title(f'{k*chunk} итераций' if k > 0 else 'старт')
    axes[k].set_xticks([]); axes[k].set_yticks([])
plt.tight_layout()
plt.show()

# ---------- 7. Финальная карта с точками (BMU) ----------
plt.figure(figsize=(7, 7))
plt.pcolor(som.distance_map().T, cmap='bone_r')
plt.colorbar(label='U-distance')

markers = ['o', 's', 'D']
colors  = ['r', 'g', 'b']
for i, x in enumerate(X):
    w = som.winner(x)
    plt.plot(w[0] + 0.5, w[1] + 0.5, markers[y[i]],
             markeredgecolor=colors[y[i]], markerfacecolor='none',
             markersize=8, markeredgewidth=1.2)

plt.title('Карта Кохонена: BMU по классам Iris')
plt.xlabel('x'); plt.ylabel('y')
plt.show()

# ---------- 8. Проверка: что было "сначала" ----------
w0 = weights_init
print("\nФорма начальных весов:", w0.shape)
print("Вес нейрона (0,0) до обучения:", w0[0, 0])
dists = np.linalg.norm(X - w0[0, 0], axis=1)
print("Ближайший сэмпл из X:", np.argmin(dists), "| расстояние:", dists.min())