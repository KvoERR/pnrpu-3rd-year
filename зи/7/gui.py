import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from main import AppError, check_password, create_hash, load_hash, save_hash
from password_hash import DEFAULT_ITERATIONS, MAX_PASSWORD_LENGTH, PasswordError


class HashApp:
    def __init__(self, root: tk.Tk) -> None:
        self.root = root
        self.root.title("Хеширование паролей (DES-32)")
        self.root.geometry("760x560")
        self.root.resizable(True, True)

        notebook = ttk.Notebook(root)
        notebook.grid(row=0, column=0, padx=10, pady=10, sticky="nsew")
        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

        self._build_hash_tab(notebook)
        self._build_verify_tab(notebook)

    def _build_hash_tab(self, notebook: ttk.Notebook) -> None:
        frame = ttk.Frame(notebook)
        notebook.add(frame, text="Хеширование")

        ttk.Label(frame, text="Пароль:").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.hash_password_entry = tk.Entry(frame, width=40, show="*")
        self.hash_password_entry.grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Показать", command=lambda: self._toggle_show(self.hash_password_entry)).grid(row=0, column=2, padx=5)

        ttk.Label(frame, text=f"Максимум {MAX_PASSWORD_LENGTH} символов", foreground="gray").grid(row=1, column=1, sticky="w", padx=5)

        ttk.Label(frame, text="Итераций:").grid(row=2, column=0, sticky="w", padx=5, pady=5)
        self.hash_iterations_entry = tk.Entry(frame, width=10)
        self.hash_iterations_entry.grid(row=2, column=1, sticky="w", padx=5, pady=5)
        self.hash_iterations_entry.insert(0, str(DEFAULT_ITERATIONS))

        ttk.Button(frame, text="Вычислить хеш", command=self._run_hash).grid(row=3, column=1, pady=10)

        ttk.Label(frame, text="Хеш (hex):").grid(row=4, column=0, sticky="w", padx=5, pady=5)
        self.hash_result_entry = tk.Entry(frame, width=40)
        self.hash_result_entry.grid(row=4, column=1, padx=5, pady=5)

        ttk.Button(frame, text="Сохранить хеш в файл", command=self._save_hash).grid(row=5, column=1, pady=5)

        self.hash_status = ttk.Label(frame, text="", foreground="gray", wraplength=700)
        self.hash_status.grid(row=6, column=0, columnspan=3, padx=5, pady=5, sticky="w")

    def _build_verify_tab(self, notebook: ttk.Notebook) -> None:
        frame = ttk.Frame(notebook)
        notebook.add(frame, text="Проверка")

        ttk.Label(frame, text="Пароль:").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.verify_password_entry = tk.Entry(frame, width=40, show="*")
        self.verify_password_entry.grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Показать", command=lambda: self._toggle_show(self.verify_password_entry)).grid(row=0, column=2, padx=5)

        ttk.Label(frame, text="Хеш (hex):").grid(row=1, column=0, sticky="w", padx=5, pady=5)
        self.verify_hash_entry = tk.Entry(frame, width=40)
        self.verify_hash_entry.grid(row=1, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Загрузить из файла", command=self._load_hash).grid(row=1, column=2, padx=5)

        ttk.Label(frame, text="Итераций:").grid(row=2, column=0, sticky="w", padx=5, pady=5)
        self.verify_iterations_entry = tk.Entry(frame, width=10)
        self.verify_iterations_entry.grid(row=2, column=1, sticky="w", padx=5, pady=5)
        self.verify_iterations_entry.insert(0, str(DEFAULT_ITERATIONS))

        ttk.Button(frame, text="Проверить", command=self._run_verify).grid(row=3, column=1, pady=10)

        self.verify_status = ttk.Label(frame, text="", foreground="gray", wraplength=700)
        self.verify_status.grid(row=4, column=0, columnspan=3, padx=5, pady=5, sticky="w")

    def _toggle_show(self, entry: tk.Entry) -> None:
        entry.config(show="" if entry.cget("show") else "*")

    def _read_iterations(self, entry: tk.Entry):
        try:
            value = int(entry.get().strip())
        except ValueError:
            messagebox.showerror("Ошибка", "Итераций должно быть целое число")
            return None
        if value < 1:
            messagebox.showerror("Ошибка", "Итераций должно быть ≥ 1")
            return None
        return value

    def _run_hash(self) -> None:
        password = self.hash_password_entry.get()
        if not password:
            messagebox.showerror("Ошибка", "Введите пароль")
            return
        iterations = self._read_iterations(self.hash_iterations_entry)
        if iterations is None:
            return

        try:
            stored = create_hash(password, iterations=iterations)
            self.hash_result_entry.delete(0, "end")
            self.hash_result_entry.insert(0, stored)
            self.hash_status.config(text=f"Хеш вычислен ({len(stored) // 2} байт)", foreground="green")
        except (PasswordError, AppError) as error:
            messagebox.showerror("Ошибка", str(error))
            self.hash_status.config(text="Ошибка", foreground="red")

    def _run_verify(self) -> None:
        password = self.verify_password_entry.get()
        stored_hex = self.verify_hash_entry.get().strip()
        if not password or not stored_hex:
            messagebox.showerror("Ошибка", "Введите пароль и хеш")
            return
        iterations = self._read_iterations(self.verify_iterations_entry)
        if iterations is None:
            return

        try:
            ok = check_password(password, stored_hex, iterations=iterations)
            if ok:
                self.verify_status.config(text="Пароль верный", foreground="green")
            else:
                self.verify_status.config(text="Пароль неверный", foreground="red")
        except (PasswordError, AppError) as error:
            messagebox.showerror("Ошибка", str(error))
            self.verify_status.config(text="Ошибка", foreground="red")

    def _save_hash(self) -> None:
        stored = self.hash_result_entry.get().strip()
        if not stored:
            messagebox.showerror("Ошибка", "Сначала вычислите хеш")
            return
        path = filedialog.asksaveasfilename(defaultextension=".hash")
        if not path:
            return
        try:
            save_hash(path, stored)
            self.hash_status.config(text=f"Сохранено: {path}", foreground="green")
        except OSError as error:
            messagebox.showerror("Ошибка", str(error))

    def _load_hash(self) -> None:
        path = filedialog.askopenfilename(filetypes=[("Hash files", "*.hash"), ("All files", "*.*")])
        if not path:
            return
        try:
            stored = load_hash(path)
            self.verify_hash_entry.delete(0, "end")
            self.verify_hash_entry.insert(0, stored)
            self.verify_status.config(text=f"Загружено: {path}", foreground="green")
        except OSError as error:
            messagebox.showerror("Ошибка", str(error))


if __name__ == "__main__":
    root = tk.Tk()
    HashApp(root)
    root.mainloop()