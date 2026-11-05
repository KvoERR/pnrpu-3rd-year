import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from main import CompressionError, compress_file, decompress_file, verify


class CompressionApp:
    def __init__(self, root: tk.Tk) -> None:
        self.root = root
        self.root.title("Сжатие: арифметическое + LZW")
        self.root.geometry("760x560")
        self.root.resizable(True, True)

        main = ttk.Frame(root, padding=10)
        main.grid(row=0, column=0, sticky="nsew")
        root.grid_rowconfigure(0, weight=1)
        root.grid_columnconfigure(0, weight=1)

        self.source_entry = self._add_file_row(main, 0, "Исходный файл:", self._browse_open)
        self.packed_entry = self._add_file_row(main, 1, "Сжатый файл:", self._browse_save)
        ttk.Button(main, text="Сжать", command=self._run_compress).grid(row=2, column=1, pady=8)

        self.restored_entry = self._add_file_row(main, 3, "Восстановленный файл:", self._browse_save)
        ttk.Button(main, text="Восстановить", command=self._run_decompress).grid(row=4, column=1, pady=8)

        self.result = tk.Text(main, height=10, width=80, state="disabled")
        self.result.grid(row=5, column=0, columnspan=3, padx=5, pady=10)

        self.status = ttk.Label(main, text="", foreground="gray", wraplength=700)
        self.status.grid(row=6, column=0, columnspan=3, padx=5, pady=5, sticky="w")

    def _add_file_row(self, frame: ttk.Frame, row: int, label: str, browse) -> tk.Entry:
        ttk.Label(frame, text=label).grid(row=row, column=0, sticky="w", padx=5, pady=5)
        entry = tk.Entry(frame, width=60)
        entry.grid(row=row, column=1, padx=5, pady=5)
        ttk.Button(frame, text="Обзор", command=lambda: browse(entry)).grid(row=row, column=2, padx=5)
        return entry

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

    def _show(self, text: str) -> None:
        self.result.config(state="normal")
        self.result.delete("1.0", "end")
        self.result.insert("1.0", text)
        self.result.config(state="disabled")

    def _run_compress(self) -> None:
        source = self.source_entry.get().strip()
        packed = self.packed_entry.get().strip()
        if not source or not packed:
            messagebox.showerror("Ошибка", "Укажите исходный и сжатый файлы")
            return

        try:
            stats = compress_file(source, packed)
            ratio = stats["original"] / stats["lzw"] if stats["lzw"] else 0
            self.status.config(text="Готово", foreground="green")
            self._show(
                f"Исходный размер:       {stats['original']} байт\n"
                f"После арифметического: {stats['arithmetic']} байт\n"
                f"После LZW:             {stats['lzw']} байт\n"
                f"Итоговый коэффициент:  {ratio:.3f}"
            )
        except (CompressionError, OSError) as error:
            messagebox.showerror("Ошибка сжатия", str(error))
            self.status.config(text="Ошибка", foreground="red")

    def _run_decompress(self) -> None:
        packed = self.packed_entry.get().strip()
        restored = self.restored_entry.get().strip()
        if not packed or not restored:
            messagebox.showerror("Ошибка", "Укажите сжатый и восстановленный файлы")
            return

        try:
            size = decompress_file(packed, restored)
            match = verify(self.source_entry.get().strip(), restored) if self.source_entry.get().strip() else None
            self.status.config(text="Готово", foreground="green")
            lines = [
                f"Восстановлено: {size} байт",
                f"Файл: {restored}",
            ]
            if match is not None:
                lines.append("Совпадает с исходным: да" if match else "Совпадает с исходным: НЕТ")
            self._show("\n".join(lines))
        except (CompressionError, OSError) as error:
            messagebox.showerror("Ошибка восстановления", str(error))
            self.status.config(text="Ошибка", foreground="red")


if __name__ == "__main__":
    root = tk.Tk()
    CompressionApp(root)
    root.mainloop()