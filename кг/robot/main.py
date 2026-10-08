import numpy as np
import matplotlib as mpl
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
from math import cos, sin

def rot_z(t):
    return np.array([[cos(t), -sin(t), 0, 0],
                     [sin(t),  cos(t), 0, 0],
                     [0, 0, 1, 0],
                     [0, 0, 0, 1]])

def rot_y(t):
    return np.array([[cos(t), 0, sin(t), 0],
                     [0, 1, 0, 0],
                     [-sin(t), 0, cos(t), 0],
                     [0, 0, 0, 1]])

def shift(d):
    return np.array([[1, 0, 0, d],
                     [0, 1, 0, 0],
                     [0, 0, 1, 0],
                     [0, 0, 0, 1]])

class Arm:
    def __init__(self):
        self.a, self.b, self.c = 0.0, 0.4, -0.6
        self.L1, self.L2, self.L3 = 1.2, 1.0, 0.6

    def points(self):
        T = rot_z(self.a) @ shift(0.3)
        p = [np.array([0, 0, 0]), T[:3, 3].copy()]
        T = T @ rot_y(self.b) @ shift(self.L1); p.append(T[:3, 3].copy())
        T = T @ rot_y(self.c) @ shift(self.L2); p.append(T[:3, 3].copy())
        T = T @ shift(self.L3);                 p.append(T[:3, 3].copy())
        return p

arm = Arm()
keys = set()
step = np.radians(5)

fig = plt.figure(figsize=(9, 6))
ax = fig.add_subplot(111, projection='3d')

fig.canvas.mpl_connect('key_press_event',    lambda e: keys.add(e.key))
fig.canvas.mpl_connect('key_release_event',  lambda e: keys.discard(e.key))
fig.canvas.mpl_connect('figure_leave_event', lambda e: keys.clear())

p0 = arm.points()
x0 = [q[0] for q in p0]; y0 = [q[1] for q in p0]; z0 = [q[2] for q in p0]

tube1, = ax.plot(x0, y0, z0, '-', linewidth=10, color='#3a6ea5', solid_capstyle='round')
tube2, = ax.plot(x0, y0, z0, '-', linewidth=4,  color='#7fb3e8', solid_capstyle='round')
joints = ax.scatter(x0[1:-1], y0[1:-1], z0[1:-1], s=150, color='orange', edgecolor='k')
tip    = ax.scatter([x0[-1]], [y0[-1]], [z0[-1]], s=200, color='red', edgecolor='k')
title  = ax.set_title("")

ax.set_xlim(-2.5, 2.5); ax.set_ylim(-2.5, 2.5); ax.set_zlim(0, 3)
ax.set_xlabel('X'); ax.set_ylabel('Y'); ax.set_zlabel('Z')
ax.grid(True)
ax.text2D(0.02, 0.98,
          "← → основание\n↑ ↓ плечо\nA D локоть\nR сброс",
          transform=ax.transAxes, va='top', fontsize=9,
          bbox=dict(boxstyle='round', fc='lightyellow'))

def update(_):
    if 'left'  in keys: arm.a -= step
    if 'right' in keys: arm.a += step
    if 'up'    in keys: arm.b += step
    if 'down'  in keys: arm.b -= step
    if 'a' in keys or 'ф' in keys: arm.c -= step
    if 'd' in keys or 'в' in keys: arm.c += step
    if 'r' in keys or 'к' in keys:
        arm.a, arm.b, arm.c = 0.0, 0.4, -0.6
        keys.discard('r'); keys.discard('к')

    arm.a = np.clip(arm.a, -np.pi, np.pi)
    arm.b = np.clip(arm.b, -np.pi/2, np.pi)
    arm.c = np.clip(arm.c, -np.pi, 0.2)

    p = arm.points()
    x = [q[0] for q in p]; y = [q[1] for q in p]; z = [q[2] for q in p]

    tube1.set_data(x, y);  tube1.set_3d_properties(z)
    tube2.set_data(x, y);  tube2.set_3d_properties(z)
    joints._offsets3d = (x[1:-1], y[1:-1], z[1:-1])
    tip._offsets3d    = ([x[-1]], [y[-1]], [z[-1]])
    title.set_text(f"θ1={np.degrees(arm.a):+.0f}°   "
                   f"θ2={np.degrees(arm.b):+.0f}°   "
                   f"θ3={np.degrees(arm.c):+.0f}°")

anim = FuncAnimation(fig, update, interval=50, cache_frame_data=False)
plt.show()