import tkinter as tk
from tkinter import ttk
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from matplotlib.figure import Figure
import matplotlib.tri as mtri
import numpy as np
import random


class FractalLandscapeApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Фрактальный Ландшафт (Горы)")
        self.root.geometry("1000x700")

        self.iterations = tk.IntVar(value=4)
        self.roughness = tk.DoubleVar(value=0.5)

        self.fig = Figure(figsize=(8, 6), dpi=100)
        self.ax = self.fig.add_subplot(111, projection="3d")

        self.canvas = FigureCanvasTkAgg(self.fig, master=root)
        self.canvas.get_tk_widget().pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        control = ttk.Frame(root, padding=10)
        control.pack(side=tk.RIGHT, fill=tk.Y)

        ttk.Label(control, text="Итерации (Детализация):").pack(pady=(0, 2))
        ttk.Spinbox(control, from_=0, to=7, increment=1,
                    textvariable=self.iterations, width=8).pack()

        ttk.Label(control, text="Шероховатость:").pack(pady=(10, 2))
        ttk.Spinbox(control, from_=0.1, to=1.0, increment=0.1,
                    textvariable=self.roughness, width=8, format="%.1f").pack()

        ttk.Button(control, text="Сгенерировать",
                   command=self.update).pack(pady=20)

        # Начальный равносторонний треугольник
        self.p1 = np.array([0.0, 0.0, 0.0])
        self.p2 = np.array([1.0, 0.0, 0.0])
        self.p3 = np.array([0.5, 0.866, 0.0])

        self.update()

    def generate_mesh(self, depth, roughness):
        vertices = []
        triangles = []
        vertex_map = {}
        displacement_cache = {}

        def get_idx(v):
            key = (round(v[0], 5), round(v[1], 5), round(v[2], 5))
            if key not in vertex_map:
                vertex_map[key] = len(vertices)
                vertices.append(v)
            return vertex_map[key]

        def get_displaced_midpoint(a, b, d):
            # Ключ по XY без учёта порядка, чтобы обе стороны стыка
            # получили одно и то же смещение Z
            key = tuple(sorted([
                (round(a[0], 5), round(a[1], 5)),
                (round(b[0], 5), round(b[1], 5))
            ]))
            if key not in displacement_cache:
                m = (a + b) / 2.0
                disp = roughness ** d
                m[2] += random.uniform(-disp, disp)
                displacement_cache[key] = m
            return displacement_cache[key].copy()

        def subdivide(p1, p2, p3, d):
            if d == 0:
                i1 = get_idx(p1)
                i2 = get_idx(p2)
                i3 = get_idx(p3)
                triangles.append([i1, i2, i3])
                return

            m12 = get_displaced_midpoint(p1, p2, d)
            m23 = get_displaced_midpoint(p2, p3, d)
            m31 = get_displaced_midpoint(p3, p1, d)

            subdivide(p1, m12, m31, d - 1)
            subdivide(m12, p2, m23, d - 1)
            subdivide(m31, m23, p3, d - 1)
            subdivide(m12, m23, m31, d - 1)

        random.seed(42)
        subdivide(self.p1, self.p2, self.p3, depth)
        return np.array(vertices), np.array(triangles)

    def update(self):
        self.ax.clear()

        depth = self.iterations.get()
        rough = self.roughness.get()

        vertices, triangles = self.generate_mesh(depth, rough)

        x = vertices[:, 0]
        y = vertices[:, 1]
        z = vertices[:, 2]

        triang = mtri.Triangulation(x, y, triangles)

        # Обводка того же цвета перекрывает возможные микро-щели
        self.ax.plot_trisurf(
            triang, z,
            color='cyan',
            alpha=0.9,
            shade=True,
            edgecolor='cyan',
            linewidth=0.5,
            antialiased=True,
        )

        self.ax.set_title(f"Фрактальный Ландшафт (Итерация: {depth})")
        self.ax.set_zlim(-1, 1)
        self.ax.set_box_aspect((1, 0.86, 0.5))

        self.canvas.draw()


def main():
    root = tk.Tk()
    app = FractalLandscapeApp(root)
    root.mainloop()


if __name__ == "__main__":
    main()