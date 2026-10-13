import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
from math import cos, sin, radians

class Arm2D:
    def __init__(self):
        self.base_x = 0.0
        self.theta1 = radians(30)   # угол плеча
        self.theta2 = radians(-60)  # угол локтя
        self.theta3 = radians(45)   # угол кисти
        self.L1 = 1.2
        self.L2 = 1.0
        self.L3 = 0.7

    def points(self):
        x0, y0 = self.base_x, 0.0
        x1 = x0 + self.L1 * cos(self.theta1)
        y1 = y0 + self.L1 * sin(self.theta1)
        x2 = x1 + self.L2 * cos(self.theta1 + self.theta2)
        y2 = y1 + self.L2 * sin(self.theta1 + self.theta2)
        x3 = x2 + self.L3 * cos(self.theta1 + self.theta2 + self.theta3)
        y3 = y2 + self.L3 * sin(self.theta1 + self.theta2 + self.theta3)
        return [(x0, y0), (x1, y1), (x2, y2), (x3, y3)]

arm = Arm2D()
keys = set()
step = radians(5)
base_step = 0.05

fig, ax = plt.subplots(figsize=(10, 6))

fig.canvas.mpl_connect('key_press_event',    lambda e: keys.add(e.key.lower()))
fig.canvas.mpl_connect('key_release_event',  lambda e: keys.discard(e.key.lower()))
fig.canvas.mpl_connect('figure_leave_event', lambda e: keys.clear())

p0 = arm.points()
x_pts = [q[0] for q in p0]
y_pts = [q[1] for q in p0]

tube1, = ax.plot(x_pts, y_pts, '-', linewidth=10, color='#3a6ea5', solid_capstyle='round')
tube2, = ax.plot(x_pts, y_pts, '-', linewidth=4,  color='#7fb3e8', solid_capstyle='round')
joints, = ax.plot(x_pts[1:-1], y_pts[1:-1], 'o', markersize=12,
                  color='orange', markeredgecolor='k', markeredgewidth=1.5)
tip, = ax.plot([x_pts[-1]], [y_pts[-1]], 'o', markersize=14,
               color='red', markeredgecolor='k', markeredgewidth=1.5)
base_marker, = ax.plot([arm.base_x], [0], 's', markersize=10,
                       color='green', markeredgecolor='k', markeredgewidth=1.5)

title = ax.set_title("")
hint_text = ax.text(0.02, 0.98,
                    "← → основание\n1 2 плечо\n3 4 локоть\n5 6 кисть\nR сброс",
                    transform=ax.transAxes, va='top', fontsize=9,
                    bbox=dict(boxstyle='round', fc='lightyellow'))

ax.set_xlim(-4, 4)
ax.set_ylim(-0.5, 3)
ax.set_aspect('equal')
ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.grid(True)
ax.axhline(y=0, color='brown', linewidth=2, alpha=0.5)

def update(_):
    if 'left'  in keys: arm.base_x -= base_step
    if 'right' in keys: arm.base_x += base_step
    if '1' in keys: arm.theta1 += step
    if '2' in keys: arm.theta1 -= step
    if '3' in keys: arm.theta2 += step
    if '4' in keys: arm.theta2 -= step
    if '5' in keys: arm.theta3 += step
    if '6' in keys: arm.theta3 -= step
    if 'r' in keys:
        arm.base_x = 0.0
        arm.theta1 = radians(30)
        arm.theta2 = radians(-60)
        arm.theta3 = radians(45)
        keys.discard('r')
    arm.base_x = np.clip(arm.base_x, -3.5, 3.5)
    arm.theta1 = np.clip(arm.theta1, -np.pi/2, np.pi)
    arm.theta2 = np.clip(arm.theta2, -np.pi, 0)
    arm.theta3 = np.clip(arm.theta3, -np.pi, np.pi)

    p = arm.points()
    x = [q[0] for q in p]
    y = [q[1] for q in p]

    tube1.set_data(x, y)
    tube2.set_data(x, y)
    joints.set_data(x[1:-1], y[1:-1])
    tip.set_data([x[-1]], [y[-1]])
    base_marker.set_data([arm.base_x], [0])

    title.set_text(f"X={arm.base_x:+.2f}   "
                   f"θ1={np.degrees(arm.theta1):+.0f}°   "
                   f"θ2={np.degrees(arm.theta2):+.0f}°   "
                   f"θ3={np.degrees(arm.theta3):+.0f}°")

anim = FuncAnimation(fig, update, interval=50, cache_frame_data=False)
plt.show()
