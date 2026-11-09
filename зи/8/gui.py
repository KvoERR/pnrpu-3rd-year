import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from main import OtpFileError, decrypt_file, encrypt_file
from otp import OtpError


class OtpApp:
    def __init__(self, root: tk.Tk) -> None:
        self.root = root
        self.root.title("Однократное гаммирование (блоки 64 бита)")
        self.root.geometry("760x560")
        self.root.resizable(True, True)

        notebook = ttk.Notebook(root)
        notebook.grid(row=0, column=0, padx=10, pady=10, sticky="nsew")
        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

        self._build_encrypt_tab(notebook)
        self._build_decrypt_tab(notebook)

    def _build_encrypt_tab(self, notebook: ttk.Notebook) -> None:
        frame = ttk.Frame(notebook)
        notebook.add(frame, text="Шифрование")

        ttk.Label(frame, text="Входной файл:").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.enc_input = tk.Entry(frame, width=60)
        self.enc_input.grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Обзор", command=lambda: self._browse_open(self.enc_input)).grid(row=0, column=2, padx=5)

        ttk.Label(frame, text="Выходной файл:").grid(row=1, column=0, sticky="w", padx=5, pady=5)
        self.enc_output = tk.Entry(frame, width=60)
        self.enc_output.grid(row=1, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Обзор", command=lambda: self._browse_save(self.enc_output)).grid(row=1, column=2, padx=5)

        ttk.Button(frame, text="Зашифровать файл", command=self._run_encrypt).grid(row=2, column=1, pady=15)

        self.enc_status = ttk.Label(frame, text="", foreground="gray", wraplength=700)
        self.enc_status.grid(row=3, column=0, columnspan=3, padx=5, pady=5, sticky="w")

        self.enc_result = tk.Text(frame, height=8, width=80, state="disabled")
        self.enc_result.grid(row=4, column=0, columnspan=3, padx=5, pady=10)

    def _build_decrypt_tab(self, notebook: ttk.Notebook) -> None:
        frame = ttk.Frame(notebook)
        notebook.add(frame, text="Дешифрование")

        ttk.Label(frame, text="Входной файл:").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.dec_input = tk.Entry(frame, width=60)
        self.dec_input.grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Обзор", command=lambda: self._browse_open(self.dec_input)).grid(row=0, column=2, padx=5)

        ttk.Label(frame, text="Выходной файл:").grid(row=1, column=0, sticky="w", padx=5, pady=5)
        self.dec_output = tk.Entry(frame, width=60)
        self.dec_output.grid(row=1, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Обзор", command=lambda: self._browse_save(self.dec_output)).grid(row=1, column=2, padx=5)

        ttk.Button(frame, text="Расшифровать файл", command=self._run_decrypt).grid(row=2, column=1, pady=15)

        self.dec_status = ttk.Label(frame, text="", foreground="gray", wraplength=700)
        self.dec_status.grid(row=3, column=0, columnspan=3, padx=5, pady=5, sticky="w")

        self.dec_result = tk.Text(frame, height=8, width=80, state="disabled")
        self.dec_result.grid(row=4, column=0, columnspan=3, padx=5, pady=10)

    def _browse_open(self, entry: tk.Entry) -> None:
        path = filedialog.askopenfilename()
        if path:
            entry.delete(0, "end")
            entry.insert(0, path)

    def _browse_save(self, entry: tk.Entry) -> None:
        path = filedialog.asksaveasfilename()
        if path:
            entry.delete(0, "end")
            entry.insert(0, path)

    def _show(self, widget: tk.Text, text: str) -> None:
        widget.config(state="normal")
        widget.delete("1.0", "end")
        widget.insert("1.0", text)
        widget.config(state="disabled")

    def _run_encrypt(self) -> None:
        source = self.enc_input.get().strip()
        target = self.enc_output.get().strip()
        if not source or not target:
            messagebox.showerror("Ошибка", "Укажите входной и выходной файлы")
            return

        try:
            stats = encrypt_file(source, target)
            self.enc_status.config(text="Готово", foreground="green")
            self._show(
                self.enc_result,
                f"Исходный размер:    {stats['original']} байт\n"
                f"После дополнения:   {stats['padded']} байт\n"
                f"Гамма:              {stats['gamma']} байт\n"
                f"Шифротекст:         {stats['ciphertext']} байт\n"
                f"Файл:               {target}"
            )
        except (OtpError, OtpFileError, OSError) as error:
            messagebox.showerror("Ошибка шифрования", str(error))
            self.enc_status.config(text="Ошибка", foreground="red")

    def _run_decrypt(self) -> None:
        source = self.dec_input.get().strip()
        target = self.dec_output.get().strip()
        if not source or not target:
            messagebox.showerror("Ошибка", "Укажите входной и выходной файлы")
            return

        try:
            size = decrypt_file(source, target)
            self.dec_status.config(text="Готово", foreground="green")
            self._show(
                self.dec_result,
                f"Восстановлено: {size} байт\n"
                f"Файл:          {target}"
            )
        except (OtpError, OtpFileError, OSError) as error:
            messagebox.showerror("Ошибка дешифрования", str(error))
            self.dec_status.config(text="Ошибка", foreground="red")


if __name__ == "__main__":
    root = tk.Tk()
    OtpApp(root)
    root.mainloop()