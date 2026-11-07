import tkinter as tk
from tkinter import ttk
import math
import random

class FractalTreeApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Фрактальное дерево")
        self.root.geometry("900x750")
        
        # Параметры
        self.params = {
            'min_height': tk.DoubleVar(value=0.6),
            'max_height': tk.DoubleVar(value=0.8),
            'min_angle': tk.DoubleVar(value=15),
            'max_angle': tk.DoubleVar(value=35),
            'rotation': tk.DoubleVar(value=0),  # глобальный поворот в градусах
            'initial_length': tk.DoubleVar(value=120),
            'depth': tk.IntVar(value=9),
            'thickness': tk.DoubleVar(value=10),
            'random_rotation': tk.DoubleVar(value=10)  # случайный поворот ветвей
        }
        
        self.setup_ui()
        self.draw_tree()
    
    def setup_ui(self):
        # Панель управления слева
        control_frame = ttk.Frame(self.root, padding=10)
        control_frame.pack(side=tk.LEFT, fill=tk.Y)
        
        ttk.Label(control_frame, text="Настройки дерева", 
                  font=('Arial', 12, 'bold')).pack(pady=5)
        
        # Диапазон высоты ветвления
        frame_h = ttk.LabelFrame(control_frame, text="Высота ветвления (коэф.)", padding=5)
        frame_h.pack(fill=tk.X, pady=3)
        self._add_slider(frame_h, "Мин:", self.params['min_height'], 0.3, 0.95)
        self._add_slider(frame_h, "Макс:", self.params['max_height'], 0.3, 0.95)
        
        # Диапазон угла ветвления
        frame_a = ttk.LabelFrame(control_frame, text="Угол ветвления (°)", padding=5)
        frame_a.pack(fill=tk.X, pady=3)
        self._add_slider(frame_a, "Мин:", self.params['min_angle'], 0, 90)
        self._add_slider(frame_a, "Макс:", self.params['max_angle'], 0, 90)
        
        # Поворот и случайный поворот
        frame_r = ttk.LabelFrame(control_frame, text="Поворот", padding=5)
        frame_r.pack(fill=tk.X, pady=3)
        self._add_slider(frame_r, "Общий поворот (°):", self.params['rotation'], -180, 180)
        self._add_slider(frame_r, "Случ. отклонение (°):", self.params['random_rotation'], 0, 45)
        
        # Прочие параметры
        frame_o = ttk.LabelFrame(control_frame, text="Прочее", padding=5)
        frame_o.pack(fill=tk.X, pady=3)
        self._add_slider(frame_o, "Нач. длина:", self.params['initial_length'], 30, 250)
        self._add_slider(frame_o, "Толщина:", self.params['thickness'], 1, 25)
        self._add_slider(frame_o, "Глубина:", self.params['depth'], 1, 13)
        
        # Кнопки
        btn_frame = ttk.Frame(control_frame)
        btn_frame.pack(fill=tk.X, pady=10)
        ttk.Button(btn_frame, text="Перерисовать", 
                   command=self.draw_tree).pack(fill=tk.X, pady=2)
        ttk.Button(btn_frame, text="Новый случай", 
                   command=self.randomize).pack(fill=tk.X, pady=2)
        
        # Canvas справа
        self.canvas = tk.Canvas(self.root, bg='white', width=600, height=700)
        self.canvas.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True)
    
    def _add_slider(self, parent, label, var, from_, to):
        frame = ttk.Frame(parent)
        frame.pack(fill=tk.X, pady=2)
        ttk.Label(frame, text=label, width=14).pack(side=tk.LEFT)
        scale = ttk.Scale(frame, from_=from_, to=to, variable=var, 
                          orient=tk.HORIZONTAL, command=lambda v: self.draw_tree())
        scale.pack(side=tk.LEFT, fill=tk.X, expand=True)
        val_label = ttk.Label(frame, textvariable=var, width=6)
        val_label.pack(side=tk.LEFT)
        # Обновление метки значения
        var.trace_add('write', lambda *a: self._update_label(var))
    
    def _update_label(self, var):
        # Обновить отображение (tkinter сам это делает через textvariable)
        pass
    
    def draw_tree(self):
        self.canvas.delete("all")
        
        # Проверка корректности диапазонов
        h_min = self.params['min_height'].get()
        h_max = self.params['max_height'].get()
        a_min = self.params['min_angle'].get()
        a_max = self.params['max_angle'].get()
        
        if h_min > h_max:
            h_min, h_max = h_max, h_min
        if a_min > a_max:
            a_min, a_max = a_max, a_min
        
        # Начальная точка (у основания дерева)
        cx = 300
        cy = 680
        
        # Начальный угол: -90° (вверх) + общий поворот
        rotation = self.params['rotation'].get()
        start_angle = -90 + rotation
        
        initial_len = self.params['initial_length'].get()
        depth = self.params['depth'].get()
        thickness = self.params['thickness'].get()
        rand_rot = self.params['random_rotation'].get()
        
        self._draw_branch(cx, cy, initial_len, start_angle, depth, thickness,
                          h_min, h_max, a_min, a_max, rand_rot)
    
    def _draw_branch(self, x, y, length, angle, depth, thickness,
                     h_min, h_max, a_min, a_max, rand_rot):
        if depth <= 0 or length < 2:
            return
        
        # Конечная точка ветви
        rad = math.radians(angle)
        x2 = x + length * math.cos(rad)
        y2 = y + length * math.sin(rad)
        
        # Цвет зависит от толщины (ствол - коричневый, ветки - зелёные)
        if thickness > 4:
            color = "#5a3a22"
        elif thickness > 2:
            color = "#4a6b2a"
        else:
            color = "#2d5a1f"
        
        # Рисуем линию
        self.canvas.create_line(x, y, x2, y2, fill=color, 
                                width=max(1, thickness), capstyle=tk.ROUND)
        
        # Листья на кончиках
        if depth <= 2:
            r = random.uniform(3, 6)
            self.canvas.create_oval(x2-r, y2-r, x2+r, y2+r, 
                                    fill="#3a8f2a", outline="")
        
        # Рекурсия: две ветви
        for direction in (-1, 1):
            branch_angle = random.uniform(a_min, a_max)
            new_length = length * random.uniform(h_min, h_max)
            new_thickness = thickness * 0.7
            # Случайное отклонение
            rand_offset = random.uniform(-rand_rot, rand_rot)
            new_angle = angle + direction * branch_angle + rand_offset
            self._draw_branch(x2, y2, new_length, new_angle, depth - 1,
                              new_thickness, h_min, h_max, a_min, a_max, rand_rot)
    
    def randomize(self):
        self.params['min_height'].set(round(random.uniform(0.5, 0.75), 2))
        self.params['max_height'].set(round(random.uniform(0.75, 0.9), 2))
        self.params['min_angle'].set(round(random.uniform(10, 30), 1))
        self.params['max_angle'].set(round(random.uniform(30, 55), 1))
        self.params['rotation'].set(round(random.uniform(-30, 30), 1))
        self.params['random_rotation'].set(round(random.uniform(0, 20), 1))
        self.params['initial_length'].set(round(random.uniform(80, 160), 1))
        self.params['thickness'].set(round(random.uniform(6, 14), 1))
        self.params['depth'].set(random.randint(7, 11))
        self.draw_tree()


if __name__ == "__main__":
    root = tk.Tk()
    app = FractalTreeApp(root)
    root.mainloop()