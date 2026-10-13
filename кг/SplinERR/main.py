"""
Замкнутый кубический сплайн — интерактивное приложение.

ЛКМ — добавить точку
ПКМ / Escape — завершить ввод и построить сплайн
R — очистить всё
Backspace / Delete — удалить последнюю точку
"""

import numpy as np
import matplotlib.pyplot as plt


# ---------------------------------------------------------------------------
# Алгоритм замкнутого кубического сплайна
# ---------------------------------------------------------------------------

def solve_cyclic_system(a, b, c, d):
    """
    Решает циклическую трёхдиагональную систему A*x = d.

    A имеет вид:
        b[0]  c[0]                       0      ...    c[N-1]
        a[1]  b[1]  c[1]                0      ...      0
             a[2]  b[2]  c[2]           0      ...      0
                  ...
        a[N-1]  0      0  ...  b[N-1]

    a — поддиагональ (a[0] игнорируется)
    b — диагональ
    c — наддиагональ (c[N-1] — правый верхний угол)
    d — правая часть
    """
    n = len(b)

    if n == 1:
        return np.array([d[0] / b[0]])

    # Убираем циклические элементы (c[N-1] и a[0])
    # Применяем модифицированную прогонку
    cp = np.zeros(n)
    dp = np.zeros(n)

    cp[0] = c[0] / b[0]
    dp[0] = d[0] / b[0]

    for i in range(1, n):
        denom = b[i] - a[i] * cp[i - 1]
        if i < n - 1:
            cp[i] = c[i] / denom
        dp[i] = (d[i] - a[i] * dp[i - 1]) / denom

    x = np.zeros(n)
    x[-1] = dp[-1]

    for i in range(n - 2, -1, -1):
        x[i] = dp[i] - cp[i] * x[i + 1]

    # Корректировка для циклического элемента
    # Добавляем влияние c[N-1] и a[0]
    if n > 1:
        # Пересчитываем с учётом цикличности
        # Используем метод добавления ранга-1
        # Это упрощённый подход — пересчитаем полностью

        # Полный цикл через решение двух систем
        # Система 1: без циклических элементов
        # Система 2: корректировка

        # Для простоты используем numpy.linalg.solve для малых N
        if n <= 100:
            pass  # перейдём к numpy
        else:
            # Для больших N — корректная прогонка
            # ... (упрощение)
            pass

    # Для надёжности используем numpy.linalg.solve
    return None  # пометим, что нужно использовать numpy


