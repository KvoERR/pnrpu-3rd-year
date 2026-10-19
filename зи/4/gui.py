import tkinter as tk
from tkinter import ttk, messagebox, filedialog

from main import (
    CipherError,
    encrypt_des32,
    decrypt_des32,
    encrypt_file,
    decrypt_file,
)


class DesApp:
    def __init__(self, root):
        self.root = root
        self.root.title("DES-32 Шифратор файлов")
        self.root.geometry("750x620")
        self.root.resizable(True, True)

        notebook = ttk.Notebook(root)
        notebook.grid(row=0, column=0, padx=10, pady=10, sticky="nsew")

        # ---------- Вкладка шифрования ----------
        enc_frame = ttk.Frame(notebook)
        notebook.add(enc_frame, text="Шифрование")

        ttk.Label(enc_frame, text="Входной файл:").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.enc_input = tk.Entry(enc_frame, width=60)
        self.enc_input.grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(enc_frame, text="Обзор", command=lambda: self._browse(self.enc_input)).grid(row=0, column=2, padx=5)

        ttk.Label(enc_frame, text="Выходной файл:").grid(row=1, column=0, sticky="w", padx=5, pady=5)
        self.enc_output = tk.Entry(enc_frame, width=60)
        self.enc_output.grid(row=1, column=1, padx=5, pady=5)
        ttk.Button(enc_frame, text="Обзор", command=lambda: self._browse_save(self.enc_output)).grid(row=1, column=2, padx=5)

        ttk.Label(enc_frame, text="Ключ (32 бита, целое число):").grid(row=2, column=0, sticky="w", padx=5, pady=5)
        self.enc_key = tk.Entry(enc_frame, width=20)
        self.enc_key.grid(row=2, column=1, sticky="w", padx=5, pady=5)
        ttk.Button(enc_frame, text="Случайный ключ", command=self._random_key).grid(row=2, column=2, padx=5)

        ttk.Button(enc_frame, text="Зашифровать файл", command=self._do_encrypt).grid(row=3, column=1, pady=15)

        self.enc_status = ttk.Label(enc_frame, text="", foreground="gray")
        self.enc_status.grid(row=4, column=0, columnspan=3, padx=5, pady=5, sticky="w")

        # ---------- Вкладка дешифрования ----------
        dec_frame = ttk.Frame(notebook)
        notebook.add(dec_frame, text="Дешифрование")

        ttk.Label(dec_frame, text="Входной файл:").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.dec_input = tk.Entry(dec_frame, width=60)
        self.dec_input.grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(dec_frame, text="Обзор", command=lambda: self._browse(self.dec_input)).grid(row=0, column=2, padx=5)

        ttk.Label(dec_frame, text="Выходной файл:").grid(row=1, column=0, sticky="w", padx=5, pady=5)
        self.dec_output = tk.Entry(dec_frame, width=60)
        self.dec_output.grid(row=1, column=1, padx=5, pady=5)
        ttk.Button(dec_frame, text="Обзор", command=lambda: self._browse_save(self.dec_output)).grid(row=1, column=2, padx=5)

        ttk.Label(dec_frame, text="Ключ (32 бита, целое число):").grid(row=2, column=0, sticky="w", padx=5, pady=5)
        self.dec_key = tk.Entry(dec_frame, width=20)
        self.dec_key.grid(row=2, column=1, sticky="w", padx=5, pady=5)

        ttk.Button(dec_frame, text="Расшифровать файл", command=self._do_decrypt).grid(row=3, column=1, pady=15)

        self.dec_status = ttk.Label(dec_frame, text="", foreground="gray")
        self.dec_status.grid(row=4, column=0, columnspan=3, padx=5, pady=5, sticky="w")

        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

    def _browse(self, entry):
        path = filedialog.askopenfilename()
        if path:
            entry.delete(0, "end")
            entry.insert(0, path)

    def _browse_save(self, entry):
        path = filedialog.asksaveasfilename()
        if path:
            entry.delete(0, "end")
            entry.insert(0, path)

    def _random_key(self):
        import random
        key = random.randint(0, (1 << 32) - 1)
        self.enc_key.delete(0, "end")
        self.enc_key.insert(0, str(key))

    def _do_encrypt(self):
        input_path = self.enc_input.get().strip()
        output_path = self.enc_output.get().strip()
        try:
            key32 = int(self.enc_key.get().strip())
        except ValueError:
            messagebox.showerror("Ошибка", "Ключ должен быть целым числом")
            return

        if not input_path or not output_path:
            messagebox.showerror("Ошибка", "Укажите входной и выходной файлы")
            return

        try:
            self.enc_status.config(text="Шифрование...", foreground="blue")
            self.root.update()
            encrypt_file(input_path, output_path, key32)
            self.enc_status.config(text=f"Готово: {output_path}", foreground="green")
        except CipherError as e:
            messagebox.showerror("Ошибка шифрования", str(e))
            self.enc_status.config(text="Ошибка", foreground="red")
        except Exception as e:
            messagebox.showerror("Ошибка", f"Не удалось зашифровать файл:\n{e}")
            self.enc_status.config(text="Ошибка", foreground="red")

    def _do_decrypt(self):
        input_path = self.dec_input.get().strip()
        output_path = self.dec_output.get().strip()
        try:
            key32 = int(self.dec_key.get().strip())
        except ValueError:
            messagebox.showerror("Ошибка", "Ключ должен быть целым числом")
            return

        if not input_path or not output_path:
            messagebox.showerror("Ошибка", "Укажите входной и выходной файлы")
            return

        try:
            self.dec_status.config(text="Расшифрование...", foreground="blue")
            self.root.update()
            decrypt_file(input_path, output_path, key32)
            self.dec_status.config(text=f"Готово: {output_path}", foreground="green")
        except CipherError as e:
            messagebox.showerror("Ошибка расшифрования", str(e))
            self.dec_status.config(text="Ошибка", foreground="red")
        except Exception as e:
            messagebox.showerror("Ошибка", f"Не удалось расшифровать файл:\n{e}")
            self.dec_status.config(text="Ошибка", foreground="red")


if __name__ == "__main__":
    root = tk.Tk()
    app = DesApp(root)
    root.mainloop()