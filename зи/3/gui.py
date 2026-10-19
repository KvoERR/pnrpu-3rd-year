import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import re

from main import (
    CipherError,
    is_prime_miller_rabin,
    generate_elgamal_keys,
    encrypt_elgamal,
    decrypt_elgamal,
)


def bytes_len(n):
    return (n.bit_length() + 7) // 8


class CipherApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Эль-Гамаль Шифратор")
        self.root.geometry("820x900")
        self.root.resizable(True, True)

        notebook = ttk.Notebook(root)
        notebook.grid(row=0, column=0, padx=10, pady=10, sticky="nsew")

        eg_frame = ttk.Frame(notebook)
        notebook.add(eg_frame, text="Эль-Гамаль")

        ttk.Label(eg_frame, text="Текст:").pack(anchor="w", padx=10, pady=(5, 0))
        self.eg_text = tk.Text(eg_frame, height=5, width=90)
        self.eg_text.pack(fill="x", padx=10, pady=5)

        ttk.Button(eg_frame, text="Импорт из файла", command=self.import_file).pack(anchor="w", padx=10, pady=(0, 5))

        ttk.Button(
            eg_frame, text="Генерировать ключи Эль-Гамаля", command=self.generate_elgamal_keys
        ).pack(pady=5)

        keys_frame = ttk.Frame(eg_frame)
        keys_frame.pack(fill="x", padx=10, pady=5)

        ttk.Label(keys_frame, text="p:").grid(row=0, column=0, sticky="w")
        self.eg_p_entry = tk.Entry(keys_frame, width=60)
        self.eg_p_entry.grid(row=0, column=1, sticky="w", padx=5)

        ttk.Label(keys_frame, text="g:").grid(row=1, column=0, sticky="w", pady=(5, 0))
        self.eg_g_entry = tk.Entry(keys_frame, width=60)
        self.eg_g_entry.grid(row=1, column=1, sticky="w", padx=5, pady=(5, 0))

        ttk.Label(keys_frame, text="y:").grid(row=2, column=0, sticky="w", pady=(5, 0))
        self.eg_y_entry = tk.Entry(keys_frame, width=60)
        self.eg_y_entry.grid(row=2, column=1, sticky="w", padx=5, pady=(5, 0))

        ttk.Label(keys_frame, text="x:").grid(row=3, column=0, sticky="w", pady=(5, 0))
        self.eg_x_entry = tk.Entry(keys_frame, width=60)
        self.eg_x_entry.grid(row=3, column=1, sticky="w", padx=5, pady=(5, 0))

        ttk.Button(eg_frame, text="Зашифровать", command=self.do_encrypt_elgamal).pack(pady=5)

        ttk.Label(eg_frame, text="Результат шифрования:").pack(anchor="w", padx=10, pady=(5, 0))
        self.eg_enc_result = tk.Text(eg_frame, height=5, width=90, state="disabled")
        self.eg_enc_result.pack(fill="x", padx=10, pady=5)

        ttk.Label(eg_frame, text="Шифротекст (c1,c2 через запятую, пары в скобках):").pack(anchor="w", padx=10, pady=(10, 0))
        self.eg_dec_text = tk.Text(eg_frame, height=4, width=90)
        self.eg_dec_text.pack(fill="x", padx=10, pady=5)

        ttk.Button(eg_frame, text="Расшифровать", command=self.do_decrypt_elgamal).pack(pady=5)

        ttk.Label(eg_frame, text="Результат расшифровки:").pack(anchor="w", padx=10, pady=(5, 0))
        self.eg_dec_result = tk.Text(eg_frame, height=5, width=90, state="disabled")
        self.eg_dec_result.pack(fill="x", padx=10, pady=5)

        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

        self.info_label = ttk.Label(root, text="Готово", foreground="gray")
        self.info_label.grid(row=1, column=0, padx=10, pady=(0, 10), sticky="w")

    def import_file(self):
        file_path = filedialog.askopenfilename(
            title="Выберите файл",
            filetypes=[("Текстовые файлы", "*.txt"), ("Все файлы", "*.*")]
        )
        if file_path:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    text = f.read()
                self.eg_text.delete("1.0", "end")
                self.eg_text.insert("1.0", text)
                self.info_label.config(text=f"Импортировано: {file_path}", foreground="green")
            except Exception as e:
                messagebox.showerror("Ошибка импорта", str(e))
                self.info_label.config(text="Ошибка импорта", foreground="red")

    def _show_result(self, result_text, result_widget):
        result_widget.config(state="normal")
        result_widget.delete("1.0", "end")
        result_widget.insert("1.0", result_text)
        result_widget.config(state="disabled")

    def generate_elgamal_keys(self):
        try:
            self.info_label.config(text="Генерация ключей Эль-Гамаля...", foreground="blue")
            self.root.update()

            p, g, x, y = generate_elgamal_keys()

            self.eg_p_entry.delete(0, "end")
            self.eg_p_entry.insert(0, str(p))
            self.eg_g_entry.delete(0, "end")
            self.eg_g_entry.insert(0, str(g))
            self.eg_y_entry.delete(0, "end")
            self.eg_y_entry.insert(0, str(y))
            self.eg_x_entry.delete(0, "end")
            self.eg_x_entry.insert(0, str(x))

            info = (
                f"p = {bytes_len(p)} байт, "
                f"g = {bytes_len(g)} байт, "
                f"x = {bytes_len(x)} байт, "
                f"y = {bytes_len(y)} байт"
            )
            self.info_label.config(text=info, foreground="green")
        except Exception as e:
            messagebox.showerror("Ошибка генерации", str(e))
            self.info_label.config(text="Ошибка генерации", foreground="red")

    def _parse_elgamal_ciphertext(self, text):
        text = text.strip()
        pairs = []
        matches = re.findall(r'\(\s*(\d+)\s*,\s*(\d+)\s*\)', text)
        for m in matches:
            pairs.append((int(m[0]), int(m[1])))
        if not pairs:
            raise CipherError("Не удалось распознать шифротекст. Используйте формат: (c1,c2),(c1,c2),...")
        return pairs

    def do_encrypt_elgamal(self):
        try:
            text = self.eg_text.get("1.0", "end-1c")
            if not text:
                raise CipherError("Введите текст")

            p = int(self.eg_p_entry.get())
            g = int(self.eg_g_entry.get())
            y = int(self.eg_y_entry.get())
        except ValueError:
            messagebox.showerror("Ошибка", "Заполните поля p, g, y числами")
            return
        except CipherError as e:
            messagebox.showerror("Ошибка", str(e))
            return

        if not is_prime_miller_rabin(p):
            messagebox.showerror("Ошибка", f"p={p} — не простое число")
            return
        if bytes_len(p) < 20:
            messagebox.showerror("Ошибка", f"p — {bytes_len(p)} байт, нужно ≥ 20")
            return
        if not is_prime_miller_rabin(g):
            messagebox.showerror("Ошибка", f"g={g} — не простое число")
            return
        if bytes_len(g) < 12:
            messagebox.showerror("Ошибка", f"g — {bytes_len(g)} байт, нужно ≥ 12")
            return

        try:
            self.info_label.config(text="Шифрование Эль-Гамаля...", foreground="blue")
            self.root.update()

            ciphertext = encrypt_elgamal(text, p, g, y)

            result = (
                f"p = {p} ({bytes_len(p)} байт)\n"
                f"g = {g} ({bytes_len(g)} байт, простое)\n"
                f"y = {y}\n\n"
                f"Зашифрованный текст:\n"
                + ", ".join(f"({c1},{c2})" for c1, c2 in ciphertext)
            )
            self._show_result(result, self.eg_enc_result)
            self.info_label.config(text="Шифрование Эль-Гамаля выполнено", foreground="green")
        except (CipherError, TypeError) as e:
            messagebox.showerror("Ошибка", str(e))
            self.info_label.config(text="Ошибка шифрования", foreground="red")

    def do_decrypt_elgamal(self):
        try:
            text = self.eg_dec_text.get("1.0", "end-1c").strip()
            if not text:
                raise CipherError("Введите зашифрованный текст")

            x = int(self.eg_x_entry.get())
            p = int(self.eg_p_entry.get())
        except ValueError:
            messagebox.showerror("Ошибка", "Заполните поля x и p числами")
            return
        except CipherError as e:
            messagebox.showerror("Ошибка", str(e))
            return

        if x <= 0 or p <= 0:
            messagebox.showerror("Ошибка", "x и p должны быть положительными числами")
            return

        try:
            ciphertext_pairs = self._parse_elgamal_ciphertext(text)
        except CipherError as e:
            messagebox.showerror("Ошибка", str(e))
            return

        try:
            self.info_label.config(text="Расшифровка Эль-Гамаля...", foreground="blue")
            self.root.update()

            plaintext = decrypt_elgamal(ciphertext_pairs, x, p)

            result = f"Расшифрованный текст:\n{plaintext}"
            self._show_result(result, self.eg_dec_result)
            self.info_label.config(text="Расшифровка Эль-Гамаля выполнена", foreground="green")
        except (CipherError, TypeError) as e:
            messagebox.showerror("Ошибка", str(e))
            self.info_label.config(text="Ошибка расшифровки", foreground="red")


if __name__ == "__main__":
    root = tk.Tk()
    app = CipherApp(root)
    root.mainloop()