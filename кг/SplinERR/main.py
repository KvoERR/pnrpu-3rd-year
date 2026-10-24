"""
Замкнутый кубический B-сплайн — интерактивное приложение.

ЛКМ            — добавить точку
ПКМ            — удалить ближайшую точку
1              — построить / перестроить сплайн
R              — очистить всё
Backspace/Del  — удалить последнюю точку
"""

import numpy as np
import matplotlib.pyplot as plt


# ---------------------------------------------------------------------------
# Замкнутый (периодический) кубический B-сплайн
# ---------------------------------------------------------------------------

def closed_cubic_bspline(points, samples_per_segment=60):
    """
    Строит замкнутый однородный кубический B-сплайн по контрольным точкам.

    Возвращает (x_fine, y_fine) — массивы точек кривой.

    Используется периодический базис: контрольные точки «зациклены»,
    поэтому кривая гладко замкнута (C^2).
    """
    P = np.asarray(points, dtype=float)
    n = len(P)
    if n < 3:
        return None, None

    # Замыкаем контрольные точки: для сегмента i нужны P[i-1], P[i], P[i+1], P[i+2]
    # (индексы по модулю n).
    t = np.linspace(0.0, 1.0, samples_per_segment, endpoint=False)

    # Однородные кубические B-сплайн базисные функции на [0,1):
    # B0(t) = (1-t)^3 / 6
    # B1(t) = (3t^3 - 6t^2 + 4) / 6
    # B2(t) = (-3t^3 + 3t^2 + 3t + 1) / 6
    # B3(t) = t^3 / 6
    t2 = t * t
    t3 = t2 * t
    B0 = (1 - 3 * t + 3 * t2 - t3) / 6.0
    B1 = (4 - 6 * t2 + 3 * t3) / 6.0
    B2 = (1 + 3 * t + 3 * t2 - 3 * t3) / 6.0
    B3 = t3 / 6.0

    xs = []
    ys = []
    for i in range(n):
        p0 = P[(i - 1) % n]
        p1 = P[i]
        p2 = P[(i + 1) % n]
        p3 = P[(i + 2) % n]
        x = B0 * p0[0] + B1 * p1[0] + B2 * p2[0] + B3 * p3[0]
        y = B0 * p0[1] + B1 * p1[1] + B2 * p2[1] + B3 * p3[1]
        xs.append(x)
        ys.append(y)

    x_fine = np.concatenate(xs + [xs[0][:1]])
    y_fine = np.concatenate(ys + [ys[0][:1]])
    return x_fine, y_fine


# ---------------------------------------------------------------------------
# GUI на matplotlib
# ---------------------------------------------------------------------------