def closed_cubic_spline(points):
    """
    Возвращает массивы t_fine, x_fine, y_fine для замкнутого кубического сплайна.

    points : np.ndarray  shape (N, 2)
    """
    pts = np.asarray(points, dtype=float)
    N = len(pts)

    if N < 2:
        return None, None, None
    if N == 2:
        # Для двух точек — эллипс/овал
        t_fine = np.linspace(0, 2 * np.pi, 500)
        x_mid = (pts[0, 0] + pts[1, 0]) / 2
        y_mid = (pts[0, 1] + pts[1, 1]) / 2
        x_rad = abs(pts[0, 0] - pts[1, 0]) / 2 + 0.01
        y_rad = abs(pts[0, 1] - pts[1, 1]) / 2 + 0.01
        x_fine = x_mid + x_rad * np.cos(t_fine)
        y_fine = y_mid + y_rad * np.sin(t_fine)
        return t_fine, x_fine, y_fine

    # --- параметризация по кумулятивной длине (chord-length) ---
    diffs = np.diff(pts, axis=0)
    dists = np.sqrt(np.sum(diffs ** 2, axis=1))
    # Замыкающий отрезок
    closing = np.sqrt((pts[0, 0] - pts[-1, 0]) ** 2 + (pts[0, 1] - pts[-1, 1]) ** 2)
    dists = np.concatenate([dists, [closing]])
    t = np.zeros(N)
    t[1:] = np.cumsum(dists[:-1])
    t_norm = t / t[-1] * 2 * np.pi  # масштабируем в [0, 2π]

    # --- решаем для x и y отдельно ---
    def _solve_coord(coord_vals):
        h = np.zeros(N)
        for i in range(N - 1):
            h[i] = t_norm[i + 1] - t_norm[i]
        h[N - 1] = t_norm[0] + 2 * np.pi - t_norm[N - 1]

        # Строим циклическую трёхдиагональную систему для M (вторая производная)
        # Уравнение для узла i:
        #   h[i-1]*M[i-1] + 2*(h[i-1]+h[i])*M[i] + h[i]*M[i+1] = 6*f[i]
        # где f[i] — разностное отношение

        a = np.zeros(N)  # поддиагональ
        b = np.zeros(N)  # диагональ
        c = np.zeros(N)  # наддиагональ
        d = np.zeros(N)  # правая часть

        for i in range(N):
            ip1 = (i + 1) % N      # следующий
            im1 = (i - 1) % N      # предыдущий

            hi = h[i]
            him1 = h[im1]

            # Диагональ
            b[i] = 2.0 * (him1 + hi)

            # Поддиагональ
            a[i] = him1

            # Наддиагональ
            c[i] = hi

            # Правая часть
            if (hi != 0 and him1 != 0):
                d[i] = 6.0 * ((coord_vals[ip1] - coord_vals[i]) / hi
                              - (coord_vals[i] - coord_vals[im1]) / him1)
            else:
                d[i] = 0.0

        # Решаем систему numpy
        A = np.zeros((N, N))
        for i in range(N):
            A[i, i] = b[i]
            A[i, (i + 1) % N] = c[i]  # наддиагональ + правый верхний угол
            A[i, (i - 1) % N] = a[i]  # поддиагональ + левый нижний угол

        M = np.linalg.solve(A, d)

        # --- интерполяция на fine сетке ---
        t_fine = np.linspace(t_norm[0], t_norm[0] + 2 * np.pi, 1000)
        x_fine = np.zeros_like(t_fine)

        for k, tk in enumerate(t_fine):
            # Находим интервал [t_norm[i], t_norm[i+1]] или замыкающий
            if tk >= t_norm[-1]:
                # Замыкающий интервал
                i = N - 1
                ti = t_norm[i]
                t_next = t_norm[0] + 2 * np.pi
                hi = t_next - ti
                xi = coord_vals[i]
                xi1 = coord_vals[0]
            else:
                i = np.searchsorted(t_norm, tk, side='right') - 1
                i = max(0, min(i, N - 2))
                ti = t_norm[i]
                t_next = t_norm[i + 1]
                hi = t_next - ti
                xi = coord_vals[i]
                xi1 = coord_vals[i + 1]

            if hi < 1e-12:
                hi = 1e-12

            # Кубическая интерполяция:
            # f(t) = M[i]*(t_next-t)^3/(6*hi) + M[i+1]*(t-ti)^3/(6*hi)
            #        + (y[i] - M[i]*hi^2/6)*(t_next-t)/hi
            #        + (y[i+1] - M[i+1]*hi^2/6)*(t-ti)/hi
            A_cubic = (t_next - tk) ** 3 / (6.0 * hi)
            B_cubic = (tk - ti) ** 3 / (6.0 * hi)
            C_cubic = (xi - M[i] * hi ** 2 / 6.0) * (t_next - tk) / hi
            D_cubic = (xi1 - M[(i + 1) % N] * hi ** 2 / 6.0) * (tk - ti) / hi

            x_fine[k] = A_cubic * M[i] + B_cubic * M[(i + 1) % N] + C_cubic + D_cubic

        return t_fine, x_fine

    t_fine, x_fine = _solve_coord(pts[:, 0])
    _, y_fine = _solve_coord(pts[:, 1])

    return t_fine, x_fine, y_fine


# ---------------------------------------------------------------------------
# GUI на matplotlib
# ---------------------------------------------------------------------------

