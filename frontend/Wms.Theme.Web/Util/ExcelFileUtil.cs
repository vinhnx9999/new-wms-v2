using ClosedXML.Excel;
using Wms.Theme.Web.Util.ExcelColumns;
using WMSSolution.Shared.Excel;

namespace Wms.Theme.Web.Util;

public class ExcelFileUtil
{
    public async Task<List<InputSku>> ReadSheetInputSkuAsync(IFormFile excelFile, int startRow = 1)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InputSku>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var sku = new InputSku
                {
                    Code = row.Cell(ColExcelDefineSku.ColCode).GetString(),
                    Name = row.Cell(ColExcelDefineSku.ColName).GetString(),
                    Category = row.Cell(ColExcelDefineSku.ColCategory).GetString(),
                    Property = row.Cell(ColExcelDefineSku.ColProperty).GetString(),
                    Unit = row.Cell(ColExcelDefineSku.ColUnit).GetString(),
                    Specification = row.Cell(ColExcelDefineSku.ColSpecification).GetString(),
                    ProductCode = row.Cell(ColExcelDefineSku.ColProductCode).GetString(),
                    ProductName = row.Cell(ColExcelDefineSku.ColProductName).GetString(),
                    AllowDuplicate = row.Cell(ColExcelDefineSku.ColProductDuplicate).GetString()
                };

