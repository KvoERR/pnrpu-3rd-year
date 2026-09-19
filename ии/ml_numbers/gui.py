import tkinter as tk
from tkinter import ttk, filedialog
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from PIL import Image, ImageTk
import torch
import torch.nn.functional as F
import json
from pathlib import Path
from main import SimpleNet, load_custom_dataset


class DigitGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("Распознавание цифр")
        self.root.geometry("1000x750")

        self.notebook = ttk.Notebook(root)
        self.notebook.pack(fill="both", expand=True)

        self.tab_dataset = ttk.Frame(self.notebook)
        self.tab_predict = ttk.Frame(self.notebook)

        self.notebook.add(self.tab_dataset, text="Датасет")
        self.notebook.add(self.tab_predict, text="Проверка числа")

        self.setup_tab_dataset()
        self.setup_tab_predict()
        self.model, self.device = self._load_model()

    # ── модель ──────────────────────────────────────────────

    def _load_model(self):
        device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        model = SimpleNet().to(device)
        model.load_state_dict(torch.load("model.pth", weights_only=True))
        model.eval()
        return model, device

    def _get_stats(self):
        with open("dataset_stats.json") as f:
            stats = json.load(f)
        return stats["mean"], stats["std"]

    # ── вкладка 1: датасет ──────────────────────────────────

    def setup_tab_dataset(self):
        btn = tk.Button(
            self.tab_dataset, text="Загрузить датасет", command=self.load_dataset
        )
        btn.pack(pady=10)

        self.dataset_frame = tk.Frame(self.tab_dataset)
        self.dataset_frame.pack(fill="both", expand=True, padx=10, pady=10)

    def load_dataset(self):
        images, labels = load_custom_dataset("./data/custom")

        # Очистка
        for widget in self.dataset_frame.winfo_children():
            widget.destroy()

        # Сетка изображений
        n = min(len(images), 50)
        rows = (n + 9) // 10
        fig, axes = plt.subplots(rows, 10, figsize=(15, rows * 1.5))
        if rows == 1:
            axes = axes.reshape(1, -1)

        for i, (img, label) in enumerate(zip(images[:n], labels[:n])):
            r, c = divmod(i, 10)
            axes[r, c].imshow(img, cmap="gray")
            axes[r, c].set_title(f"{label}", fontsize=8)
            axes[r, c].axis("off")

        for i in range(n, rows * 10):
            r, c = divmod(i, 10)
            axes[r, c].axis("off")

        plt.tight_layout()
        canvas = FigureCanvasTkAgg(fig, master=self.dataset_frame)
        canvas.draw()
        canvas.get_tk_widget().pack(fill="both", expand=True)

        # Гистограмма
        fig2, ax = plt.subplots(figsize=(8, 3))
        ax.hist(labels, bins=range(-0.5, 9.5), align="left", rwidth=0.8, color="steelblue")
        ax.set_xlabel("Цифра")
        ax.set_ylabel("Количество")
        ax.set_xticks(range(10))
        plt.tight_layout()

        canvas2 = FigureCanvasTkAgg(fig2, master=self.dataset_frame)
        canvas2.draw()
        canvas2.get_tk_widget().pack(fill="x", pady=10)

    # ── вкладка 2: проверка числа ───────────────────────────

    def setup_tab_predict(self):
        btn = tk.Button(self.tab_predict, text="Выбрать файл", command=self.select_image)
        btn.pack(pady=10)

        self.path_label = tk.Label(self.tab_predict, text="Файл не выбран", wraplength=400)
        self.path_label.pack()

        self.image_canvas = tk.Canvas(self.tab_predict, width=200, height=200, bg="white")
        self.image_canvas.pack(pady=10)

        btn_pred = tk.Button(self.tab_predict, text="Распознать", command=self.predict_image)
        btn_pred.pack(pady=5)

        self.result_label = tk.Label(self.tab_predict, text="", font=("Arial", 20, "bold"))
        self.result_label.pack(pady=10)

        self.probs_label = tk.Label(self.tab_predict, text="", wraplength=400, justify="left")
        self.probs_label.pack()

        # Кнопка очистки холста
        btn_clear = tk.Button(
            self.tab_predict, text="Очистить холст", command=self.clear_canvas
        )
        btn_clear.pack(pady=5)

    def select_image(self):
        filepath = filedialog.askopenfilename(
            title="Выберите изображение",
            filetypes=[("PNG files", "*.png"), ("JPG files", "*.jpg"), ("All files", "*.*")],
        )
        if filepath:
            self.selected_path = filepath
            self.path_label.config(text=filepath)

            img = Image.open(filepath).convert("L").resize((200, 200))
            imgtk = ImageTk.PhotoImage(img)
            self.image_canvas.create_image(0, 0, anchor="nw", image=imgtk)
            self.image_canvas.image = imgtk

            self.result_label.config(text="")
            self.probs_label.config(text="")

    def clear_canvas(self):
        self.image_canvas.delete("all")
        self.path_label.config(text="Файл не выбран")
        self.result_label.config(text="")
        self.probs_label.config(text="")
        if hasattr(self, "selected_path"):
            del self.selected_path

    def predict_image(self):
        if not hasattr(self, "selected_path"):
            self.result_label.config(text="Сначала выберите файл")
            return

        model, device = self.model
        mean, std = self._get_stats()

        img = Image.open(self.selected_path).convert("L").resize((64, 64))
        to_tensor = torch.vmap(torch.flatten) if hasattr(torch, "vmap") else None

        tensor = torch.from_numpy(img.convert("L").numpy()).float() / 255.0
        tensor = tensor.unsqueeze(0).unsqueeze(0)  # (1, 1, 64, 64)
        tensor = (tensor - mean) / std
        tensor = tensor.to(device)

        with torch.no_grad():
            output = model(tensor)
            probs = F.softmax(output, dim=1)
            pred = probs.argmax(1).item()
            probs_list = probs[0].cpu().numpy()

        self.result_label.config(text=f"Распознано: {pred}")

        top5 = sorted(enumerate(probs_list), key=lambda x: -x[1])[:5]
        probs_text = "   ".join(f"{d}: {p:.1%}" for d, p in top5)
        self.probs_label.config(text=f"Топ-5: {probs_text}")


if __name__ == "__main__":
    root = tk.Tk()
    app = DigitGUI(root)
    root.mainloop()
