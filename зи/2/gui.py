import tkinter as tk
from tkinter import ttk, messagebox, filedialog

from main import CipherError, encrypt, decrypt, generate_prime, is_prime_miller_rabin


class CipherApp:
    def __init__(self, root):
        self.root = root
        self.root.title("RSA Шифратор")
        self.root.geometry("600x600")
        self.root.resizable(False, False)

        # Параметры последнего шифрования (нужны для дешифровки)
        self.last_k = None
        self.last_total_bits = None

        # --- Вкладки ---
        notebook = ttk.Notebook(root)
        notebook.grid(row=0, column=0, padx=10, pady=10, sticky="nsew")

        # ===== Вкладка "Шифровка" =====
        enc_frame = ttk.Frame(notebook)
        notebook.add(enc_frame, text="Шифровка")

        ttk.Label(enc_frame, text="Текст:").pack(anchor="w", padx=10, pady=(5, 0))
        self.enc_text = tk.Text(enc_frame, height=5, width=70)
        self.enc_text.pack(fill="x", padx=10, pady=5)

        # Импорт из файла
        self.import_btn = ttk.Button(enc_frame, text="Импорт из файла", command=self.import_file)
        self.import_btn.pack(anchor="w", padx=10, pady=(0, 5))

        p_frame = ttk.Frame(enc_frame)
        p_frame.pack(fill="x", padx=10, pady=5)

        ttk.Label(p_frame, text="p:").pack(side="left")
        self.p_entry = tk.Entry(p_frame, width=20)
        self.p_entry.pack(side="left", padx=5)

        ttk.Label(p_frame, text="q:").pack(side="left", padx=(10, 0))
        self.q_entry = tk.Entry(p_frame, width=20)
        self.q_entry.pack(side="left", padx=5)

        self.generate_btn = ttk.Button(
            enc_frame, text="Генерировать p и q", command=self.generate_primes
        )
        self.generate_btn.pack(pady=5)

        ttk.Button(enc_frame, text="Зашифровать", command=self.do_encrypt).pack(pady=5)

        ttk.Label(enc_frame, text="Результат:").pack(anchor="w", padx=10, pady=(5, 0))
        self.enc_result = tk.Text(enc_frame, height=6, width=70, state="disabled")
        self.enc_result.pack(fill="both", expand=True, padx=10, pady=5)

        # ===== Вкладка "Дешифровка" =====
        dec_frame = ttk.Frame(notebook)
        notebook.add(dec_frame, text="Дешифровка")

        ttk.Label(dec_frame, text="Шифротекст (числа через запятую):").pack(anchor="w", padx=10, pady=(5, 0))
        self.dec_text = tk.Text(dec_frame, height=5, width=70)
        self.dec_text.pack(fill="x", padx=10, pady=5)

        ttk.Button(dec_frame, text="Импорт из файла", command=self.import_file_dec).pack(anchor="w", padx=10, pady=(0, 5))

        pq_frame = ttk.Frame(dec_frame)
        pq_frame.pack(fill="x", padx=10, pady=5)

        ttk.Label(pq_frame, text="d:").pack(side="left")
        self.d_entry = tk.Entry(pq_frame, width=35)
        self.d_entry.pack(side="left", padx=5)

        ttk.Label(pq_frame, text="n:").pack(side="left", padx=(10, 0))
        self.n_entry = tk.Entry(pq_frame, width=35)
        self.n_entry.pack(side="left", padx=5)

        ttk.Button(dec_frame, text="Расшифровать", command=self.do_decrypt).pack(pady=5)

        ttk.Label(dec_frame, text="Результат:").pack(anchor="w", padx=10, pady=(5, 0))
        self.dec_result = tk.Text(dec_frame, height=6, width=70, state="disabled")
        self.dec_result.pack(fill="both", expand=True, padx=10, pady=5)

        # Распределить пространство
        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

        # --- Информационное поле ---
        self.info_label = ttk.Label(root, text="Готово", foreground="gray")
        self.info_label.grid(row=1, column=0, padx=10, pady=(0, 10), sticky="w")

    def generate_primes(self):
        """Генерирует простые числа p и q разрядностью ≥ 20 бит."""
        try:
            self.info_label.config(text="Генерация...", foreground="blue")
            self.root.update()

            p = generate_prime(16)
            q = generate_prime(16)

            self.p_entry.delete(0, "end")
            self.p_entry.insert(0, str(p))
            self.q_entry.delete(0, "end")
            self.q_entry.insert(0, str(q))

            info = f"p = {p} ({p.bit_length()} бит)\nq = {q} ({q.bit_length()} бит)"
            self.info_label.config(text=info, foreground="green")
        except Exception as e:
            messagebox.showerror("Ошибка генерации", str(e))
            self.info_label.config(text="Ошибка генерации", foreground="red")

    def import_file(self):
        """Импорт текста из файла."""
        file_path = filedialog.askopenfilename(
            title="Выберите файл",
            filetypes=[("Текстовые файлы", "*.txt"), ("Все файлы", "*.*")]
        )
        if file_path:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    text = f.read()
                self.enc_text.delete("1.0", "end")
                self.enc_text.insert("1.0", text)
                self.info_label.config(text=f"Импортировано: {file_path}", foreground="green")
            except Exception as e:
                messagebox.showerror("Ошибка импорта", str(e))
                self.info_label.config(text="Ошибка импорта", foreground="red")

    def import_file_dec(self):
        """Импорт шифротекста из файла."""
        file_path = filedialog.askopenfilename(
            title="Выберите файл с шифротекстом",
            filetypes=[("Текстовые файлы", "*.txt"), ("Все файлы", "*.*")]
        )
        if file_path:
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    text = f.read()
                self.dec_text.delete("1.0", "end")
                self.dec_text.insert("1.0", text)
                self.info_label.config(text=f"Импортировано: {file_path}", foreground="green")
            except Exception as e:
                messagebox.showerror("Ошибка импорта", str(e))
                self.info_label.config(text="Ошибка импорта", foreground="red")

    def _show_result(self, result_text, result_widget):
        result_widget.config(state="normal")
        result_widget.delete("1.0", "end")
        result_widget.insert("1.0", result_text)
        result_widget.config(state="disabled")

    def do_encrypt(self):
        try:
            text = self.enc_text.get("1.0", "end-1c")
            if not text:
                raise CipherError("Введите текст")

            p = int(self.p_entry.get())
            q = int(self.q_entry.get())
        except ValueError:
            messagebox.showerror("Ошибка", "p и q должны быть числами")
            return

        if not is_prime_miller_rabin(p):
            messagebox.showerror("Ошибка", f"p={p} — не простое число")
            return
        if not is_prime_miller_rabin(q):
            messagebox.showerror("Ошибка", f"q={q} — не простое число")
            return
        if p.bit_length() < 16:
            messagebox.showerror("Ошибка", f"p={p} — разрядность {p.bit_length()} бит, нужно ≥ 16")
            return
        if q.bit_length() < 16:
            messagebox.showerror("Ошибка", f"q={q} — разрядность {q.bit_length()} бит, нужно ≥ 16")
            return

        try:
            self.info_label.config(text="Шифрование...", foreground="blue")
            self.root.update()

            ciphertext, k, total_bits, e, n, d = encrypt(text, p, q)
            self.last_k = k
            self.last_total_bits = total_bits

            self.d_entry.delete(0, "end")
            self.d_entry.insert(0, str(d))
            self.n_entry.delete(0, "end")
            self.n_entry.insert(0, str(n))

            result = (
                f"k={k} бит, total_bits={total_bits}\n"
                f"Ключи: e={e}, n={n}, d={d}\n\n"
                f"Зашифрованный текст:\n"
                f"{', '.join(str(c) for c in ciphertext)}"
            )
            self._show_result(result, self.enc_result)
            self.info_label.config(text="Шифрование выполнено", foreground="green")
        except (CipherError, TypeError) as e:
            messagebox.showerror("Ошибка", str(e))
            self.info_label.config(text="Ошибка шифрования", foreground="red")

    def do_decrypt(self):
        try:
            ciphertext_str = self.dec_text.get("1.0", "end-1c").strip()
            if not ciphertext_str:
                raise CipherError("Введите зашифрованный текст (числа через запятую)")

            d = int(self.d_entry.get())
            n = int(self.n_entry.get())
        except ValueError:
            messagebox.showerror("Ошибка", "d и n должны быть числами")
            return

        if d <= 0 or n <= 0:
            messagebox.showerror("Ошибка", "d и n должны быть положительными числами")
            return

        if self.last_k is None or self.last_total_bits is None:
            messagebox.showerror(
                "Ошибка",
                "Нет параметров k и total_bits. Сначала зашифруйте текст на вкладке «Шифровка»."
            )
            return

        try:
            self.info_label.config(text="Расшифровка...", foreground="blue")
            self.root.update()

            plaintext = decrypt(ciphertext_str, self.last_k, self.last_total_bits, d, n)

            result = f"Расшифрованный текст:\n{plaintext}"
            self._show_result(result, self.dec_result)
            self.info_label.config(text="Расшифровка выполнена", foreground="green")
        except (CipherError, TypeError) as e:
            messagebox.showerror("Ошибка", str(e))
            self.info_label.config(text="Ошибка расшифровки", foreground="red")


if __name__ == "__main__":
    root = tk.Tk()
    app = CipherApp(root)
    root.mainloop()