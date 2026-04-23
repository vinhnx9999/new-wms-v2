/**
 * QR Print Utility
 * Opens a new tab with QR codes for printing
 * Generates QR codes from data returned by server
 */

/**
 * Generate QR code as SVG string using qrcodegen library
 * @param {string} text - Text to encode in QR
 * @returns {string} - SVG string
 */
function generateQrSvg(text) {
    if (!text || text.trim() === '') {
        return '<p style="color:red;">No data</p>';
    }

    // Use qrcodegen library (must be loaded before this script)
    if (typeof qrcodegen !== 'undefined' && qrcodegen.QrCode) {
        try {
            const QRC = qrcodegen.QrCode;
            const qrCode = QRC.encodeText(text, QRC.Ecc.MEDIUM);

            // Manually create SVG since toSvgString may not exist in this version
            const border = 4;
            const size = qrCode.size;
            const svgSize = size + border * 2;

            let svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${svgSize} ${svgSize}" shape-rendering="crispEdges" style="max-width:180px;height:auto;">`;
            svg += `<rect width="100%" height="100%" fill="#ffffff"/>`;
            svg += `<path d="`;

            for (let y = 0; y < size; y++) {
                for (let x = 0; x < size; x++) {
                    if (qrCode.getModule(x, y)) {
                        svg += `M${x + border},${y + border}h1v1h-1z `;
                    }
                }
            }

            svg += `" fill="#000000"/>`;
            svg += `</svg>`;

            return svg;
        } catch (e) {
            console.error('qrcodegen failed:', e);
            return '<p style="color:red;">QR Error: ' + e.message + '</p>';
        }
    }

    return '<p style="color:red;">QR library not loaded</p>';
}

/**
 * Print QR codes from a provided data array
 * @param {Array} qrData 
 * @param {Object} headerInfo 
 * @param {Object} messages 
 */
function printQrCode(qrData, headerInfo = {}, messages = {}) {
    const defaultMessages = {
        noQrFound: 'No QR codes found in the provided data',
        popupBlocked: 'Popup blocked. Please allow popups for this site.',
        systemError: 'System Error'
    };

    const msg = { ...defaultMessages, ...messages };

    try {
      
        if (!Array.isArray(qrData) || qrData.length === 0) {
            if (typeof Notify !== 'undefined') {
                Notify.warning(msg.noQrFound);
            }
            return;
        }

     
        const printWindow = window.open('', '_blank');
        if (!printWindow) {
            if (typeof Notify !== 'undefined') {
                Notify.error(msg.popupBlocked);
            }
            return;
        }

        const pageTitle = headerInfo.title ;
        const pageSubtitle = headerInfo.subtitle ;

      
        let qrHtml = '<!DOCTYPE html><html><head>';
        qrHtml += '<meta charset="UTF-8">';
        qrHtml += '<title>' + pageTitle + '</title>';
        qrHtml += '<style>';
        qrHtml += '* { margin: 0; padding: 0; box-sizing: border-box; }';

        /* Căn giữa toàn màn hình, màu nền xám nhạt như ảnh */
        qrHtml += 'body { font-family: Arial, sans-serif; background-color: #f0f2f5; display: flex; flex-direction: column; align-items: center; justify-content: flex-start; min-height: 100vh; padding-top: 60px; }';

        qrHtml += '.header { text-align: center; margin-bottom: 40px; }';
        qrHtml += '.header h1 { font-size: 28px; color: #333; }';
        if (pageSubtitle) qrHtml += '.header p { color: #555; font-size: 16px; margin-top: 8px; }';

        qrHtml += '.qr-container { display: flex; flex-wrap: wrap; gap: 30px; justify-content: center; max-width: 1200px; }';

        /* Box chứa QR: nền trắng, viền xám mỏng, bo góc, bóng đổ nhẹ */
        qrHtml += '.qr-item { background: #ffffff; border: 1px solid #e0e0e0; padding: 40px 30px; text-align: center; page-break-inside: avoid; width: 320px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }';
        qrHtml += '.qr-item svg { max-width: 200px; height: auto; margin: 0 auto 20px; display: block; }';

        /* Text mã Pallet màu xanh đậm, in đậm */
        qrHtml += '.qr-item .qr-title { font-weight: bold; font-size: 18px; color: #1a56db; word-break: break-all; }';
        qrHtml += '.qr-item .qr-desc { font-size: 14px; color: #555; margin-top: 8px; }';

        /* Loại bỏ nền và viền khi máy in thực sự in ra giấy */
        qrHtml += '@media print { body { background-color: white; padding-top: 0; } .qr-item { box-shadow: none; border: 1px solid #000; padding: 20px; } }';
        qrHtml += '</style>';
        qrHtml += '</head><body>';

        qrHtml += '<div class="header">';
        qrHtml += '<h1>' + pageTitle + '</h1>';
        if (pageSubtitle) qrHtml += '<p>' + pageSubtitle + '</p>';
        qrHtml += '</div>';

        qrHtml += '<div class="qr-container">';

        qrData.forEach(function (qr) {
            const qrContent = qr.content || '';
            const itemTitle = qr.title || '';
            const itemDesc1 = qr.desc1 || '';
            const itemDesc2 = qr.desc2 || '';

            const qrSvg = generateQrSvg(qrContent);

            qrHtml += '<div class="qr-item">';
            qrHtml += qrSvg;
            if (itemTitle) qrHtml += '<div class="qr-title">' + itemTitle + '</div>';
            if (itemDesc1) qrHtml += '<div class="qr-desc">' + itemDesc1 + '</div>';
            if (itemDesc2) qrHtml += '<div class="qr-desc">' + itemDesc2 + '</div>';
            qrHtml += '</div>';
        });

        qrHtml += '</div>';

        /* Script xử lý tự động in và đóng tab */
        qrHtml += '<script>';
        qrHtml += 'window.onafterprint = function() { window.close(); };';
        qrHtml += 'window.onload = function() { setTimeout(function() { window.print(); }, 300); };';
        qrHtml += '</script>';

        qrHtml += '</body></html>';

        printWindow.document.write(qrHtml);
        printWindow.document.close();

    } catch (error) {
        console.error('Print QR error:', error);
        // Giữ nguyên logic bắt lỗi hệ thống
        if (typeof Notify !== 'undefined') {
            Notify.error(msg.systemError);
        }
    }
}

// Export for use in other modules if needed
if (typeof window !== 'undefined') {
    window.printQrCode = printQrCode;
    window.generateQrSvg = generateQrSvg;
}