                results.Add(sku);
            }

            return results;
        }


        return [];
    }

    public async Task<List<InputSupplier>> ReadSheetInputSupplierAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InputSupplier>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var supplier = new InputSupplier
                {
                    Code = row.Cell(ColExcelDefineSupplier.ColCode).GetString(),
                    Name = row.Cell(ColExcelDefineSupplier.ColName).GetString(),
                    Address = row.Cell(ColExcelDefineSupplier.ColAddress).GetString(),
                    GroupSupplier = row.Cell(ColExcelDefineSupplier.ColGroupSupplier).GetString(),
                    TaxNumber = row.Cell(ColExcelDefineSupplier.ColTax).GetString(),
                    Phone = row.Cell(ColExcelDefineSupplier.ColPhone).GetString(),
                    IsFollow = row.Cell(ColExcelDefineSupplier.ColFollow).GetString()
                };

                results.Add(supplier);
            }

            return results;
        }


        return [];
    }

    public async Task<List<InputCustomer>> ReadSheetInputCustomerAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InputCustomer>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var customer = new InputCustomer
                {
                    Code = row.Cell(ColExcelDefineSupplier.ColCode).GetString(),
                    Name = row.Cell(ColExcelDefineSupplier.ColName).GetString(),
                    Address = row.Cell(ColExcelDefineSupplier.ColAddress).GetString(),
                    GroupCustomer = row.Cell(ColExcelDefineSupplier.ColGroupSupplier).GetString(),
                    TaxNumber = row.Cell(ColExcelDefineSupplier.ColTax).GetString(),
                    Phone = row.Cell(ColExcelDefineSupplier.ColPhone).GetString(),
                    IsFollow = row.Cell(ColExcelDefineSupplier.ColFollow).GetString()
                };

                results.Add(customer);
            }

            return results;
        }


        return [];
    }

    public async Task<List<InputUnitOfMeasure>> ReadSheetInputUnitAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InputUnitOfMeasure>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var customer = new InputUnitOfMeasure
                {
                    Name = row.Cell(ColExcelDefineUOM.ColName).GetString(),
                    Description = row.Cell(ColExcelDefineUOM.ColDescription).GetString(),
                    IsFollow = row.Cell(ColExcelDefineUOM.ColFollow).GetString()
                };

                results.Add(customer);
            }

            return results;
        }


        return [];
    }

    public async Task<List<InputSpecification>> ReadSheetInputSpecificationAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InputSpecification>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var customer = new InputSpecification
                {
                    Code = row.Cell(ColExcelDefineSpecification.ColCode).GetString(),
                    DisplayName = row.Cell(ColExcelDefineSpecification.ColDisplayName).GetString(),
                    AllowDuplicate = row.Cell(ColExcelDefineSpecification.ColAllowDuplicate).GetString()
                };

                results.Add(customer);
            }

            return results;
        }


        return [];
    }

    public async Task<List<OutboundOrderExcel>> ReadSheetOutboundOrderAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<OutboundOrderExcel>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var customer = new OutboundOrderExcel
                {
                    OrderCode = row.Cell(ColExcelDefOutbound.ColOrderCode).GetString(),
                    LocationCode = row.Cell(ColExcelDefOutbound.ColLocationCode).GetString(),
                    Note = row.Cell(ColExcelDefOutbound.ColNote).GetString(),
                    Qty = row.Cell(ColExcelDefOutbound.ColQty).GetString(),
                    SkuCode = row.Cell(ColExcelDefOutbound.ColSKU).GetString(),
                    CustomerName = row.Cell(ColExcelDefOutbound.ColCustomerName).GetString(),
                    UnitName = row.Cell(ColExcelDefOutbound.ColUnit).GetString(),
                    WareHouseName = row.Cell(ColExcelDefOutbound.ColWareHouseName).GetString()
                };

                results.Add(customer);
            }

            return results;
        }


        return [];
    }

    public async Task<List<InboundOrderExcel>> ReadSheetInboundOrderAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InboundOrderExcel>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var customer = new InboundOrderExcel
                {
                    OrderCode = row.Cell(ColExcelDefineInbound.ColOrderCode).GetString(),
                    ExpireDate = TryGetDateTime(row, ColExcelDefineInbound.ColExpireDate),
                    IsPutaway = row.Cell(ColExcelDefineInbound.ColIsPutaway).GetString(),
                    LocationCode = row.Cell(ColExcelDefineInbound.ColLocationCode).GetString(),
                    Note = row.Cell(ColExcelDefineInbound.ColNote).GetString(),
                    Qty = row.Cell(ColExcelDefineInbound.ColQty).GetString(),
                    SkuCode = row.Cell(ColExcelDefineInbound.ColSKU).GetString(),
                    SupplierName = row.Cell(ColExcelDefineInbound.ColSupplier).GetString(),
                    UnitName = row.Cell(ColExcelDefineInbound.ColUnit).GetString(),
                    WareHouseName = row.Cell(ColExcelDefineInbound.ColWareHouseName).GetString()
                };

                results.Add(customer);
            }

            return results;
        }


        return [];
    }

    public async Task<List<BeginMerchandiseExcel>> ReadSheetBeginMerchandiseAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<BeginMerchandiseExcel>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                var customer = new BeginMerchandiseExcel
                {
                    ExpireDate = TryGetDateTime(row, ColExcelDefBeginMerchandise.ColExpireDate),
                    IsPutaway = row.Cell(ColExcelDefBeginMerchandise.ColIsPutaway).GetString(),
                    LocationCode = row.Cell(ColExcelDefBeginMerchandise.ColLocationCode).GetString(),
                    Note = row.Cell(ColExcelDefBeginMerchandise.ColNote).GetString(),
                    Qty = row.Cell(ColExcelDefBeginMerchandise.ColQty).GetString(),
                    SkuCode = row.Cell(ColExcelDefBeginMerchandise.ColSKU).GetString(),
                    SupplierName = row.Cell(ColExcelDefBeginMerchandise.ColSupplier).GetString(),
                    UnitName = row.Cell(ColExcelDefBeginMerchandise.ColUnit).GetString(),
                    WareHouseName = row.Cell(ColExcelDefBeginMerchandise.ColWareHouseName).GetString()
                };

                results.Add(customer);
            }

            return results;
        }

        return [];
    }

    private DateTime? TryGetDateTime(IXLRangeRow row, int colIdx)
    {
        try
        {
            return row.Cell(colIdx).GetDateTime();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }

    public async Task<List<InputSkuSafetyStock>> ReadSheetSkuSafetyStockAsync(IFormFile excelFile, int startRow)
    {
        if (excelFile != null && excelFile.Length > 0)
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0; // reset before reading

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var results = new List<InputSkuSafetyStock>();
            // Loop through rows (skip header row)
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
            {
                results.Add(new InputSkuSafetyStock
                {
                    WareHouseName = row.Cell(ColExcelDefSkuSafetyStock.ColWareHouseName).GetString(),
                    SkuCode = row.Cell(ColExcelDefSkuSafetyStock.ColSKU).GetString(),
                    Qty = row.Cell(ColExcelDefSkuSafetyStock.ColQty).GetString()
                });
            }

            return results;
        }


        return [];
    }

    public async Task<List<InputLocationExcel>> ReadSheetInputLocationAsync(IFormFile excelFile, int startRow = 1)
    {
        if (excelFile == null || excelFile.Length <= 0)
        {
            return [];
        }

        using var stream = new MemoryStream();
        await excelFile.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var results = new List<InputLocationExcel>();

        foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(startRow))
        {
            var warehouseName = row.Cell(ColExcelDefineLocation.ColWarehouseName).GetString().Trim();
            var locationName = row.Cell(ColExcelDefineLocation.ColLocationName).GetString().Trim();
            var coordinateX = row.Cell(ColExcelDefineLocation.ColCoordinateX).GetString().Trim();
            var coordinateY = row.Cell(ColExcelDefineLocation.ColCoordinateY).GetString().Trim();
            var coordinateZ = row.Cell(ColExcelDefineLocation.ColCoordinateZ).GetString().Trim();
            var isVirtualRaw = row.Cell(ColExcelDefineLocation.ColIsVirtual).GetString().Trim();
            var priorityRaw = row.Cell(ColExcelDefineLocation.ColPriority).GetString().Trim();

            if (string.IsNullOrWhiteSpace(warehouseName))
            {
                continue;
            }

            var isVirtual = isVirtualRaw.Equals("true", StringComparison.OrdinalIgnoreCase)
                            || isVirtualRaw.Equals("yes", StringComparison.OrdinalIgnoreCase)
                            || isVirtualRaw == "1";

            var priority = 1;
            if (!string.IsNullOrWhiteSpace(priorityRaw) && int.TryParse(priorityRaw, out var p))
            {
                priority = p;
            }

            results.Add(new InputLocationExcel
            {
                WarehouseName = warehouseName,
                LocationName = locationName,
                CoordinateX = coordinateX,
                CoordinateY = coordinateY,
                CoordinateZ = coordinateZ,
                IsVirtualLocation = isVirtual,
                Priority = priority
            });
        }

        return results;
    }
}

