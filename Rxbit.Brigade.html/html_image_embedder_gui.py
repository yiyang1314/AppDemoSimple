import os
import base64
import tkinter as tk
from tkinter import filedialog, messagebox, scrolledtext
from pathlib import Path
from bs4 import BeautifulSoup

# ==================== 核心功能 ====================
def image_to_base64(image_path):
    """将图片文件转为 data:image/...;base64 字符串"""
    ext = Path(image_path).suffix.lower()
    mime_map = {
        '.jpg': 'image/jpeg',
        '.jpeg': 'image/jpeg',
        '.png': 'image/png',
        '.gif': 'image/gif',
        '.bmp': 'image/bmp',
        '.svg': 'image/svg+xml',
        '.webp': 'image/webp',
        '.ico': 'image/x-icon'
    }
    mime = mime_map.get(ext, 'application/octet-stream')
    with open(image_path, 'rb') as f:
        data = f.read()
    b64 = base64.b64encode(data).decode('utf-8')
    return f"data:{mime};base64,{b64}"

def embed_images_in_html(html_path, output_path, log_callback=None):
    """
    将 HTML 中所有本地图片 src 替换为 Base64
    log_callback: 用于实时显示日志的函数
    返回 (成功数量, 错误信息列表)
    """
    errors = []
    success_count = 0

    # 检查输入文件
    if not os.path.exists(html_path):
        raise FileNotFoundError(f"HTML 文件不存在: {html_path}")

    html_dir = os.path.dirname(html_path)

    with open(html_path, 'r', encoding='utf-8') as f:
        soup = BeautifulSoup(f, 'html.parser')

    img_tags = soup.find_all('img')
    total = len(img_tags)
    if log_callback:
        log_callback(f"找到 {total} 个 <img> 标签\n")

    for idx, img in enumerate(img_tags, 1):
        src = img.get('src')
        if not src:
            continue
        # 跳过已经是 data: 的
        if src.startswith('data:'):
            if log_callback:
                log_callback(f"[{idx}/{total}] 跳过: 已内嵌 {src[:30]}...\n")
            success_count += 1  # 视为已处理
            continue
        # 跳过网络 URL 和绝对路径（可根据需要修改）
        if src.startswith('http://') or src.startswith('https://') or os.path.isabs(src):
            msg = f"[{idx}/{total}] 跳过: 非本地相对路径 {src}"
            errors.append(msg)
            if log_callback:
                log_callback(msg + "\n")
            continue

        local_path = os.path.join(html_dir, src)
        if not os.path.exists(local_path):
            msg = f"[{idx}/{total}] 错误: 文件不存在 {local_path}"
            errors.append(msg)
            if log_callback:
                log_callback(msg + "\n")
            continue

        try:
            b64_data = image_to_base64(local_path)
            img['src'] = b64_data
            success_count += 1
            if log_callback:
                log_callback(f"[{idx}/{total}] 替换: {src} -> Base64 (大小 {len(b64_data)//1024} KB)\n")
        except Exception as e:
            msg = f"[{idx}/{total}] 处理 {src} 失败: {str(e)}"
            errors.append(msg)
            if log_callback:
                log_callback(msg + "\n")

    # 保存
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(str(soup))

    return success_count, errors

# ==================== GUI 部分 ====================
class EmbedderApp:
    def __init__(self, root):
        self.root = root
        root.title("HTML 图片内嵌工具")
        root.geometry("650x550")
        root.resizable(True, True)

        # 输入文件选择
        tk.Label(root, text="输入 HTML 文件:").grid(row=0, column=0, sticky='w', padx=10, pady=5)
        self.input_path = tk.StringVar()
        tk.Entry(root, textvariable=self.input_path, width=50).grid(row=0, column=1, padx=5)
        tk.Button(root, text="浏览...", command=self.select_input).grid(row=0, column=2, padx=5)

        # 输出文件选择
        tk.Label(root, text="输出 HTML 文件:").grid(row=1, column=0, sticky='w', padx=10, pady=5)
        self.output_path = tk.StringVar()
        tk.Entry(root, textvariable=self.output_path, width=50).grid(row=1, column=1, padx=5)
        tk.Button(root, text="浏览...", command=self.select_output).grid(row=1, column=2, padx=5)

        # 执行按钮
        self.run_btn = tk.Button(root, text="开始内嵌", command=self.run_embed, bg="#4CAF50", fg="white", font=("Arial", 10, "bold"))
        self.run_btn.grid(row=2, column=1, pady=15)

        # 日志显示区域（带滚动条）
        self.log_text = scrolledtext.ScrolledText(root, width=80, height=25, state='normal')
        self.log_text.grid(row=3, column=0, columnspan=3, padx=10, pady=5, sticky='nsew')
        # 设置网格权重，使日志区域可拉伸
        root.grid_rowconfigure(3, weight=1)
        root.grid_columnconfigure(1, weight=1)

        # 清空日志按钮
        tk.Button(root, text="清空日志", command=self.clear_log).grid(row=4, column=1, pady=5)

    def select_input(self):
        file_path = filedialog.askopenfilename(
            title="选择 HTML 文件",
            filetypes=[("HTML files", "*.html *.htm"), ("All files", "*.*")]
        )
        if file_path:
            self.input_path.set(file_path)
            # 如果输出路径为空，自动填充为 输入文件名_embedded.html
            if not self.output_path.get():
                base, ext = os.path.splitext(file_path)
                self.output_path.set(f"{base}_embedded{ext}")

    def select_output(self):
        file_path = filedialog.asksaveasfilename(
            title="保存 HTML 文件为",
            defaultextension=".html",
            filetypes=[("HTML files", "*.html *.htm"), ("All files", "*.*")]
        )
        if file_path:
            self.output_path.set(file_path)

    def clear_log(self):
        self.log_text.delete('1.0', tk.END)

    def log(self, message):
        self.log_text.insert(tk.END, message)
        self.log_text.see(tk.END)  # 自动滚动到底部
        self.root.update_idletasks()

    def run_embed(self):
        input_file = self.input_path.get().strip()
        output_file = self.output_path.get().strip()

        if not input_file:
            messagebox.showerror("错误", "请选择输入 HTML 文件")
            return
        if not output_file:
            messagebox.showerror("错误", "请指定输出文件路径")
            return

        # 禁用按钮防止重复点击
        self.run_btn.config(state='disabled', text='处理中...')
        self.log_text.delete('1.0', tk.END)

        try:
            # 调用核心功能
            success, errors = embed_images_in_html(input_file, output_file, log_callback=self.log)
            self.log(f"\n=== 处理完成 ===\n成功内嵌: {success} 个图片\n")
            if errors:
                self.log(f"共有 {len(errors)} 个警告/错误，详见上方日志。\n")
            else:
                self.log("所有图片处理成功！\n")
            messagebox.showinfo("完成", f"处理完成！\n成功内嵌 {success} 个图片。\n输出文件: {output_file}")
        except Exception as e:
            error_msg = f"发生异常: {str(e)}"
            self.log(f"\n{error_msg}\n")
            messagebox.showerror("错误", error_msg)
        finally:
            self.run_btn.config(state='normal', text='开始内嵌')

# ==================== 程序入口 ====================
if __name__ == "__main__":
    root = tk.Tk()
    app = EmbedderApp(root)
    root.mainloop()