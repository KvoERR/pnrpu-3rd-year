import tkinter as tk
from tkinter import messagebox, ttk

from main import (
    CodeError,
    bits_to_text,
    decode,
    encode,
    error_position,
    syndrome,
    text_to_bits,
)


class CodeApp:
    def __init__(self, root: tk.Tk) -> None:
        self.root = root
        self.root.title("Код (4, 2): коррекция ошибок")
        self.root.geometry("760x620")
        self.root.resizable(True, True)

        main = ttk.Frame(root, padding=10)
        main.grid(row=0, column=0, sticky="nsew")
        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

        ttk.Label(main, text="Сообщение (2 бита):").grid(row=0, column=0, sticky="w", padx=5, pady=5)
        self.message_entry = tk.Entry(main, width=20)
        self.message_entry.grid(row=0, column=1, sticky="w", padx=5, pady=5)
        self.message_entry.insert(0, "10")

        ttk.Button(main, text="Закодировать", command=self._run_encode).grid(row=0, column=2, padx=5)

        ttk.Label(main, text="Кодовое слово:").grid(row=1, column=0, sticky="w", padx=5, pady=5)
        self.codeword_entry = tk.Entry(main, width=20)
        self.codeword_entry.grid(row=1, column=1, sticky="w", padx=5, pady=5)

        ttk.Label(main, text="Принятое слово (4 бита):").grid(row=2, column=0, sticky="w", padx=5, pady=5)
        self.received_entry = tk.Entry(main, width=20)
        self.received_entry.grid(row=2, column=1, sticky="w", padx=5, pady=5)
        self.received_entry.insert(0, "1101")

        ttk.Button(main, text="Декодировать", command=self._run_decode).grid(row=2, column=2, padx=5)

        ttk.Label(main, text="Синдром:").grid(row=3, column=0, sticky="w", padx=5, pady=5)
        self.syndrome_entry = tk.Entry(main, width=20, state="readonly")
        self.syndrome_entry.grid(row=3, column=1, sticky="w", padx=5, pady=5)

        ttk.Label(main, text="Позиция ошибки:").grid(row=4, column=0, sticky="w", padx=5, pady=5)
        self.position_entry = tk.Entry(main, width=20, state="readonly")
        self.position_entry.grid(row=4, column=1, sticky="w", padx=5, pady=5)

        ttk.Label(main, text="Исправленное слово:").grid(row=5, column=0, sticky="w", padx=5, pady=5)
        self.corrected_entry = tk.Entry(main, width=20, state="readonly")
        self.corrected_entry.grid(row=5, column=1, sticky="w", padx=5, pady=5)

        ttk.Label(main, text="Декодированное сообщение:").grid(row=6, column=0, sticky="w", padx=5, pady=5)
        self.decoded_entry = tk.Entry(main, width=20, state="readonly")
        self.decoded_entry.grid(row=6, column=1, sticky="w", padx=5, pady=5)

        ttk.Button(main, text="Показать все кодовые слова", command=self._show_all_codewords).grid(row=7, column=1, pady=15)

        self.status = ttk.Label(main, text="", foreground="gray", wraplength=700)
        self.status.grid(row=8, column=0, columnspan=3, padx=5, pady=5, sticky="w")

        self._run_encode()

    def _run_encode(self) -> None:
        try:
            message = text_to_bits(self.message_entry.get().strip())
            codeword = encode(message)
            self.codeword_entry.delete(0, "end")
            self.codeword_entry.insert(0, bits_to_text(codeword))
            self.status.config(text=f"Сообщение {bits_to_text(message)} → слово {bits_to_text(codeword)}", foreground="green")
        except CodeError as error:
            messagebox.showerror("Ошибка", str(error))
            self.status.config(text="Ошибка", foreground="red")

    def _run_decode(self) -> None:
        try:
            received = text_to_bits(self.received_entry.get().strip())
            result = syndrome(received)
            position = error_position(received)
            decoded, _, corrected = decode(received)

            self.syndrome_entry.config(state="normal")
            self.syndrome_entry.delete(0, "end")
            self.syndrome_entry.insert(0, bits_to_text(list(result)))
            self.syndrome_entry.config(state="readonly")

            self.position_entry.config(state="normal")
            self.position_entry.delete(0, "end")
            self.position_entry.insert(0, str(position) if position is not None else "нет")
            self.position_entry.config(state="readonly")

            self.corrected_entry.config(state="normal")
            self.corrected_entry.delete(0, "end")
            self.corrected_entry.insert(0, bits_to_text(corrected))
            self.corrected_entry.config(state="readonly")

            self.decoded_entry.config(state="normal")
            self.decoded_entry.delete(0, "end")
            self.decoded_entry.insert(0, bits_to_text(decoded))
            self.decoded_entry.config(state="readonly")

            if position is None:
                self.status.config(text="Ошибки нет", foreground="green")
            else:
                self.status.config(text=f"Ошибка в позиции {position} исправлена", foreground="orange")
        except CodeError as error:
            messagebox.showerror("Ошибка", str(error))
            self.status.config(text="Ошибка", foreground="red")

    def _show_all_codewords(self) -> None:
        lines = []
        for value in range(4):
            message = [(value >> shift) & 1 for shift in (1, 0)]
            codeword = encode(message)
            lines.append(f"{bits_to_text(message)} → {bits_to_text(codeword)}")
        messagebox.showinfo("Кодовые слова (4, 2)", "\n".join(lines))


if __name__ == "__main__":
    root = tk.Tk()
    CodeApp(root)
    root.mainloop()