class SplineApp:
    def __init__(self):
        self.points = []          # список [x, y]
        self.spline_built = False
        self.x_fine = None
        self.y_fine = None

        self.fig, self.ax = plt.subplots(figsize=(9, 9))
        self.ax.set_aspect('equal')
        self.ax.set_xlabel('X')
        self.ax.set_ylabel('Y')
        self.ax.set_title(
            'Замкнутый кубический B-сплайн\n'
            'ЛКМ — добавить | ПКМ — удалить точку | 1 — построить | R — очистить'
        )
        self.ax.grid(True, alpha=0.3)
        self.ax.axhline(0, color='black', linewidth=0.5)
        self.ax.axvline(0, color='black', linewidth=0.5)

        self.point_scatter = None
        self.point_annotations = []
        self.spline_line, = self.ax.plot([], [], 'b-', linewidth=2, label='B-сплайн')
        self.status_text = self.ax.text(
            0.02, 0.02, '', transform=self.ax.transAxes,
            fontsize=9, verticalalignment='bottom',
            bbox=dict(boxstyle='round,pad=0.3', facecolor='white', alpha=0.8)
        )

        self.fig.canvas.mpl_connect('button_press_event', self.on_click)
        self.fig.canvas.mpl_connect('motion_notify_event', self.on_motion)
        self.fig.canvas.mpl_connect('key_press_event', self.on_key)

        self._update_view()
        plt.show()

    # ---- обработчики событий ------------------------------------------------

    def on_click(self, event):
        if event.inaxes != self.ax:
            return
        if event.xdata is None or event.ydata is None:
            return

        if event.button == 1:                      # ЛКМ — добавить
            self.points.append([event.xdata, event.ydata])
            self.spline_built = False
            self._redraw_points()
            self._update_view()
            self.fig.canvas.draw_idle()

        elif event.button == 3:                    # ПКМ — удалить ближайшую
            self._remove_nearest(event.xdata, event.ydata)

    def on_motion(self, event):
        if event.inaxes != self.ax or event.xdata is None:
            self.status_text.set_text('')
            self.fig.canvas.draw_idle()
            return
        info = f'Координаты: ({event.xdata:.2f}, {event.ydata:.2f})'
        if self.points:
            info += f'  |  Точек: {len(self.points)}'
        if self.spline_built:
            info += '  |  Сплайн построен'
        self.status_text.set_text(info)
        self.fig.canvas.draw_idle()

    def on_key(self, event):
        if event.key in ('r', 'R'):
            self._clear()
        elif event.key in ('backspace', 'delete'):
            if self.points:
                self.points.pop()
                self.spline_built = False
                self._redraw_points()
                self._update_view()
                self.fig.canvas.draw_idle()
        elif event.key == '1':
            self._build_spline()

    # ---- удаление ближайшей точки ------------------------------------------

    def _remove_nearest(self, x, y):
        if not self.points:
            return
        pts = np.array(self.points)
        d2 = (pts[:, 0] - x) ** 2 + (pts[:, 1] - y) ** 2
        idx = int(np.argmin(d2))
        # «Радиус захвата»: 10% от текущего размаха осей — чтобы не удалять
        # точку, если кликнули далеко от неё.
        xlim = self.ax.get_xlim()
        ylim = self.ax.get_ylim()
        grab = 0.05 * max(xlim[1] - xlim[0], ylim[1] - ylim[0])
        if d2[idx] <= grab ** 2:
            self.points.pop(idx)
            self.spline_built = False
            self._redraw_points()
            self._update_view()
            self.fig.canvas.draw_idle()

    # ---- отрисовка ----------------------------------------------------------

    def _redraw_points(self):
        if self.point_scatter is not None:
            self.point_scatter.remove()
            self.point_scatter = None
        for ann in self.point_annotations:
            ann.remove()
        self.point_annotations.clear()

        if not self.points:
            return

        pts = np.array(self.points)
        self.point_scatter = self.ax.scatter(
            pts[:, 0], pts[:, 1], c='red', s=60, zorder=5,
            edgecolors='darkred', label='Контрольные точки'
        )
        for i, (x, y) in enumerate(pts):
            ann = self.ax.annotate(
                str(i), (x, y), textcoords='offset points', xytext=(6, 6),
                fontsize=8, color='darkred', fontweight='bold'
            )
            self.point_annotations.append(ann)

    def _build_spline(self):
        if len(self.points) < 3:
            print("Нужно минимум 3 точки для замкнутого B-сплайна.")
            return

        print(f"Построение B-сплайна по {len(self.points)} точкам...")
        self.x_fine, self.y_fine = closed_cubic_bspline(self.points)
        if self.x_fine is None:
            print("Ошибка: недостаточно точек.")
            return

        self.spline_line.set_data(self.x_fine, self.y_fine)
        self.spline_line.set_visible(True)
        self.spline_built = True
        self._update_view()
        self.fig.canvas.draw_idle()

    def _clear(self):
        self.points.clear()
        self.spline_built = False
        self.x_fine = self.y_fine = None
        self.spline_line.set_visible(False)
        self.spline_line.set_data([], [])
        self._redraw_points()
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

def main():
    SplineApp()


if __name__ == '__main__':
    main()