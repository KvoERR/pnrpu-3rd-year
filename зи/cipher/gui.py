import tkinter as tk
from tkinter import ttk, messagebox


class CipherApp:
    def __init__(self, root):
        root.title("Шифратор")
        root.geometry("500x400")
        root.resizable(False, False)

        # Поле ввода
        ttk.Label(root, text="Текст:").grid(row=0, column=0, padx=10, pady=(10, 0), sticky="w")
        self.text_entry = tk.Text(root, height=5, width=50)
        self.text_entry.grid(row=1, column=0, padx=10, pady=5)

        # Поле ключа
        ttk.Label(root, text="Ключ:").grid(row=2, column=0, padx=10, sticky="w")
        self.key_entry = tk.Entry(root, width=50)
        self.key_entry.grid(row=3, column=0, padx=10, pady=5)

        # Кнопки
        btn_frame = ttk.Frame(root)
        btn_frame.grid(row=4, column=0, pady=10)
        ttk.Button(btn_frame, text="Зашифровать", command=self.encrypt).pack(side="left", padx=5)
        ttk.Button(btn_frame, text="Расшифровать", command=self.decrypt).pack(side="left", padx=5)

        # Поле результата
        ttk.Label(root, text="Результат:").grid(row=5, column=0, padx=10, pady=(10, 0), sticky="w")
        self.result_text = tk.Text(root, height=5, width=50, state="disabled")
        self.result_text.grid(row=6, column=0, padx=10, pady=5)

    def encrypt(self):
        try:
            word = self.text_entry.get("1.0", "end-1c")
            key = self.key_entry.get()
            encrypted = encrypt(word, key)
            self._show_result(encrypted)
        except (CipherError, TypeError) as e:
            messagebox.showerror("Ошибка", str(e))

    def decrypt(self):
        try:
            word = self.text_entry.get("1.0", "end-1c")
            key = self.key_entry.get()
            decrypted = decrypt(word, key)
            self._show_result(decrypted)
        except (CipherError, TypeError) as e:
            messagebox.showerror("Ошибка", str(e))

    def _show_result(self, result):
        self.result_text.config(state="normal")
        self.result_text.delete("1.0", "end")
        self.result_text.insert("1.0", result)
        self.result_text.config(state="disabled")


if __name__ == "__main__":
    from main import CipherError, encrypt, decrypt
    root = tk.Tk()
    app = CipherApp(root)
    root.mainloop()
