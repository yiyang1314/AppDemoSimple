import os
import base64
from pathlib import Path
from bs4 import BeautifulSoup

def image_to_base64(image_path):
    """将本地图片文件转换为 data:image/...;base64 格式"""
    # 自动检测图片类型（根据扩展名）
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

def replace_images_with_base64(html_file_path, output_file_path=None):
    """
    读取 HTML 文件，将所有 img 标签的 src（本地相对路径）替换为 Base64。
    支持相对路径（相对于 HTML 所在目录）。
    """
    html_dir = os.path.dirname(html_file_path)
    with open(html_file_path, 'r', encoding='utf-8') as f:
        soup = BeautifulSoup(f, 'html.parser')
    
    img_tags = soup.find_all('img')
    for img in img_tags:
        src = img.get('src')
        if not src:
            continue
        # 跳过已经是 data: 协议的
        if src.startswith('data:'):
            continue
        # 构建本地绝对路径
        # 如果 src 是绝对路径或网络URL，则跳过（或可自行处理下载）
        if src.startswith('http://') or src.startswith('https://') or os.path.isabs(src):
            print(f"跳过非本地相对路径: {src}")
            continue
        local_path = os.path.join(html_dir, src)
        if not os.path.exists(local_path):
            print(f"文件不存在: {local_path}")
            continue
        try:
            b64_data = image_to_base64(local_path)
            img['src'] = b64_data
            print(f"替换: {src} -> Base64")
        except Exception as e:
            print(f"处理 {src} 失败: {e}")
    
    # 保存
    output_path = output_file_path or html_file_path
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(str(soup))
    print(f"完成！输出文件: {output_path}")

if __name__ == '__main__':
    # 示例调用
    replace_images_with_base64('index.html', 'index_embedded.html')