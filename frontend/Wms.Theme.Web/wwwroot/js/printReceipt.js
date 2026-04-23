function printInbound() {
   
    const printContents = document.getElementById('printTemplate').innerHTML;

  
    const printWindow = window.open('', '_blank', 'width=900,height=800');

   
    printWindow.document.write(`
        <html>
            <head>
                <script src="https://cdn.tailwindcss.com"></script>
                <style>
                    body { background: white !important; color: black !important; padding: 20px; }
                    .print-grid { display: grid !important; grid-template-columns: repeat(4, 1fr) !important; width: 100% !important; }
                    table { width: 100% !important; border-collapse: collapse !important; }
                    th, td { border: 1px solid black !important; padding: 4px !important; }
                </style>
            </head>
            <body>
                ${printContents}
                <script>
                  
                    setTimeout(() => {
                        window.print();
                        window.close();
                    }, 500);
                </script>
            </body>
        </html>
    `);

    printWindow.document.close();
}