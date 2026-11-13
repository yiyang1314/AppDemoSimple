const fs = require('fs');
const path = require('path');
const cheerio = require('cheerio');

// ====================== 【配置区，请修改这里】======================
const INPUT_HTML = './index.html';    // 你的源HTML文件
const OUTPUT_HTML = '../output.html';  // 转换后输出的文件
// =================================================================

// MIME类型映射
const MIME_MAP = {
    png: 'image/png',
    jpg: 'image/jpeg',
    jpeg: 'image/jpeg',
    gif: 'image/gif',
    webp: 'image/webp',
    svg: 'image/svg+xml',
    ico: 'image/x-icon',
    bmp: 'image/bmp'
};

/**
 * 本地图片转 DataURL(Base64)
 * @param {string} imgAbsPath 图片绝对路径
 * @returns {string|null} base64字符串
 */
function imageToBase64(imgAbsPath) {
    try {
        if (!fs.existsSync(imgAbsPath)) {
            console.warn(`⚠️ 文件不存在：${imgAbsPath}`);
            return null;
        }
        const ext = path.extname(imgAbsPath).toLowerCase().replace('.', '');
        const mime = MIME_MAP[ext];
        if (!mime) {
            console.warn(`⚠️ 不支持的图片格式 ${ext}：${imgAbsPath}`);
            return null;
        }
        const buffer = fs.readFileSync(imgAbsPath);
        const base64 = buffer.toString('base64');
        return `data:${mime};base64,${base64}`;
    } catch (err) {
        console.error(`❌ 转换失败 ${imgAbsPath}:`, err.message);
        return null;
    }
}

async function runConvert() {
    const htmlAbsPath = path.resolve(INPUT_HTML);
    const htmlDir = path.dirname(htmlAbsPath);

    if (!fs.existsSync(htmlAbsPath)) {
        console.error(`❌ 输入文件不存在: ${htmlAbsPath}`);
        return;
    }

    const rawHtml = fs.readFileSync(htmlAbsPath, 'utf8');
    const $ = cheerio.load(rawHtml);
    const imgList = $('img');
    let replaceCount = 0;

    for (const el of imgList) {
        const src = $(el).attr('src');
        if (!src) continue;

        // 跳过网络图片、base64已经内嵌的图片
        if (src.startsWith('http://') || src.startsWith('https://') || src.startsWith('data:')) {
            continue;
        }

        // 拼接图片绝对路径
        const imgAbsPath = path.resolve(htmlDir, src);
        const base64Url = imageToBase64(imgAbsPath);

        if (base64Url) {
            $(el).attr('src', base64Url);
            replaceCount++;
            console.log(`✅ 已转换：${src}`);
        }
    }

    // 写出新HTML
    const newHtml = $.html();
    fs.writeFileSync(OUTPUT_HTML, newHtml, 'utf8');
    console.log(`\n🎉 转换完成！一共替换 ${replaceCount} 张图片`);
    console.log(`📄 输出文件：${path.resolve(OUTPUT_HTML)}`);
}

runConvert();