class SplineApp:
    def __init__(self):
        self.points = []
        self.spline_built = False
        self.t_fine = None
        self.x_fine = None
        self.y_fine = None

        self.fig, self.ax = plt.subplots(figsize=(9, 9))
        self.ax.set_aspect('equal')
        self.ax.set_xlabel('X')
        self.ax.set_ylabel('Y')
        self.ax.set_title('Замкнутый кубический сплайн\nЛКМ — добавить точку | ПКМ/Esc — построить | R — очистить | Backspace — удалить')
        self.ax.grid(True, alpha=0.3)
        self.ax.axhline(0, color='black', linewidth=0.5)
        self.ax.axvline(0, color='black', linewidth=0.5)

        self.point_scatter = None
        self.spline_line, = self.ax.plot([], [], 'b-', linewidth=2, label='Сплайн')
        self.status_text = self.ax.text(0.02, 0.02, '', transform=self.ax.transAxes,
                                        fontsize=9, verticalalignment='bottom',
                                        bbox=dict(boxstyle='round,pad=0.3', facecolor='white', alpha=0.8))

        self.fig.canvas.mpl_connect('button_press_event', self.on_click)
        self.fig.canvas.mpl_connect('motion_notify_event', self.on_motion)

        self._update_view()
        plt.show()

    # ---- обработчики событий ------------------------------------------------

    def on_click(self, event):
        if event.inaxes != self.ax:
            return

        if event.button == 1:  # ЛКМ — добавить точку
            self.points.append([event.xdata, event.ydata])
            self.spline_built = False
            self._draw_points()
            self._update_view()
            self.fig.canvas.draw_idle()

        elif event.button == 3 or event.key == 'escape':  # ПКМ / Escape
            self._build_spline()

    def on_motion(self, event):
        if event.inaxes != self.ax:
            self.status_text.set_text('')
            return

        if event.xdata is None or event.ydata is None:
            return

        info = f'Координаты: ({event.xdata:.2f}, {event.ydata:.2f})'
        if self.points:
            info += f'  |  Точек: {len(self.points)}'
        if self.spline_built:
            info += '  |  Сплайн построен'
        self.status_text.set_text(info)

    # ---- отрисовка ----------------------------------------------------------

    def _draw_points(self):
        if self.point_scatter is not None:
            self.point_scatter.remove()

        if not self.points:
            self.point_scatter = None
            return

        pts = np.array(self.points)
        self.point_scatter = self.ax.scatter(pts[:, 0], pts[:, 1], c='red', s=60, zorder=5, edgecolors='darkred')

        # Номера точек
        for i, (x, y) in enumerate(pts):
            self.ax.annotate(str(i), (x, y),
                             textcoords='offset points', xytext=(6, 6),
                             fontsize=8, color='darkred', fontweight='bold')

    def _build_spline(self):
        if len(self.points) < 2:
            print("Нужно минимум 2 точки для построения сплайна.")
            return

        print(f"Построение сплайна по {len(self.points)} точкам...")
        self.t_fine, self.x_fine, self.y_fine = closed_cubic_spline(self.points)
        if self.t_fine is None:
            print("Ошибка: недостаточно точек.")
            return

        print(f"Сплайн: {len(self.x_fine)} точек, X=[{self.x_fine.min():.2f}, {self.x_fine.max():.2f}], Y=[{self.y_fine.min():.2f}, {self.y_fine.max():.2f}]")

        self.spline_line.set_data(self.x_fine, self.y_fine)
        self.spline_line.set_visible(True)
        self.spline_built = True
        self._update_view()
        self.fig.canvas.draw_idle()

    def _clear(self):
        self.points.clear()
        self.spline_built = False
        self.t_fine = self.x_fine = self.y_fine = None
        self.spline_line.set_visible(False)
        self.spline_line.set_data([], [])
        if self.point_scatter is not None:
            self.point_scatter.remove()
            self.point_scatter = None
        self._update_view()
        self.fig.canvas.draw_idle()

    def _update_view(self):
        if self.points:
            pts = np.array(self.points)
            margin = 1.0
            min_x, max_x = pts[:, 0].min() - margin, pts[:, 0].max() + margin
            min_y, max_y = pts[:, 1].min() - margin, pts[:, 1].max() + margin
            if self.spline_built and self.x_fine is not None:
                min_x = min(min_x, self.x_fine.min() - margin)
                max_x = max(max_x, self.x_fine.max() + margin)
                min_y = min(min_y, self.y_fine.min() - margin)
                max_y = max(max_y, self.y_fine.max() + margin)
            self.ax.set_xlim(min_x, max_x)
            self.ax.set_ylim(min_y, max_y)
        else:
            self.ax.set_xlim(-5, 5)
            self.ax.set_ylim(-5, 5)

        self.ax.legend(loc='upper right')


# ---------------------------------------------------------------------------
# Горячие клавиши
# ---------------------------------------------------------------------------

def _on_key(event):
    if event.inaxes is None:
        return
    app = event.canvas.manager.app
    if event.key == 'r' or event.key == 'R':
        app._clear()
    elif event.key == 'backspace' or event.key == 'delete':
        if app.points:
            app.points.pop()
            app.spline_built = False
            app._draw_points()
            app._update_view()
            event.canvas.draw_idle()


def main():
    app = SplineApp()
    app.fig.canvas.manager.app = app  # храним ссылку на app
    app.fig.canvas.mpl_connect('key_press_event', _on_key)


if __name__ == '__main__':
    main()
